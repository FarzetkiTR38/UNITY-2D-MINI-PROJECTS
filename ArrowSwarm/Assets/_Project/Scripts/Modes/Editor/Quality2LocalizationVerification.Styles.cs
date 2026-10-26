namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Reflection;
    using TMPro;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Distinguishes immutable text authoring from legal rendered font-size changes made by TMP autosizing.</summary>
    public static partial class Quality2LocalizationVerification
    {
        private static readonly FieldInfo BaseSizeField = typeof(TMP_Text).GetField("m_fontSizeBase", BindingFlags.Instance | BindingFlags.NonPublic);

        private static float AuthoredBase(TMP_Text text) => new SerializedObject(text).FindProperty("m_fontSizeBase").floatValue;

        /// <summary>Captures authored styling and size policy while retaining exact rendered-size checks for fixed-size labels.</summary>
        private readonly struct TextStyle
        {
            private readonly TMP_FontAsset _font;
            private readonly Material _material;
            private readonly Color _color;
            private readonly float _size, _base, _min, _max;
            private readonly bool _auto;
            private readonly Vector2 _bounds;

            /// <summary>Snapshots the serialized base and public autosize policy alongside existing font, material, color and rectangle checks.</summary>
            public TextStyle(TMP_Text text)
            {
                _font = text.font; _material = text.fontSharedMaterial; _color = text.color; _size = text.fontSize;
                _base = AuthoredBase(text); _auto = text.enableAutoSizing; _min = text.fontSizeMin; _max = text.fontSizeMax;
                _bounds = text.rectTransform.sizeDelta;
            }

            /// <summary>Accepts only unchanged authoring and a rendered size allowed by that original policy.</summary>
            public bool Matches(TMP_Text text) => text.font == _font && text.fontSharedMaterial == _material &&
                text.color == _color && text.rectTransform.sizeDelta == _bounds &&
                Mathf.Approximately(AuthoredBase(text), _base) && text.enableAutoSizing == _auto &&
                Mathf.Approximately(text.fontSizeMin, _min) && Mathf.Approximately(text.fontSizeMax, _max) &&
                (_auto ? text.fontSize >= _min - .01f && text.fontSize <= _max + .01f : Mathf.Approximately(text.fontSize, _size));
        }

        private static void CheckTextStyleNegativeControls(TMP_Text text)
        {
            Check(text.enableAutoSizing && BaseSizeField != null, "Style controls use the actual authored autosized level-range label");
            var baseline = new TextStyle(text);
            float size = text.fontSize, authoredBase = AuthoredBase(text), min = text.fontSizeMin, max = text.fontSizeMax;
            Color color = text.color; Vector2 bounds = text.rectTransform.sizeDelta;
            void Reject(Action change, Action restore, string reason)
            {
                try { change(); Check(!baseline.Matches(text), "Negative style control rejects " + reason); }
                finally { restore(); }
            }
            try
            {
                // This serialized field is changed directly so the temporary negative control cannot trigger Inspector OnValidate side effects.
                Reject(() => BaseSizeField.SetValue(text, authoredBase + 1f), () => BaseSizeField.SetValue(text, authoredBase), "changed serialized base size");
                Reject(() => text.fontSizeMin = min + 1f, () => text.fontSizeMin = min, "changed autosize minimum");
                Reject(() => text.fontSizeMax = max + 1f, () => text.fontSizeMax = max, "changed autosize maximum");
                Reject(() => text.enableAutoSizing = false, () => text.enableAutoSizing = true, "disabled authored autosizing");
                Reject(() => text.color = new Color(color.r, color.g, color.b, color.a * .5f), () => text.color = color, "changed authored color");
                Reject(() => text.rectTransform.sizeDelta = bounds + Vector2.right, () => text.rectTransform.sizeDelta = bounds, "changed authored rectangle");
                Reject(() => text.fontSize = max + 1f, () => text.fontSize = size, "rendered size outside the original autosize range");
                text.fontSize = (min + max) * .5f;
                Check(baseline.Matches(text), "Positive style control accepts a different rendered size only within the unchanged authored autosize policy");
                text.fontSize = size; text.enableAutoSizing = false;
                var fixedStyle = new TextStyle(text);
                text.fontSize = size + 1f; BaseSizeField.SetValue(text, authoredBase);
                Check(!fixedStyle.Matches(text), "Negative fixed-size control rejects a changed rendered size even when the serialized base is unchanged");
            }
            finally
            {
                text.enableAutoSizing = true; text.fontSizeMin = min; text.fontSizeMax = max; text.fontSize = size;
                BaseSizeField.SetValue(text, authoredBase); text.color = color; text.rectTransform.sizeDelta = bounds;
                text.ForceMeshUpdate(true, true);
            }
            Check(baseline.Matches(text), "Negative style controls restore actual font, material, color, rectangle, serialized base and sizing policy");
        }
    }
}
