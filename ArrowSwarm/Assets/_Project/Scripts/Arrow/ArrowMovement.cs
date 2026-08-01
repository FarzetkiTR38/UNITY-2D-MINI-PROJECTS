namespace ArrowSwarm.Arrow
{
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>
    /// Handles arrow movement after firing:
    /// 1. Moves from grid position to grid edge (straight line).
    /// 2. Follows the path toward spawn point (against mob direction).
    /// Damages mobs along the way via trigger collisions.
    /// </summary>
    [RequireComponent(typeof(Arrow))]
    public class ArrowMovement : MonoBehaviour
    {
        private Arrow _arrow;
        private float _speed;
        private bool _isMoving;
        private int _currentWaypointIndex;

        // Movement phases
        private enum MovePhase { GridExit, PathFollow, Done }
        private MovePhase _phase = MovePhase.Done;

        private Vector2 _targetPosition;
        private List<Vector2> _pathWaypoints;

        /// <summary>Whether the arrow is currently in motion.</summary>
        public bool IsMoving => _isMoving;

        private void Awake()
        {
            _arrow = GetComponent<Arrow>();
        }

        /// <summary>
        /// Starts the arrow movement toward the grid exit point, then along the path.
        /// </summary>
        public void StartMovement(ArrowDirection direction, Vector2 gridExitPoint)
        {
            _speed = GameManager.Instance?.Config?.ArrowMoveSpeed ?? 15f;
            _targetPosition = gridExitPoint;
            _phase = MovePhase.GridExit;
            _isMoving = true;

            LogDebug($"Movement started. Phase=GridExit, Target={gridExitPoint}");
        }

        /// <summary>
        /// Sets the path waypoints for the arrow to follow after exiting the grid.
        /// Waypoints should be ordered from the grid exit toward the spawn point.
        /// </summary>
        public void SetPathWaypoints(List<Vector2> waypoints)
        {
            _pathWaypoints = waypoints;
        }

        private void Update()
        {
            if (!_isMoving) return;

            switch (_phase)
            {
                case MovePhase.GridExit:
                    MoveToward(_targetPosition);
                    if (HasReachedTarget(_targetPosition))
                    {
                        TransitionToPathFollow();
                    }
                    break;

                case MovePhase.PathFollow:
                    if (_pathWaypoints == null || _currentWaypointIndex >= _pathWaypoints.Count)
                    {
                        CompleteMovement();
                        break;
                    }
                    MoveToward(_pathWaypoints[_currentWaypointIndex]);
                    if (HasReachedTarget(_pathWaypoints[_currentWaypointIndex]))
                    {
                        _currentWaypointIndex++;
                        if (_currentWaypointIndex >= _pathWaypoints.Count)
                        {
                            CompleteMovement();
                        }
                    }
                    break;
            }
        }

        private void MoveToward(Vector2 target)
        {
            Vector2 currentPos = transform.position;
            Vector2 newPos = Vector2.MoveTowards(currentPos, target, _speed * Time.deltaTime);
            transform.position = newPos;
        }

        private bool HasReachedTarget(Vector2 target)
        {
            return Vector2.Distance(transform.position, target) < 0.05f;
        }

        private void TransitionToPathFollow()
        {
            _phase = MovePhase.PathFollow;
            _currentWaypointIndex = 0;

            // Path waypoints will be set by PathManager when arrow exits grid
            // If no waypoints available, complete immediately
            if (_pathWaypoints == null || _pathWaypoints.Count == 0)
            {
                CompleteMovement();
            }

            LogDebug("Phase=PathFollow");
        }

        private void CompleteMovement()
        {
            _phase = MovePhase.Done;
            _isMoving = false;
            _arrow.OnPathComplete();
            LogDebug("Movement completed.");
        }

        /// <summary>
        /// Handles collision with mobs while arrow is moving.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isMoving) return;

            // Check if the collider belongs to a mob (component check)
            // Mob damage is handled by the Mob system (Phase 3)
            // Arrow sends its damage value via the collision
            
            // TODO: Uncomment in Phase 3 when Mob system is implemented
            /*
            var mob = other.GetComponent<ArrowSwarm.Mob.Mob>();
            if (mob != null)
            {
                mob.TakeDamage(_arrow.GetDamage());
                LogDebug($"Hit mob! Damage={_arrow.GetDamage()}");
            }
            */
        }

        /// <summary>
        /// Resets movement state for pool reuse.
        /// </summary>
        public void ResetMovement()
        {
            _isMoving = false;
            _phase = MovePhase.Done;
            _currentWaypointIndex = 0;
            _pathWaypoints = null;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] ArrowMovement: {message}");
        }
    }
}
