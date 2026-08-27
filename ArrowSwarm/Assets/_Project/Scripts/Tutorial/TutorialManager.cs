namespace ArrowSwarm.Tutorial
{
    using System;
    using System.Collections;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Grid;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Coordinates the interactive step-by-step tutorial on Level 1.
    /// Dynamically finds free unblocked arrows on the board, guides the player with
    /// the animated hand cursor, and tracks multi-step progression to victory.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("--- UI References ---")]
        [SerializeField] private TutorialHandUI _handUI;
        [SerializeField] private TutorialOverlayUI _overlayUI;
        [SerializeField] private Canvas _parentCanvas;

        private int _currentStep = 0;
        private bool _isTutorialActive = false;
        private Vector2Int _currentTargetGridPos;
        private Coroutine _stepTransitionRoutine;

        /// <summary>Whether the interactive tutorial is currently running.</summary>
        public bool IsTutorialActive => _isTutorialActive;

        /// <summary>Current target grid position for tutorial click restriction.</summary>
        public Vector2Int CurrentTargetGridPos => _currentTargetGridPos;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            LevelManager.OnLevelReady += HandleLevelReady;
            Arrow.OnArrowFiredEvent += HandleArrowFired;
            GameManager.OnLevelWon += HandleLevelWon;
        }

        private void OnDisable()
        {
            LevelManager.OnLevelReady -= HandleLevelReady;
            Arrow.OnArrowFiredEvent -= HandleArrowFired;
            GameManager.OnLevelWon -= HandleLevelWon;
        }

        private void HandleLevelReady(LevelParams levelParams)
        {
            var mapCtrl = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.Core.MapSceneController>();
            bool isMapTestTutorial = mapCtrl != null && mapCtrl.EnableTutorialTest;

            if (isMapTestTutorial || (levelParams.Level <= 1 && (DataManager.Instance == null || !DataManager.Instance.IsTutorialCompleted)))
            {
                StartTutorial();
            }
            else
            {
                EndTutorialSilently();
            }
        }

        /// <summary>
        /// Starts the interactive tutorial sequence on Level 1.
        /// </summary>
        public void StartTutorial()
        {
            _isTutorialActive = true;
            _currentStep = 0;

            EnsureCanvas();
            if (_overlayUI != null) _overlayUI.Show();

            // Advance to step 1
            AdvanceToStep(1);
        }

        private void AdvanceToStep(int step)
        {
            _currentStep = step;
            LogDebug($"Advancing to Tutorial Step {step}");

            Arrow targetArrow = FindBestUnblockedArrow();
            if (targetArrow == null)
            {
                LogDebug("No unblocked arrow found for tutorial step!");
                _handUI?.Hide();
                return;
            }

            _currentTargetGridPos = targetArrow.HeadPoint;

            int remainingArrows = ArrowSpawner.Instance != null ? ArrowSpawner.Instance.RemainingArrows : 1;
            string instruction;

            if (remainingArrows == 1 || targetArrow.IsRainbow)
            {
                instruction = GetLocalizedText(
                    "Son Rainbow oku ateşle ve tüm düşmanları yok et!",
                    "Fire the final Rainbow arrow to wipe out all enemies!"
                );
            }
            else
            {
                switch (step)
                {
                    case 1:
                        instruction = GetLocalizedText(
                            "Düşmanları vurmak için serbest oka dokun!",
                            "Tap the clear arrow to fire and hit enemies!"
                        );
                        break;
                    case 2:
                        instruction = GetLocalizedText(
                            "Harika! Şimdi yolu açılan bir sonraki oka dokun!",
                            "Great! Now tap the next unblocked arrow!"
                        );
                        break;
                    case 3:
                        instruction = GetLocalizedText(
                            "Çok iyi! Zincirleme olarak okları temizlemeye devam et!",
                            "Awesome! Keep clearing the unblocked arrows!"
                        );
                        break;
                    case 4:
                        instruction = GetLocalizedText(
                            "Düşmanların geçmesine izin verme, okları ateşle!",
                            "Don't let enemies pass, keep firing arrows!"
                        );
                        break;
                    case 5:
                        instruction = GetLocalizedText(
                            "Neredeyse bitti! Kalan okları serbest bırak!",
                            "Almost there! Clear the remaining arrows!"
                        );
                        break;
                    default:
                        instruction = GetLocalizedText(
                            "Tüm okları temizle ve seviyeyi kazan!",
                            "Clear all arrows to win the level!"
                        );
                        break;
                }
            }

            _overlayUI?.SetInstruction(instruction);
            PointHandAtArrow(targetArrow);
        }

        private Arrow FindBestUnblockedArrow()
        {
            if (ArrowSpawner.Instance == null || GridManager.Instance == null) return null;

            var activeArrows = ArrowSpawner.Instance.ActiveArrows;
            if (activeArrows == null || activeArrows.Count == 0) return null;

            // Prioritize clear arrows
            for (int i = 0; i < activeArrows.Count; i++)
            {
                var arrow = activeArrows[i];
                if (arrow != null && !arrow.IsFired && !arrow.IsBlockedAnimating)
                {
                    if (GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection))
                    {
                        return arrow;
                    }
                }
            }

            // Fallback to any unfired arrow
            for (int i = 0; i < activeArrows.Count; i++)
            {
                var arrow = activeArrows[i];
                if (arrow != null && !arrow.IsFired) return arrow;
            }

            return null;
        }

        private void PointHandAtArrow(Arrow arrow)
        {
            if (arrow == null) return;
            EnsureCanvas();

            if (_handUI == null)
            {
                _handUI = GetComponentInChildren<TutorialHandUI>(true);
            }

            if (_handUI != null && _parentCanvas != null)
            {
                _handUI.PointToWorldPosition(arrow.transform.position, _parentCanvas);
            }
        }

        private void EnsureCanvas()
        {
            if (_parentCanvas == null)
            {
                _parentCanvas = GetComponentInParent<Canvas>();
                if (_parentCanvas == null)
                {
                    var hud = FindFirstObjectByType<ArrowSwarm.UI.GameHUD>();
                    if (hud != null) _parentCanvas = hud.GetComponent<Canvas>();
                }
            }
        }

        private void HandleArrowFired(Arrow arrow)
        {
            if (!_isTutorialActive) return;

            _handUI?.Hide();

            if (_stepTransitionRoutine != null) StopCoroutine(_stepTransitionRoutine);
            _stepTransitionRoutine = StartCoroutine(AdvanceStepDelayed(_currentStep + 1, 0.45f));
        }

        private IEnumerator AdvanceStepDelayed(int nextStep, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (_isTutorialActive)
            {
                AdvanceToStep(nextStep);
            }
        }

        private void HandleLevelWon()
        {
            if (!_isTutorialActive) return;

            if (_stepTransitionRoutine != null) StopCoroutine(_stepTransitionRoutine);
            _handUI?.HideImmediately();

            string title = GetLocalizedText("EĞİTİM TAMAMLANDI!", "TUTORIAL COMPLETED!");
            string subtitle = GetLocalizedText(
                "Artık savaşa hazırsın! Ana Menüye dön ve maceraya başla.",
                "You are ready to play! Return to Main Menu to start your adventure."
            );

            _overlayUI?.ShowCompletionCard(title, subtitle);

            // Persist tutorial completion and advance to Level 2
            if (DataManager.Instance != null)
            {
                DataManager.Instance.SetTutorialCompleted(true);
                DataManager.Instance.SetLevelStars(1, 3);
                DataManager.Instance.UnlockNextLevel(1);
                DataManager.Instance.SetCurrentLevel(2);
            }
        }

        /// <summary>
        /// Skips the tutorial immediately.
        /// </summary>
        public void SkipTutorial()
        {
            if (DataManager.Instance != null)
            {
                DataManager.Instance.SetTutorialCompleted(true);
                DataManager.Instance.SetLevelStars(1, 3);
                DataManager.Instance.UnlockNextLevel(1);
                DataManager.Instance.SetCurrentLevel(2);
            }
            CompleteAndReturnToMenu();
        }

        /// <summary>
        /// Finishes the tutorial and transitions back to MainMenuScene.
        /// </summary>
        public void CompleteAndReturnToMenu()
        {
            _isTutorialActive = false;
            _handUI?.HideImmediately();
            _overlayUI?.Hide();
            Time.timeScale = 1f;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMainMenu();
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }

        /// <summary>
        /// Silently terminates the tutorial overlay and hand guidance.
        /// </summary>
        public void EndTutorialSilently()
        {
            _isTutorialActive = false;
            _handUI?.HideImmediately();
            _overlayUI?.Hide();
        }

        private string GetLocalizedText(string tr, string en)
        {
            string lang = DataManager.Instance?.PlayerData?.selectedLanguage ?? "ENGLISH";
            return lang.Equals("TURKISH", StringComparison.OrdinalIgnoreCase) ? tr : en;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string msg)
        {
            UnityEngine.Debug.Log($"[ArrowSwarm] TutorialManager: {msg}");
        }
    }
}
