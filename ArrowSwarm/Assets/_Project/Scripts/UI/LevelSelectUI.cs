namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the Level Selection panel.
    /// Dynamically populates level buttons based on player progression.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _levelGridContainer;
        [SerializeField] private LevelButtonUI _levelButtonPrefab;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeSpeed = 5f;

        private const int MAX_LEVELS = 30; // Just a placeholder for total possible levels

        private void OnEnable()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                StopAllCoroutines();
                StartCoroutine(FadeTo(1f));
            }
            
            PopulateLevels();
        }

        private void Start()
        {
            _closeButton?.onClick.AddListener(Close);
        }

        private void PopulateLevels()
        {
            // Clear existing children
            foreach (Transform child in _levelGridContainer)
            {
                Destroy(child.gameObject);
            }

            int highestLevel = DataManager.Instance?.PlayerData?.highestLevel ?? 1;

            for (int i = 1; i <= MAX_LEVELS; i++)
            {
                LevelButtonUI btn = Instantiate(_levelButtonPrefab, _levelGridContainer);
                int stars = DataManager.Instance?.GetLevelStars(i) ?? 0;
                bool isUnlocked = i <= highestLevel;
                
                btn.Setup(i, isUnlocked, stars, OnLevelClicked);
            }
        }

        private void OnLevelClicked(int level)
        {
            // Start the game at the specific level
            DataManager.Instance?.SetCurrentLevel(level);
            GameManager.Instance?.StartGame();
        }

        public void Close()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                StopAllCoroutines();
                StartCoroutine(FadeTo(0f, true));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private System.Collections.IEnumerator FadeTo(float target, bool disableOnComplete = false)
        {
            while (Mathf.Abs(_canvasGroup.alpha - target) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, target, Time.deltaTime * _fadeSpeed);
                yield return null;
            }
            _canvasGroup.alpha = target;

            if (disableOnComplete)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _closeButton?.onClick.RemoveListener(Close);
        }
    }
}
