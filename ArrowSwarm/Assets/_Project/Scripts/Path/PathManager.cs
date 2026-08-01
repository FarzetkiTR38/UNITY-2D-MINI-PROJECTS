namespace ArrowSwarm.Path
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages the path waypoints that mobs follow.
    /// Provides waypoint data for both mob movement and arrow path-following.
    /// </summary>
    public class PathManager : Singleton<PathManager>
    {
        private List<Vector2> _waypoints = new List<Vector2>();
        private Vector2 _spawnPoint;
        private Vector2 _finishPoint;
        private float _totalPathLength;

        /// <summary>Ordered path waypoints (spawn → finish).</summary>
        public IReadOnlyList<Vector2> Waypoints => _waypoints;

        /// <summary>Where mobs spawn.</summary>
        public Vector2 SpawnPoint => _spawnPoint;

        /// <summary>Where mobs are trying to reach.</summary>
        public Vector2 FinishPoint => _finishPoint;

        /// <summary>Total length of the path in world units.</summary>
        public float TotalPathLength => _totalPathLength;

        /// <summary>Fired when path is initialized.</summary>
        public static event Action OnPathInitialized;

        /// <summary>
        /// Initializes the path from MapData.
        /// </summary>
        public void InitializePath(MapData mapData)
        {
            _spawnPoint = mapData.SpawnPoint;
            _finishPoint = mapData.FinishPoint;

            _waypoints.Clear();
            _waypoints.Add(_spawnPoint);
            for (int i = 0; i < mapData.PathWaypoints.Count; i++)
            {
                _waypoints.Add(mapData.PathWaypoints[i]);
            }
            _waypoints.Add(_finishPoint);

            CalculateTotalLength();
            OnPathInitialized?.Invoke();
            LogDebug($"Path initialized: {_waypoints.Count} waypoints, Length={_totalPathLength:F1}");
        }

        /// <summary>
        /// Gets the waypoints for an arrow to follow on the path
        /// (reversed direction — from entry point toward spawn point).
        /// </summary>
        /// <param name="entryWorldPos">Where the arrow enters the path.</param>
        /// <returns>Ordered waypoints from entry to spawn point.</returns>
        public List<Vector2> GetArrowPathWaypoints(Vector2 entryWorldPos)
        {
            // Find the closest segment on the path
            int closestSegmentIndex = FindClosestSegment(entryWorldPos);

            // Build reverse path: from entry point back toward spawn
            List<Vector2> arrowPath = new List<Vector2>();
            arrowPath.Add(GetClosestPointOnSegment(entryWorldPos, closestSegmentIndex));

            // Add waypoints in reverse from closest segment back to spawn
            for (int i = closestSegmentIndex; i >= 0; i--)
            {
                arrowPath.Add(_waypoints[i]);
            }

            return arrowPath;
        }

        /// <summary>
        /// Gets the position along the path at a given normalized progress (0-1).
        /// </summary>
        public Vector2 GetPositionAtProgress(float progress)
        {
            if (_waypoints.Count < 2) return _spawnPoint;

            progress = Mathf.Clamp01(progress);
            float targetDist = progress * _totalPathLength;
            float accumulated = 0f;

            for (int i = 0; i < _waypoints.Count - 1; i++)
            {
                float segmentLength = Vector2.Distance(_waypoints[i], _waypoints[i + 1]);
                if (accumulated + segmentLength >= targetDist)
                {
                    float t = (targetDist - accumulated) / segmentLength;
                    return Vector2.Lerp(_waypoints[i], _waypoints[i + 1], t);
                }
                accumulated += segmentLength;
            }

            return _finishPoint;
        }

        private int FindClosestSegment(Vector2 point)
        {
            float minDist = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < _waypoints.Count - 1; i++)
            {
                Vector2 closest = ClosestPointOnLine(_waypoints[i], _waypoints[i + 1], point);
                float dist = Vector2.Distance(point, closest);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private Vector2 GetClosestPointOnSegment(Vector2 point, int segmentIndex)
        {
            if (segmentIndex >= _waypoints.Count - 1) return _waypoints[_waypoints.Count - 1];
            return ClosestPointOnLine(_waypoints[segmentIndex], _waypoints[segmentIndex + 1], point);
        }

        private static Vector2 ClosestPointOnLine(Vector2 a, Vector2 b, Vector2 point)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / Vector2.Dot(ab, ab));
            return a + ab * t;
        }

        private void CalculateTotalLength()
        {
            _totalPathLength = 0f;
            for (int i = 0; i < _waypoints.Count - 1; i++)
            {
                _totalPathLength += Vector2.Distance(_waypoints[i], _waypoints[i + 1]);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] PathManager: {message}");
        }
    }
}
