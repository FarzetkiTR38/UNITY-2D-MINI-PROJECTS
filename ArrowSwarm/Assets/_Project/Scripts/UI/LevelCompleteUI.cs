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

        [Header("Stars")]
        [SerializeField] private GameObject[] _stars = new GameObject[3]; // Drag 3 star images here in Editor

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
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _nextLevelButton?.onClick.AddListener(OnNextLevel);
            _mainMenuButton?.onClick.AddListener(OnMainMenu);
            Hide(instant: true);
        }

        private void Show()
        {
            int level = LevelManager.Instance != null && LevelManager.Instance.CurrentParams.Level > 0 
                ? LevelManager.Instance.CurrentParams.Level 
                : (Data.DataManager.Instance?.PlayerData?.currentLevel ?? 1);

            if (_titleText != null) _titleText.text = "LEVEL COMPLETE!";
            if (_levelText != null) _levelText.text = $"Level {level} Cleared";

            // Update stars based on remaining lives
            int currentLives = GameManager.Instance != null ? GameManager.Instance.CurrentLives : 3;
            int starsEarned = Mathf.Clamp(currentLives, 0, 3);

            // Record stars and progression in persistent storage & cloud
            Data.DataManager.Instance?.SetLevelStars(level, starsEarned);
            Data.DataManager.Instance?.UnlockNextLevel(level);

            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    _stars[i].SetActive(i < starsEarned);
                }
            }

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f));
        }

        private void Hide(bool instant = false)
        {
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

        private void OnNextLevel() => LevelManager.Instance?.NextLevel();
        private void OnMainMenu() => GameManager.Instance?.GoToMainMenu();

        private void OnDestroy()
        {
            _nextLevelButton?.onClick.RemoveListener(OnNextLevel);
            _mainMenuButton?.onClick.RemoveListener(OnMainMenu);
        }
    }
}
