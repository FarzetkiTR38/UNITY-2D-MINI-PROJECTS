namespace ArrowSwarm.Puzzle
{
    using ArrowSwarm.Modes;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Pooled bent stroke with a sliding path window, independent of combat Arrow.</summary>
    public sealed partial class PuzzleArrowView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _body;
        [SerializeField] private LineRenderer _head;
        [Tooltip("Optional authored upward triangle. The legacy line head remains a fallback.")]
        [SerializeField] private SpriteRenderer _filledHead;
        private PuzzleArrowData _data;
        private PuzzleConfig _config;
        private Vector3 _origin;
        private Vector3[] _path;
        private float _length;
        private bool _usingFilledHead;
        private Transform _filledHeadTransform;
        private Vector3 _filledHeadBaseScale, _filledHeadTipOffset;
        private float _filledHeadDepth, _headScale = 1f, _travel;
        public int Index { get; private set; }
        public float Length => _length;

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged += ApplyTheme;
            ApplyTheme(ThemeManager.CurrentTheme);
        }

        private void OnDisable() => ThemeManager.OnThemeChanged -= ApplyTheme;

        private void ApplyTheme(ThemeMode theme)
        {
            if (_data == null || _config == null) return;
            Color color = _config.ThemeColor(_data.Color, theme);
            _body.startColor = _body.endColor = color;
            if (_head != null) _head.startColor = _head.endColor = color;
            if (_filledHead != null) _filledHead.color = color;
        }

        /// <summary>Assigns path data and color when acquired from the pool.</summary>
        public void Bind(PuzzleArrowData data, int index, Vector3 origin, PuzzleConfig config)
        {
            _data = data; Index = index; _origin = origin; _config = config;
            _length = data.Points.Count - 1;
            _path = new Vector3[data.Points.Count];
            for (int i = 0; i < _path.Length; i++) _path[i] = origin + (Vector3)(Vector2)data.Points[_path.Length - 1 - i];
            _body.widthMultiplier = config.StrokeWidth;
            _headScale = 1f;
            transform.localScale = Vector3.one;
            BindHead(config.ThemeColor(data.Color, ThemeManager.CurrentTheme));
            PrepareStroke();
            ApplyTheme(ThemeManager.CurrentTheme);
            RenderTravel(0);
        }

        private void BindHead(Color color)
        {
            _usingFilledHead = _filledHead != null && _filledHead.sprite != null;
            if (_head != null)
            {
                _head.enabled = !_usingFilledHead;
                _head.startColor = _head.endColor = color;
                _head.widthMultiplier = _config.StrokeWidth;
            }
            if (_filledHead == null) return;
            _filledHead.enabled = _usingFilledHead;
            _filledHead.color = color;
            if (!_usingFilledHead) return;
            _filledHeadTransform = _filledHead.transform;
            Bounds bounds = _filledHead.sprite.bounds;
            float scale = _config.HeadSize / Mathf.Max(.001f, bounds.size.y);
            _filledHeadBaseScale = new Vector3(scale, scale, 1f);
            _filledHeadTipOffset = new Vector3(bounds.center.x * scale, bounds.max.y * scale, 0f);
            _filledHeadDepth = _filledHeadTransform.localPosition.z;
            _filledHeadTransform.localScale = _filledHeadBaseScale;
            MeasureHeadInset();
        }

        /// <summary>Moves a fixed-length snake window along its own path then its forward ray.</summary>
        public void RenderTravel(float distance)
        {
            _travel = distance;
            float begin = Mathf.Max(0, distance), end = _length + distance;
            float bodyEnd = _usingFilledHead ? Mathf.Max(begin, end - _headInset * _headScale) : end;
            RenderStroke(begin, bodyEnd);
            Vector3 tip = PointAt(end);
            Vector3 direction = end > _length ? (Vector3)(Vector2)_data.Direction : (tip - PointAt(Mathf.Max(begin, end - .01f))).normalized;
            if (direction.sqrMagnitude < .1f) direction = (Vector3)(Vector2)_data.Direction;
            if (_usingFilledHead)
            {
                Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
                Vector3 position = tip - rotation * (_filledHeadTipOffset * _headScale);
                position.z += _filledHeadDepth;
                _filledHeadTransform.SetPositionAndRotation(position, rotation);
                return;
            }
            if (_head == null) return;
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
        {
            _headScale = value ? 1.8f : 1f;
            _body.widthMultiplier = _config.StrokeWidth * _headScale;
            if (_head != null) _head.widthMultiplier = _config.StrokeWidth * _headScale;
            if (_usingFilledHead)
            {
                _filledHeadTransform.localScale = new Vector3(
                    _filledHeadBaseScale.x * _headScale, _filledHeadBaseScale.y * _headScale, _filledHeadBaseScale.z);
            }
            PrepareStroke();
            RenderTravel(_travel);
        }
    }
}
