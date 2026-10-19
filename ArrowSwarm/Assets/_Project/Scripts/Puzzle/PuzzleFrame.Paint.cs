namespace ArrowSwarm.Puzzle
{
    using System;
    using ArrowSwarm.Modes;
    using UnityEngine;

    public sealed partial class PuzzleFrame
    {
        private readonly Gradient[] _gradients = new Gradient[4];
        private readonly float[] _gradientTimes = new float[8];
        private readonly GradientColorKey[][] _gradientBuffers = new GradientColorKey[9][];
        private readonly GradientAlphaKey[] _opacity = { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

        private void PaintWalls()
        {
            if (_level == null || _config == null || _level.Mode != GameModeId.ColorShift || _walls == null) return;
            Color first = _config.ThemeColor(_level.WallColors[0], _theme);
            Color second = _config.ThemeColor(_level.WallColors[1], _theme);
            float longest = Mathf.Max(Mathf.Max(_wallLengths[0], _wallLengths[1]), Mathf.Max(_wallLengths[2], _wallLengths[3]));
            float feather = Mathf.Clamp(Mathf.Min(_paintFeather, _cornerBlendLength) / Mathf.Max(.001f, longest), .00001f, .2f);
            for (int side = 0; side < Mathf.Min(4, _walls.Length); side++)
            {
                if (_walls[side] == null || _wallLengths[side] <= 0f) continue;
                float start = side - _paintOffset;
                int count = 2; _gradientTimes[0] = 0f; _gradientTimes[1] = 1f;
                for (int band = Mathf.FloorToInt(start) - 1; band <= Mathf.CeilToInt(start + 1f) + 1; band++)
                {
                    float center = band - start;
                    AddTime(center - feather, ref count); AddTime(center, ref count); AddTime(center + feather, ref count);
                }
                Array.Sort(_gradientTimes, 0, count);
                var keys = _gradientBuffers[count] ?? (_gradientBuffers[count] = new GradientColorKey[count]);
                for (int index = 0; index < count; index++)
                    keys[index] = new GradientColorKey(PaintColor(start + _gradientTimes[index], feather, first, second), _gradientTimes[index]);
                var gradient = _gradients[side] ?? (_gradients[side] = new Gradient());
                gradient.mode = GradientMode.Blend; gradient.SetKeys(keys, _opacity);
                _walls[side].colorGradient = gradient;
            }
        }

        private void AddTime(float value, ref int count)
        {
            if (value <= .00001f || value >= .99999f) return;
            for (int index = 0; index < count; index++) if (Mathf.Abs(_gradientTimes[index] - value) < .00001f) return;
            if (count < _gradientTimes.Length) _gradientTimes[count++] = value;
        }

        private static Color PaintColor(float position, float feather, Color first, Color second)
        {
            int band = Mathf.FloorToInt(position);
            float fraction = position - band;
            Color current = (band & 1) == 0 ? first : second;
            Color adjacent = (band & 1) == 0 ? second : first;
            if (fraction < feather)
                return Color.Lerp(adjacent, current, Mathf.SmoothStep(0f, 1f, (fraction + feather) / (2f * feather)));
            if (fraction > 1f - feather)
                return Color.Lerp(current, adjacent, Mathf.SmoothStep(0f, 1f, (fraction - 1f + feather) / (2f * feather)));
            return current;
        }
    }
}
