namespace ArrowSwarm.Puzzle
{
    using ArrowSwarm.Modes;
    using UnityEngine;

    /// <summary>Pooled bent stroke with a sliding path window, independent of combat Arrow.</summary>
    public sealed class PuzzleArrowView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _body;
        [SerializeField] private LineRenderer _head;
        private PuzzleArrowData _data;
        private PuzzleConfig _config;
        private Vector3 _origin;
        private Vector3[] _path;
        private float _length;
        public int Index { get; private set; }
        public float Length => _length;

        /// <summary>Assigns path data and color when acquired from the pool.</summary>
        public void Bind(PuzzleArrowData data, int index, Vector3 origin, PuzzleConfig config)
        {
            _data = data; Index = index; _origin = origin; _config = config;
            _length = data.Points.Count - 1;
            _path = new Vector3[data.Points.Count];
            for (int i = 0; i < _path.Length; i++) _path[i] = origin + (Vector3)(Vector2)data.Points[_path.Length - 1 - i];
            _body.startColor = _body.endColor = _head.startColor = _head.endColor = data.Color;
            _body.widthMultiplier = _head.widthMultiplier = config.StrokeWidth;
            transform.localScale = Vector3.one;
            RenderTravel(0);
        }

        /// <summary>Moves a fixed-length snake window along its own path then its forward ray.</summary>
        public void RenderTravel(float distance)
        {
            float begin = Mathf.Max(0, distance), end = _length + distance;
            int count = 2;
            for (int i = 1; i < _path.Length; i++) if (i > begin && i < end) count++;
            _body.positionCount = count;
            int n = 0; _body.SetPosition(n++, PointAt(begin));
            for (int i = 1; i < _path.Length; i++) if (i > begin && i < end) _body.SetPosition(n++, _path[i]);
            Vector3 tip = PointAt(end);
            _body.SetPosition(n, tip);
            Vector3 direction = end > _length ? (Vector3)(Vector2)_data.Direction : (tip - PointAt(Mathf.Max(begin, end - .01f))).normalized;
            if (direction.sqrMagnitude < .1f) direction = (Vector3)(Vector2)_data.Direction;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x);
            _head.positionCount = 3;
            _head.SetPosition(0, tip - direction * _config.HeadSize + perpendicular * _config.HeadSize * .65f);
            _head.SetPosition(1, tip);
            _head.SetPosition(2, tip - direction * _config.HeadSize - perpendicular * _config.HeadSize * .65f);
        }

        private Vector3 PointAt(float t)
        {
            if (t >= _length) return _path[_path.Length - 1] + (Vector3)(Vector2)_data.Direction * (t - _length);
            int index = Mathf.Min(Mathf.FloorToInt(t), _path.Length - 2);
            return Vector3.Lerp(_path[index], _path[index + 1], t - index);
        }

        /// <summary>Exact point-to-segment distance used for touch selection.</summary>
        public float DistanceTo(Vector2 world)
        {
            float result = float.PositiveInfinity;
            for (int i = 1; i < _path.Length; i++)
            {
                Vector2 a = _path[i - 1], b = _path[i];
                float t = Mathf.Clamp01(Vector2.Dot(world - a, b - a) / (b - a).sqrMagnitude);
                result = Mathf.Min(result, Vector2.Distance(world, Vector2.Lerp(a, b, t)));
            }
            return result;
        }

        /// <summary>Highlights a safe hint without changing its configured color.</summary>
        public void Highlight(bool value)
        { _body.widthMultiplier = _head.widthMultiplier = _config.StrokeWidth * (value ? 1.8f : 1f); }
    }
}
