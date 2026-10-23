namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Measures repeated hint clearing and checks that genuine hint changes still render correctly.</summary>
    public static class QualityHintPerformanceVerification
    {
        /// <summary>Checks the real puzzle prefab without touching its authored assets or player state.</summary>
        public static string Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Run this isolated fixture in Edit Mode.");
            const int iterations = 10000;
            var scene = EditorSceneManager.NewPreviewScene();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/PuzzleArrow.prefab");
            var instance = UnityEngine.Object.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(instance, scene);
            var source = AssetDatabase.LoadAssetAtPath<PuzzleConfig>("Assets/_Project/ScriptableObjects/Puzzles/ShapesPuzzleConfig.asset");
            var config = UnityEngine.Object.Instantiate(source); config.hideFlags = HideFlags.HideAndDontSave;
            string originalConfig = EditorJsonUtility.ToJson(source);
            var view = instance.GetComponent<PuzzleArrowView>();
            var body = instance.transform.Find("Body").GetComponent<LineRenderer>();
            var head = instance.transform.Find("FilledHead").GetComponent<SpriteRenderer>();
            var results = new List<string>();
            try
            {
                Check(QualityAllocationEvents.Measure(() => { }) == 0, "Empty native profiler bracket records no allocation", results);
                Check(QualityAllocationEvents.Measure(() => GC.KeepAlive(new byte[128])) >= 1,
                    "Positive control proves the native profiler detects managed allocations", results);
                var points = new List<Vector2Int> { new Vector2Int(4, 4), new Vector2Int(4, 3),
                    new Vector2Int(4, 2), new Vector2Int(3, 2), new Vector2Int(2, 2),
                    new Vector2Int(2, 3), new Vector2Int(1, 3), new Vector2Int(0, 3) };
                var data = new PuzzleArrowData(points, Vector2Int.up, new Color(.94f, .3f, .46f));
                view.Bind(data, 5, Vector3.zero, config);
                var normal = Positions(body); Vector3 normalScale = head.transform.localScale;
                Measure(view, false, iterations, results);
                Check(Same(body, normal) && head.transform.localScale == normalScale,
                    "Repeated clear keeps every normal point and head pose unchanged", results);
                view.Highlight(true); var highlighted = Positions(body);
                Check(Mathf.Abs(body.widthMultiplier - config.StrokeWidth * 1.8f) < .00001f &&
                    head.transform.localScale.x > normalScale.x, "First hint request enlarges the authored head/stroke", results);
                Measure(view, true, iterations, results);
                Check(Same(body, highlighted), "Repeated hint request keeps highlighted geometry unchanged", results);
                view.Highlight(false);
                Check(Same(body, normal) && head.transform.localScale == normalScale,
                    "A real clear transition restores all normal geometry", results);
                view.RenderTravel(1.01f); var moving = Positions(body); Vector3 movingHead = head.transform.position;
                view.Highlight(false);
                Check(Same(body, moving) && head.transform.position == movingHead,
                    "No-op clear never resets a moving arrow to its spawn pose", results);
                view.Highlight(true); view.Highlight(false);
                Check(Same(body, moving) && head.transform.position == movingHead,
                    "Hint transitions preserve the current travel window", results);
                view.RenderRocket(.7f, Vector3.up * 3); view.Bind(data, 9, Vector3.zero, config);
                Check(Same(body, normal) && body.startColor.a == 1f && head.color.a == 1f && view.Index == 9,
                    "Pool rebind after Rocket restores normal geometry and opacity", results);
                Check(EditorJsonUtility.ToJson(source) == originalConfig, "Authored Inspector config remains unchanged", results);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance); UnityEngine.Object.DestroyImmediate(config);
                EditorSceneManager.ClosePreviewScene(scene);
            }
            string result = "PASS: no-op hint work skipped; genuine hint, flight and pool geometry preserved";
            Directory.CreateDirectory("Captures/QualityPass");
            File.WriteAllText("Captures/QualityPass/hint-performance-verification.txt", result + "\n" + string.Join("\n", results) + "\n");
            return result;
        }

        private static void Measure(PuzzleArrowView view, bool enabled, int iterations, List<string> results)
        {
            for (int i = 0; i < 100; i++) view.Highlight(enabled);
            var clock = new Stopwatch();
            clock.Start();
            int allocations = QualityAllocationEvents.Measure(() =>
            { for (int i = 0; i < iterations; i++) view.Highlight(enabled); });
            clock.Stop();
            Check(allocations == 0, iterations + " repeated " + (enabled ? "hint" : "clear") + " calls produce zero GC.Alloc events", results);
            results.Add("MEASURE: " + iterations + " repeated " + enabled + " calls: " +
                clock.Elapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture) + " ms including profiler bracket; " + allocations + " GC.Alloc events");
        }

        private static Vector3[] Positions(LineRenderer body)
        { var points = new Vector3[body.positionCount]; body.GetPositions(points); return points; }

        private static bool Same(LineRenderer body, Vector3[] points)
        {
            if (body.positionCount != points.Length) return false;
            for (int i = 0; i < points.Length; i++) if ((body.GetPosition(i) - points[i]).sqrMagnitude > .00000001f) return false;
            return true;
        }

        private static void Check(bool condition, string label, List<string> results)
        { if (!condition) throw new InvalidOperationException(label); results.Add("PASS: " + label); }
    }
}
