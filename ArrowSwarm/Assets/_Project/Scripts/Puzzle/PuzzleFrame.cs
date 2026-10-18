namespace ArrowSwarm.Puzzle
{
    using System.Collections;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Rounded fixed border whose paint flows by a quarter turn, independently of arrows.</summary>
    public sealed partial class PuzzleFrame : MonoBehaviour
    {
        [SerializeField] private LineRenderer[] _walls;
        [Header("Rounded Border")]
        [SerializeField, Min(.01f)] private float _cornerRadius = .8f;
        [SerializeField, Range(2, 24)] private int _cornerSamples = 8;
        [SerializeField, Range(1f, 12f)] private float _samplesPerUnit = 4f;
        [Header("Paint Flow")]
        [Tooltip("Soft transition width in board units; steady wall centers keep their exact logical color.")]
        [SerializeField, Min(.01f)] private float _paintFeather = .35f;
        [SerializeField] private AnimationCurve _flowEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
        private PuzzleLevel _level;
        private PuzzleConfig _config;
        private ThemeMode _theme;
        private int _generation, _orientation;
        private float _paintOffset, _cornerBlendLength;
        private readonly float[] _wallLengths = new float[4];
        public bool IsAnimating { get; private set; }
        public int VisualOrientation => _orientation;
        public float PaintPhase => _paintOffset;

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged += ApplyTheme;
            ApplyTheme(ThemeManager.CurrentTheme);
        }

        private void OnDisable()
        {
            ThemeManager.OnThemeChanged -= ApplyTheme;
            CancelFlow();
        }

        private void ApplyTheme(ThemeMode theme)
        {
            _theme = theme;
            PaintWalls();
        }

        /// <summary>Sets level-dependent wall geometry and palette, leaving arrow transforms alone.</summary>
        public void Setup(PuzzleLevel level, PuzzleConfig config)
        {
            _generation++; IsAnimating = false;
            _level = level; _config = config; _orientation = level.InitialOrientation & 1;
            _paintOffset = _orientation;
            transform.rotation = Quaternion.identity;
            bool visible = level.Mode == GameModeId.ColorShift;
            if (!visible) { gameObject.SetActive(false); return; }
            float x = (level.Width - 1) * .5f + config.WallOffset;
            float y = (level.Height - 1) * .5f + config.WallOffset;
            BuildRoundedWalls(x, y);
            ApplyTheme(ThemeManager.CurrentTheme);
            gameObject.SetActive(true);
        }

        /// <summary>Flows paint clockwise/right or counterclockwise/left. Session gates shots until complete.</summary>
        public IEnumerator Rotate(int direction, float duration)
        {
            if (_level == null || _level.Mode != GameModeId.ColorShift) yield break;
            int generation = ++_generation;
            float from = _paintOffset, to = from + (direction > 0 ? 1f : -1f);
            float elapsed = 0f; duration = Mathf.Max(.01f, duration); IsAnimating = true;
            try
            {
                while (elapsed < duration)
                {
                    if (generation != _generation) yield break;
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / duration);
                    float eased = _flowEasing != null ? Mathf.Clamp01(_flowEasing.Evaluate(progress)) : Mathf.SmoothStep(0f, 1f, progress);
                    _paintOffset = Mathf.Lerp(from, to, eased);
                    PaintWalls();
                    yield return null;
                }
                if (generation != _generation) yield break;
                _paintOffset = Mathf.Repeat(to, 2f); _orientation ^= 1;
                PaintWalls();
            }
            finally { if (generation == _generation) IsAnimating = false; }
        }

        private void CancelFlow()
        {
            _generation++; IsAnimating = false; _paintOffset = _orientation;
            PaintWalls();
        }
    }
}
