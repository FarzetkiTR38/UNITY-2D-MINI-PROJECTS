namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>Executes off-scene GPU regressions for rounded button artwork and the Swarm iris wipe.</summary>
    public static partial class MenuShaderVerification
    {
        private static int _checks, _failed;
        private static StringBuilder _report;
        private static readonly Vector4 FullUv = new Vector4(0,0,1,1);

        /// <summary>Writes and returns shader test counters; throws after reporting any failed check.</summary>
        public static string Run(string reportFolder = "Captures/MenuPolish")
        {
            _checks = _failed = 0; _report = new StringBuilder("Actual GPU UI shader verification\n");
            _report.AppendLine("RGBA clear-target alpha follows srcAlpha squared under Blend SrcAlpha OneMinusSrcAlpha.");
            try
            {
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                    throw new InvalidOperationException("A real graphics device is required; headless output cannot verify shaders.");
                if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32) || !SystemInfo.SupportsTextureFormat(TextureFormat.RGBA32))
                    throw new InvalidOperationException("The device cannot provide the required RGBA render and readback formats.");
                using (var surface = new ShaderVerificationSurface()) { VerifyButtons(surface); VerifyIris(surface); }
            }
            catch (Exception error)
            { _failed++; _report.AppendLine("FAIL: execution exception: " + error); }
            string outcome = (_failed == 0 ? "PASS" : "FAIL") + ": " + _checks + " checks, " + _failed + " failures";
            _report.Insert(0, outcome + "\n"); Directory.CreateDirectory(reportFolder);
            File.WriteAllText(Path.Combine(reportFolder,"shader-verification.txt"), _report.ToString());
            if (_failed > 0) throw new InvalidOperationException("Menu shader verification " + outcome);
            Debug.Log("[ArrowSwarm] Menu shader verification " + outcome); return _report.ToString();
        }

        private static void Check(bool condition, string message)
        {
            _checks++; if (!condition) _failed++;
            _report.AppendLine((condition ? "PASS: " : "FAIL: ") + message);
        }

        private static Shader RequireShader(string name)
        {
            var shader = Shader.Find(name);
            if (shader == null || !shader.isSupported) throw new InvalidOperationException("Missing or unsupported shader: " + name);
            return shader;
        }

        private static float Luminance(Color color) => color.r * .2126f + color.g * .7152f + color.b * .0722f;
        private static bool Near(float actual, float expected) => Mathf.Abs(actual - expected) <= .015f;
        private static bool CornersClear(ShaderPixelFrame frame) => frame.At(.01f,.01f).a < .01f &&
            frame.At(.01f,.99f).a < .01f && frame.At(.99f,.01f).a < .01f && frame.At(.99f,.99f).a < .01f;
    }
}
