namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Pause menu overlay with Resume, Restart, and Main Menu options.
    /// Uses CanvasGroup for fade animation.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private float _fadeSpeed = 5f;

        private bool _isShowing;

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleStateChanged;
        }

        private void Start()
        {
            _resumeButton?.onClick.AddListener(OnResumeClicked);
            _restartButton?.onClick.AddListener(OnRestartClicked);
            _mainMenuButton?.onClick.AddListener(OnMainMenuClicked);

            Hide(instant: true);
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Paused)
            {
                Show();
            }
            else if (_isShowing)
            {
                Hide();
            }
        }

        private void Show()
        {
            _isShowing = true;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f));
        }

        private void Hide(bool instant = false)
        {
            _isShowing = false;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (instant)
            {
                _canvasGroup.alpha = 0f;
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(FadeTo(0f));
            }
        }

        private System.Collections.IEnumerator FadeTo(float target)
        {
            while (Mathf.Abs(_canvasGroup.alpha - target) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, target, Time.unscaledDeltaTime * _fadeSpeed);
                yield return null;
            }
            _canvasGroup.alpha = target;
        }

        private void OnResumeClicked() => GameManager.Instance?.ResumeGame();
        private void OnRestartClicked() => LevelManager.Instance?.RetryLevel();
        private void OnMainMenuClicked() => GameManager.Instance?.GoToMainMenu();

        private void OnDestroy()
        {
            _resumeButton?.onClick.RemoveListener(OnResumeClicked);
            _restartButton?.onClick.RemoveListener(OnRestartClicked);
            _mainMenuButton?.onClick.RemoveListener(OnMainMenuClicked);
        }
    }
}
