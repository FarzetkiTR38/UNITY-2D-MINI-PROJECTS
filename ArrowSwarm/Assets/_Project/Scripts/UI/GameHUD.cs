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
        [SerializeField] private RectTransform _topPanelRect;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image[] _heartIcons;
        [SerializeField] private TextMeshProUGUI _tipCountText;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _tipButton;

        [Header("Bottom Bar")]
        [SerializeField] private RectTransform _bottomPanelRect;
        [SerializeField] private TextMeshProUGUI _arrowCountText;
        [SerializeField] private Slider _zoomSlider;

        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup _canvasGroup;

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
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _pauseButton?.onClick.AddListener(OnPauseClicked);
            _tipButton?.onClick.AddListener(OnTipClicked);

            if (GameManager.Instance != null)
            {
                HandleStateChanged(GameManager.Instance.CurrentState);
            }
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
                if (_heartIcons[i] != null)
                {
                    _heartIcons[i].gameObject.SetActive(i < lives);
                }
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
            // HUD visibility based on state using CanvasGroup
            bool visible = (state == GameState.Playing || state == GameState.Paused);
            SetHUDVisible(visible);
        }

        private void SetHUDVisible(bool visible)
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        private void OnPauseClicked()
        {
            GameManager.Instance?.PauseGame();
        }

        private void OnTipClicked()
        {
            ArrowSwarm.Tips.TipManager.Instance?.UseTip();
        }

        private void OnDestroy()
        {
            _pauseButton?.onClick.RemoveListener(OnPauseClicked);
            _tipButton?.onClick.RemoveListener(OnTipClicked);
        }
    }
}
