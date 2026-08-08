namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Game Over screen shown when the player loses.
    /// Shows "Game Over" with Retry and Main Menu buttons.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private float _fadeSpeed = 3f;

        private void OnEnable()
        {
            GameManager.OnLevelLost += Show;
        }

        private void OnDisable()
        {
            GameManager.OnLevelLost -= Show;
        }

        private void Start()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _retryButton?.onClick.AddListener(OnRetry);
            _mainMenuButton?.onClick.AddListener(OnMainMenu);
            Hide(instant: true);
        }

        private void Show()
        {
            int level = Data.DataManager.Instance?.PlayerData?.currentLevel ?? 1;
            if (_titleText != null) _titleText.text = "GAME OVER";
            if (_levelText != null) _levelText.text = $"Level {level} Failed";

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f));
        }

        private void Hide(bool instant = false)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            if (instant) _canvasGroup.alpha = 0f;
            else { StopAllCoroutines(); StartCoroutine(FadeTo(0f)); }
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

        private void OnRetry() => LevelManager.Instance?.RetryLevel();
        private void OnMainMenu() => GameManager.Instance?.GoToMainMenu();

        private void OnDestroy()
        {
            _retryButton?.onClick.RemoveListener(OnRetry);
            _mainMenuButton?.onClick.RemoveListener(OnMainMenu);
        }
    }
}
