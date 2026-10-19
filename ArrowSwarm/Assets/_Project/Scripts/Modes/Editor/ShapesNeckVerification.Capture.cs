namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Native Unity before/after renders of a head with exactly one unit before its first bend.</summary>
    public static partial class ShapesNeckVerification
    {
        /// <summary>Writes eight direction/hint fixtures for each anchor without modifying a production asset.</summary>
        public static string Capture()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Capture requires Edit Mode.");
            const int tile = 320; Directory.CreateDirectory(Folder);
            Scene scene = EditorSceneManager.NewPreviewScene();
            RenderTexture previous = RenderTexture.active;
            var target = new RenderTexture(tile, tile, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            var comparison = new Texture2D(tile * 8, tile * 2, TextureFormat.RGBA32, false);
            UnityEngine.Camera camera = null;
            try
            {
                var cameraObject = new GameObject("Natural Neck Camera", typeof(UnityEngine.Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                camera = cameraObject.GetComponent<UnityEngine.Camera>(); camera.scene = scene;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.orthographic = true; camera.orthographicSize = 2.65f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.99f, .98f, .96f);
                camera.nearClipPlane = .01f; camera.farClipPlane = 30f;
                camera.targetTexture = target; camera.allowHDR = false; camera.allowMSAA = true;
                for (int row = 0; row < 2; row++)
                {
                    var config = CloneConfig(row == 1);
                    var strip = new Texture2D(tile * 8, tile, TextureFormat.RGBA32, false);
                    var view = Spawn(scene);
                    try
                    {
                        for (int direction = 0; direction < 4; direction++)
                            for (int hint = 0; hint < 2; hint++)
                            {
                                var data = Bent(direction);
                                view.Bind(data, 0, Center(data), config); view.Highlight(hint == 1);
                                camera.Render(); RenderTexture.active = target;
                                var image = new Texture2D(tile, tile, TextureFormat.RGBA32, false);
                                try
                                {
                                    image.ReadPixels(new Rect(0, 0, tile, tile), 0, 0); image.Apply();
                                    int x = (direction * 2 + hint) * tile;
                                    var pixels = image.GetPixels(); strip.SetPixels(x, 0, tile, tile, pixels);
                                    comparison.SetPixels(x, (1 - row) * tile, tile, tile, pixels);
                                }
                                finally { UnityEngine.Object.DestroyImmediate(image); }
                            }
                        strip.Apply();
                        File.WriteAllBytes(Folder + (row == 0 ? "/before.png" : "/after.png"), strip.EncodeToPNG());
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(view.gameObject);
                        UnityEngine.Object.DestroyImmediate(config); UnityEngine.Object.DestroyImmediate(strip);
                    }
                }
                comparison.Apply(); string path = Folder + "/comparison.png";
                File.WriteAllBytes(path, comparison.EncodeToPNG());
                File.WriteAllLines(Folder + "/capture-verification.txt", new[]
                {
                    "PASS: real Unity camera renders of production PuzzleArrow.prefab in an isolated preview scene.",
                    "Before/upper row: cloned config with tip anchored at logical head (previous Shapes presentation).",
                    "After/lower row: cloned config with sprite center on logical head (Defense-style presentation).",
                    "Columns: up normal/hint, right normal/hint, down normal/hint, left normal/hint.",
                    "All fixtures use the same unchanged unit-neck path, stroke width, head size and color.",
                    "No production config, level, prefab, scene or player save is modified by capture."
                });
                return System.IO.Path.GetFullPath(path);
            }
            finally
            {
                RenderTexture.active = previous;
                if (camera != null) camera.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(comparison); target.Release(); UnityEngine.Object.DestroyImmediate(target);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
