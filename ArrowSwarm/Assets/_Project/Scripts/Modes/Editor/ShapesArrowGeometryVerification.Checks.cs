namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using Camera = UnityEngine.Camera;

    /// <summary>Checks production geometry across thickness, turns, travel boundaries and pool rebinding.</summary>
    public static partial class ShapesArrowGeometryVerification
    {
        private static readonly float[] Travels = { 0f, .01f, .5f, .99f, 1f, 1.01f, 1.5f, 1.99f, 2f, 2.01f, 3.99f, 4f, 4.01f, 5f, 5.01f, 7f };
        private static int _checks;
        private static List<string> _failures;

        /// <summary>Runs isolated native LineRenderer mesh assertions without changing assets or player data.</summary>
        public static string Verify()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Verification requires Edit Mode");
            _checks = 0; _failures = new List<string>(); Directory.CreateDirectory(Folder);
            var scene = EditorSceneManager.NewPreviewScene();
            var cameraObject = new GameObject("Geometry Bake Camera", typeof(Camera));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
            var camera = cameraObject.GetComponent<Camera>(); camera.scene = scene;
            camera.transform.position = new Vector3(0f, 0f, -10f); camera.orthographic = true;
            var mesh = new Mesh();
            try
            {
                for (int row = 0; row < Widths.Length; row++)
                {
                    var config = CloneConfig(row); var view = Spawn(scene);
                    try
                    {
                        for (int direction = 0; direction < 4; direction++)
                        {
                            for (int shape = 0; shape < 3; shape++)
                            {
                                var data = shape == 0 ? Bent(direction) : shape == 1 ? Short(direction) : LongStairs(direction);
                                string id = "width=" + Widths[row] + " direction=" + direction + " shape=" + shape;
                                for (int hint = 0; hint < 2; hint++)
                                {
                                    view.Bind(data, 0, Center(data), config); view.Highlight(hint == 1);
                                    foreach (float travel in Travels)
                                    {
                                        view.RenderTravel(travel);
                                        CheckPose(view, data, config, hint == 1, travel, camera, mesh, id);
                                    }
                                }
                                view.Highlight(true); view.RenderRocket(.6f, Vector3.up * 2f);
                                view.gameObject.SetActive(false); view.gameObject.SetActive(true);
                                view.Bind(data, 17, Center(data), config);
                                var body = view.transform.Find("Body").GetComponent<LineRenderer>();
                                var head = view.transform.Find("FilledHead").GetComponent<SpriteRenderer>();
                                Check(Mathf.Abs(body.widthMultiplier - config.StrokeWidth) < .00001f,
                                    id + ": pool bind restores configured body thickness");
                                Check(body.startColor.a == 1f && head.color.a == 1f && view.transform.localScale == Vector3.one,
                                    id + ": pool bind restores opacity and transform after rocket/highlight");
                                Check(view.Index == 17 && Mathf.Abs(head.transform.localScale.x - config.HeadSize) < .00001f,
                                    id + ": pool bind restores triangle size and identity");
                            }
                        }
                    }
                    finally { UnityEngine.Object.DestroyImmediate(view.gameObject); UnityEngine.Object.DestroyImmediate(config); }
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(mesh); EditorSceneManager.ClosePreviewScene(scene); }
            string result = (_failures.Count == 0 ? "PASS" : "FAIL") + ": " + _checks + " geometry assertions; " + _failures.Count + " failures";
            var lines = new List<string> { result,
                "Real PuzzleArrow.prefab; cloned ShapesPuzzleConfig; edit-mode PreviewScene only",
                "3 widths x 4 directions x bent/2-point/long-stairs x normal/highlight x 16 travel samples",
                "Travel probes include .01/.99/1/1.01 and subsequent integer boundaries",
                "Finite native mesh, continuous rounded centerline, no duplicate segments, bounded mesh envelope, stable corner tessellation, triangle containment, pooled rocket reset" };
            lines.AddRange(_failures); File.WriteAllLines(Folder + "/geometry-verification.txt", lines); return result;
        }

        private static PuzzleArrowData Short(int direction)
        {
            return new PuzzleArrowData(new List<Vector2Int> { Rotate(Vector2Int.up, direction), Vector2Int.zero },
                Rotate(Vector2Int.up, direction), Ink);
        }

        private static PuzzleArrowData LongStairs(int direction)
        {
            var points = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1),
                new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(3, 2), new Vector2Int(3, 3),
                new Vector2Int(4, 3), new Vector2Int(4, 4), new Vector2Int(5, 4), new Vector2Int(5, 5),
                new Vector2Int(5, 6), new Vector2Int(5, 7), new Vector2Int(5, 8), new Vector2Int(5, 9) };
            points.Reverse();
            for (int i = 0; i < points.Count; i++) points[i] = Rotate(points[i], direction);
            return new PuzzleArrowData(points, Rotate(Vector2Int.up, direction), Ink);
        }

        private static void CheckPose(PuzzleArrowView view, PuzzleArrowData data, PuzzleConfig config, bool hint,
            float travel, Camera camera, Mesh mesh, string id)
        {
            id += " hint=" + hint + " travel=" + travel;
            var body = view.transform.Find("Body").GetComponent<LineRenderer>();
            var head = view.transform.Find("FilledHead").GetComponent<SpriteRenderer>();
            float scale = hint ? 1.8f : 1f, half = body.widthMultiplier * .5f;
            Vector3 tip = Center(data) + (Vector3)(Vector2)data.Head + (Vector3)(Vector2)data.Direction * travel;
            Vector3 forward = (Vector3)(Vector2)data.Direction, side = new Vector3(-forward.y, forward.x);
            bool smooth = true, duplicates = false;
            for (int i = 1; i < body.positionCount; i++)
            {
                Vector3 delta = body.GetPosition(i) - body.GetPosition(i - 1);
                if (delta.sqrMagnitude < .00000001f) duplicates = true;
                if (i > 1)
                {
                    Vector3 previous = body.GetPosition(i - 1) - body.GetPosition(i - 2);
                    if (Vector3.Dot(previous.normalized, delta.normalized) < .65f) smooth = false;
                }
            }
            Check(smooth && !duplicates, id + ": continuous nonduplicate stroke segments");
            if (body.positionCount == 0)
            {
                Check(config.HeadSize * scale >= view.Length, id + ": head-only short arrow is fully covered by its enlarged triangle");
                return;
            }
            mesh.Clear(); body.BakeMesh(mesh, camera, true);
            bool finite = true, contained = true, bounded = true;
            var positions = new Vector3[body.positionCount]; body.GetPositions(positions);
            Vector3 endCenter = positions[positions.Length - 1];
            float capRadiusSquared = (half + .0001f) * (half + .0001f);
            foreach (Vector3 vertex in mesh.vertices)
            {
                if (!Finite(vertex.x) || !Finite(vertex.y) || !Finite(vertex.z)) { finite = false; continue; }
                float distance = float.PositiveInfinity;
                for (int i = 1; i < positions.Length; i++) distance = Mathf.Min(distance, SegmentDistance(vertex, positions[i - 1], positions[i]));
                if (distance > half * 1.45f + .0001f) bounded = false;
                Vector3 fromEnd = vertex - endCenter;
                // Other bends can share the head's projection while being far outside its local join.
                if (fromEnd.sqrMagnitude <= capRadiusSquared && Vector3.Dot(fromEnd, forward) >= -.0001f)
                {
                    float depth = Vector3.Dot(tip - vertex, forward);
                    float available = Mathf.Max(0f, depth - config.HeadSize * scale * (1.5f / 32f)) * (14f / 29f);
                    if (depth < 0f || Mathf.Abs(Vector3.Dot(vertex - tip, side)) > available + .015f) contained = false;
                }
            }
            Check(finite && mesh.vertexCount > 0, id + ": finite nonempty native baked mesh");
            Check(bounded, id + ": all body vertices stay within the local stroke envelope");
            Check(contained, id + ": no round shaft cap protrudes from the narrow triangle tip");
            Check(body.numCornerVertices == 0, id + ": explicit smooth corners replace unstable native corner fans");
            Check(Mathf.Abs(Vector3.Dot(head.transform.up, forward) - 1f) < .00001f,
                id + ": triangle remains aligned with its cardinal direction");
        }

        private static float SegmentDistance(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 delta = b - a;
            return Vector3.Distance(point, a + delta * Mathf.Clamp01(Vector3.Dot(point - a, delta) / delta.sqrMagnitude));
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static void Check(bool passed, string message)
        {
            _checks++; if (!passed) _failures.Add("FAIL: " + message);
        }
    }
}
