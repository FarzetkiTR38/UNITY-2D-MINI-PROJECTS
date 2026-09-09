namespace ArrowSwarm.UI
{
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Popup shown when player has no tips and tries to use one.
    /// Offers to watch an ad for a free tip.
    /// </summary>
    public class TipPopupUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _dialogBox;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _watchAdButton;
        [SerializeField] private Button _closeButton;

        private bool _isShowing;

        private void OnEnable()
        {
            ArrowSwarm.Tips.TipManager.OnNoTipsAvailable += Show;
        }

        private void OnDisable()
        {
            ArrowSwarm.Tips.TipManager.OnNoTipsAvailable -= Show;
        }

        private void Start()
        {
            _watchAdButton?.onClick.AddListener(OnWatchAd);
            _closeButton?.onClick.AddListener(Hide);
            if (!_isShowing)
            {
                Hide(instant: true);
            }
        }

        private void AutoWire()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            if (_dialogBox == null)
            {
                var b = transform.Find("DialogBox") ?? transform.Find("BoardFrame") ?? transform.Find("Card");
                if (b != null) _dialogBox = b.GetComponent<RectTransform>();
            }
        }

        /// <summary>Shows the tip popup with elastic pop-in.</summary>
        public void Show()
        {
            _isShowing = true;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            AutoWire();

            if (_messageText != null)
            {
                var localized = _messageText.GetComponent<ArrowSwarm.Localization.LocalizedText>();
                if (localized != null)
                {
                    localized.RefreshText();
                }
                else if (ArrowSwarm.Localization.LocalizationManager.Instance != null)
                {
                    _messageText.text = ArrowSwarm.Localization.LocalizationManager.Instance.GetText("tip_popup_subtitle", "Watch an ad to get 1 hint");
                }
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                StopAllCoroutines();
                StartCoroutine(UIPopupAnimator.AnimateOpen(_dialogBox, _canvasGroup));
            }
        }

        /// <summary>Hides the tip popup.</summary>
        public void Hide()
        {
            Hide(false);
        }

        public void Hide(bool instant = false)
        {
            _isShowing = false;
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
            }

            StopAllCoroutines();

            if (instant)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.blocksRaycasts = false;
                }
                if (_dialogBox != null) _dialogBox.localScale = Vector3.one;
                gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(UIPopupAnimator.AnimateClose(_dialogBox, _canvasGroup, onComplete: () => gameObject.SetActive(false)));
            }
        }

        private void OnWatchAd()
        {
            ArrowSwarm.Ads.AdManager.Instance?.ShowRewardedAd(success =>
            {
                if (success)
                {
                    DataManager.Instance?.ModifyTipCount(1);
                    Hide();
                }
            });
        }

        private void OnDestroy()
        {
            _watchAdButton?.onClick.RemoveListener(OnWatchAd);
            _closeButton?.onClick.RemoveListener(Hide);
        }
    }
}
