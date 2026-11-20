namespace ArrowSwarm.Core.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Modes.Editor;
    using UnityEditor;
    using UnityEditor.Profiling;
    using UnityEditorInternal;
    using UnityEngine;
    using Profiler = UnityEngine.Profiling.Profiler;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>
    /// Play Mode stress probe: loads a late Defense level, fires every clear arrow in quick
    /// succession while the Profiler records, then reports the slowest frames with their
    /// most expensive markers and the per-frame GC allocation. Player data is restored.
    /// </summary>
    public static class GameplayPerfProbe
    {
        private static readonly Stack<IEnumerator> Routines = new Stack<IEnumerator>();
        private static readonly Dictionary<string, float> GcByOwner = new Dictionary<string, float>();
        private static float _waitUntil;
        private static QualityUIVerificationState _data;
        private static string _debugJson;
        private static int _firstFrame, _fired, _particlesBefore, _particlesAfter;

        /// <summary>Level used by the probe; late levels have the largest boards.</summary>
        public static int Level = 150;

        /// <summary>Runs the probe; Play Mode must already be active.</summary>
        [MenuItem("ArrowSwarm/Verification/Gameplay Perf Probe (Play Mode)")]
        public static void Run()
        {
            if (!Application.isPlaying || Routines.Count > 0) return;
            _data = new QualityUIVerificationState();
            var debug = DebugManager.Instance; _debugJson = JsonUtility.ToJson(debug);
            var so = new SerializedObject(debug);
            so.FindProperty("_useDebugLevel").boolValue = true; so.FindProperty("_debugLevel").intValue = Level;
            so.FindProperty("_infiniteLives").boolValue = true; so.ApplyModifiedPropertiesWithoutUndo();
            Routines.Push(Steps());
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (Time.realtimeSinceStartup < _waitUntil) return;
            try
            {
                while (Routines.Count > 0)
                {
                    var top = Routines.Peek();
                    if (!top.MoveNext()) { Routines.Pop(); continue; }
                    if (top.Current is float seconds) { _waitUntil = Time.realtimeSinceStartup + seconds; return; }
                }
                Finish(Report());
            }
            catch (System.Exception error) { Routines.Clear(); Finish("ERROR " + error); }
        }

        private static IEnumerator Steps()
        {
            GameManager.Instance.StartGame();
            yield return 4f;
            ProfilerDriver.ClearAllFrames();
            ProfilerDriver.enabled = true; Profiler.enabled = true;
            yield return 0.2f;
            _firstFrame = ProfilerDriver.lastFrameIndex; _fired = 0;
            _particlesBefore = ArrowSwarm.Effects.ParticleManager.Instance.CreatedCount;
            float end = Time.realtimeSinceStartup + 12f;
            while (Time.realtimeSinceStartup < end && GameManager.Instance.CurrentState == GameState.Playing)
            {
                foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
                {
                    if (arrow == null || arrow.IsFired || arrow.IsBlockedAnimating) continue;
                    if (!GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection)) continue;
                    arrow.OnPlayerClick(); _fired++;
                    break;
                }
                yield return 0.1f;
            }
            // Finish like a rainbow arrow does: every live mob dies in the same frame.
            var spawner = ArrowSwarm.Mob.MobSpawner.Instance;
            if (spawner != null) spawner.DestroyAllMobs();
            yield return 0.5f;
            _particlesAfter = ArrowSwarm.Effects.ParticleManager.Instance.CreatedCount;
            Profiler.enabled = false; ProfilerDriver.enabled = false;
        }

        private static string Report()
        {
            int last = ProfilerDriver.lastFrameIndex;
            var frames = new List<KeyValuePair<int, float>>();
            GcByOwner.Clear();
            var totals = new Dictionary<string, float>();
            float gcTotal = 0f; int count = 0;
            for (int f = Mathf.Max(_firstFrame, ProfilerDriver.firstFrameIndex); f <= last; f++)
            {
                using (var view = ProfilerDriver.GetHierarchyFrameDataView(f, 0, HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName,
                    HierarchyFrameDataView.columnSelfTime, false))
                {
                    if (!view.valid) continue;
                    frames.Add(new KeyValuePair<int, float>(f, view.frameTimeMs)); count++;
                    Accumulate(view, view.GetRootItemID(), totals, ref gcTotal);
                }
            }
            frames.Sort((a, b) => b.Value.CompareTo(a.Value));
            var sb = new StringBuilder();
            sb.AppendLine("level " + Level + " frames " + count + " fired " + _fired + " particlesCreatedMidLevel " + (_particlesAfter - _particlesBefore) + " avgGC/frame " + (count > 0 ? gcTotal / count / 1024f : 0f).ToString("F1") + " KB");
            sb.AppendLine("TOP SELF (ms total over run):");
            var ordered = new List<KeyValuePair<string, float>>(totals);
            ordered.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < ordered.Count && i < 25; i++) sb.AppendLine("  " + ordered[i].Value.ToString("F1") + "  " + ordered[i].Key);
            sb.AppendLine("GC BY CALLER (KB over run):");
            var gcOrdered = new List<KeyValuePair<string, float>>(GcByOwner);
            gcOrdered.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < gcOrdered.Count && i < 15; i++) sb.AppendLine("  " + (gcOrdered[i].Value / 1024f).ToString("F1") + "  " + gcOrdered[i].Key);
            sb.AppendLine("SLOWEST FRAMES:");
            for (int i = 0; i < frames.Count && i < 6; i++)
            {
                sb.AppendLine(" frame " + frames[i].Key + " " + frames[i].Value.ToString("F1") + " ms");
                using (var view = ProfilerDriver.GetHierarchyFrameDataView(frames[i].Key, 0, HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName,
                    HierarchyFrameDataView.columnSelfTime, false))
                {
                    var local = new Dictionary<string, float>(); float gc = 0f;
                    Accumulate(view, view.GetRootItemID(), local, ref gc);
                    var top = new List<KeyValuePair<string, float>>(local);
                    top.Sort((a, b) => b.Value.CompareTo(a.Value));
                    for (int j = 0; j < top.Count && j < 8; j++) sb.AppendLine("    " + top[j].Value.ToString("F2") + "  " + top[j].Key);
                    sb.AppendLine("    GC " + (gc / 1024f).ToString("F1") + " KB");
                }
            }
            return sb.ToString();
        }

        private static void Accumulate(HierarchyFrameDataView view, int id, Dictionary<string, float> totals, ref float gc)
        {
            var children = new List<int>();
            view.GetItemChildren(id, children);
            foreach (int child in children)
            {
                string name = view.GetItemName(child);
                float self = view.GetItemColumnDataAsFloat(child, HierarchyFrameDataView.columnSelfTime);
                if (name == "GC.Alloc")
                {
                    float bytes = view.GetItemColumnDataAsFloat(child, HierarchyFrameDataView.columnGcMemory);
                    gc += bytes;
                    string owner = view.GetItemName(id);
                    GcByOwner.TryGetValue(owner, out float total); GcByOwner[owner] = total + bytes;
                }
                if (name != "EditorLoop" && !name.StartsWith("PlayerLoop"))
                {
                    totals.TryGetValue(name, out float value); totals[name] = value + self;
                }
                Accumulate(view, child, totals, ref gc);
            }
        }

        private static void Finish(string report)
        {
            EditorApplication.update -= Tick;
            Profiler.enabled = false; ProfilerDriver.enabled = false;
            if (DebugManager.HasInstance) JsonUtility.FromJsonOverwrite(_debugJson, DebugManager.Instance);
            if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
            _data?.Dispose();
            bool restored = _data != null && _data.IsRestored(); _data = null;
            UnityEngine.Debug.Log("[ArrowSwarm] PerfProbe (data restored " + restored + ")\n" + report);
        }
    }
}
