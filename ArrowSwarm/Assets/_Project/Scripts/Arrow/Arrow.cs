namespace ArrowSwarm.Arrow
{
    using System;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using UnityEngine;

    /// <summary>
    /// Main arrow component. Holds arrow data (direction, weight, grid position),
    /// handles click detection, and coordinates with movement and visuals.
    /// </summary>
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private ArrowDirection _direction;
        [SerializeField] private int _weight = 1;
        [SerializeField] private Vector2Int _gridPosition;
        [SerializeField] private bool _isFired;
        [SerializeField] private bool _isRainbow;

        private ArrowMovement _movement;
        private ArrowVisuals _visuals;

        /// <summary>Direction this arrow faces.</summary>
        public ArrowDirection Direction => _direction;

        /// <summary>Damage value of this arrow.</summary>
        public int Weight => _weight;

        /// <summary>Grid position (col, row).</summary>
        public Vector2Int GridPosition => _gridPosition;

        /// <summary>Whether this arrow has been fired.</summary>
        public bool IsFired => _isFired;

        /// <summary>Whether this is the rainbow (last) arrow.</summary>
        public bool IsRainbow => _isRainbow;

        // --- Events ---
        /// <summary>Fired when any arrow is clicked (arrow instance, was successful).</summary>
        public static event Action<Arrow, bool> OnArrowClicked;

        /// <summary>Fired when an arrow is successfully fired.</summary>
        public static event Action<Arrow> OnArrowFiredEvent;

        /// <summary>Fired when an arrow finishes its path and is done.</summary>
        public static event Action<Arrow> OnArrowCompleted;

        /// <summary>
        /// Initializes the arrow with placement data.
        /// Called by ArrowSpawner when placing arrows on the grid.
        /// </summary>
        public void Initialize(Vector2Int gridPos, ArrowDirection direction, int weight, bool isRainbow = false)
        {
            _gridPosition = gridPos;
            _direction = direction;
            _weight = weight;
            _isFired = false;
            _isRainbow = isRainbow;

            _movement = GetComponent<ArrowMovement>();
            _visuals = GetComponent<ArrowVisuals>();

            _visuals?.SetupVisuals(direction, weight, isRainbow);

            LogDebug($"Arrow initialized at {gridPos}, Dir={direction}, W={weight}, Rainbow={isRainbow}");
        }

        /// <summary>
        /// Called when the player taps/clicks this arrow.
        /// Checks if the path is clear and either fires or penalizes.
        /// </summary>
        public void OnPlayerClick()
        {
            if (_isFired) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            bool pathClear = GridManager.Instance.IsPathClear(_gridPosition, _direction);

            if (pathClear)
            {
                Fire();
                OnArrowClicked?.Invoke(this, true);
            }
            else
            {
                // Path blocked — wrong click
                OnArrowClicked?.Invoke(this, false);
                GameManager.Instance.HandleWrongClick();
                _visuals?.PlayBlockedEffect();
                LogDebug($"Arrow at {_gridPosition} BLOCKED (Dir={_direction})");
            }
        }

        /// <summary>
        /// Fires the arrow — removes from grid and starts movement.
        /// </summary>
        private void Fire()
        {
            _isFired = true;

            // Remove from grid
            GridManager.Instance.RemoveArrow(_gridPosition);

            // Calculate exit point and start movement
            Vector2 exitPoint = GridManager.Instance.GetGridExitPoint(_gridPosition, _direction);
            _movement?.StartMovement(_direction, exitPoint);

            // Update visuals
            _visuals?.PlayFireEffect();

            OnArrowFiredEvent?.Invoke(this);
            GameManager.Instance.HandleArrowFired();

            LogDebug($"Arrow FIRED at {_gridPosition}, Dir={_direction}, W={_weight}");
        }

        /// <summary>
        /// Called by ArrowMovement when the arrow completes its path.
        /// </summary>
        public void OnPathComplete()
        {
            OnArrowCompleted?.Invoke(this);
            LogDebug($"Arrow completed path. W={_weight}");
        }

        /// <summary>
        /// Gets the effective damage of this arrow.
        /// Rainbow arrows deal 999 damage.
        /// </summary>
        public int GetDamage()
        {
            if (_isRainbow)
            {
                return GameManager.Instance?.Config?.RainbowArrowDamage ?? 999;
            }
            return _weight;
        }

        /// <summary>
        /// Resets the arrow for object pool reuse.
        /// </summary>
        public void ResetArrow()
        {
            _isFired = false;
            _isRainbow = false;
            _weight = 1;
            _gridPosition = Vector2Int.zero;
            _visuals?.ResetVisuals();
        }

        private void OnMouseDown()
        {
            OnPlayerClick();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] Arrow: {message}");
        }
    }
}
