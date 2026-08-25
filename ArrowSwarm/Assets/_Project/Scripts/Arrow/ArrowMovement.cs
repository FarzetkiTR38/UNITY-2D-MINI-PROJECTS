namespace ArrowSwarm.Arrow
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Handles continuous snake-like arrow movement after firing, including:
    /// 1. Normal slither flight along grid exit and reverse enemy path.
    /// 2. Blocked bounce slither: advances forward, impacts obstacle arrow, shakes/flashes red,
    ///    and slithers back in reverse to its resting position.
    /// </summary>
    [RequireComponent(typeof(Arrow))]
    public class ArrowMovement : MonoBehaviour
    {
        private enum MovementMode
        {
            None,
            NormalFlight,
            BlockedBounce
        }

        private enum BlockedState
        {
            MovingForward,
            ImpactShake,
            ReturningReverse
        }

        private Arrow _arrow;
        private BoxCollider2D _boxCollider;
        private float _speed;
        private bool _isMoving;

        private MovementMode _mode = MovementMode.None;
        private BlockedState _blockedState = BlockedState.MovingForward;
        private float _shakeTimer;
        private float _maxForwardDistance;
        private Arrow _obstacleArrow;
        private Action _onBounceComplete;

        private readonly List<Vector2> _trajectory = new List<Vector2>();
        private readonly List<float> _trajectoryDistances = new List<float>();
        private readonly List<Vector3> _bodyPointsBuffer = new List<Vector3>();

        private float _bodyLength;
        private float _totalTrajectoryLength;
        private float _currentHeadDistance;

        /// <summary>Whether the arrow is currently in motion.</summary>
        public bool IsMoving => _isMoving;

        private void Awake()
        {
            _arrow = GetComponent<Arrow>();
            _boxCollider = GetComponent<BoxCollider2D>();
            if (_boxCollider == null)
            {
                _boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }

        /// <summary>
        /// Starts the slithering arrow movement along its unified trajectory:
        /// Tail -> Head -> Grid Exit -> Reverse Enemy Path.
        /// </summary>
        public void StartMovement(Arrow arrow, Vector2 gridExitPoint)
        {
            _arrow = arrow;
            _mode = MovementMode.NormalFlight;
            _speed = GameManager.Instance?.Config?.ArrowMoveSpeed ?? 15f;

            GridManager grid = GridManager.Instance;
            float spacing = grid.PointSpacing;
            Vector2 origin = grid.Origin;

            if (_boxCollider != null)
            {
                _boxCollider.size = Vector2.one * (spacing * 0.9f);
                _boxCollider.offset = Vector2.zero;
            }

            // 1. Build unified continuous trajectory
            _trajectory.Clear();
            var pathPoints = arrow.PathPoints;
            int n = pathPoints.Count;

            // Step A: Add resting arrow points from Tail (index n-1) to Head (index 0)
            for (int i = n - 1; i >= 0; i--)
            {
                _trajectory.Add(pathPoints[i].PointToWorld(spacing, origin));
            }

            // Step B: Add grid exit point
            Vector2 headWorld = pathPoints[0].PointToWorld(spacing, origin);
            if (Vector2.Distance(gridExitPoint, headWorld) > 0.02f)
            {
                _trajectory.Add(gridExitPoint);
            }

            // Step C: Add reverse enemy path waypoints
            var reversePath = ArrowSwarm.Path.PathManager.Instance?.GetArrowPathWaypoints(gridExitPoint);
            if (reversePath != null)
            {
                for (int i = 0; i < reversePath.Count; i++)
                {
                    if (_trajectory.Count == 0 || Vector2.Distance(_trajectory[_trajectory.Count - 1], reversePath[i]) > 0.02f)
                    {
                        _trajectory.Add(reversePath[i]);
                    }
                }
            }

            if (_trajectory.Count < 2)
            {
                CompleteMovement();
                return;
            }

            // 2. Compute cumulative distance table along the trajectory
            _trajectoryDistances.Clear();
            _trajectoryDistances.Add(0f);
            for (int i = 1; i < _trajectory.Count; i++)
            {
                float segmentDist = Vector2.Distance(_trajectory[i], _trajectory[i - 1]);
                _trajectoryDistances.Add(_trajectoryDistances[i - 1] + segmentDist);
            }

            _totalTrajectoryLength = _trajectoryDistances[_trajectoryDistances.Count - 1];

            // Resting arrow body length corresponds to the distance between Tail and Head
            _bodyLength = _trajectoryDistances[n - 1];
            _currentHeadDistance = _bodyLength;
            _isMoving = true;

            // Set initial position and rotation
            Vector2 initialHead = _trajectory[n - 1];
            transform.position = new Vector3(initialHead.x, initialHead.y, 0f);

            Vector2 initialDir = arrow.HeadDirection switch
            {
                ArrowDirection.Up => Vector2.up,
                ArrowDirection.Right => Vector2.right,
                ArrowDirection.Down => Vector2.down,
                ArrowDirection.Left => Vector2.left,
                _ => Vector2.up
            };
            RotateArrow(initialDir);
        }

        /// <summary>
        /// Starts the blocked bounce animation:
        /// Slithers forward to the obstacle collision point, impacts with red flash & shake,
        /// and slithers back in reverse to its resting position.
        /// </summary>
        public void StartBlockedBounce(Arrow arrow, Vector2 collisionPoint, Arrow obstacleArrow, Action onComplete)
        {
            _arrow = arrow;
            _obstacleArrow = obstacleArrow;
            _onBounceComplete = onComplete;
            _mode = MovementMode.BlockedBounce;
            _blockedState = BlockedState.MovingForward;
            _speed = GameManager.Instance?.Config?.ArrowMoveSpeed ?? 15f;

            GridManager grid = GridManager.Instance;
            float spacing = grid.PointSpacing;
            Vector2 origin = grid.Origin;

            // Build bounce trajectory: Tail -> Head -> CollisionPoint
            _trajectory.Clear();
            var pathPoints = arrow.PathPoints;
            int n = pathPoints.Count;

            for (int i = n - 1; i >= 0; i--)
            {
                _trajectory.Add(pathPoints[i].PointToWorld(spacing, origin));
            }

            Vector2 headWorld = pathPoints[0].PointToWorld(spacing, origin);
            if (Vector2.Distance(collisionPoint, headWorld) > 0.02f)
            {
                _trajectory.Add(collisionPoint);
            }

            if (_trajectory.Count < 2)
            {
                _mode = MovementMode.None;
                onComplete?.Invoke();
                return;
            }

            _trajectoryDistances.Clear();
            _trajectoryDistances.Add(0f);
            for (int i = 1; i < _trajectory.Count; i++)
            {
                float segmentDist = Vector2.Distance(_trajectory[i], _trajectory[i - 1]);
                _trajectoryDistances.Add(_trajectoryDistances[i - 1] + segmentDist);
            }

            _totalTrajectoryLength = _trajectoryDistances[_trajectoryDistances.Count - 1];
            _bodyLength = _trajectoryDistances[n - 1];
            _maxForwardDistance = _totalTrajectoryLength;
            _currentHeadDistance = _bodyLength;
            _isMoving = true;
        }

        private void Update()
        {
            if (!_isMoving || _trajectory.Count < 2) return;

            if (_mode == MovementMode.NormalFlight)
            {
                UpdateNormalFlight();
            }
            else if (_mode == MovementMode.BlockedBounce)
            {
                UpdateBlockedBounce();
            }
        }

        private void UpdateNormalFlight()
        {
            _currentHeadDistance += _speed * Time.deltaTime;
            float tailDistance = Mathf.Max(0f, _currentHeadDistance - _bodyLength);

            // Complete movement once the tail has fully passed through the entire path
            if (tailDistance >= _totalTrajectoryLength)
            {
                _isMoving = false;
                _mode = MovementMode.None;
                CompleteMovement();
                return;
            }

            bool isHeadActive = _currentHeadDistance <= _totalTrajectoryLength;
            float clampedHeadDist = Mathf.Min(_currentHeadDistance, _totalTrajectoryLength);

            Vector2 headPos = SamplePolyline(clampedHeadDist, out Vector2 headTangent);
            Vector2 tailPos = SamplePolyline(tailDistance, out _);

            Quaternion headRot = transform.rotation;
            if (headTangent.sqrMagnitude > 0.001f)
            {
                float rawAngle = Mathf.Atan2(headTangent.y, headTangent.x) * Mathf.Rad2Deg - 90f;
                float snappedAngle = Mathf.Round(rawAngle / 90f) * 90f;
                headRot = Quaternion.Euler(0, 0, snappedAngle);
            }

            ExtractSubPolyline(tailDistance, clampedHeadDist, tailPos, headPos);

            transform.position = new Vector3(headPos.x, headPos.y, 0f);
            transform.rotation = headRot;

            if (_arrow != null && _arrow.Visuals != null)
            {
                _arrow.Visuals.UpdateSlitheringBody(_bodyPointsBuffer, new Vector3(headPos.x, headPos.y, -0.05f), headRot, isHeadActive);
            }
        }

        private void UpdateBlockedBounce()
        {
            if (_blockedState == BlockedState.MovingForward)
            {
                float forwardSpeed = _speed * 1.3f;
                _currentHeadDistance += forwardSpeed * Time.deltaTime;

                if (_currentHeadDistance >= _maxForwardDistance)
                {
                    _currentHeadDistance = _maxForwardDistance;
                    _blockedState = BlockedState.ImpactShake;
                    _shakeTimer = 0.08f;

                    // Trigger impact effects
                    _arrow.Visuals?.PlayBlockedEffect();

                    if (_obstacleArrow != null && _obstacleArrow.Visuals != null)
                    {
                        Vector2 impactDir = (_trajectory[_trajectory.Count - 1] - _trajectory[_trajectory.Count - 2]).normalized;
                        _obstacleArrow.Visuals.PlayBumpedReactionEffect(impactDir);
                    }
                }
            }
            else if (_blockedState == BlockedState.ImpactShake)
            {
                _shakeTimer -= Time.deltaTime;
                if (_shakeTimer <= 0f)
                {
                    _blockedState = BlockedState.ReturningReverse;
                }
            }
            else if (_blockedState == BlockedState.ReturningReverse)
            {
                float returnSpeed = _speed * 1.5f;
                _currentHeadDistance -= returnSpeed * Time.deltaTime;

                if (_currentHeadDistance <= _bodyLength)
                {
                    _currentHeadDistance = _bodyLength;
                    _isMoving = false;
                    _mode = MovementMode.None;

                    // Restore exact resting appearance on grid
                    _arrow.Visuals?.RestoreRestingVisuals();
                    _onBounceComplete?.Invoke();
                    return;
                }
            }

            float tailDistance = Mathf.Max(0f, _currentHeadDistance - _bodyLength);
            Vector2 headPos = SamplePolyline(_currentHeadDistance, out Vector2 headTangent);
            Vector2 tailPos = SamplePolyline(tailDistance, out _);

            Quaternion headRot = transform.rotation;
            if (headTangent.sqrMagnitude > 0.001f)
            {
                float rawAngle = Mathf.Atan2(headTangent.y, headTangent.x) * Mathf.Rad2Deg - 90f;
                float snappedAngle = Mathf.Round(rawAngle / 90f) * 90f;
                headRot = Quaternion.Euler(0, 0, snappedAngle);
            }

            ExtractSubPolyline(tailDistance, _currentHeadDistance, tailPos, headPos);

            transform.position = new Vector3(headPos.x, headPos.y, 0f);
            transform.rotation = headRot;

            if (_arrow != null && _arrow.Visuals != null)
            {
                _arrow.Visuals.UpdateSlitheringBody(_bodyPointsBuffer, new Vector3(headPos.x, headPos.y, -0.05f), headRot, true);
            }
        }

        private Vector2 SamplePolyline(float targetDist, out Vector2 tangent)
        {
            if (_trajectory.Count == 0)
            {
                tangent = Vector2.up;
                return Vector2.zero;
            }
            if (targetDist <= 0f)
            {
                tangent = _trajectory.Count > 1 ? (_trajectory[1] - _trajectory[0]).normalized : Vector2.up;
                return _trajectory[0];
            }
            if (targetDist >= _totalTrajectoryLength)
            {
                int last = _trajectory.Count - 1;
                tangent = (_trajectory[last] - _trajectory[last - 1]).normalized;
                return _trajectory[last];
            }

            for (int i = 0; i < _trajectoryDistances.Count - 1; i++)
            {
                if (targetDist >= _trajectoryDistances[i] && targetDist <= _trajectoryDistances[i + 1])
                {
                    float segLen = _trajectoryDistances[i + 1] - _trajectoryDistances[i];
                    float t = segLen > 0.0001f ? (targetDist - _trajectoryDistances[i]) / segLen : 0f;
                    tangent = (_trajectory[i + 1] - _trajectory[i]).normalized;
                    return Vector2.Lerp(_trajectory[i], _trajectory[i + 1], t);
                }
            }

            tangent = Vector2.up;
            return _trajectory[_trajectory.Count - 1];
        }

        private void ExtractSubPolyline(float sTail, float sHead, Vector2 tailPos, Vector2 headPos)
        {
            _bodyPointsBuffer.Clear();
            _bodyPointsBuffer.Add(new Vector3(tailPos.x, tailPos.y, 0f));

            for (int i = 0; i < _trajectoryDistances.Count; i++)
            {
                float d = _trajectoryDistances[i];
                if (d > sTail + 0.01f && d < sHead - 0.01f)
                {
                    _bodyPointsBuffer.Add(new Vector3(_trajectory[i].x, _trajectory[i].y, 0f));
                }
            }

            _bodyPointsBuffer.Add(new Vector3(headPos.x, headPos.y, 0f));
        }

        private void RotateArrow(Vector2 dir)
        {
            if (dir == Vector2.zero) return;
            float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            float snappedAngle = Mathf.Round(rawAngle / 90f) * 90f;
            Quaternion targetRot = Quaternion.Euler(0, 0, snappedAngle);
            transform.rotation = targetRot;

            if (_arrow != null && _arrow.Visuals != null && _arrow.Visuals.HeadTransform != null)
            {
                _arrow.Visuals.HeadTransform.rotation = targetRot;
            }
        }

        private void CompleteMovement()
        {
            _arrow.OnPathComplete();
        }

        /// <summary>
        /// Handles collision with mobs while arrow is moving in normal flight mode.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsMoving || _mode != MovementMode.NormalFlight) return;

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
            _isMoving = false;
            _mode = MovementMode.None;
            _obstacleArrow = null;
            _onBounceComplete = null;
            _trajectory.Clear();
            _trajectoryDistances.Clear();
            _bodyPointsBuffer.Clear();
            transform.rotation = Quaternion.identity;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] ArrowMovement: {message}");
        }
    }
}
