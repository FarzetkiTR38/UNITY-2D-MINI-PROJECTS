namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.IO;
    using ArrowSwarm.Data;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks actual Slider geometry and rendered pixels, including its previously broken low endpoints.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator VerifyProgress(BootLoadingUI boot, ThemeMode theme)
        {
            var slider = Field<Slider>(boot, "_progressSlider");
            var fill = Field<Image>(boot, "_progressFillImage");
            var area = fill.rectTransform.parent as RectTransform;
            Check(area != null && area.name == "FillArea" && fill.type == Image.Type.Simple,
                "Loading fill has a dedicated inset area and no stretched sprite caps / " + theme);
            Check(fill.material.shader.name == "ArrowSwarm/UI/SharedLoadingBar", "Shared rounded loading shader is bound / " + theme);
            float previous = -1, left = float.NaN, height = area.rect.height;
            var percent = Field<TMPro.TextMeshProUGUI>(boot, "_percentText");
            var labelRect = percent.rectTransform;
            Vector2 labelPosition = labelRect.anchoredPosition;
            foreach (float value in new[] { 0f, .01f, .05f, .25f, .5f, .65f, .99f, 1f })
            {
                boot.SetProgress(value); Canvas.ForceUpdateCanvases(); yield return Settled(.04f);
                var rect = fill.rectTransform;
                Check(rect.rect.width >= 0 && Mathf.Abs(rect.rect.width - area.rect.width * value) < .1f,
                    "Exact nonnegative inner fill width at " + value + " / " + theme);
                Check(rect.rect.width >= previous && Mathf.Abs(rect.rect.height - height) < .01f,
                    "Fill width is monotonic and height stays fixed at " + value + " / " + theme);
                Check(rect.offsetMin.sqrMagnitude < .001f && rect.offsetMax.sqrMagnitude < .001f,
                    "Slider never scales padding or the end caps at " + value + " / " + theme);
                var corners = new Vector3[4]; rect.GetWorldCorners(corners);
                if (float.IsNaN(left)) left = corners[0].x;
                Check(Mathf.Abs(corners[0].x - left) < .1f, "Fill left edge stays fixed at " + value + " / " + theme);
                previous = rect.rect.width;
                var trackRect = slider.GetComponent<RectTransform>().rect;
                Check(labelRect.anchorMin.x == 1 && labelRect.anchorMax.x == 1 && labelRect.pivot.x == 1 &&
                    labelRect.anchoredPosition == labelPosition && labelPosition.x < 0 &&
                    labelRect.rect.width < trackRect.width * .25f && percent.alignment == TMPro.TextAlignmentOptions.MidlineRight,
                    "Percentage stays right-aligned inside the track at " + value + " / " + theme);
                yield return CaptureProgress(slider, area, value, theme);
            }
            boot.SetProgress(.65f); yield return Settled(.04f);
            yield return VerifyProgressFlow(slider, area, theme);
        }

        private static IEnumerator VerifyProgressFlow(Slider slider, RectTransform area, ThemeMode theme)
        {
            // Static composition captures pause game time. Let the shader's real Boot clock advance for this check.
            float previousScale = Time.timeScale;
            var before = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var after = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Time.timeScale = 1;
                yield return CaptureProgress(slider, area, .65f, theme);
                string imagePath = Path.Combine(_folder, "progress-" + theme.ToString().ToLowerInvariant() + "-065.png");
                byte[] first = File.ReadAllBytes(imagePath);
                yield return Settled(.25f);
                yield return CaptureProgress(slider, area, .65f, theme);
                ImageConversion.LoadImage(before, first); ImageConversion.LoadImage(after, File.ReadAllBytes(imagePath));
                var a = before.GetPixels32(); var b = after.GetPixels32(); int changed = 0;
                for (int i = 0; i < a.Length; i++) if (Mathf.Abs(a[i].g - b[i].g) > 5) changed++;
                Check(a.Length == b.Length && changed > a.Length * .03f && Mathf.Approximately(slider.value, .65f),
                    "Actual chevrons flow while progress geometry and percentage stay fixed / " + theme);
            }
            finally
            {
                Time.timeScale = previousScale;
                UnityEngine.Object.DestroyImmediate(before); UnityEngine.Object.DestroyImmediate(after);
            }
        }

        private static IEnumerator CaptureProgress(Slider slider, RectTransform area, float value, ThemeMode theme)
        {
            string name = "progress-" + theme.ToString().ToLowerInvariant() + "-" + Mathf.RoundToInt(value * 100).ToString("000");
            string raw = Path.Combine(_folder, name + ".raw.png"), output = Path.Combine(_folder, name + ".png");
            if (File.Exists(raw)) File.Delete(raw);
            int frame = Time.frameCount; ScreenCapture.CaptureScreenshot(raw);
            yield return (Func<bool>)(() => Time.frameCount > frame + 2 && File.Exists(raw) && new FileInfo(raw).Length > 0);
            Texture2D source = null, crop = null;
            try
            {
                source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                Check(ImageConversion.LoadImage(source, File.ReadAllBytes(raw)), "Actual native progress screenshot decodes / " + name);
                var corners = new Vector3[4]; slider.GetComponent<RectTransform>().GetWorldCorners(corners);
                Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
                Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
                int x = Mathf.Max(0, Mathf.FloorToInt(min.x) - 6), y = Mathf.Max(0, Mathf.FloorToInt(min.y) - 6);
                int width = Mathf.Min(source.width - x, Mathf.CeilToInt(max.x) + 6 - x);
                int rows = Mathf.Min(source.height - y, Mathf.CeilToInt(max.y) + 6 - y);
                area.GetWorldCorners(corners);
                var start = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
                var end = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
                int row = Mathf.RoundToInt((start.y + end.y) * .5f);
                float inset = value == 0 ? 35 : Mathf.Min((end.x - start.x) * value * .5f, 35);
                var sample = source.GetPixel(Mathf.RoundToInt(start.x + inset), row);
                Check(value == 0 ? sample.g < .3f && sample.b < .65f : sample.g > .3f && sample.b > .5f,
                    (value == 0 ? "Zero renders the empty dark track" : "Nonzero fill renders real blue pixels") + " / " + name);
                if (Mathf.Approximately(value, .05f))
                {
                    int capX = Mathf.RoundToInt(start.x + (end.x - start.x) * value - 3);
                    var corner = source.GetPixel(capX, Mathf.RoundToInt(start.y + 3));
                    var middle = source.GetPixel(capX, row);
                    Check(corner.g < .3f && corner.b < .65f && middle.g > .3f && middle.b > .5f,
                        "Rendered low-progress end cap is rounded, not a stretched rectangle / " + name);
                }
                if (value <= .65f)
                {
                    // Beyond the 65% fill, before the dedicated right-hand TMP percentage box.
                    var empty = source.GetPixel(Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, .70f)), row);
                    Check(empty.g < .3f && empty.b < .65f, "Unfilled part remains dark / " + name);
                }
                crop = new Texture2D(width, rows, TextureFormat.RGBA32, false);
                crop.SetPixels(source.GetPixels(x, y, width, rows)); crop.Apply();
                File.WriteAllBytes(output, ImageConversion.EncodeToPNG(crop));
                Results.Add("CAPTURE: " + output + "; native progress region=" + width + "x" + rows);
                File.Delete(raw);
            }
            finally
            {
                if (source != null) UnityEngine.Object.DestroyImmediate(source);
                if (crop != null) UnityEngine.Object.DestroyImmediate(crop);
            }
        }
    }
}
