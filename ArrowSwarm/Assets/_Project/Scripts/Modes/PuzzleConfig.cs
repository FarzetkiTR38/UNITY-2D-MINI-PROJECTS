namespace ArrowSwarm.Modes
{
    using UnityEngine;

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
    }
}
