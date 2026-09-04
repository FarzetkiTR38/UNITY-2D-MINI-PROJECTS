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
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _watchAdButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private float _fadeSpeed = 5f;

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

        /// <summary>Shows the tip popup.</summary>
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
                _messageText.text = "No tips left!\nWatch an ad to get +1 tip?";
            }

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f));
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

        private System.Collections.IEnumerator FadeTo(float target)
        {
            while (_canvasGroup != null && Mathf.Abs(_canvasGroup.alpha - target) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(
                    _canvasGroup.alpha, target, Time.unscaledDeltaTime * _fadeSpeed);
                yield return null;
            }
            if (_canvasGroup != null) _canvasGroup.alpha = target;

            if (target <= 0.01f)
            {
                if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            }
        }

        private void OnWatchAd()
        {
            ArrowSwarm.Ads.MockAdService.Instance?.ShowRewardedAd(success =>
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
