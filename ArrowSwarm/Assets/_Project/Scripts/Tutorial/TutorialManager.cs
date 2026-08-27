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

            // Hide HUD bars for clean immersion during tutorial
            FindFirstObjectByType<ArrowSwarm.UI.GameHUD>()?.SetBarsVisible(false, false);

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
            string actionTag;

            if (remainingArrows == 1 || targetArrow.IsRainbow)
            {
                instruction = GetLocalizedText(
                    "Son <color=#FF66B2>Rainbow</color> oku ateşle ve <color=#FFE066>kazan</color>!",
                    "Fire the final <color=#FF66B2>Rainbow</color> arrow to <color=#FFE066>win</color>!"
                );
                actionTag = "<color=#FF66B2>RAINBOW!</color>";
            }
            else
            {
                switch (step)
                {
                    case 1:
                        instruction = GetLocalizedText(
                            "Düşmanları vurmak için <color=#FFE066>serbest oka</color> dokun!",
                            "Tap the <color=#FFE066>clear arrow</color> to hit enemies!"
                        );
                        actionTag = GetLocalizedText("DOKUN!", "TAP!");
                        break;
                    case 2:
                        instruction = GetLocalizedText(
                            "Harika! Şimdi <color=#66E0FF>yolu açılan</color> oka dokun!",
                            "Great! Now tap the <color=#66E0FF>unblocked</color> arrow!"
                        );
                        actionTag = GetLocalizedText("YOLU AÇ!", "FIRE!");
                        break;
                    case 3:
                        instruction = GetLocalizedText(
                            "Çok iyi! <color=#FFE066>Zincirleme</color> okları temizlemeye devam et!",
                            "Awesome! Keep <color=#FFE066>chaining</color> clear arrows!"
                        );
                        actionTag = GetLocalizedText("ZİNCİRLE!", "CHAIN!");
                        break;
                    case 4:
                        instruction = GetLocalizedText(
                            "Düşmanları durdur, <color=#66E0FF>okları ateşle</color>!",
                            "Stop enemies, <color=#66E0FF>keep firing arrows</color>!"
                        );
                        actionTag = GetLocalizedText("ATEŞLE!", "SHOOT!");
                        break;
                    case 5:
                        instruction = GetLocalizedText(
                            "Neredeyse bitti! <color=#FFE066>Kalan okları</color> temizle!",
                            "Almost there! Clear the <color=#FFE066>remaining arrows</color>!"
                        );
                        actionTag = GetLocalizedText("AZ KALDI!", "ALMOST!");
                        break;
                    default:
                        instruction = GetLocalizedText(
                            "Tüm okları temizle ve <color=#FFE066>seviyeyi kazan</color>!",
                            "Clear all arrows to <color=#FFE066>win the level</color>!"
                        );
                        actionTag = GetLocalizedText("TEMİZLE!", "CLEAR!");
                        break;
                }
            }

            _overlayUI?.SetInstruction(instruction);
            _handUI?.SetActionTag(actionTag);
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
                _handUI.PointToWorldPosition(arrow.transform, _parentCanvas);
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

            // Freeze the hand at the launch site so it doesn't fly with the projectile
            _handUI?.FreezeCurrentWorldPosition();

            // Advance step smoothly with glide to the next arrow
            if (_stepTransitionRoutine != null) StopCoroutine(_stepTransitionRoutine);
            _stepTransitionRoutine = StartCoroutine(AdvanceStepDelayed(_currentStep + 1, 0.28f));
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

            // Restore HUD bars for level complete and next levels
            FindFirstObjectByType<ArrowSwarm.UI.GameHUD>()?.SetBarsVisible(true, true);

            // Spawn celebration fireworks barrage
            ArrowSwarm.Effects.ParticleManager.Instance?.SpawnFireworksCelebration();

            string title = GetLocalizedText("<color=#FFE066>TEBRİKLER!</color>\nTUTORİAL'I TAMAMLADIN", "<color=#FFE066>CONGRATULATIONS!</color>\nTUTORIAL COMPLETED!");
            string subtitle = GetLocalizedText(
                "Tüm temel kuralları öğrendin!\nŞimdi büyük maceraya başla.",
                "You mastered all the basics!\nNow start your grand adventure."
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
        /// Finishes the tutorial and transitions back to MainMenuScene with Iris Circle Wipe.
        /// </summary>
        public void CompleteAndReturnToMenu()
        {
            _isTutorialActive = false;
            _handUI?.HideImmediately();
            _overlayUI?.Hide();
            FindFirstObjectByType<ArrowSwarm.UI.GameHUD>()?.SetBarsVisible(true, true);
            Time.timeScale = 1f;

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene("MainMenuScene");
            }
            else if (GameManager.Instance != null)
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
            FindFirstObjectByType<ArrowSwarm.UI.GameHUD>()?.SetBarsVisible(true, true);
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
