namespace ArrowSwarm.Path
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Component that follows a series of waypoints along a continuous 1D path distance.
    /// Supports forward movement, bidirectional speeds, and gap-closing reverse pulls.
    /// </summary>
    public class PathFollower : MonoBehaviour
    {
        private List<Vector2> _waypoints = new List<Vector2>();
        private readonly List<float> _cumulativeDistances = new List<float>();
        private float _totalLength;
        private float _currentDistance;
        private float _speed;
        private bool _isFollowing;

        /// <summary>Whether the follower is currently moving.</summary>
        public bool IsFollowing => _isFollowing;

        /// <summary>Current distance traveled along the path in world units.</summary>
        public float CurrentDistance => _currentDistance;

        /// <summary>Total length of the waypoints path in world units.</summary>
        public float TotalLength => _totalLength;

        /// <summary>Current movement speed in units/second (positive = forward, negative = reverse).</summary>
        public float Speed => _speed;

        /// <summary>Progress ratio (0 to 1) through the waypoints.</summary>
        public float Progress => _totalLength > 0.001f ? Mathf.Clamp01(_currentDistance / _totalLength) : 0f;

        /// <summary>Fired when the follower reaches the end of the path.</summary>
        public event Action OnPathEnd;

        /// <summary>Fired each frame with the current facing direction.</summary>
        public event Action<Vector2> OnDirectionChanged;

        /// <summary>
        /// Starts following the given waypoints at the specified speed.
        /// </summary>
        public void StartFollowing(List<Vector2> waypoints, float speed, float initialDistance = 0f)
        {
            if (waypoints == null || waypoints.Count < 2)
            {
                Debug.LogWarning("[ArrowSwarm] PathFollower: Not enough waypoints.");
                return;
            }

            _waypoints.Clear();
            _waypoints.AddRange(waypoints);
            _speed = speed;

            CalculateDistances();

            _currentDistance = Mathf.Clamp(initialDistance, 0f, _totalLength);
            _isFollowing = true;

            UpdatePositionAndDirection();
        }

        /// <summary>
        /// Stops following the path.
        /// </summary>
        public void StopFollowing()
        {
            _isFollowing = false;
        }

        /// <summary>
        /// Updates the movement speed (positive = forward, negative = reverse/gap-closing).
        /// </summary>
        public void SetSpeed(float newSpeed)
        {
            _speed = newSpeed;
        }

        /// <summary>
        /// Sets the exact distance along the path.
        /// </summary>
        public void SetDistance(float distance)
        {
            _currentDistance = Mathf.Clamp(distance, 0f, _totalLength);
            UpdatePositionAndDirection();
        }

        /// <summary>
        /// Resets the follower state for pool reuse.
        /// </summary>
        public void ResetFollower()
        {
            _isFollowing = false;
            _currentDistance = 0f;
            _totalLength = 0f;
            _waypoints.Clear();
            _cumulativeDistances.Clear();
        }

        private void Update()
        {
            if (!_isFollowing || _waypoints == null || _waypoints.Count < 2) return;

            if (_speed != 0f)
            {
                _currentDistance += _speed * Time.deltaTime;

                if (_currentDistance >= _totalLength)
                {
                    _currentDistance = _totalLength;
                    UpdatePositionAndDirection();
                    _isFollowing = false;
                    OnPathEnd?.Invoke();
                    return;
                }
                else if (_currentDistance < 0f)
                {
                    _currentDistance = 0f;
                }

                UpdatePositionAndDirection();
            }
        }

        private void CalculateDistances()
        {
            _cumulativeDistances.Clear();
            _cumulativeDistances.Add(0f);
            _totalLength = 0f;

            for (int i = 0; i < _waypoints.Count - 1; i++)
            {
                float segmentLength = Vector2.Distance(_waypoints[i], _waypoints[i + 1]);
                _totalLength += segmentLength;
                _cumulativeDistances.Add(_totalLength);
            }
        }

        private void UpdatePositionAndDirection()
        {
            if (_waypoints.Count < 2) return;

            Vector2 pos = SamplePositionAtDistance(_currentDistance, out Vector2 direction);
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);

            if (direction.sqrMagnitude > 0.001f)
            {
                OnDirectionChanged?.Invoke(direction);
            }
        }

        private Vector2 SamplePositionAtDistance(float distance, out Vector2 direction)
        {
            if (_cumulativeDistances.Count < 2)
            {
                direction = Vector2.right;
                return _waypoints.Count > 0 ? _waypoints[0] : Vector2.zero;
            }

            if (distance <= 0f)
            {
                direction = (_waypoints[1] - _waypoints[0]).normalized;
                return _waypoints[0];
            }

            if (distance >= _totalLength)
            {
                int last = _waypoints.Count - 1;
                direction = (_waypoints[last] - _waypoints[last - 1]).normalized;
                return _waypoints[last];
            }

            for (int i = 0; i < _cumulativeDistances.Count - 1; i++)
            {
                float dStart = _cumulativeDistances[i];
                float dEnd = _cumulativeDistances[i + 1];

                if (distance >= dStart && distance <= dEnd)
                {
                    float segLen = dEnd - dStart;
                    float t = segLen > 0.0001f ? (distance - dStart) / segLen : 0f;
                    direction = (_waypoints[i + 1] - _waypoints[i]).normalized;
                    return Vector2.Lerp(_waypoints[i], _waypoints[i + 1], t);
                }
            }

            int endIdx = _waypoints.Count - 1;
            direction = (_waypoints[endIdx] - _waypoints[endIdx - 1]).normalized;
            return _waypoints[endIdx];
        }
    }
}
