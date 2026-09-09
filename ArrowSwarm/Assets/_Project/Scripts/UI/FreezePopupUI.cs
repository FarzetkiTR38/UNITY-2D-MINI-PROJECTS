namespace ArrowSwarm.UI
{
    using System.Collections;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Skills;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Popup shown when the player has no freeze charges left and attempts to use the freeze skill.
    /// Offers to watch a rewarded ad to gain 1 free freeze charge.
    /// </summary>
    [DisallowMultipleComponent]
    public class FreezePopupUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _dialogBox;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _watchAdButton;
        [SerializeField] private Button _closeButton;

        private bool _isShowing;

        private void OnEnable()
        {
            FreezeManager.OnNoFreezesAvailable += Show;
        }

        private void OnDisable()
        {
            FreezeManager.OnNoFreezesAvailable -= Show;
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

        /// <summary>
        /// Displays the freeze popup and initiates elastic pop-in animation.
        /// </summary>
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
                var localized = _messageText.GetComponent<LocalizedText>();
                if (localized != null)
                {
                    localized.RefreshText();
                }
                else if (LocalizationManager.Instance != null)
                {
                    _messageText.text = LocalizationManager.Instance.GetText("freeze_popup_subtitle", "Watch an ad to get 1 freeze");
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

        /// <summary>
        /// Hides the freeze popup smoothly.
        /// </summary>
        public void Hide()
        {
            Hide(false);
        }

        /// <summary>
        /// Hides the freeze popup with optional instant transition.
        /// </summary>
        /// <param name="instant">If true, dismisses immediately without animation.</param>
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
                    DataManager.Instance?.ModifyFreezeCount(1);
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
