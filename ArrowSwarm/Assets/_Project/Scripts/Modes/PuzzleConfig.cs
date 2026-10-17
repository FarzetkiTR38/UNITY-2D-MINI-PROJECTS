namespace ArrowSwarm.Modes
{
    using UnityEngine;
    using ArrowSwarm.Data;

    /// <summary>Inspector-owned puzzle tuning, shared by runtime and authoring preview.</summary>
    [CreateAssetMenu(menuName = "Arrow Swarm/Puzzle Config")]
    public sealed class PuzzleConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _lives = 3;
        [SerializeField, Min(.01f)] private float _strokeWidth = .16f;
        [SerializeField, Min(.01f)] private float _headSize = .4f;
        [SerializeField, Min(.01f)] private float _speed = 23f;
        [SerializeField, Min(.01f)] private float _rotationDuration = .3f;
        [SerializeField, Min(.01f)] private float _hitRadius = .38f;
        [SerializeField, Min(1)] private float _swipeThreshold = 60f;
        [SerializeField, Min(1)] private float _tapThreshold = 25f;
        [SerializeField, Min(.01f)] private float _wallOffset = .9f;
        [SerializeField, Min(.01f)] private float _wallWidth = .3f;
        [SerializeField, Min(1)] private int _poolSize = 96;
        [Header("Dark Theme Palette Presentation")]
        [Tooltip("Minimum displayed linear luminance on dark boards. Logical level colors remain unchanged.")]
        [SerializeField, Range(0f, 1f)] private float _minimumDarkLuminance = .55f;
        public int Lives => _lives;
        public float StrokeWidth => _strokeWidth;
        public float HeadSize => _headSize;
        public float Speed => _speed;
        public float RotationDuration => _rotationDuration;
        public float HitRadius => _hitRadius;
        public float SwipeThreshold => _swipeThreshold;
        public float TapThreshold => _tapThreshold;
        public float WallOffset => _wallOffset;
        public float WallWidth => _wallWidth;
        public int PoolSize => _poolSize;

        /// <summary>Returns the same readable display color for arrows and their matching frame walls.</summary>
        public Color ThemeColor(Color source, ThemeMode theme)
        {
            if (theme != ThemeMode.Dark) return source;
            Color linear = source.linear;
            float luminance = .2126f * linear.r + .7152f * linear.g + .0722f * linear.b;
            float minimum = Mathf.Clamp01(_minimumDarkLuminance);
            if (luminance >= minimum) return source;
            float lift = (minimum - luminance) / Mathf.Max(.0001f, 1f - luminance);
            Color displayed = Color.Lerp(linear, Color.white, lift).gamma;
            displayed.a = source.a;
            return displayed;
        }
    }
}
