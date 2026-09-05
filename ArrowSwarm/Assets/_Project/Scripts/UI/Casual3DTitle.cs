namespace ArrowSwarm.UI
{
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Renders dynamic 3D titles with equal letter gap spacing, arched curving, and multi-tier extrusion.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Casual3DTitle : MonoBehaviour
    {
        [Header("Text Content")]
        [TextArea(1, 3)]
        [SerializeField] private string _text = "SETTINGS";
        [SerializeField] private string _localizationKey = "";
        [Header("Layers & Offsets")]
        [SerializeField] private TextMeshProUGUI _frontText;
        [SerializeField] private TextMeshProUGUI _rimText;
        [SerializeField] private TextMeshProUGUI _extrusionText;
        [SerializeField] private TextMeshProUGUI _shadowText;
        [SerializeField] private Vector2 _rimOffset = new Vector2(0f, -4f);
        [SerializeField] private Vector2 _extrusionOffset = new Vector2(0f, -12f);
        [SerializeField] private Vector2 _shadowOffset = new Vector2(0f, -20f);

        [Header("Typography & Curving")]
        [SerializeField] private float _fontSize = 86f;
        [SerializeField] private FontStyles _fontStyle = FontStyles.Bold;
        [SerializeField] private float _letterGap = 2f;
        [SerializeField] private float _wordGap = 22f;
        [SerializeField] private float _verticalStretch = 1.08f;
        [SerializeField] private bool _enableCurve = true;
        [SerializeField] private float _curveScale = 16f;
        [Range(0f, 1f)] [SerializeField] private float _rotationMultiplier = 0.55f;
        [SerializeField] private AnimationCurve _vertexCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2.2f), new Keyframe(0.5f, 1f, 0f, 0f), new Keyframe(1f, 0f, -2.2f, 0f));

        private string _lastRenderedText;
        private bool _isDirty;

        /// <summary>Gets or sets the text and updates all 3D layers.</summary>
        public string Text { get => _text; set { if (_text != value) { _text = value; MarkDirty(); } } }

        /// <summary>Gets or sets the localization key for automatic translation.</summary>
        public string LocalizationKey { get => _localizationKey; set { _localizationKey = value; RefreshLocalized(); } }

        /// <summary>Gets or sets font size across all 3D layers.</summary>
        public float FontSize { get => _fontSize; set { if (Mathf.Abs(_fontSize - value) > 0.01f) { _fontSize = value; MarkDirty(); } } }

        private void OnEnable() { LocalizationManager.OnLanguageChanged += RefreshLocalized; RefreshLocalized(); MarkDirty(); }
        private void OnDisable() => LocalizationManager.OnLanguageChanged -= RefreshLocalized;
        private void Start() { RefreshLocalized(); ApplyAll(); }
        private void OnValidate() => MarkDirty();
        private void LateUpdate() { if (_isDirty || _text != _lastRenderedText) ApplyAll(); }

        /// <summary>Flags this title as needing a geometry and text refresh.</summary>
        public void MarkDirty() => _isDirty = true;

        /// <summary>Translates text using active localization manager if a key is assigned.</summary>
        public void RefreshLocalized()
        {
            if (!string.IsNullOrEmpty(_localizationKey) && LocalizationManager.HasInstance)
                Text = LocalizationManager.Instance.GetText(_localizationKey, _text);
            else MarkDirty();
        }

        /// <summary>Applies text, offsets, equal gap spacing, and vertex arch curve across all layers.</summary>
        public void ApplyAll()
        {
            _isDirty = false;
            _lastRenderedText = _text;

            UpdateLayer(_frontText, Vector2.zero);
            UpdateLayer(_rimText, _rimOffset);
            UpdateLayer(_extrusionText, _extrusionOffset);
            UpdateLayer(_shadowText, _shadowOffset);

            if (_frontText == null || string.IsNullOrEmpty(_text)) return;
            _frontText.ForceMeshUpdate(true, true);
            var ti = _frontText.textInfo;
            int count = ti.characterCount;
            if (count <= 0) return;

            float[] shifts = new float[count];
            float currentRight = 0f, layoutMin = 0f, layoutMax = 0f;
            bool first = true;

            for (int i = 0; i < count; i++)
            {
                if (!ti.characterInfo[i].isVisible) { currentRight += _wordGap; continue; }
                int vIdx = ti.characterInfo[i].vertexIndex;
                int mIdx = ti.characterInfo[i].materialReferenceIndex;
                var v = ti.meshInfo[mIdx].vertices;
                float origMin = Mathf.Min(v[vIdx].x, v[vIdx + 1].x, v[vIdx + 2].x, v[vIdx + 3].x);
                float origMax = Mathf.Max(v[vIdx].x, v[vIdx + 1].x, v[vIdx + 2].x, v[vIdx + 3].x);
                float w = origMax - origMin;

                float newLeft;
                if (first) { newLeft = 0f; first = false; layoutMin = 0f; }
                else
                {
                    float g = _letterGap + GetOpticalNudge(ti.characterInfo[i - 1].character, ti.characterInfo[i].character);
                    newLeft = currentRight + g;
                }
                float newRight = newLeft + w;
                shifts[i] = (newLeft + newRight) * 0.5f - (origMin + origMax) * 0.5f;
                currentRight = newRight;
                layoutMax = newRight;
            }

            float centerShift = -(layoutMin + layoutMax) * 0.5f;
            WarpLayer(_frontText, shifts, centerShift);
            WarpLayer(_rimText, shifts, centerShift);
            WarpLayer(_extrusionText, shifts, centerShift);
            WarpLayer(_shadowText, shifts, centerShift);
        }

        private void UpdateLayer(TextMeshProUGUI tmp, Vector2 offset)
        {
            if (tmp == null) return;
            tmp.text = _text;
            tmp.fontSize = _fontSize;
            tmp.fontStyle = _fontStyle;
            tmp.characterSpacing = 0f;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.anchoredPosition = offset;
            tmp.rectTransform.localScale = new Vector3(1f, _verticalStretch, 1f);
        }

        private void WarpLayer(TextMeshProUGUI tmp, float[] shifts, float centerShift)
        {
            if (tmp == null || string.IsNullOrEmpty(_text)) return;
            tmp.ForceMeshUpdate(true, true);
            TMP_TextInfo textInfo = tmp.textInfo;
            int count = textInfo.characterCount;
            if (count <= 0) return;

            float minX = float.MaxValue, maxX = float.MinValue;
            for (int i = 0; i < count; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;
                int vIdx = textInfo.characterInfo[i].vertexIndex;
                int mIdx = textInfo.characterInfo[i].materialReferenceIndex;
                var v = textInfo.meshInfo[mIdx].vertices;
                float s = shifts[i] + centerShift;
                v[vIdx].x += s; v[vIdx + 1].x += s; v[vIdx + 2].x += s; v[vIdx + 3].x += s;
                minX = Mathf.Min(minX, v[vIdx].x, v[vIdx + 1].x, v[vIdx + 2].x, v[vIdx + 3].x);
                maxX = Mathf.Max(maxX, v[vIdx].x, v[vIdx + 1].x, v[vIdx + 2].x, v[vIdx + 3].x);
            }

            if (!_enableCurve) { tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices); return; }
            float width = maxX - minX;
            if (width <= 0.001f) { tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices); return; }

            float effCurve = count <= 3 ? _curveScale * 0.35f : _curveScale;
            for (int i = 0; i < count; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;
                int vIdx = textInfo.characterInfo[i].vertexIndex;
                int mIdx = textInfo.characterInfo[i].materialReferenceIndex;
                Vector3[] v = textInfo.meshInfo[mIdx].vertices;
                Vector3 mid = new Vector2((v[vIdx].x + v[vIdx + 2].x) * 0.5f, textInfo.characterInfo[i].baseLine);
                v[vIdx] -= mid; v[vIdx + 1] -= mid; v[vIdx + 2] -= mid; v[vIdx + 3] -= mid;

                float x0 = Mathf.Clamp01((mid.x - minX) / width);
                float x1 = Mathf.Clamp01(x0 + 0.001f);
                float y0 = _vertexCurve.Evaluate(x0) * effCurve;
                float y1 = _vertexCurve.Evaluate(x1) * effCurve;

                Vector3 tangent = new Vector3((x1 - x0) * width, y1 - y0, 0f);
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg * _rotationMultiplier;
                Matrix4x4 mat = Matrix4x4.TRS(new Vector3(0f, y0, 0f), Quaternion.Euler(0f, 0f, angle), Vector3.one);

                v[vIdx] = mat.MultiplyPoint3x4(v[vIdx]) + mid;
                v[vIdx + 1] = mat.MultiplyPoint3x4(v[vIdx + 1]) + mid;
                v[vIdx + 2] = mat.MultiplyPoint3x4(v[vIdx + 2]) + mid;
                v[vIdx + 3] = mat.MultiplyPoint3x4(v[vIdx + 3]) + mid;
            }
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }

        private float GetOpticalNudge(char c1, char c2)
        {
            c1 = char.ToUpperInvariant(c1); c2 = char.ToUpperInvariant(c2);
            if (c1 == 'A' && "TVWY".IndexOf(c2) >= 0) return -3.5f;
            if ("VWY".IndexOf(c1) >= 0 && c2 == 'A') return -3.5f;
            if (c1 == 'T' && "EAO".IndexOf(c2) >= 0) return -3.0f;
            return 0f;
        }
    }
}
