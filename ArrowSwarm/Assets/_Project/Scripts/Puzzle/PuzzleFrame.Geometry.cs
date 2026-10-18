namespace ArrowSwarm.Puzzle
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed partial class PuzzleFrame
    {
        private void BuildRoundedWalls(float x, float y)
        {
            if (_walls == null) return;
            // Keep every arrow's exit ray on its flat logical side, outside the corner blend.
            float available = Mathf.Max(.01f, _config.WallOffset - _config.WallWidth * .5f);
            float radius = Mathf.Min(_cornerRadius, available, x * .8f, y * .8f);
            _cornerBlendLength = radius * Mathf.PI * .25f;
            int samples = Mathf.Max(2, _cornerSamples);
            Vector2[] centers = { new Vector2(-x + radius, y - radius), new Vector2(x - radius, y - radius),
                new Vector2(x - radius, -y + radius), new Vector2(-x + radius, -y + radius) };
            for (int side = 0; side < Mathf.Min(4, _walls.Length); side++)
            {
                var wall = _walls[side];
                if (wall == null) continue;
                float angle = 90f - 90f * side;
                var points = new List<Vector3>();
                for (int step = 0; step <= samples; step++)
                    points.Add(ArcPoint(centers[side], radius, angle + 45f - 45f * step / samples));
                Vector3 start = points[points.Count - 1];
                Vector3 end = ArcPoint(centers[(side + 1) % 4], radius, angle);
                int straightSamples = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(start, end) * _samplesPerUnit));
                for (int step = 1; step <= straightSamples; step++) points.Add(Vector3.Lerp(start, end, step / (float)straightSamples));
                for (int step = 1; step <= samples; step++)
                    points.Add(ArcPoint(centers[(side + 1) % 4], radius, angle - 45f * step / samples));
                _wallLengths[side] = 0f;
                for (int step = 1; step < points.Count; step++) _wallLengths[side] += Vector3.Distance(points[step - 1], points[step]);
                wall.useWorldSpace = false; wall.loop = false; wall.numCapVertices = 6;
                wall.positionCount = points.Count; wall.SetPositions(points.ToArray());
                wall.widthMultiplier = _config.WallWidth;
            }
        }

        private static Vector3 ArcPoint(Vector2 center, float radius, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
        }
    }
}
