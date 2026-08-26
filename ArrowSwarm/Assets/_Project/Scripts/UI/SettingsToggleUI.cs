namespace ArrowSwarm.UI
{
    using System;
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Custom toggle switch component supporting dual-sprite switching (ON / OFF)
    /// and optional animated knob and status label.
    /// </summary>
    public class SettingsToggleUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private RectTransform _knobRect;
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Sprites (ON / OFF)")]
        [Tooltip("Sprite displayed when toggle is ON")]
        [SerializeField] private Sprite _onSprite;
        [Tooltip("Sprite displayed when toggle is OFF")]
        [SerializeField] private Sprite _offSprite;

        [Header("Procedural Fallback Styling")]
        [Tooltip("Apply color tint even if custom sprites are assigned")]
        [SerializeField] private bool _useColorTint = false;
        [SerializeField] private Color _onColor = new Color(0.09f, 0.52f, 0.96f, 1f);
        [SerializeField] private Color _offColor = new Color(0.65f, 0.72f, 0.82f, 1f);

        [Header("Knob & Text Positions")]
        [SerializeField] private float _onKnobX = 55f;
        [SerializeField] private float _offKnobX = -55f;
        [SerializeField] private float _onTextX = -35f;
        [SerializeField] private float _offTextX = 35f;
        [SerializeField] private string _onText = "ON";
        [SerializeField] private string _offText = "OFF";
        [SerializeField] private float _animSpeed = 1000f;

        /// <summary>Current toggle state.</summary>
        public bool IsOn { get; private set; } = true;

        /// <summary>Fired when the toggle value changes via interaction or code.</summary>
        public event Action<bool> OnValueChanged;

        private Coroutine _animCoroutine;

        private void Awake()
        {
            AutoWire();
        }

        private void OnEnable()
        {
            if (_button == null) _button = GetComponent<Button>();
            _button?.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button?.onClick.RemoveListener(OnButtonClicked);
        }

        /// <summary>
        /// Automatically discovers child and sibling UI components if unassigned.
        /// </summary>
        public void AutoWire()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_backgroundImage == null) _backgroundImage = GetComponent<Image>();

            if (_knobRect == null)
            {
                var knob = transform.Find("Knob");
                if (knob != null) _knobRect = knob.GetComponent<RectTransform>();
            }

            if (_statusText == null)
            {
                var txt = transform.Find("StatusText") ?? transform.Find("Text");
                if (txt != null) _statusText = txt.GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// Sets the toggle state with optional animation and sprite swap.
        /// </summary>
        public void SetIsOn(bool isOn, bool animate = true)
        {
            AutoWire();
            IsOn = isOn;

            // 1. Sprite & Image update
            if (_backgroundImage != null)
            {
                Sprite targetSprite = isOn ? _onSprite : _offSprite;
                if (targetSprite != null)
                {
                    _backgroundImage.sprite = targetSprite;
                    if (!_useColorTint)
                    {
                        _backgroundImage.color = Color.white;
                    }
                    else
                    {
                        _backgroundImage.color = isOn ? _onColor : _offColor;
                    }
                }
                else
                {
                    // Fallback to solid color tint if no sprite assigned
                    _backgroundImage.color = isOn ? _onColor : _offColor;
                }
            }

            // 2. Status text update (if present)
            if (_statusText != null && _statusText.gameObject.activeInHierarchy)
            {
                _statusText.text = isOn ? _onText : _offText;
                var textRT = _statusText.GetComponent<RectTransform>();
                if (textRT != null)
                {
                    textRT.anchoredPosition = new Vector2(isOn ? _onTextX : _offTextX, textRT.anchoredPosition.y);
                }
            }

            // 3. Knob position update (if present)
            float targetKnobX = isOn ? _onKnobX : _offKnobX;
            if (_knobRect != null && _knobRect.gameObject.activeInHierarchy)
            {
                if (animate && gameObject.activeInHierarchy)
                {
                    if (_animCoroutine != null) StopCoroutine(_animCoroutine);
                    _animCoroutine = StartCoroutine(AnimateKnob(targetKnobX));
                }
                else
                {
                    _knobRect.anchoredPosition = new Vector2(targetKnobX, _knobRect.anchoredPosition.y);
                }
            }
        }

        /// <summary>
        /// Inverts the current toggle state.
        /// </summary>
        public void Toggle()
        {
            SetIsOn(!IsOn, true);
            OnValueChanged?.Invoke(IsOn);
        }

        /// <summary>
        /// Configures ON/OFF sprites at runtime if needed.
        /// </summary>
        public void SetSprites(Sprite onSprite, Sprite offSprite)
        {
            _onSprite = onSprite;
            _offSprite = offSprite;
            SetIsOn(IsOn, false);
        }

        private void OnButtonClicked()
        {
            Toggle();
        }

        private IEnumerator AnimateKnob(float targetX)
        {
            if (_knobRect == null) yield break;

            while (Mathf.Abs(_knobRect.anchoredPosition.x - targetX) > 0.5f)
            {
                float newX = Mathf.MoveTowards(_knobRect.anchoredPosition.x, targetX, Time.unscaledDeltaTime * _animSpeed);
                _knobRect.anchoredPosition = new Vector2(newX, _knobRect.anchoredPosition.y);
                yield return null;
            }

            _knobRect.anchoredPosition = new Vector2(targetX, _knobRect.anchoredPosition.y);
            _animCoroutine = null;
        }
    }
}
