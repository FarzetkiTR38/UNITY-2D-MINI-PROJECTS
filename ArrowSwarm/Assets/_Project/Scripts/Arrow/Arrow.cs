namespace ArrowSwarm.Arrow
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using UnityEngine;

    /// <summary>
    /// Main arrow component. Holds multi-point path data (direction, weight),
    /// handles click detection with edge-based rules, and coordinates
    /// with movement and visuals. Weight = number of path segments.
    /// </summary>
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private ArrowDirection _headDirection;
        [SerializeField] private List<Vector2Int> _pathPoints = new List<Vector2Int>();
        [SerializeField] private bool _isFired;
        [SerializeField] private bool _isRainbow;

        private ArrowMovement _movement;
        private ArrowVisuals _visuals;
        private bool _isBlockedAnimating;

        /// <summary>Direction the arrow head faces.</summary>
        public ArrowDirection HeadDirection => _headDirection;

        /// <summary>All points this arrow occupies on the grid.</summary>
        public IReadOnlyList<Vector2Int> PathPoints => _pathPoints;

        /// <summary>
        /// The arrow head point (first point in path).
        /// This is where the arrow tip is and the direction it faces.
        /// </summary>
        public Vector2Int HeadPoint => _pathPoints.Count > 0 ? _pathPoints[0] : Vector2Int.zero;

        /// <summary>The arrow tail point (last point in path).</summary>
        public Vector2Int TailPoint => _pathPoints.Count > 0 ? _pathPoints[_pathPoints.Count - 1] : Vector2Int.zero;

        /// <summary>
        /// Weight (damage) of this arrow = number of segments = pathPoints.Count - 1.
        /// Minimum 1.
        /// </summary>
        public int Weight => Mathf.Max(1, _pathPoints.Count - 1);

        /// <summary>Whether this arrow has been fired.</summary>
        public bool IsFired => _isFired;

        /// <summary>Whether this arrow is currently performing its blocked bounce animation.</summary>
        public bool IsBlockedAnimating => _isBlockedAnimating;

        /// <summary>Whether this is the rainbow (last) arrow.</summary>
        public bool IsRainbow => _isRainbow;

        // --- Events ---
        /// <summary>Fired when any arrow is clicked (arrow instance, was successful).</summary>
        public static event Action<Arrow, bool> OnArrowClicked;

        /// <summary>Fired when an arrow is successfully fired.</summary>
        public static event Action<Arrow> OnArrowFiredEvent;

        /// <summary>Fired when an arrow finishes its movement and is done.</summary>
        public static event Action<Arrow> OnArrowCompleted;

        /// <summary>
        /// Initializes the arrow with multi-point path data.
        /// Called by ArrowSpawner when placing arrows on the grid.
        /// </summary>
        /// <param name="pathPoints">Ordered list of grid points. First = head, last = tail.</param>
        /// <param name="headDirection">Direction the arrow head faces (for click/fire direction).</param>
        /// <param name="isRainbow">Whether this arrow is the rainbow (last) arrow.</param>
        public void Initialize(List<Vector2Int> pathPoints, ArrowDirection headDirection, bool isRainbow = false)
        {
            _pathPoints.Clear();
            _pathPoints.AddRange(pathPoints);
            _headDirection = headDirection;
            _isFired = false;
            _isBlockedAnimating = false;
            _isRainbow = isRainbow;

            _movement = GetComponent<ArrowMovement>();
            _visuals = GetComponent<ArrowVisuals>();

            _visuals?.SetupVisuals(this);

            LogDebug($"Arrow initialized: Head={HeadPoint}, Dir={headDirection}, W={Weight}, Rainbow={isRainbow}, Points={_pathPoints.Count}");
        }

        /// <summary>
        /// Called when the player taps/clicks this arrow.
        /// Arrow fires if its path is clear; otherwise slithers forward, impacts the obstacle, and bounces back.
        /// </summary>
        public void OnPlayerClick()
        {
            if (_isFired || _isBlockedAnimating) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            bool canFire = GridManager.Instance.IsPathClear(HeadPoint, _headDirection);

            if (canFire)
            {
                Fire();
                OnArrowClicked?.Invoke(this, true);
            }
            else
            {
                // Wrong click: slither forward, impact obstacle, shake/flash red, and slither back in reverse
                _isBlockedAnimating = true;
                OnArrowClicked?.Invoke(this, false);
                GameManager.Instance.HandleWrongClick();

                Vector2 collisionPoint = GridManager.Instance.GetCollisionPoint(HeadPoint, _headDirection, out Arrow obstacleArrow);
                _movement?.StartBlockedBounce(this, collisionPoint, obstacleArrow, () =>
                {
                    _isBlockedAnimating = false;
                });

                LogDebug($"Arrow at {HeadPoint} BLOCKED BOUNCE (Dir={_headDirection}, Collision={collisionPoint})");
            }
        }

        /// <summary>
        /// Fires the arrow — removes from grid and starts movement.
        /// </summary>
        private void Fire()
        {
            _isFired = true;

            // Remove from grid (all occupied points)
            GridManager.Instance.RemoveArrowFromPoints(_pathPoints);

            // Calculate exit point and start movement
            Vector2 exitPoint = GridManager.Instance.GetGridExitPoint(HeadPoint, _headDirection);
            _movement?.StartMovement(this, exitPoint);

            // Update visuals
            _visuals?.PlayFireEffect();

            OnArrowFiredEvent?.Invoke(this);
            GameManager.Instance.HandleArrowFired();

            LogDebug($"Arrow FIRED at {HeadPoint}, Dir={_headDirection}, W={Weight}");
        }

        /// <summary>
        /// Called by ArrowMovement when the arrow completes its path.
        /// </summary>
        public void OnPathComplete()
        {
            OnArrowCompleted?.Invoke(this);
            LogDebug($"Arrow completed path. W={Weight}");
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
            return Weight;
        }

        /// <summary>Visuals component reference.</summary>
        public ArrowVisuals Visuals => _visuals != null ? _visuals : (_visuals = GetComponent<ArrowVisuals>());

        /// <summary>
        /// Sets rainbow mode on this arrow.
        /// </summary>
        public void SetRainbow(bool rainbow)
        {
            _isRainbow = rainbow;
            _visuals?.SetRainbowMode(rainbow);
        }

        /// <summary>
        /// Resets the arrow for object pool reuse.
        /// </summary>
        public void ResetArrow()
        {
            _isFired = false;
            _isBlockedAnimating = false;
            _isRainbow = false;
            _pathPoints.Clear();
            _movement?.ResetMovement();
            _visuals?.ResetVisuals();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] Arrow: {message}");
        }
    }
}
