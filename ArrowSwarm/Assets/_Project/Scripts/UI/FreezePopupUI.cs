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
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _watchAdButton;
        [SerializeField] private Button _closeButton;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeSpeed = 5f;

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

        /// <summary>
        /// Displays the freeze popup and initiates fade-in animation.
        /// </summary>
        public void Show()
        {
            _isShowing = true;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            }

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

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f));
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

            if (instant)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.blocksRaycasts = false;
                }
                gameObject.SetActive(false);
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(FadeTo(0f));
            }
        }

        private IEnumerator FadeTo(float target)
        {
            while (_canvasGroup != null && Mathf.Abs(_canvasGroup.alpha - target) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(
                    _canvasGroup.alpha, target, Time.unscaledDeltaTime * _fadeSpeed);
                yield return null;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = target;
            }

            if (target <= 0.01f)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.blocksRaycasts = false;
                }
                gameObject.SetActive(false);
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
