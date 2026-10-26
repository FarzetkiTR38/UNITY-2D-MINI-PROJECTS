namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Reflection;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;

    /// <summary>Measures the actual four-layer mesh after custom spacing/arch and detects fitting or styling drift.</summary>
    public static partial class Quality4PauseTitleVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        private static Casual3DTitle FindPauseTitle()
        {
            Casual3DTitle found = null;
            foreach (var title in UnityEngine.Object.FindObjectsByType<Casual3DTitle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (title.LocalizationKey == "pause_title")
                { Check(found == null, "Scene contains one actual pause title"); found = title; }
            Check(found != null, "Actual authored pause title exists"); return found;
        }

        private static CanvasGroup PauseGroup(Casual3DTitle title)
        {
            Transform item = title.transform;
            while (item != null && item.name != "PausePanel") item = item.parent;
            Check(item != null && item.GetComponent<CanvasGroup>() != null, "Pause title belongs to its real CanvasGroup popup");
            return item.GetComponent<CanvasGroup>();
        }

        private static IEnumerator VerifyTitle(Casual3DTitle title, string label, string language)
        {
            var rect = title.transform as RectTransform;
            float padding = Field<float>(title, "_horizontalFitPadding");
            Check(title.TryGetRenderedBounds(out Bounds before), label + ": all four warped layers have visible geometry");
            bool fits = before.min.x >= rect.rect.xMin + padding - .05f && before.max.x <= rect.rect.xMax - padding + .05f;
            Verify(fits, label + ": combined glyph geometry fits the authored title width");
            Verify(InViewport(title, before), label + ": all title geometry fits the actual screen viewport");
            var layers = new[] { Field<TextMeshProUGUI>(title, "_frontText"), Field<TextMeshProUGUI>(title, "_rimText"),
                Field<TextMeshProUGUI>(title, "_extrusionText"), Field<TextMeshProUGUI>(title, "_shadowText") };
            var baseline = new LayerState[layers.Length];
            for (int index = 0; index < layers.Length; index++)
            { Check(layers[index] != null, label + ": authored layer " + index + " exists"); baseline[index] = new LayerState(layers[index]); }
            float scale = layers[0].rectTransform.localScale.x;
            if (language == "en")
            {
                float unscaledWidth = before.size.x / Mathf.Max(.001f, scale);
                bool nativeFits = unscaledWidth <= rect.rect.width - padding * 2f + .05f;
                Verify(!nativeFits || Mathf.Abs(scale - 1f) < .001f,
                    label + ": short original English retains full scale whenever its natural width fits");
                Results.Add("ENGLISH: " + label + "; naturalWidth=" + unscaledWidth + "; scale=" + scale);
            }
            title.ApplyAll(); title.ApplyAll(); yield return Settled(.08f);
            Check(title.TryGetRenderedBounds(out Bounds after), label + ": repeated application retains visible geometry");
            Verify((before.min - after.min).sqrMagnitude < .01f && (before.max - after.max).sqrMagnitude < .01f,
                label + ": repeated ApplyAll and actual rendered frames produce no bounds drift");
            bool stable = true;
            for (int index = 0; index < layers.Length; index++) stable &= baseline[index].Matches(layers[index]);
            Verify(stable, label + ": all four layer fonts, colors, materials, styles, offsets and scales stay unchanged");
            Verify(Mathf.Abs(Field<float>(title, "_fontSize") - 155f) < .001f && Field<bool>(title, "_enableCurve"),
                label + ": authored font155 and arch remain enabled");
            Results.Add("BOUNDS: " + label + "; min=" + before.min + "; max=" + before.max +
                "; authoredWidth=" + rect.rect.width + "; layerScale=" + scale + "; frame=" + Time.frameCount);
        }

        private static bool InViewport(Casual3DTitle title, Bounds bounds)
        {
            var canvas = title.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            foreach (Vector2 corner in new[] { new Vector2(bounds.min.x, bounds.min.y), new Vector2(bounds.min.x, bounds.max.y),
                new Vector2(bounds.max.x, bounds.min.y), new Vector2(bounds.max.x, bounds.max.y) })
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, title.transform.TransformPoint(corner));
                if (screen.x < -1f || screen.x > Screen.width + 1f || screen.y < -1f || screen.y > Screen.height + 1f) return false;
            }
            return true;
        }

        /// <summary>Records stable visual and layer-transform properties without changing any designer-owned styling.</summary>
        private sealed class LayerState
        {
            private readonly TMP_FontAsset _font;
            private readonly Material _material;
            private readonly Color _color;
            private readonly FontStyles _style;
            private readonly float _size;
            private readonly Vector3 _position, _scale;
            internal LayerState(TextMeshProUGUI text)
            { _font = text.font; _material = text.fontSharedMaterial; _color = text.color; _style = text.fontStyle;
                _size = text.fontSize; _position = text.rectTransform.localPosition; _scale = text.rectTransform.localScale; }
            internal bool Matches(TextMeshProUGUI text) => text.font == _font && text.fontSharedMaterial == _material &&
                text.color == _color && text.fontStyle == _style && Mathf.Approximately(text.fontSize, _size) &&
                (text.rectTransform.localPosition - _position).sqrMagnitude < .001f &&
                (text.rectTransform.localScale - _scale).sqrMagnitude < .00001f;
        }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
    }
}
