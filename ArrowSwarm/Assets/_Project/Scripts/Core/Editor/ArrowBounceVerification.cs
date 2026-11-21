namespace ArrowSwarm.Core.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Modes.Editor;
    using ArrowSwarm.Utils;
    using UnityEditor;
    using UnityEngine;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>
    /// Play Mode check: after a blocked shot bounces back, the arrow's head, root and body end
    /// must sit exactly on its head grid point, and bumped obstacle arrows must not drift,
    /// even when several bumps and miss-click wiggles overlap.
    /// </summary>
    public static class ArrowBounceVerification
    {
        private static readonly List<string> Results = new List<string>();
        private static readonly Stack<IEnumerator> Routines = new Stack<IEnumerator>();
        private static float _waitUntil;
        private static int _failures;
        private static QualityUIVerificationState _data;
        private static string _debugJson;

        /// <summary>Runs the verification; Play Mode must already be active.</summary>
        [MenuItem("ArrowSwarm/Verification/Arrow Bounce Rest Position (Play Mode)")]
        public static void Run()
        {
            if (!Application.isPlaying || Routines.Count > 0) return;
            Results.Clear(); _failures = 0;
            _data = new QualityUIVerificationState();
            var debug = DebugManager.Instance; _debugJson = JsonUtility.ToJson(debug);
            var so = new SerializedObject(debug);
            so.FindProperty("_useDebugLevel").boolValue = true; so.FindProperty("_debugLevel").intValue = 40;
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
            }
            catch (System.Exception error) { Results.Add("ERROR: " + error); _failures++; Routines.Clear(); }
            Finish();
        }

        private static IEnumerator Steps()
        {
            GameManager.Instance.StartGame();
            yield return 2.5f;
            var blocked = new List<Arrow>();
            foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
                if (arrow != null && !arrow.IsFired && !GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection)) blocked.Add(arrow);
            Check(blocked.Count >= 3, "level has at least three blocked arrows (" + blocked.Count + ")");
            if (blocked.Count < 3) yield break;

            // Single bounce.
            var first = blocked[0];
            GridManager.Instance.GetCollisionPoint(first.HeadPoint, first.HeadDirection, out Arrow obstacle);
            first.OnPlayerClick();
            yield return 1.6f;
            Rest(first, "bounced arrow rests on its head point");
            if (obstacle != null) Rest(obstacle, "bumped obstacle returns to its head point");

            // Overlapping bumps, repeated miss-click wiggles and a bounce interrupted by quick re-taps.
            for (int i = 1; i < blocked.Count && i < 6; i++) { blocked[i].OnPlayerClick(); yield return 0.03f; }
            for (int i = 0; i < 6; i++) { first.OnPlayerClick(); yield return 0.02f; }
            yield return 2.2f;
            foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
                if (arrow != null && !arrow.IsFired) Rest(arrow, null);
            Check(_drifted == 0, "no arrow drifted after overlapping bumps and wiggles (" + _drifted + " drifted)");
        }

        private static int _drifted;

        private static void Rest(Arrow arrow, string label)
        {
            var grid = GridManager.Instance;
            Vector2 expected = arrow.HeadPoint.PointToWorld(grid.PointSpacing, grid.Origin);
            var head = (Transform)typeof(ArrowVisuals).GetField("_headTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(arrow.Visuals);
            var line = arrow.Visuals.GetComponent<LineRenderer>();
            float headError = Vector2.Distance(head.position, expected);
            float rootError = Vector2.Distance(arrow.transform.position, expected);
            float lineError = line != null && line.positionCount > 0 ? Vector2.Distance(line.GetPosition(0), expected) : 0f;
            bool ok = headError < 0.001f && rootError < 0.001f && lineError < 0.001f;
            if (label == null) { if (!ok) _drifted++; return; }
            Check(ok, label + " (head " + headError.ToString("F4") + ", root " + rootError.ToString("F4") + ", body " + lineError.ToString("F4") + ")");
        }

        private static void Check(bool condition, string label)
        { Results.Add((condition ? "PASS: " : "FAIL: ") + label); if (!condition) _failures++; }

        private static void Finish()
        {
            EditorApplication.update -= Tick;
            if (DebugManager.HasInstance) JsonUtility.FromJsonOverwrite(_debugJson, DebugManager.Instance);
            if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
            _data?.Dispose();
            Check(_data != null && _data.IsRestored(), "player data and preferences restored");
            _data = null; _drifted = 0;
            UnityEngine.Debug.Log("[ArrowSwarm] ArrowBounce " + (_failures == 0 ? "PASS" : "FAIL (" + _failures + ")") + "\n" + string.Join("\n", Results));
        }
    }
}
