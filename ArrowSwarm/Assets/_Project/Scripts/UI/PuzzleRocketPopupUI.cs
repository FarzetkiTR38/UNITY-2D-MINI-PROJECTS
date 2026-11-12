namespace ArrowSwarm.UI
{
    using ArrowSwarm.Audio;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Displays the scene-authored Rocket card while its current puzzle attempt owns the prompt.</summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleRocketPopupUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _dialogBox;
        [SerializeField] private Button _watchAdButton, _closeButton;
        private PuzzleSession _session;
        private int _attempt = -1;
        private bool _visible;

        private void Awake() => HideImmediately();

        private void OnEnable()
        {
            PuzzleSession.OnChanged += Refresh;
            if (_watchAdButton != null) _watchAdButton.onClick.AddListener(WatchAd);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            Refresh();
        }

        private void OnDisable()
        {
            PuzzleSession.OnChanged -= Refresh;
            if (_watchAdButton != null) _watchAdButton.onClick.RemoveListener(WatchAd);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
            HideImmediately();
        }

        private void Refresh()
        {
            var session = PuzzleSession.Active;
            if (_session != session || (session != null && _attempt != session.AttemptVersion))
            {
                HideImmediately(); _session = session;
                _attempt = session != null ? session.AttemptVersion : -1;
            }
            bool visible = session != null && session.Level != null && session.RocketPromptOpen &&
                !session.Won && !session.Lost && !session.Paused;
            if (_watchAdButton != null) _watchAdButton.interactable = visible && !session.RocketAdPending;
            if (_closeButton != null) _closeButton.interactable = visible && !session.RocketAdPending;
            if (visible == _visible) return;
            _visible = visible; StopAllCoroutines();
            if (visible)
            {
                _dialogBox.gameObject.SetActive(true);
                StartCoroutine(UIPopupAnimator.AnimateOpen(_dialogBox, _canvasGroup));
            }
            else StartCoroutine(UIPopupAnimator.AnimateClose(_dialogBox, _canvasGroup, onComplete: HideImmediately));
        }

        private void HideImmediately()
        {
            StopAllCoroutines(); _visible = false;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = _canvasGroup.blocksRaycasts = false;
            }
            if (_dialogBox != null)
            {
                _dialogBox.localScale = Vector3.one;
                _dialogBox.gameObject.SetActive(false);
            }
        }

        private void WatchAd()
        {
            AudioManager.Instance?.PlayButtonClick();
            PuzzleSession.Active?.WatchRocketAd();
        }

        private void Close()
        {
            AudioManager.Instance?.PlayButtonClick();
            PuzzleSession.Active?.CloseRocketPrompt();
        }
    }
}
