namespace ArrowSwarm.Path
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Component that follows a series of waypoints at a given speed.
    /// Used by both mobs (spawn→finish) and arrows (entry→spawn).
    /// </summary>
    public class PathFollower : MonoBehaviour
    {
        private List<Vector2> _waypoints;
        private int _currentIndex;
        private float _speed;
        private bool _isFollowing;

        /// <summary>Whether the follower is currently moving.</summary>
        public bool IsFollowing => _isFollowing;

        /// <summary>Current waypoint index.</summary>
        public int CurrentWaypointIndex => _currentIndex;

        /// <summary>Progress ratio (0 to 1) through the waypoints.</summary>
        public float Progress
        {
            get
            {
                if (_waypoints == null || _waypoints.Count <= 1) return 0f;
                return (float)_currentIndex / (_waypoints.Count - 1);
            }
        }

        /// <summary>Fired when the follower reaches the end of the path.</summary>
        public event Action OnPathEnd;

        /// <summary>Fired each frame with the current facing direction.</summary>
        public event Action<Vector2> OnDirectionChanged;

        /// <summary>
        /// Starts following the given waypoints at the specified speed.
        /// </summary>
        public void StartFollowing(List<Vector2> waypoints, float speed)
        {
            if (waypoints == null || waypoints.Count < 2)
            {
                Debug.LogWarning("[ArrowSwarm] PathFollower: Not enough waypoints.");
                return;
            }

            _waypoints = waypoints;
            _speed = speed;
            _currentIndex = 0;
            _isFollowing = true;

            transform.position = _waypoints[0];
        }

        /// <summary>
        /// Stops following the path.
        /// </summary>
        public void StopFollowing()
        {
            _isFollowing = false;
        }

        /// <summary>
        /// Updates the movement speed (e.g., for difficulty changes).
        /// </summary>
        public void SetSpeed(float newSpeed)
        {
            _speed = newSpeed;
        }

        /// <summary>
        /// Resets the follower state for pool reuse.
        /// </summary>
        public void ResetFollower()
        {
            _isFollowing = false;
            _currentIndex = 0;
            _waypoints = null;
        }

        private void Update()
        {
            if (!_isFollowing || _waypoints == null) return;

            if (_currentIndex >= _waypoints.Count - 1)
            {
                _isFollowing = false;
                OnPathEnd?.Invoke();
                return;
            }

            Vector2 target = _waypoints[_currentIndex + 1];
            Vector2 currentPos = transform.position;
            Vector2 direction = (target - currentPos).normalized;

            Vector2 newPos = Vector2.MoveTowards(currentPos, target, _speed * Time.deltaTime);
            transform.position = newPos;

            OnDirectionChanged?.Invoke(direction);

            if (Vector2.Distance(newPos, target) < 0.01f)
            {
                _currentIndex++;
            }
        }
    }
}
