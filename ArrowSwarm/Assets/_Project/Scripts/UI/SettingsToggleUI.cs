namespace ArrowSwarm.UI
{
    using System;
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Custom animated toggle switch component matching the mockup UI style.
    /// Handles ON/OFF state transitions, knob sliding animation, status text, and colors.
    /// </summary>
    public class SettingsToggleUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private RectTransform _knobRect;
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Styling")]
        [SerializeField] private Color _onColor = new Color(0.09f, 0.52f, 0.96f, 1f); // Vibrant Blue
        [SerializeField] private Color _offColor = new Color(0.65f, 0.72f, 0.82f, 1f); // Grey
        [SerializeField] private Sprite _onSprite;
        [SerializeField] private Sprite _offSprite;
        [SerializeField] private float _onKnobX = 55f;
        [SerializeField] private float _offKnobX = -55f;
        [SerializeField] private float _onTextX = -35f;
        [SerializeField] private float _offTextX = 35f;
        [SerializeField] private string _onText = "ON";
        [SerializeField] private string _offText = "OFF";
        [SerializeField] private float _animSpeed = 10f;

        public bool IsOn { get; private set; } = true;
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
        /// Sets the toggle value with optional sliding animation.
        /// </summary>
        public void SetIsOn(bool isOn, bool animate = true)
        {
            AutoWire();
            IsOn = isOn;

            if (_statusText != null)
            {
                _statusText.text = isOn ? _onText : _offText;
                var textRT = _statusText.GetComponent<RectTransform>();
                if (textRT != null)
                {
                    textRT.anchoredPosition = new Vector2(isOn ? _onTextX : _offTextX, textRT.anchoredPosition.y);
                }
            }

            if (_backgroundImage != null)
            {
                if (isOn && _onSprite != null) _backgroundImage.sprite = _onSprite;
                else if (!isOn && _offSprite != null) _backgroundImage.sprite = _offSprite;
                _backgroundImage.color = isOn ? _onColor : _offColor;
            }

            float targetKnobX = isOn ? _onKnobX : _offKnobX;

            if (_knobRect != null)
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

        private void OnButtonClicked()
        {
            SetIsOn(!IsOn, true);
            OnValueChanged?.Invoke(IsOn);
        }

        private IEnumerator AnimateKnob(float targetX)
        {
            if (_knobRect == null) yield break;

            while (Mathf.Abs(_knobRect.anchoredPosition.x - targetX) > 0.5f)
            {
                float newX = Mathf.MoveTowards(_knobRect.anchoredPosition.x, targetX, Time.unscaledDeltaTime * 1000f);
                _knobRect.anchoredPosition = new Vector2(newX, _knobRect.anchoredPosition.y);
                yield return null;
            }

            _knobRect.anchoredPosition = new Vector2(targetX, _knobRect.anchoredPosition.y);
            _animCoroutine = null;
        }
    }
}
