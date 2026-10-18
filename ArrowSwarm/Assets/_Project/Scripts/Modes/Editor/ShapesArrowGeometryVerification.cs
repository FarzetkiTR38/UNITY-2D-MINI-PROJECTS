namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Camera = UnityEngine.Camera;

    /// <summary>Renders the production puzzle prefab in an isolated editor preview scene.</summary>
    public static partial class ShapesArrowGeometryVerification
    {
        private const string Folder = "Captures/ShapesArrowGeometry";
        private const int Tile = 256;
        private static readonly float[] Widths = { .28f, .42f, .56f };
        private static readonly float[] Heads = { .58f, .72f, .9f };
        private static readonly Color Ink = new Color(.82f, .22f, .4f);

        /// <summary>Captures actual Unity renders; before explicitly reconstructs the original stroke algorithm.</summary>
        public static string Capture(string label)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Capture requires Edit Mode");
            if (label != "before" && label != "after") throw new ArgumentException("Use before or after", nameof(label));
            Directory.CreateDirectory(Folder);
            Scene scene = EditorSceneManager.NewPreviewScene();
            RenderTexture previous = RenderTexture.active;
            var target = new RenderTexture(Tile, Tile, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            UnityEngine.Camera captureCamera = null;
            var sheet = new Texture2D(Tile * 8, Tile * 3, TextureFormat.RGBA32, false);
            var notes = new List<string> { "Actual Unity camera renders: " + label,
                "Rows: Color Shift .28/.58, Shapes .42/.72, thick Shapes .56/.90",
                "Columns: up, right, down, left; normal then highlighted for each direction",
                "Before: previous rounded LineRenderer, all unit points, center-head body end",
                "After: current production PuzzleArrowView geometry; cloned config only" };
            try
            {
                var cameraObject = new GameObject("Isolated Arrow Camera", typeof(Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                var camera = cameraObject.GetComponent<Camera>(); camera.scene = scene;
                captureCamera = camera;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.orthographic = true; camera.orthographicSize = 2.65f;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.99f, .98f, .96f);
                camera.nearClipPlane = .01f; camera.farClipPlane = 30f;
                camera.targetTexture = target; camera.allowHDR = false; camera.allowMSAA = true;
                for (int row = 0; row < Widths.Length; row++)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        for (int hint = 0; hint < 2; hint++)
                        {
                            var config = CloneConfig(row);
                            var view = Spawn(scene); var data = Bent(direction);
                            try
                            {
                                view.Bind(data, 0, Center(data), config); view.Highlight(hint == 1);
                                if (label == "before") OldBody(view, data, config, hint == 1);
                                camera.Render(); RenderTexture.active = target;
                                var tile = new Texture2D(Tile, Tile, TextureFormat.RGBA32, false);
                                try
                                {
                                    tile.ReadPixels(new Rect(0, 0, Tile, Tile), 0, 0); tile.Apply();
                                    sheet.SetPixels((direction * 2 + hint) * Tile, (2 - row) * Tile, Tile, Tile, tile.GetPixels());
                                }
                                finally { UnityEngine.Object.DestroyImmediate(tile); }
                            }
                            finally { UnityEngine.Object.DestroyImmediate(view.gameObject); UnityEngine.Object.DestroyImmediate(config); }
                        }
                    }
                }
                sheet.Apply(); string path = Folder + "/" + label + ".png";
                File.WriteAllBytes(path, sheet.EncodeToPNG());
                File.WriteAllLines(Folder + "/" + label + "-capture.txt", notes);
                return System.IO.Path.GetFullPath(path);
            }
            finally
            {
                RenderTexture.active = previous;
                if (captureCamera != null) captureCamera.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(sheet); target.Release(); UnityEngine.Object.DestroyImmediate(target);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static PuzzleConfig CloneConfig(int row)
        {
            var asset = AssetDatabase.LoadAssetAtPath<PuzzleConfig>("Assets/_Project/ScriptableObjects/Puzzles/ShapesPuzzleConfig.asset");
            var config = UnityEngine.Object.Instantiate(asset); config.hideFlags = HideFlags.HideAndDontSave;
            var serialized = new SerializedObject(config);
            serialized.FindProperty("_strokeWidth").floatValue = Widths[row];
            serialized.FindProperty("_headSize").floatValue = Heads[row]; serialized.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static PuzzleArrowView Spawn(Scene scene)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/PuzzleArrow.prefab");
            var instance = UnityEngine.Object.Instantiate(prefab); instance.name = "Geometry Fixture";
            SceneManager.MoveGameObjectToScene(instance, scene); return instance.GetComponent<PuzzleArrowView>();
        }

        private static PuzzleArrowData Bent(int turns)
        {
            var points = new List<Vector2Int> { new Vector2Int(3, 2), new Vector2Int(3, 1), new Vector2Int(2, 1),
                new Vector2Int(2, 2), new Vector2Int(1, 2), new Vector2Int(0, 2) };
            for (int i = 0; i < points.Count; i++) points[i] = Rotate(points[i], turns);
            return new PuzzleArrowData(points, Rotate(Vector2Int.up, turns), Ink);
        }

        private static Vector2Int Rotate(Vector2Int value, int turns)
        {
            for (int i = 0; i < turns; i++) value = new Vector2Int(value.y, -value.x);
            return value;
        }

        private static Vector3 Center(PuzzleArrowData data)
        {
            Vector2 minimum = data.Points[0], maximum = minimum;
            foreach (Vector2Int point in data.Points) { minimum = Vector2.Min(minimum, point); maximum = Vector2.Max(maximum, point); }
            return -(Vector3)((minimum + maximum) * .5f);
        }

        private static void OldBody(PuzzleArrowView view, PuzzleArrowData data, PuzzleConfig config, bool highlighted)
        {
            var body = view.transform.Find("Body").GetComponent<LineRenderer>();
            body.numCapVertices = 6; body.numCornerVertices = 6;
            float end = view.Length - config.HeadSize * (highlighted ? 1.8f : 1f) * .5f;
            var positions = new List<Vector3>(); Vector3 origin = Center(data);
            positions.Add(origin + (Vector3)(Vector2)data.Points[data.Points.Count - 1]);
            for (int i = 1; i < data.Points.Count && i < end; i++)
                positions.Add(origin + (Vector3)(Vector2)data.Points[data.Points.Count - 1 - i]);
            int segment = Mathf.FloorToInt(end);
            Vector2 a = data.Points[data.Points.Count - 1 - segment];
            Vector2 b = data.Points[data.Points.Count - 2 - segment];
            positions.Add(origin + (Vector3)Vector2.Lerp(a, b, end - segment));
            body.positionCount = positions.Count; body.SetPositions(positions.ToArray());
        }
    }
}
