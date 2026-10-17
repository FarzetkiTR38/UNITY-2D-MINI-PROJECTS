namespace ArrowSwarm.Puzzle
{
    using System.Collections;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Four scene-authored wall strokes; only this transform rotates.</summary>
    public sealed class PuzzleFrame : MonoBehaviour
    {
        [SerializeField] private LineRenderer[] _walls;
        private PuzzleLevel _level;
        private PuzzleConfig _config;

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged += ApplyTheme;
            ApplyTheme(ThemeManager.CurrentTheme);
        }

        private void OnDisable() => ThemeManager.OnThemeChanged -= ApplyTheme;

        private void ApplyTheme(ThemeMode theme)
        {
            if (_level == null || _config == null) return;
            for (int i = 0; i < _walls.Length; i++)
            {
                Color color = _config.ThemeColor(_level.WallColors[(i + _level.InitialOrientation) & 1], theme);
                _walls[i].startColor = _walls[i].endColor = color;
            }
        }

        /// <summary>Sets level-dependent wall geometry and palette, leaving arrow transforms alone.</summary>
        public void Setup(PuzzleLevel level, PuzzleConfig config)
        {
            _level = level; _config = config;
            gameObject.SetActive(level.Mode == GameModeId.ColorShift);
            transform.rotation = Quaternion.identity;
            float x = (level.Width - 1) * .5f + config.WallOffset;
            float y = (level.Height - 1) * .5f + config.WallOffset;
            Vector3[] corners = { new Vector3(-x,y),new Vector3(x,y),new Vector3(x,-y),new Vector3(-x,-y) };
            for (int i = 0; i < _walls.Length; i++)
            {
                _walls[i].positionCount = 2; _walls[i].useWorldSpace = false;
                _walls[i].SetPosition(0,corners[i]); _walls[i].SetPosition(1,corners[(i+1)%4]);
                _walls[i].widthMultiplier = config.WallWidth;
            }
            ApplyTheme(ThemeManager.CurrentTheme);
        }

        /// <summary>Animates exactly a quarter turn, preserving wall colors and stationary arrows.</summary>
        public IEnumerator Rotate(int direction, float duration)
        {
            Quaternion from = transform.rotation;
            Quaternion to = from * Quaternion.Euler(0,0,direction > 0 ? -90 : 90);
            float elapsed = 0;
            while (elapsed < duration) { elapsed += Time.deltaTime; transform.rotation = Quaternion.Slerp(from,to,Mathf.SmoothStep(0,1,elapsed/duration)); yield return null; }
            transform.rotation = to;
        }
    }
}
