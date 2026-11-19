namespace ArrowSwarm.Tutorial
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Saves the tutorial outcome and handles the player’s next destination.</summary>
    public partial class TutorialManager
    {
        private const int TutorialLevel = 1;
        private const int FirstRegularLevel = TutorialLevel + 1;

        private void HandleLevelWon()
        {
            if (!_isTutorialActive || _awaitingCompletionChoice) return;
            _awaitingCompletionChoice = true;
            if (_stepTransitionRoutine != null) StopCoroutine(_stepTransitionRoutine);
            _handUI?.HideImmediately();
            FindFirstObjectByType<ArrowSwarm.UI.GameHUD>()?.SetBarsVisible(true, true);
            SaveTutorialProgress(GameManager.Instance.CurrentLives);
            _overlayUI?.ShowCompletionCard();
        }

        /// <summary>
        /// Skips the tutorial immediately.
        /// </summary>
        public void SkipTutorial()
        {
            if (!_isTutorialActive || _awaitingCompletionChoice) return;
            SaveTutorialProgress(0);
            CompleteAndReturnToMenu();
        }

        /// <summary>Loads the second Defense level after a completed tutorial.</summary>
        public void ContinueToNextLevel()
        {
            if (!_awaitingCompletionChoice) return;
            var data = DataManager.Instance;
            if (data != null && data.PlayerData.currentLevel != FirstRegularLevel)
                data.SetCurrentLevel(FirstRegularLevel);
            EndTutorialSilently();
            GameManager.Instance.RestartLevel();
        }

        private static void SaveTutorialProgress(int stars)
        {
            var data = DataManager.Instance;
            if (data == null) return;
            if (stars > 0) data.SetLevelStars(TutorialLevel, stars);
            if (data.PlayerData.currentLevel < FirstRegularLevel) data.SetCurrentLevel(FirstRegularLevel);
            data.SetTutorialCompleted(true);
        }

        /// <summary>
        /// Finishes the tutorial and transitions back to MainMenuScene with Iris Circle Wipe.
        /// </summary>
        public void CompleteAndReturnToMenu()
        {
            if (!_isTutorialActive) return;
            EndTutorialSilently();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMainMenu();
            }
            else
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenuScene");
            }
        }

        /// <summary>
        /// Silently terminates the tutorial overlay and hand guidance.
        /// </summary>
        public void EndTutorialSilently()
        {
            _isTutorialActive = false;
            _awaitingCompletionChoice = false;
            _handUI?.HideImmediately();
            _overlayUI?.Hide();
            FindFirstObjectByType<ArrowSwarm.UI.GameHUD>()?.SetBarsVisible(true, true);
            gameObject.SetActive(false);
        }
    }
}
