namespace ArrowSwarm.Skills
{
    using System;
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages the Freeze skill: halts all mob movement and pauses mob spawning
    /// for a configurable duration (default 5s) with live tick events and visual effects.
    /// </summary>
    public class FreezeManager : Singleton<FreezeManager>
    {
        [Header("Settings")]
        [Tooltip("Fallback freeze duration if GameConfig is unavailable.")]
        [SerializeField] private float _fallbackDuration = 5.0f;

        private Coroutine _freezeRoutine;
        private bool _isFrozen;
        private float _remainingTime;
        private float _totalDuration;

        /// <summary>Whether enemies and spawning are currently frozen.</summary>
        public bool IsFrozen => _isFrozen;

        /// <summary>Remaining freeze time in seconds.</summary>
        public float RemainingTime => _remainingTime;

        /// <summary>Total duration of the active freeze.</summary>
        public float TotalDuration => _totalDuration;

        /// <summary>Fired when freeze starts (total duration).</summary>
        public static event Action<float> OnFreezeStarted;

        /// <summary>Fired every frame while frozen (remaining, total).</summary>
        public static event Action<float, float> OnFreezeTick;

        /// <summary>Fired when freeze duration completes.</summary>
        public static event Action OnFreezeEnded;

        /// <summary>Fired when user attempts to use freeze with 0 charges.</summary>
        public static event Action OnNoFreezesAvailable;

        protected override void OnSingletonAwake()
        {
            LevelManager.OnLevelReady += HandleLevelReady;
            GameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            LevelManager.OnLevelReady -= HandleLevelReady;
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleLevelReady(LevelParams levelParams)
        {
            EndFreezeSilently();
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state != GameState.Playing && state != GameState.Paused)
            {
                EndFreezeSilently();
            }
        }

        /// <summary>
        /// Attempts to activate the freeze skill.
        /// Consumes 1 charge if available, or fires OnNoFreezesAvailable.
        /// </summary>
        public void UseFreeze()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;

            if (_isFrozen)
            {
                LogDebug("Freeze already active!");
                return;
            }

            PlayerData data = DataManager.Instance?.PlayerData;
            if (data == null) return;

            if (data.freezeCount <= 0)
            {
                OnNoFreezesAvailable?.Invoke();
                LogDebug("No freeze charges available!");
                return;
            }

            // Deduct charge
            DataManager.Instance.ModifyFreezeCount(-1);

            // Determine duration from GameConfig or fallback
            GameConfig config = GameManager.Instance != null ? GameManager.Instance.Config : null;
            float duration = config != null ? config.FreezeDuration : _fallbackDuration;

            StartFreeze(duration);
        }

        /// <summary>
        /// Starts the freeze effect for the specified duration.
        /// </summary>
        public void StartFreeze(float duration)
        {
            if (_freezeRoutine != null) StopCoroutine(_freezeRoutine);

            _isFrozen = true;
            _totalDuration = duration;
            _remainingTime = duration;

            LogDebug($"Freeze activated for {duration:F1}s! Charges remaining: {DataManager.Instance?.FreezeCount}");
            OnFreezeStarted?.Invoke(duration);

            _freezeRoutine = StartCoroutine(FreezeRoutine(duration));
        }

        private IEnumerator FreezeRoutine(float duration)
        {
            _remainingTime = duration;

            while (_remainingTime > 0f)
            {
                if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
                {
                    _remainingTime -= Time.deltaTime;
                    OnFreezeTick?.Invoke(Mathf.Max(0f, _remainingTime), _totalDuration);
                }
                yield return null;
            }

            _remainingTime = 0f;
            _isFrozen = false;
            _freezeRoutine = null;

            LogDebug("Freeze ended!");
            OnFreezeEnded?.Invoke();
        }

        /// <summary>
        /// Immediately cancels any active freeze without firing the OnFreezeEnded event.
        /// </summary>
        public void EndFreezeSilently()
        {
            if (_freezeRoutine != null)
            {
                StopCoroutine(_freezeRoutine);
                _freezeRoutine = null;
            }

            if (_isFrozen)
            {
                _isFrozen = false;
                _remainingTime = 0f;
                OnFreezeEnded?.Invoke();
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] FreezeManager: {message}");
        }
    }
}
