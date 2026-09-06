namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Skills;
    using ArrowSwarm.Tips;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the in-game HUD: level display, lives (hearts),
    /// arrow counter, active skills (Tips &amp; Freeze), and pause menu.
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

        [Header("Active Skills (Bottom Bar)")]
        [SerializeField] private Button _skill1TipButton;
        [SerializeField] private TextMeshProUGUI _skill1CountText;
        [SerializeField] private GameObject _skill1AdBadge;

        [SerializeField] private Button _skill2FreezeButton;
        [SerializeField] private TextMeshProUGUI _skill2CountText;
        [SerializeField] private GameObject _skill2AdBadge;
        [SerializeField] private GameObject _skill2ActiveTimerRoot;
        [SerializeField] private TextMeshProUGUI _skill2ActiveTimerText;

        /// <summary>RectTransform of the top HUD bar.</summary>
        public RectTransform TopPanelRect => _topPanelRect;

        /// <summary>RectTransform of the bottom HUD bar.</summary>
        public RectTransform BottomPanelRect => _bottomPanelRect;

        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            EnsurePanels();
            AutoWireSkillButtons();
        }

        private void EnsurePanels()
        {
            if (_topPanelRect == null)
            {
                _topPanelRect = transform.Find("TopPanel") as RectTransform 
                             ?? transform.Find("TopBar") as RectTransform 
                             ?? transform.Find("Header") as RectTransform;
            }
            if (_bottomPanelRect == null)
            {
                _bottomPanelRect = transform.Find("BottomPanel") as RectTransform 
                                ?? transform.Find("BottomBar") as RectTransform 
                                ?? transform.Find("Footer") as RectTransform;
            }
        }

        private void AutoWireSkillButtons()
        {
            if (_skill1TipButton == null)
            {
                _skill1TipButton = transform.Find("BottomBar/Skill1")?.GetComponent<Button>();
            }
            if (_skill1CountText == null && _skill1TipButton != null)
            {
                _skill1CountText = _skill1TipButton.transform.Find("Badge/Text")?.GetComponent<TextMeshProUGUI>()
                                ?? _skill1TipButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (_skill1AdBadge == null && _skill1TipButton != null)
            {
                _skill1AdBadge = _skill1TipButton.transform.Find("AdBadge")?.gameObject;
            }

            if (_skill2FreezeButton == null)
            {
                _skill2FreezeButton = transform.Find("BottomBar/Skill2")?.GetComponent<Button>();
            }
            if (_skill2CountText == null && _skill2FreezeButton != null)
            {
                _skill2CountText = _skill2FreezeButton.transform.Find("Badge/Text")?.GetComponent<TextMeshProUGUI>()
                                ?? _skill2FreezeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (_skill2AdBadge == null && _skill2FreezeButton != null)
            {
                _skill2AdBadge = _skill2FreezeButton.transform.Find("AdBadge")?.gameObject;
            }
            if (_skill2ActiveTimerRoot == null && _skill2FreezeButton != null)
            {
                _skill2ActiveTimerRoot = _skill2FreezeButton.transform.Find("ActiveTimer")?.gameObject;
            }
            if (_skill2ActiveTimerText == null && _skill2FreezeButton != null)
            {
                _skill2ActiveTimerText = _skill2FreezeButton.transform.Find("ActiveTimer/Text")?.GetComponent<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            GameManager.OnLivesChanged += UpdateLives;
            GameManager.OnGameStateChanged += HandleStateChanged;
            LevelManager.OnArrowCountChanged += UpdateArrowCount;
            LevelManager.OnLevelReady += HandleLevelReady;
            DataManager.OnPlayerDataChanged += HandlePlayerDataChanged;

            TipManager.OnTipUsed += HandleTipUsed;
            FreezeManager.OnFreezeStarted += HandleFreezeStarted;
            FreezeManager.OnFreezeTick += HandleFreezeTick;
            FreezeManager.OnFreezeEnded += HandleFreezeEnded;
        }

        private void OnDisable()
        {
            GameManager.OnLivesChanged -= UpdateLives;
            GameManager.OnGameStateChanged -= HandleStateChanged;
            LevelManager.OnArrowCountChanged -= UpdateArrowCount;
            LevelManager.OnLevelReady -= HandleLevelReady;
            DataManager.OnPlayerDataChanged -= HandlePlayerDataChanged;

            TipManager.OnTipUsed -= HandleTipUsed;
            FreezeManager.OnFreezeStarted -= HandleFreezeStarted;
            FreezeManager.OnFreezeTick -= HandleFreezeTick;
            FreezeManager.OnFreezeEnded -= HandleFreezeEnded;
        }



        private void Start()
        {
            EnsurePanels();
            AutoWireSkillButtons();

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            }

            _pauseButton?.onClick.AddListener(OnPauseClicked);
            _tipButton?.onClick.AddListener(OnTipClicked);

            if (_skill1TipButton != null)
            {
                _skill1TipButton.onClick.RemoveAllListeners();
                _skill1TipButton.onClick.AddListener(OnTipClicked);
            }

            if (_skill2FreezeButton != null)
            {
                _skill2FreezeButton.onClick.RemoveAllListeners();
                _skill2FreezeButton.onClick.AddListener(OnFreezeClicked);
            }

            UpdateSkillBadges();

            if (GameManager.Instance != null)
            {
                HandleStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void HandleLevelReady(LevelParams levelParams)
        {
            EnsurePanels();

            if (_levelText != null)
            {
                _levelText.text = $"Lv.{levelParams.Level}";
            }

            UpdateSkillBadges();

            bool isTutorial = (levelParams.Level <= 1 && (DataManager.Instance == null || !DataManager.Instance.IsTutorialCompleted))
                           || (ArrowSwarm.Tutorial.TutorialManager.Instance != null && ArrowSwarm.Tutorial.TutorialManager.Instance.IsTutorialActive);

            // In tutorial mode, hide both top and bottom bars for cinematic immersion
            SetBarsVisible(!isTutorial, !isTutorial);
        }

        private void HandlePlayerDataChanged(PlayerData data)
        {
            UpdateSkillBadges();
        }

        private void HandleTipUsed(int remaining)
        {
            UpdateSkillBadges();
        }

        private void HandleFreezeStarted(float duration)
        {
            UpdateSkillBadges();
            if (_skill2ActiveTimerRoot != null)
            {
                _skill2ActiveTimerRoot.SetActive(true);
            }
            if (_skill2ActiveTimerText != null)
            {
                _skill2ActiveTimerText.gameObject.SetActive(true);
                _skill2ActiveTimerText.text = $"{duration:F1}s";
            }
        }

        private void HandleFreezeTick(float remaining, float total)
        {
            if (_skill2ActiveTimerRoot != null && !_skill2ActiveTimerRoot.activeSelf)
            {
                _skill2ActiveTimerRoot.SetActive(true);
            }
            if (_skill2ActiveTimerText != null)
            {
                _skill2ActiveTimerText.text = $"{remaining:F1}s";
            }
        }

        private void HandleFreezeEnded()
        {
            if (_skill2ActiveTimerRoot != null)
            {
                _skill2ActiveTimerRoot.SetActive(false);
            }
            UpdateSkillBadges();
        }

        /// <summary>
        /// Updates the badges and remaining counts for Tips and Freeze skills.
        /// </summary>
        public void UpdateSkillBadges()
        {
            PlayerData data = DataManager.Instance?.PlayerData;
            int tips = data?.tipCount ?? 0;
            int freezes = data?.freezeCount ?? 0;

            // Legacy top bar tip text
            if (_tipCountText != null) _tipCountText.text = $"x{tips}";

            // Bottom Bar Skill 1 (Tip)
            if (_skill1CountText != null)
            {
                _skill1CountText.gameObject.SetActive(tips > 0);
                _skill1CountText.text = tips.ToString();
            }
            if (_skill1AdBadge != null)
            {
                _skill1AdBadge.SetActive(tips <= 0);
            }

            // Bottom Bar Skill 2 (Freeze)
            bool isFrozen = FreezeManager.Instance != null && FreezeManager.Instance.IsFrozen;
            if (_skill2ActiveTimerRoot != null)
            {
                _skill2ActiveTimerRoot.SetActive(isFrozen);
            }
            if (_skill2CountText != null)
            {
                _skill2CountText.gameObject.SetActive(!isFrozen && freezes > 0);
                _skill2CountText.text = freezes.ToString();
            }
            if (_skill2AdBadge != null)
            {
                _skill2AdBadge.SetActive(!isFrozen && freezes <= 0);
            }
        }

        /// <summary>
        /// Controls individual visibility of the Top and Bottom HUD bars.
        /// </summary>
        public void SetBarsVisible(bool showTop, bool showBottom)
        {
            EnsurePanels();

            if (_topPanelRect != null)
            {
                var topCG = _topPanelRect.GetComponent<CanvasGroup>() ?? _topPanelRect.gameObject.AddComponent<CanvasGroup>();
                topCG.alpha = showTop ? 1f : 0f;
                topCG.interactable = showTop;
                topCG.blocksRaycasts = showTop;
                _topPanelRect.gameObject.SetActive(showTop);
            }

            if (_bottomPanelRect != null)
            {
                var bottomCG = _bottomPanelRect.GetComponent<CanvasGroup>() ?? _bottomPanelRect.gameObject.AddComponent<CanvasGroup>();
                bottomCG.alpha = showBottom ? 1f : 0f;
                bottomCG.interactable = showBottom;
                bottomCG.blocksRaycasts = showBottom;
                _bottomPanelRect.gameObject.SetActive(showBottom);
            }
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

        private void HandleStateChanged(GameState state)
        {
            bool isTutorial = (LevelManager.Instance != null && LevelManager.Instance.CurrentParams.Level <= 1 && (DataManager.Instance == null || !DataManager.Instance.IsTutorialCompleted))
                           || (ArrowSwarm.Tutorial.TutorialManager.Instance != null && ArrowSwarm.Tutorial.TutorialManager.Instance.IsTutorialActive);

            if (isTutorial)
            {
                SetBarsVisible(false, false);
                return;
            }

            // HUD visibility based on state using CanvasGroup
            bool visible = (state == GameState.Playing || state == GameState.Paused);
            SetHUDVisible(visible);
        }

        private void SetHUDVisible(bool visible)
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
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
            int tips = DataManager.Instance?.PlayerData?.tipCount ?? 0;
            if (tips > 0)
            {
                TipManager.Instance?.UseTip();
                UpdateSkillBadges();
            }
            else
            {
                // Request rewarded ad via AdManager
                ArrowSwarm.Ads.AdManager.Instance?.ShowRewardedAd(rewardGranted =>
                {
                    if (rewardGranted)
                    {
                        DataManager.Instance?.ModifyTipCount(1);
                        TipManager.Instance?.UseTip();
                        UpdateSkillBadges();
                    }
                });
            }
        }

        private void OnFreezeClicked()
        {
            int freezes = DataManager.Instance?.PlayerData?.freezeCount ?? 0;
            if (freezes > 0)
            {
                FreezeManager.Instance?.UseFreeze();
                UpdateSkillBadges();
            }
            else
            {
                // Request rewarded ad via AdManager
                ArrowSwarm.Ads.AdManager.Instance?.ShowRewardedAd(rewardGranted =>
                {
                    if (rewardGranted)
                    {
                        DataManager.Instance?.ModifyFreezeCount(1);
                        FreezeManager.Instance?.UseFreeze();
                        UpdateSkillBadges();
                    }
                });
            }
        }

        private void OnDestroy()
        {
            _pauseButton?.onClick.RemoveListener(OnPauseClicked);
            _tipButton?.onClick.RemoveListener(OnTipClicked);
            _skill1TipButton?.onClick.RemoveListener(OnTipClicked);
            _skill2FreezeButton?.onClick.RemoveListener(OnFreezeClicked);
        }
    }
}
