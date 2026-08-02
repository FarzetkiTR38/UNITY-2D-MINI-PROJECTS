namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Win screen shown when the player completes a level.
    /// Shows "Level X Completed!" with Next Level and Main Menu buttons.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private float _fadeSpeed = 3f;

        private void OnEnable()
        {
            GameManager.OnLevelWon += Show;
        }

        private void OnDisable()
        {
            GameManager.OnLevelWon -= Show;
        }

        private void Start()
        {
            _nextLevelButton?.onClick.AddListener(OnNextLevel);
            _mainMenuButton?.onClick.AddListener(OnMainMenu);
            Hide(instant: true);
        }

        private void Show()
        {
            int level = Data.DataManager.Instance?.PlayerData?.currentLevel ?? 1;
            if (_titleText != null) _titleText.text = "LEVEL COMPLETE!";
            if (_levelText != null) _levelText.text = $"Level {level} Cleared";

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

        private void OnNextLevel() => LevelManager.Instance?.NextLevel();
        private void OnMainMenu() => GameManager.Instance?.GoToMainMenu();

        private void OnDestroy()
        {
            _nextLevelButton?.onClick.RemoveListener(OnNextLevel);
            _mainMenuButton?.onClick.RemoveListener(OnMainMenu);
        }
    }
}
