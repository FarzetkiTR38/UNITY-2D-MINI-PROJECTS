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
            AlignPanelsToEdges();
            ApplyPastelThemeHUDStyle();

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

        private void AlignPanelsToEdges()
        {
            if (_topPanelRect != null)
            {
                _topPanelRect.anchorMin = new Vector2(0f, 1f);
                _topPanelRect.anchorMax = new Vector2(1f, 1f);
                _topPanelRect.pivot = new Vector2(0.5f, 1f);
                _topPanelRect.anchoredPosition = Vector2.zero;
            }
            else if (_levelText != null && _levelText.transform.parent is RectTransform parentTop)
            {
                parentTop.anchorMin = new Vector2(0f, 1f);
                parentTop.anchorMax = new Vector2(1f, 1f);
                parentTop.pivot = new Vector2(0.5f, 1f);
                parentTop.anchoredPosition = Vector2.zero;
            }

            if (_bottomPanelRect != null)
            {
                _bottomPanelRect.anchorMin = new Vector2(0f, 0f);
                _bottomPanelRect.anchorMax = new Vector2(1f, 0f);
                _bottomPanelRect.pivot = new Vector2(0.5f, 0f);
                _bottomPanelRect.anchoredPosition = Vector2.zero;
            }
            else if (_arrowCountText != null && _arrowCountText.transform.parent is RectTransform parentBottom)
            {
                parentBottom.anchorMin = new Vector2(0f, 0f);
                parentBottom.anchorMax = new Vector2(1f, 0f);
                parentBottom.pivot = new Vector2(0.5f, 0f);
                parentBottom.anchoredPosition = Vector2.zero;
            }
        }

        private void ApplyPastelThemeHUDStyle()
        {
            // Dark Indigo Text color (#3D344B)
            Color textDarkIndigo = new Color(0.24f, 0.20f, 0.29f, 1f);

            if (_levelText != null) _levelText.color = textDarkIndigo;
            if (_arrowCountText != null) _arrowCountText.color = textDarkIndigo;
            if (_tipCountText != null) _tipCountText.color = textDarkIndigo;

            // Clear background images on top and bottom panel containers (remove dark translucent boxes)
            if (_topPanelRect != null)
            {
                var img = _topPanelRect.GetComponent<Image>();
                if (img != null) img.color = Color.clear;
            }
            if (_bottomPanelRect != null)
            {
                var img = _bottomPanelRect.GetComponent<Image>();
                if (img != null) img.color = Color.clear;
            }

            // Style buttons as clean 100% opaque floating white rounded cards
            Color whiteCard = Color.white;
            if (_pauseButton != null && _pauseButton.targetGraphic is Image pauseImg)
            {
                pauseImg.color = whiteCard;
            }
            if (_tipButton != null && _tipButton.targetGraphic is Image tipImg)
            {
                tipImg.color = whiteCard;
            }

            // Heart icons: Golden Active, Soft Taupe Inactive
            _heartInactiveColor = new Color(0.84f, 0.80f, 0.75f, 1f); // #D6CBC0
            _heartActiveColor = new Color(1.00f, 0.64f, 0.11f, 1f);   // #FFA41B (Star Gold)
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
