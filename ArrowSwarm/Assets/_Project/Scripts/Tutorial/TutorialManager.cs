namespace ArrowSwarm.Tutorial
{
    using System.Collections;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Localization;
    using UnityEngine;

    /// <summary>
    /// Coordinates the interactive step-by-step tutorial on Level 1.
    /// Dynamically finds free unblocked arrows on the board, guides the player with
    /// the animated hand cursor, and tracks multi-step progression to victory.
    /// </summary>
    public partial class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("--- UI References ---")]
        [SerializeField] private TutorialHandUI _handUI;
        [SerializeField] private TutorialOverlayUI _overlayUI;
        [SerializeField] private Canvas _parentCanvas;

        private int _currentStep = 0;
        private bool _isTutorialActive = false;
        private bool _awaitingCompletionChoice;
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

        private void OnEnable()
        {
            Instance = this;
            Arrow.OnArrowFiredEvent += HandleArrowFired;
            GameManager.OnLevelWon += HandleLevelWon;
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
            Arrow.OnArrowFiredEvent -= HandleArrowFired;
            GameManager.OnLevelWon -= HandleLevelWon;
        }

        /// <summary>
        /// Starts the interactive tutorial sequence on Level 1.
        /// </summary>
        public void StartTutorial()
        {
            _isTutorialActive = true;
            _awaitingCompletionChoice = false;
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

            var localization = LocalizationManager.Instance;
            string instruction = targetArrow.IsRainbow || ArrowSpawner.Instance.RemainingArrows == 1
                ? localization.GetText("tutorial_rainbow", "Fire the final rainbow arrow!")
                : step == 1
                    ? localization.GetText("tutorial_welcome", "Tap the unblocked arrow to shoot it!")
                    : localization.GetText("tutorial_path_guide", "Arrows hit enemies along the track!");
            _overlayUI?.SetInstruction(instruction);
            _handUI?.SetActionTag(localization.GetText("tutorial_tap", "TAP!"));
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
            if (!_isTutorialActive || _awaitingCompletionChoice) return;

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

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string msg)
        {
            UnityEngine.Debug.Log($"[ArrowSwarm] TutorialManager: {msg}");
        }
    }
}
