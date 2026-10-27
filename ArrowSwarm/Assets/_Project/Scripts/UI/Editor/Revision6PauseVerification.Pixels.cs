namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Measures the real Pause sprite through both real shader passes and a deliberately broken control.</summary>
    public static partial class Revision6PauseVerification
    {
        private static void VerifyPixels()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Canvas_HUD.prefab");
            Image image = null;
            foreach (var candidate in prefab.GetComponentsInChildren<Image>(true))
                if (candidate.name == "PauseButton") { image = candidate; break; }
            Check(image != null && image.sprite != null, "Original authored Pause sprite exists");
            var source = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Theme/DarkPause.mat");
            var light = new Material(image.material) { hideFlags = HideFlags.HideAndDontSave };
            var dark = new Material(source) { hideFlags = HideFlags.HideAndDontSave };
            var broken = new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/DarkUI.shader"))
                { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var sprite = image.sprite; var rect = sprite.textureRect;
                var uv = new Vector4(rect.xMin / sprite.texture.width, rect.yMin / sprite.texture.height,
                    rect.xMax / sprite.texture.width, rect.yMax / sprite.texture.height);
                using (var surface = new ShaderVerificationSurface())
                {
                    foreach (var tint in new[] { Color.white, new Color(.78431374f, .78431374f, .78431374f, 1), new Color(1, 1, 1, .5f) })
                    {
                        var reference = surface.Render(light, sprite.texture, uv, tint, 320, 375);
                        var candidate = surface.Render(dark, sprite.texture, uv, tint, 320, 375);
                        double difference = Difference(reference, candidate); float alpha = reference.MeanAlphaDifference(candidate);
                        Results.Add("GPU: tint=" + tint + "; meanRGBAByteDifference=" + difference + "; meanAlphaDifference=" + alpha);
                        Check(difference <= 1.1 && alpha <= 1f / 255f,
                            "Dedicated Dark material preserves actual Light sprite RGB/alpha for normal, pressed and fade tint");
                    }
                    broken.SetFloat("_SurfaceStrength", 1); broken.SetFloat("_BevelStrength", 1);
                    broken.SetFloat("_OpaqueInterior", 1); broken.SetFloat("_AccentStrength", .96f); broken.SetFloat("_Artwork", 5);
                    broken.SetColor("_SurfaceColor", new Color(.1254902f, .19607843f, .30980393f, 1));
                    broken.SetVector("_SurfaceInset", new Vector4(.06f, .06f, 0, 0));
                    broken.SetVector("_Corner", new Vector4(.12f, .12f, 0, 0));
                    var good = surface.Render(light, sprite.texture, uv, Color.white, 320, 375);
                    var defect = surface.Render(broken, sprite.texture, uv, Color.white, 320, 375);
                    double failedDifference = Difference(good, defect);
                    Results.Add("NEGATIVE CONTROL: original dark-interior recipe meanRGBAByteDifference=" + failedDifference);
                    Check(failedDifference > 3, "Pixel checker rejects the original navy-interior regression");
                }
                Check(source.shader == UnityEngine.UI.Graphic.defaultGraphicMaterial.shader && source.color == Color.white,
                    "Only the dedicated Pause material uses the exact original UI shader and tint");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(light); UnityEngine.Object.DestroyImmediate(dark);
                UnityEngine.Object.DestroyImmediate(broken);
            }
        }

        private static double Difference(ShaderPixelFrame a, ShaderPixelFrame b)
        {
            if (a.Pixels.Length != b.Pixels.Length) throw new InvalidOperationException("Frame dimensions differ.");
            double total = 0;
            for (int i = 0; i < a.Pixels.Length; i++)
            {
                var x = a.Pixels[i]; var y = b.Pixels[i];
                total += Math.Abs(x.r - y.r) + Math.Abs(x.g - y.g) + Math.Abs(x.b - y.b) + Math.Abs(x.a - y.a);
            }
            return total / (a.Pixels.Length * 4d);
        }
    }
}
