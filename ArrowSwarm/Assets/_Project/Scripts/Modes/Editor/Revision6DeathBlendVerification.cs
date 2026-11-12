namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.UI.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>Executes the death-ring shader with correct straight-alpha and broken premultiply controls.</summary>
    public static class Revision6DeathBlendVerification
    {
        private const string Path = "Assets/_Project/Art/Materials/Mat_RingGlow.mat";
        private const string Keyword = "_ALPHAPREMULTIPLY_ON";

        /// <summary>Writes native GPU evidence; before records the known defect rather than mutating the source material.</summary>
        public static string Run(string label = "before")
        {
            label = label == "before" ? "before" : "after";
            string folder = "Captures/Revision6/Death/" + label;
            Directory.CreateDirectory(folder);
            var lines = new List<string>(); int failures = 0;
            var source = AssetDatabase.LoadAssetAtPath<Material>(Path);
            if (source == null) throw new InvalidOperationException("Authored ring material missing.");
            string original = File.ReadAllText(Path);
            var actual = new Material(source) { hideFlags = HideFlags.HideAndDontSave };
            var fixedControl = new Material(source) { hideFlags = HideFlags.HideAndDontSave };
            var brokenControl = new Material(source) { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                // The controlled quad has no camera winding state; disable only clone culling to test blending.
                actual.SetFloat("_Cull", 0); fixedControl.SetFloat("_Cull", 0); brokenControl.SetFloat("_Cull", 0);
                fixedControl.SetFloat("_SrcBlend", 5); fixedControl.DisableKeyword(Keyword);
                brokenControl.SetFloat("_SrcBlend", 1); brokenControl.DisableKeyword(Keyword);
                Color dark = new Color(.055f, .078f, .13f, 1), light = new Color(.96f, .94f, .9f, 1);
                using (var surface = new ParticleSurface())
                {
                    var darkActual = surface.Render(actual, dark, folder + "/ring-actual-dark.png");
                    surface.Render(actual, light, folder + "/ring-actual-light.png");
                    var corrected = surface.Render(fixedControl, dark, folder + "/ring-positive-premultiply.png");
                    var defective = surface.Render(brokenControl, dark, folder + "/ring-negative-unpremultiplied.png");
                    float actualError = OutsideError(darkActual, dark);
                    float positiveError = OutsideError(corrected, dark), negativeError = OutsideError(defective, dark);
                    Record(positiveError < .012f, "Straight-alpha positive control preserves alpha-zero corners and ring hole", lines, ref failures);
                    Record(negativeError > .08f, "Unpremultiplied negative control reproduces colored transparent quad", lines, ref failures);
                    int visible = 0;
                    foreach (var pixel in corrected.Pixels) if (RgbError(pixel, dark) > .05f) visible++;
                    Record(visible > 50, "Positive control still draws the real visible ring; blank shader output cannot pass", lines, ref failures);
                    Record(actualError < .012f && source.GetFloat("_Blend") == 0 && source.GetFloat("_SrcBlend") == 5 &&
                        source.GetFloat("_DstBlend") == 10 && !source.IsKeywordEnabled(Keyword),
                        "Authored straight-alpha blend matches its texture and survives URP material validation", lines, ref failures);
                    lines.Add("MEASURED: actualOutsideError=" + actualError.ToString("F6") + "; positive=" +
                        positiveError.ToString("F6") + "; negative=" + negativeError.ToString("F6") + "; visibleRingPixels=" + visible);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(actual); UnityEngine.Object.DestroyImmediate(fixedControl);
                UnityEngine.Object.DestroyImmediate(brokenControl);
                Record(File.ReadAllText(Path) == original, "Source material bytes unchanged by GPU verification", lines, ref failures);
            }
            string result = (failures == 0 ? "PASS" : "FAIL") + ": death-ring GPU transparency; failures=" + failures;
            File.WriteAllText(folder + "/blend-verification.txt", result + "\n" + string.Join("\n", lines) + "\n");
            return result;
        }

        private static float OutsideError(ShaderPixelFrame frame, Color background)
        {
            float error = 0;
            foreach (var uv in new[] { new Vector2(.02f,.02f), new Vector2(.98f,.02f),
                new Vector2(.02f,.98f), new Vector2(.98f,.98f), new Vector2(.5f,.5f) })
                error = Mathf.Max(error, RgbError(frame.At(uv.x, uv.y), background));
            return error;
        }

        private static float RgbError(Color a, Color b) => Mathf.Max(Mathf.Abs(a.r-b.r), Mathf.Abs(a.g-b.g), Mathf.Abs(a.b-b.b));
        private static void Record(bool value, string text, List<string> lines, ref int failures)
        { lines.Add((value ? "PASS: " : "FAIL: ") + text); if (!value) failures++; }

        /// <summary>Owns only a temporary colored mesh; the source particle material is never edited.</summary>
        private sealed class ParticleSurface : IDisposable
        {
            private readonly Mesh _mesh;
            internal ParticleSurface()
            {
                _mesh = new Mesh { name = "Death ring GPU verification", hideFlags = HideFlags.HideAndDontSave };
                _mesh.vertices = new[] { new Vector3(-1,-1,0), new Vector3(-1,1,0), new Vector3(1,1,0), new Vector3(1,-1,0) };
                _mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
                _mesh.uv2 = _mesh.uv;
                _mesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };
                _mesh.triangles = new[] { 0,1,2,0,2,3 };
            }

            internal ShaderPixelFrame Render(Material material, Color background, string path)
            {
                const int size = 128;
                var target = RenderTexture.GetTemporary(size, size, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true) { hideFlags = HideFlags.HideAndDontSave };
                var previous = RenderTexture.active; bool srgb = GL.sRGBWrite;
                var commands = new CommandBuffer { name = "Death ring particle transparency" };
                var projection = GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-1,1,-1,1,.1f,10), true);
                try
                {
                    GL.sRGBWrite = false; commands.SetRenderTarget(target);
                    commands.ClearRenderTarget(true, true, background); commands.SetViewport(new Rect(0,0,size,size));
                    commands.SetViewProjectionMatrices(Matrix4x4.Translate(new Vector3(0,0,-1)), projection);
                    commands.DrawMesh(_mesh, Matrix4x4.identity, material, 0, 0); Graphics.ExecuteCommandBuffer(commands);
                    RenderTexture.active = target;
                    texture.ReadPixels(new Rect(0,0,size,size), 0,0,false); texture.Apply(false,false);
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                    return new ShaderPixelFrame(size, size, texture.GetPixels32(), projection.m11 < 0);
                }
                finally
                {
                    commands.Release(); GL.sRGBWrite = srgb; RenderTexture.active = previous;
                    UnityEngine.Object.DestroyImmediate(texture); RenderTexture.ReleaseTemporary(target);
                }
            }

            /// <summary>Releases the test-owned mesh.</summary>
            public void Dispose() => UnityEngine.Object.DestroyImmediate(_mesh);
        }
    }
}
