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
        [Tooltip("Maximum legal escape animation duration; long paths speed up rather than wait outside the board.")]
        [SerializeField, Min(.1f)] private float _maximumFlightDuration = .7f;
        [SerializeField, Min(.01f)] private float _rotationDuration = .3f;
        [SerializeField, Min(.01f)] private float _hitRadius = .38f;
        [SerializeField, Min(1)] private float _swipeThreshold = 60f;
        [SerializeField, Min(1)] private float _tapThreshold = 25f;
        [SerializeField, Min(.01f)] private float _wallOffset = .9f;
        [SerializeField, Min(.01f)] private float _wallWidth = .3f;
        [SerializeField, Min(1)] private int _poolSize = 96;
        [Header("Rescue Rocket (Puzzle Modes Only)")]
        [SerializeField, Min(0)] private int _rocketCharges = 1;
        [SerializeField, Min(.1f)] private float _rocketDuration = .7f;
        [SerializeField, Range(.1f, 1f)] private float _rocketLiftFraction = .4f;
        [SerializeField, Range(.03f, .3f)] private float _rocketIconFraction = .12f;
        [Header("Dark Theme Palette Presentation")]
        [Tooltip("Minimum displayed linear luminance on dark boards. Logical level colors remain unchanged.")]
        [SerializeField, Range(0f, 1f)] private float _minimumDarkLuminance = .55f;
        [Tooltip("Maximum displayed linear luminance on the light grid, keeping pale silhouette details readable.")]
        [SerializeField, Range(.1f, 1f)] private float _maximumLightLuminance = .38f;
        public int Lives => _lives;
        public float StrokeWidth => _strokeWidth;
        public float HeadSize => _headSize;
        public float Speed => _speed;
        public float MaximumFlightDuration => _maximumFlightDuration;
        public float RotationDuration => _rotationDuration;
        public float HitRadius => _hitRadius;
        public float SwipeThreshold => _swipeThreshold;
        public float TapThreshold => _tapThreshold;
        public float WallOffset => _wallOffset;
        public float WallWidth => _wallWidth;
        public int PoolSize => _poolSize;
        public int RocketCharges => _rocketCharges;
        public float RocketDuration => _rocketDuration;
        public float RocketLiftFraction => _rocketLiftFraction;
        public float RocketIconFraction => _rocketIconFraction;

        /// <summary>Returns the same readable display color for arrows and their matching frame walls.</summary>
        public Color ThemeColor(Color source, ThemeMode theme)
        {
            Color linear = source.linear;
            float luminance = .2126f * linear.r + .7152f * linear.g + .0722f * linear.b;
            if (theme != ThemeMode.Dark)
            {
                float maximum = Mathf.Clamp(_maximumLightLuminance, .1f, 1f);
                if (luminance <= maximum) return source;
                Color shaded = (linear * (maximum / Mathf.Max(.0001f, luminance))).gamma;
                shaded.a = source.a; return shaded;
            }
            float minimum = Mathf.Clamp01(_minimumDarkLuminance);
            if (luminance >= minimum) return source;
            float lift = (minimum - luminance) / Mathf.Max(.0001f, 1f - luminance);
            Color displayed = Color.Lerp(linear, Color.white, lift).gamma;
            displayed.a = source.a;
            return displayed;
        }
    }
}
