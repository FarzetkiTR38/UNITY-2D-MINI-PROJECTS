using System.Collections;
using UnityEngine;
using NeonGalaxy.Core;
using NeonGalaxy.Data;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Gameplay juice feedback controller. Listens to game events
    /// and applies micro-animations that enhance game feel:
    /// - Board shake on placement
    /// - Score HUD pulse on score change
    /// - Camera zoom punch on Nova Cross
    /// - Slow-motion on game over
    /// </summary>
    public class JuiceFeedbackController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform boardTransform;
        [SerializeField] private Transform scoreHUDTransform;
        [SerializeField] private Transform comboHUDTransform;
        [SerializeField] private Camera mainCamera;

        [Header("Board Shake")]
        [SerializeField] private float boardShakeIntensity = 0.03f;
        [SerializeField] private float boardShakeDuration = 0.1f;

        [Header("Score Pulse")]
        [SerializeField] private float scorePulseAmount = 0.15f;
        [SerializeField] private float scorePulseDuration = 0.2f;

        [Header("Combo Badge")]
        [SerializeField] private float comboPulseAmount = 0.25f;
        [SerializeField] private float comboPulseDuration = 0.3f;

        [Header("Game Over")]
        [SerializeField] private float gameOverSlowMoDuration = 1.0f;
        [SerializeField] private float gameOverSlowMoScale = 0.3f;

        [Header("Nova Cross Zoom")]
        [SerializeField] private float zoomPunchAmount = 0.3f;
        [SerializeField] private float zoomPunchDuration = 0.3f;

        private float _originalCameraSize;
        private Coroutine _zoomCoroutine;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
                _originalCameraSize = mainCamera.orthographicSize;
        }

        private void OnEnable()
        {
            GameEvents.OnPiecePlaced += HandlePiecePlaced;
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnComboUpdated += HandleComboUpdated;
            GameEvents.OnNovaCross += HandleNovaCross;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnNewBestScore += HandleNewBestScore;
        }

        private void OnDisable()
        {
            GameEvents.OnPiecePlaced -= HandlePiecePlaced;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnComboUpdated -= HandleComboUpdated;
            GameEvents.OnNovaCross -= HandleNovaCross;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnNewBestScore -= HandleNewBestScore;
        }

        // ── Event Handlers ──────────────────────────────────────

        private void HandlePiecePlaced(PieceInstance piece, Vector2Int pos)
        {
            // Subtle board shake for tactile feedback
            if (boardTransform != null)
            {
                StartCoroutine(UIAnimator.ShakePosition(boardTransform, boardShakeIntensity, boardShakeDuration));
            }
        }

        private void HandleScoreChanged(int newScore)
        {
            // Pulse the score HUD element
            if (scoreHUDTransform != null)
            {
                StartCoroutine(UIAnimator.PunchScale(scoreHUDTransform, scorePulseAmount, scorePulseDuration));
            }
        }

        private void HandleComboUpdated(int comboLevel)
        {
            if (comboLevel < 1) return;

            // Pulse the combo badge with escalating intensity
            if (comboHUDTransform != null)
            {
                float escalation = Mathf.Min(comboLevel * 0.05f, 0.3f);
                StartCoroutine(UIAnimator.PunchScale(comboHUDTransform, comboPulseAmount + escalation, comboPulseDuration));
            }
        }

        private void HandleNovaCross()
        {
            // Camera zoom punch for premium feel
            if (mainCamera != null)
            {
                if (_zoomCoroutine != null)
                    StopCoroutine(_zoomCoroutine);

                _zoomCoroutine = StartCoroutine(ZoomPunchRoutine());
            }
        }

        private void HandleGameOver(int finalScore)
        {
            // Dramatic slow-motion effect
            StartCoroutine(GameOverSlowMotionRoutine());
        }

        private void HandleNewBestScore(int newBest)
        {
            // Extra celebration pulse on score HUD
            if (scoreHUDTransform != null)
            {
                StartCoroutine(UIAnimator.PunchScale(scoreHUDTransform, 0.4f, 0.4f));
            }
        }

        // ── Animation Routines ──────────────────────────────────

        private IEnumerator ZoomPunchRoutine()
        {
            float targetSize = _originalCameraSize - zoomPunchAmount;
            float half = zoomPunchDuration * 0.5f;

            // Zoom in
            float elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = UIAnimator.EaseOutCubic(Mathf.Clamp01(elapsed / half));
                mainCamera.orthographicSize = Mathf.Lerp(_originalCameraSize, targetSize, t);
                yield return null;
            }

            // Zoom back
            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = UIAnimator.EaseInOutQuad(Mathf.Clamp01(elapsed / half));
                mainCamera.orthographicSize = Mathf.Lerp(targetSize, _originalCameraSize, t);
                yield return null;
            }

            mainCamera.orthographicSize = _originalCameraSize;
            _zoomCoroutine = null;
        }

        private IEnumerator GameOverSlowMotionRoutine()
        {
            Time.timeScale = gameOverSlowMoScale;
            yield return new WaitForSecondsRealtime(gameOverSlowMoDuration);
            Time.timeScale = 1f;
        }
    }
}
