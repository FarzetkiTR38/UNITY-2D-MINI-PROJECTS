namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the Main Menu screen: Play button, Leaderboard button,
    /// settings icon, and current level display.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Panels")]
        [SerializeField] private GameObject _leaderboardPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeSpeed = 2f;

        private void Start()
        {
            SetupUI();
            SetupButtons();
            AnimateIn();
        }

        private void SetupUI()
        {
            int level = DataManager.Instance?.PlayerData?.currentLevel ?? 1;
            if (_levelText != null)
            {
                _levelText.text = $"Level: {level}";
            }

            if (_titleText != null)
            {
                _titleText.text = "ARROW SWARM";
            }
        }

        private void SetupButtons()
        {
            _playButton?.onClick.AddListener(OnPlayClicked);
            _leaderboardButton?.onClick.AddListener(OnLeaderboardClicked);
            _settingsButton?.onClick.AddListener(OnSettingsClicked);
        }

        private void OnPlayClicked()
        {
            GameManager.Instance?.StartGame();
        }

        private void OnLeaderboardClicked()
        {
            _leaderboardPanel?.SetActive(true);
        }

        private void OnSettingsClicked()
        {
            _settingsPanel?.SetActive(true);
        }

        private void AnimateIn()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }

        private System.Collections.IEnumerator FadeIn()
        {
            while (_canvasGroup.alpha < 1f)
            {
                _canvasGroup.alpha += Time.deltaTime * _fadeSpeed;
                yield return null;
            }
            _canvasGroup.alpha = 1f;
        }

        private void OnDestroy()
        {
            _playButton?.onClick.RemoveListener(OnPlayClicked);
            _leaderboardButton?.onClick.RemoveListener(OnLeaderboardClicked);
            _settingsButton?.onClick.RemoveListener(OnSettingsClicked);
        }
    }
}
