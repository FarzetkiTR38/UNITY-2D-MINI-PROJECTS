namespace ArrowSwarm.Puzzle
{
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>Reusable pooled arrow extraction pose; Bind restores normal colors and geometry.</summary>
    public sealed partial class PuzzleArrowView
    {
        /// <summary>Collapses the selected path into its head, then lifts and fades it out.</summary>
        public void RenderRocket(float progress, Vector3 lift)
        {
            RenderTravel(0f);
            float collapse = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress * 2f));
            float flight = progress * progress;
            Vector3 pivot = _path[_path.Length - 1];
            for (int i = 0; i < _body.positionCount; i++)
                _body.SetPosition(i, Vector3.Lerp(_body.GetPosition(i), pivot, collapse) + lift * flight);
            if (_usingFilledHead) _filledHeadTransform.position += lift * flight;
            else if (_head != null)
                for (int i = 0; i < _head.positionCount; i++) _head.SetPosition(i, _head.GetPosition(i) + lift * flight);
            Color color = _config.ThemeColor(_data.Color, ThemeManager.CurrentTheme);
            color.a *= 1f - progress;
            _body.startColor = _body.endColor = color;
            if (_head != null) _head.startColor = _head.endColor = color;
            if (_filledHead != null) _filledHead.color = color;
        }
    }
}
