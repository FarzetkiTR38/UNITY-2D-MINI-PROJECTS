namespace ArrowSwarm.Mob
{
    using System;
    using ArrowSwarm.Path;
    using UnityEngine;

    /// <summary>
    /// Controls mob movement along the path using PathFollower.
    /// Handles reaching the finish point and supports dynamic speed changes for gap closing.
    /// </summary>
    [RequireComponent(typeof(PathFollower))]
    public class MobMovement : MonoBehaviour
    {
        private PathFollower _pathFollower;
        private bool _hasReachedFinish;

        /// <summary>Whether this mob has reached the finish point.</summary>
        public bool HasReachedFinish => _hasReachedFinish;

        /// <summary>Current movement direction (normalized).</summary>
        public Vector2 CurrentDirection { get; private set; }

        /// <summary>Current distance along the path in world units.</summary>
        public float CurrentPathDistance => _pathFollower != null ? _pathFollower.CurrentDistance : 0f;

        /// <summary>Normalized progress along the path (0 to 1).</summary>
        public float Progress => _pathFollower != null ? _pathFollower.Progress : 0f;

        /// <summary>Fired when this mob reaches the finish point.</summary>
        public event Action OnFinishReached;

        private void Awake()
        {
            _pathFollower = GetComponent<PathFollower>();
        }

        /// <summary>
        /// Starts the mob moving along the path at the given speed.
        /// </summary>
        public void StartMoving(float speed, float initialDistance = 0f)
        {
            _hasReachedFinish = false;

            PathManager pm = PathManager.Instance;
            if (pm == null || pm.Waypoints == null || pm.Waypoints.Count < 2)
            {
                Debug.LogError("[ArrowSwarm] MobMovement: PathManager not initialized!");
                return;
            }

            var waypoints = new System.Collections.Generic.List<Vector2>(pm.Waypoints);
            _pathFollower.StartFollowing(waypoints, speed, initialDistance);
            _pathFollower.OnPathEnd += HandlePathEnd;
            _pathFollower.OnDirectionChanged += HandleDirectionChanged;
        }

        /// <summary>
        /// Updates the movement speed (positive = forward, negative = reverse/gap-closing).
        /// </summary>
        public void SetSpeed(float newSpeed)
        {
            _pathFollower?.SetSpeed(newSpeed);
        }

        /// <summary>
        /// Sets the exact distance along the path.
        /// </summary>
        public void SetDistance(float distance)
        {
            _pathFollower?.SetDistance(distance);
        }

        /// <summary>
        /// Stops mob movement.
        /// </summary>
        public void StopMoving()
        {
            _pathFollower?.StopFollowing();
        }

        /// <summary>
        /// Resets movement state for pool reuse.
        /// </summary>
        public void ResetMovement()
        {
            _hasReachedFinish = false;
            CurrentDirection = Vector2.zero;

            if (_pathFollower != null)
            {
                _pathFollower.OnPathEnd -= HandlePathEnd;
                _pathFollower.OnDirectionChanged -= HandleDirectionChanged;
                _pathFollower.ResetFollower();
            }
        }

        private void HandlePathEnd()
        {
            _hasReachedFinish = true;
            OnFinishReached?.Invoke();
            LogDebug("Mob reached finish!");
        }

        private void HandleDirectionChanged(Vector2 direction)
        {
            CurrentDirection = direction;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] MobMovement: {message}");
        }
    }
}
