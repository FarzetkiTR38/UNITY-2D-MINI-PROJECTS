namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the in-game HUD: level display, lives (hearts),
    /// tip count, arrow counter, and pause button.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image[] _heartIcons;
        [SerializeField] private TextMeshProUGUI _tipCountText;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _tipButton;

        [Header("Bottom Bar")]
        [SerializeField] private TextMeshProUGUI _arrowCountText;
        [SerializeField] private Slider _zoomSlider;

        [Header("Colors")]
        [SerializeField] private Color _heartActiveColor = new Color(0.91f, 0.27f, 0.37f);
        [SerializeField] private Color _heartInactiveColor = new Color(0.4f, 0.4f, 0.5f);

        private void OnEnable()
        {
            GameManager.OnLivesChanged += UpdateLives;
            GameManager.OnGameStateChanged += HandleStateChanged;
            LevelManager.OnArrowCountChanged += UpdateArrowCount;
            LevelManager.OnLevelReady += HandleLevelReady;
        }

        private void OnDisable()
        {
            GameManager.OnLivesChanged -= UpdateLives;
            GameManager.OnGameStateChanged -= HandleStateChanged;
            LevelManager.OnArrowCountChanged -= UpdateArrowCount;
            LevelManager.OnLevelReady -= HandleLevelReady;
        }

        private void Start()
        {
            _pauseButton?.onClick.AddListener(OnPauseClicked);
            _tipButton?.onClick.AddListener(OnTipClicked);
        }

        private void HandleLevelReady(LevelParams levelParams)
        {
            if (_levelText != null)
            {
                _levelText.text = $"Lv.{levelParams.Level}";
            }

            UpdateTipCount();
        }

        private void UpdateLives(int lives)
        {
            if (_heartIcons == null) return;

            for (int i = 0; i < _heartIcons.Length; i++)
            {
                _heartIcons[i].color = i < lives ? _heartActiveColor : _heartInactiveColor;
            }
        }

        private void UpdateArrowCount(int fired, int total)
        {
            if (_arrowCountText != null)
            {
                _arrowCountText.text = $"Arrows: {fired}/{total}";
            }
        }

        private void UpdateTipCount()
        {
            if (_tipCountText == null) return;
            int tips = Data.DataManager.Instance?.PlayerData?.tipCount ?? 0;
            _tipCountText.text = $"x{tips}";
        }

        private void HandleStateChanged(GameState state)
        {
            // HUD visibility based on state
            gameObject.SetActive(state == GameState.Playing || state == GameState.Paused);
        }

        private void OnPauseClicked()
        {
            GameManager.Instance?.PauseGame();
        }

        private void OnTipClicked()
        {
            // Tip system handled in Phase 7
            // TipManager.Instance?.UseTip();
        }

        private void OnDestroy()
        {
            _pauseButton?.onClick.RemoveListener(OnPauseClicked);
            _tipButton?.onClick.RemoveListener(OnTipClicked);
        }
    }
}
