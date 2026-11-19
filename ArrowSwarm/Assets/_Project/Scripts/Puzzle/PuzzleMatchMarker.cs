namespace ArrowSwarm.Puzzle
{
    using UnityEngine;

    /// <summary>Draws a small circle or diamond on matching ColorShift arrows and exits.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class PuzzleMatchMarker : MonoBehaviour
    {
        [SerializeField] private LineRenderer _stroke;
        [SerializeField, Range(.05f, .4f)] private float _strokeFraction = .18f;
        [SerializeField] private Color _lightInk = Color.white;
        [SerializeField] private Color _darkInk = new Color(.02f, .035f, .10f);

        private static readonly Vector3[] Circle = BuildCircle();
        private static readonly Vector3[] Diamond =
        {
            Vector3.up * .5f, Vector3.right * .5f,
            Vector3.down * .5f, Vector3.left * .5f
        };
        private int _key = -1;

        /// <summary>Shows the pattern with ink chosen for the visible wall or arrow color.</summary>
        public void Show(int key, Color surface, float diameter)
        {
            if (_key != key)
            {
                _key = key;
                var points = key == 0 ? Circle : Diamond;
                _stroke.positionCount = points.Length;
                _stroke.SetPositions(points);
            }
            transform.localScale = Vector3.one * diameter;
            _stroke.widthMultiplier = diameter * _strokeFraction;
            Color linear = surface.linear;
            float lightness = .2126f * linear.r + .7152f * linear.g + .0722f * linear.b;
            _stroke.startColor = _stroke.endColor = lightness >= .35f ? _darkInk : _lightInk;
            _stroke.enabled = true;
        }

        /// <summary>Clears the mark while the board or arrow is changing state.</summary>
        public void Hide() => _stroke.enabled = false;

        private static Vector3[] BuildCircle()
        {
            var points = new Vector3[16];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / points.Length;
                points[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * .5f;
            }
            return points;
        }
    }
}
