namespace ArrowSwarm.Arrow
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Handles arrow movement after firing.
    /// The arrow head moves from its head point outward through the grid exit,
    /// damaging mobs via trigger collisions along the way.
    /// </summary>
    [RequireComponent(typeof(Arrow))]
    public class ArrowMovement : MonoBehaviour
    {
        private Arrow _arrow;
        private ArrowSwarm.Path.PathFollower _pathFollower;
        private float _speed;

        /// <summary>Whether the arrow is currently in motion.</summary>
        public bool IsMoving => _pathFollower != null && _pathFollower.IsFollowing;

        private void Awake()
        {
            _arrow = GetComponent<Arrow>();
            _pathFollower = GetComponent<ArrowSwarm.Path.PathFollower>();
            if (_pathFollower == null)
            {
                _pathFollower = gameObject.AddComponent<ArrowSwarm.Path.PathFollower>();
            }

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Kinematic;
            // No need to simulate if we just want trigger callbacks
            rb.simulated = true;
        }

        /// <summary>
        /// Starts the arrow movement. It flies straight to the grid exit, 
        /// then follows the enemy path in reverse.
        /// </summary>
        public void StartMovement(Arrow arrow, Vector2 gridExitPoint)
        {
            _speed = GameManager.Instance?.Config?.ArrowMoveSpeed ?? 15f;

            GridManager grid = GridManager.Instance;
            Vector2 headWorld = arrow.HeadPoint.PointToWorld(grid.PointSpacing, grid.Origin);
            transform.position = new Vector3(headWorld.x, headWorld.y, 0f);

            // Initial rotation
            float zRotation = arrow.HeadDirection switch
            {
                ArrowDirection.Up => 0f,
                ArrowDirection.Right => -90f,
                ArrowDirection.Down => 180f,
                ArrowDirection.Left => 90f,
                _ => 0f
            };
            transform.rotation = Quaternion.Euler(0, 0, zRotation);

            // Build full path
            var waypoints = new System.Collections.Generic.List<Vector2>();
            waypoints.Add(headWorld);
            
            // Add reverse enemy path
            var reversePath = ArrowSwarm.Path.PathManager.Instance.GetArrowPathWaypoints(gridExitPoint);
            waypoints.AddRange(reversePath);

            _pathFollower.OnPathEnd -= CompleteMovement;
            _pathFollower.OnPathEnd += CompleteMovement;
            
            _pathFollower.OnDirectionChanged -= RotateArrow;
            _pathFollower.OnDirectionChanged += RotateArrow;

            _pathFollower.StartFollowing(waypoints, _speed);
        }

        private void RotateArrow(Vector2 dir)
        {
            if (dir == Vector2.zero) return;
            float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            float snappedAngle = Mathf.Round(rawAngle / 90f) * 90f;
            transform.rotation = Quaternion.Euler(0, 0, snappedAngle);
        }

        private void CompleteMovement()
        {
            _arrow.OnPathComplete();
        }

        /// <summary>
        /// Handles collision with mobs while arrow is moving.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsMoving) return;

            var mob = other.GetComponent<ArrowSwarm.Mob.Mob>();
            if (mob != null)
            {
                mob.TakeDamage(_arrow.GetDamage());
            }
        }

        /// <summary>
        /// Resets movement state for pool reuse.
        /// </summary>
        public void ResetMovement()
        {
            transform.rotation = Quaternion.identity;
            if (_pathFollower != null)
            {
                _pathFollower.StopFollowing();
                _pathFollower.OnPathEnd -= CompleteMovement;
                _pathFollower.OnDirectionChanged -= RotateArrow;
                _pathFollower.ResetFollower();
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] ArrowMovement: {message}");
        }
    }
}
