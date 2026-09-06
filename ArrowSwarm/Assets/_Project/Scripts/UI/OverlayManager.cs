namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>
    /// Coordinates all popup overlays on Canvas_Overlay (Win, Lose, Pause, Tips, Settings).
    /// Ensures that even if popup GameObjects are disabled in the hierarchy by default,
    /// they are automatically activated and displayed when the appropriate game event occurs.
    /// </summary>
    [DisallowMultipleComponent]
    public class OverlayManager : MonoBehaviour
    {
        [Header("Popup References (Optional - Auto-found if null)")]
        [SerializeField] private LevelCompleteUI _winPanel;
        [SerializeField] private GameOverUI _losePanel;
        [SerializeField] private PauseMenuUI _pausePanel;
        [SerializeField] private TipPopupUI _tipPopup;
        [SerializeField] private FreezePopupUI _freezePopup;

        private void Awake()
        {
            AutoWirePanels();
        }

        private void OnEnable()
        {
            GameManager.OnLevelWon += HandleLevelWon;
            GameManager.OnLevelLost += HandleLevelLost;
            GameManager.OnGameStateChanged += HandleGameStateChanged;
            ArrowSwarm.Tips.TipManager.OnNoTipsAvailable += HandleNoTipsAvailable;
            ArrowSwarm.Skills.FreezeManager.OnNoFreezesAvailable += HandleNoFreezesAvailable;
        }

        private void OnDisable()
        {
            GameManager.OnLevelWon -= HandleLevelWon;
            GameManager.OnLevelLost -= HandleLevelLost;
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
            ArrowSwarm.Tips.TipManager.OnNoTipsAvailable -= HandleNoTipsAvailable;
            ArrowSwarm.Skills.FreezeManager.OnNoFreezesAvailable -= HandleNoFreezesAvailable;
        }

        /// <summary>
        /// Automatically discovers and caches references to all popup panels even if they are inactive.
        /// </summary>
        public void AutoWirePanels()
        {
            if (_winPanel == null) _winPanel = GetComponentInChildren<LevelCompleteUI>(true) ?? FindFirstObjectByType<LevelCompleteUI>(FindObjectsInactive.Include);
            if (_losePanel == null) _losePanel = GetComponentInChildren<GameOverUI>(true) ?? FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
            if (_pausePanel == null) _pausePanel = GetComponentInChildren<PauseMenuUI>(true) ?? FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
            if (_tipPopup == null) _tipPopup = GetComponentInChildren<TipPopupUI>(true) ?? FindFirstObjectByType<TipPopupUI>(FindObjectsInactive.Include);
            if (_freezePopup == null) _freezePopup = GetComponentInChildren<FreezePopupUI>(true) ?? FindFirstObjectByType<FreezePopupUI>(FindObjectsInactive.Include);
        }

        /// <summary>Activates and displays the Win / Level Complete panel.</summary>
        public void ShowWinPanel()
        {
            AutoWirePanels();
            if (_winPanel != null)
            {
                _winPanel.gameObject.SetActive(true);
                _winPanel.Show();
            }
        }

        /// <summary>Activates and displays the Game Over / Lose panel.</summary>
        public void ShowLosePanel()
        {
            AutoWirePanels();
            if (_losePanel != null)
            {
                _losePanel.gameObject.SetActive(true);
                _losePanel.Show();
            }
        }

        /// <summary>Activates and displays the Pause menu.</summary>
        public void ShowPausePanel()
        {
            AutoWirePanels();
            if (_pausePanel != null)
            {
                _pausePanel.gameObject.SetActive(true);
                _pausePanel.Show();
            }
        }

        /// <summary>Activates and displays the Tip popup.</summary>
        public void ShowTipPopup()
        {
            AutoWirePanels();
            if (_tipPopup != null)
            {
                _tipPopup.gameObject.SetActive(true);
                _tipPopup.Show();
            }
        }

        /// <summary>Activates and displays the Freeze popup.</summary>
        public void ShowFreezePopup()
        {
            AutoWirePanels();
            if (_freezePopup != null)
            {
                _freezePopup.gameObject.SetActive(true);
                _freezePopup.Show();
            }
        }

        private void HandleLevelWon()
        {
            ShowWinPanel();
        }

        private void HandleLevelLost()
        {
            ShowLosePanel();
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Paused)
            {
                ShowPausePanel();
            }
        }

        private void HandleNoTipsAvailable()
        {
            ShowTipPopup();
        }

        private void HandleNoFreezesAvailable()
        {
            ShowFreezePopup();
        }
    }
}
