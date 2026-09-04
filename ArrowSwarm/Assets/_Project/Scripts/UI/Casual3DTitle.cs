namespace ArrowSwarm.UI
{
    using System;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Renders dynamic 3D cartoon titles with arched curving, stepped extrusion, and localization support.
    /// Perfectly replicates multi-tiered casual game logo titles.
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

        [Header("Layers (Front to Back)")]
        [SerializeField] private TextMeshProUGUI _frontText;
        [SerializeField] private TextMeshProUGUI _rimText;
        [SerializeField] private TextMeshProUGUI _extrusionText;
        [SerializeField] private TextMeshProUGUI _shadowText;

        [Header("3D Layer Offsets")]
        [SerializeField] private Vector2 _rimOffset = new Vector2(0f, -4f);
        [SerializeField] private Vector2 _extrusionOffset = new Vector2(0f, -12f);
        [SerializeField] private Vector2 _shadowOffset = new Vector2(0f, -20f);

        [Header("Typography Styling")]
        [SerializeField] private float _fontSize = 86f;
        [SerializeField] private FontStyles _fontStyle = FontStyles.Bold;
        [SerializeField] private float _characterSpacing = -8f;
        [SerializeField] private float _verticalStretch = 1.08f;

        [Header("Arch Curving")]
        [SerializeField] private bool _enableCurve = true;
        [SerializeField] private float _curveScale = 16f;
        [Range(0f, 1f)]
        [SerializeField] private float _rotationMultiplier = 0.55f;
        [SerializeField] private AnimationCurve _vertexCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2.2f),
            new Keyframe(0.5f, 1f, 0f, 0f),
            new Keyframe(1f, 0f, -2.2f, 0f)
        );

        private string _lastRenderedText;
        private bool _isDirty;

        /// <summary>Gets or sets the text and updates all 3D layers.</summary>
        public string Text { get => _text; set { if (_text != value) { _text = value; MarkDirty(); } } }

        /// <summary>Gets or sets the localization key for automatic translation.</summary>
        public string LocalizationKey { get => _localizationKey; set { _localizationKey = value; RefreshLocalized(); } }

        /// <summary>Gets or sets font size across all 3D layers.</summary>
        public float FontSize { get => _fontSize; set { if (Mathf.Abs(_fontSize - value) > 0.01f) { _fontSize = value; MarkDirty(); } } }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += RefreshLocalized;
            RefreshLocalized();
            MarkDirty();
        }

        private void OnDisable() => LocalizationManager.OnLanguageChanged -= RefreshLocalized;
        private void Start() { RefreshLocalized(); ApplyAll(); }
        private void OnValidate() => MarkDirty();

        private void LateUpdate()
        {
            if (_isDirty || _text != _lastRenderedText) ApplyAll();
        }

        /// <summary>Flags this title as needing a geometry and text refresh.</summary>
        public void MarkDirty() => _isDirty = true;

        /// <summary>Translates text using active localization manager if a key is assigned.</summary>
        public void RefreshLocalized()
        {
            if (!string.IsNullOrEmpty(_localizationKey) && LocalizationManager.HasInstance)
                Text = LocalizationManager.Instance.GetText(_localizationKey, _text);
            else
                MarkDirty();
        }

        /// <summary>Applies text, offsets, and vertex arch curve across all layers.</summary>
        public void ApplyAll()
        {
            _isDirty = false;
            _lastRenderedText = _text;

            UpdateLayer(_frontText, Vector2.zero);
            UpdateLayer(_rimText, _rimOffset);
            UpdateLayer(_extrusionText, _extrusionOffset);
            UpdateLayer(_shadowText, _shadowOffset);

            if (_enableCurve && _frontText != null)
            {
                _frontText.ForceMeshUpdate(true, true);
                float minX = _frontText.bounds.min.x;
                float maxX = _frontText.bounds.max.x;
                float width = maxX - minX;

                if (width > 0.001f)
                {
                    WarpLayer(_frontText, minX, width);
                    WarpLayer(_rimText, minX, width);
                    WarpLayer(_extrusionText, minX, width);
                    WarpLayer(_shadowText, minX, width);
                }
            }
        }

        private void UpdateLayer(TextMeshProUGUI tmp, Vector2 offset)
        {
            if (tmp == null) return;
            tmp.text = _text;
            tmp.fontSize = _fontSize;
            tmp.fontStyle = _fontStyle;
            tmp.characterSpacing = _characterSpacing;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.anchoredPosition = offset;
            tmp.rectTransform.localScale = new Vector3(1f, _verticalStretch, 1f);
        }

        private void WarpLayer(TextMeshProUGUI tmp, float minX, float width)
        {
            if (tmp == null || string.IsNullOrEmpty(_text)) return;

            tmp.ForceMeshUpdate(true, true);
            TMP_TextInfo textInfo = tmp.textInfo;
            int charCount = textInfo.characterCount;
            if (charCount <= 0) return;

            float effCurve = charCount <= 3 ? _curveScale * 0.35f : _curveScale;

            for (int i = 0; i < charCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                int vIdx = textInfo.characterInfo[i].vertexIndex;
                int mIdx = textInfo.characterInfo[i].materialReferenceIndex;
                Vector3[] verts = textInfo.meshInfo[mIdx].vertices;

                Vector3 midBaseline = new Vector2(
                    (verts[vIdx + 0].x + verts[vIdx + 2].x) * 0.5f,
                    textInfo.characterInfo[i].baseLine);

                verts[vIdx + 0] -= midBaseline;
                verts[vIdx + 1] -= midBaseline;
                verts[vIdx + 2] -= midBaseline;
                verts[vIdx + 3] -= midBaseline;

                float x0 = Mathf.Clamp01((midBaseline.x - minX) / width);
                float x1 = Mathf.Clamp01(x0 + 0.001f);
                float y0 = _vertexCurve.Evaluate(x0) * effCurve;
                float y1 = _vertexCurve.Evaluate(x1) * effCurve;

                Vector3 tangent = new Vector3((x1 - x0) * width, y1 - y0, 0f);
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg * _rotationMultiplier;

                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(0f, y0, 0f), Quaternion.Euler(0f, 0f, angle), Vector3.one);

                verts[vIdx + 0] = matrix.MultiplyPoint3x4(verts[vIdx + 0]) + midBaseline;
                verts[vIdx + 1] = matrix.MultiplyPoint3x4(verts[vIdx + 1]) + midBaseline;
                verts[vIdx + 2] = matrix.MultiplyPoint3x4(verts[vIdx + 2]) + midBaseline;
                verts[vIdx + 3] = matrix.MultiplyPoint3x4(verts[vIdx + 3]) + midBaseline;
            }

            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }
}

