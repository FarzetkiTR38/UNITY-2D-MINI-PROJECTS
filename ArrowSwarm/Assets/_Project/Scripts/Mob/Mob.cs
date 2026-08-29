namespace ArrowSwarm.Mob
{
    using System;
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>
    /// Main mob component that coordinates MobHealth, MobMovement, and MobVisuals.
    /// Handles initialization, scaling, damage, death, and pool recycling.
    /// </summary>
    public class Mob : MonoBehaviour
    {
        [SerializeField] private int _mobId;

        private MobHealth _health;
        private MobMovement _movement;
        private MobVisuals _visuals;

        /// <summary>Unique ID for this mob instance.</summary>
        public int MobId => _mobId;

        /// <summary>Whether this mob is alive.</summary>
        public bool IsAlive => _health != null && _health.IsAlive;

        /// <summary>Movement component of this mob.</summary>
        public MobMovement Movement => _movement;

        /// <summary>Health component of this mob.</summary>
        public MobHealth Health => _health;

        /// <summary>Visuals component of this mob.</summary>
        public MobVisuals Visuals => _visuals;

        // --- Static Events ---
        /// <summary>Fired when any mob is killed (mob instance).</summary>
        public static event Action<Mob> OnMobKilled;

        /// <summary>Fired when any mob reaches the finish (mob instance).</summary>
        public static event Action<Mob> OnMobFinished;

        private void EnsureComponents()
        {
            if (_health == null) _health = GetComponent<MobHealth>() ?? gameObject.AddComponent<MobHealth>();
            if (_movement == null) _movement = GetComponent<MobMovement>() ?? gameObject.AddComponent<MobMovement>();
            if (_visuals == null) _visuals = GetComponent<MobVisuals>() ?? gameObject.AddComponent<MobVisuals>();
        }

        private void Awake()
        {
            EnsureComponents();
        }

        /// <summary>
        /// Initializes the mob with given parameters and scale factor.
        /// </summary>
        public void Initialize(int id, int hp, float speed, float scaleFactor = 1.0f)
        {
            _mobId = id;
            EnsureComponents();

            transform.localScale = Vector3.one * scaleFactor;

            _health?.Initialize(hp);
            _visuals?.Initialize(hp);
            _movement?.StartMoving(speed);

            // Subscribe to events
            if (_health != null) _health.OnDeath += HandleDeath;
            if (_movement != null)
            {
                _movement.OnFinishReached += HandleFinishReached;
                var pf = _movement.GetComponent<Path.PathFollower>();
                if (pf != null) pf.OnDirectionChanged += HandleDirectionChanged;
            }

            LogDebug($"Mob #{id} initialized. HP={hp}, Speed={speed}, Scale={scaleFactor:F2}x");
        }

        /// <summary>
        /// Applies damage to this mob from an arrow hit.
        /// </summary>
        public void TakeDamage(int damage)
        {
            _health?.ApplyDamage(damage);
        }

        /// <summary>
        /// Freezes or unfreezes this mob (movement and visual icy tint).
        /// </summary>
        public void SetFrozen(bool frozen)
        {
            _movement?.SetFrozen(frozen);
            _visuals?.SetFrozen(frozen);
        }

        /// <summary>
        /// Resets the mob for object pool reuse.
        /// </summary>
        public void ResetMob()
        {
            // Unsubscribe from events
            if (_health != null) _health.OnDeath -= HandleDeath;
            if (_movement != null) _movement.OnFinishReached -= HandleFinishReached;

            var pathFollower = _movement?.GetComponent<Path.PathFollower>();
            if (pathFollower != null) pathFollower.OnDirectionChanged -= HandleDirectionChanged;

            _health?.ResetHealth();
            _movement?.ResetMovement();
            _visuals?.ResetVisuals();
            transform.localScale = Vector3.one;
        }

        private void HandleDeath()
        {
            _movement?.StopMoving();
            _visuals?.PlayDeathEffect();
            OnMobKilled?.Invoke(this);
            LogDebug($"Mob #{_mobId} killed!");
        }

        private void HandleFinishReached()
        {
            OnMobFinished?.Invoke(this);
            GameManager.Instance?.HandleMobReachedFinish();
            LogDebug($"Mob #{_mobId} reached finish!");
        }

        private void HandleDirectionChanged(Vector2 direction)
        {
            _visuals?.UpdateFacingDirection(direction);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] Mob: {message}");
        }
    }
}
