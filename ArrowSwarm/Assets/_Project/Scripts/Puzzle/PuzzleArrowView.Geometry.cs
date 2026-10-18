namespace ArrowSwarm.Puzzle
{
    using UnityEngine;

    /// <summary>Width-aware head overlap and cached rounded stroke geometry for puzzle arrows.</summary>
    public sealed partial class PuzzleArrowView
    {
        private const float GeometryEpsilon = .0001f;
        private Vector3[] _strokePoints;
        private float[] _strokeDistances;
        private int _strokeCount, _curveSteps;
        private float _headInset;
        private static Sprite _measuredHead;
        private static float _measuredSlope, _measuredTipMargin;

        private void MeasureHeadInset()
        {
            Sprite sprite = _filledHead.sprite;
            if (_measuredHead != sprite)
            {
                Bounds bounds = sprite.bounds;
                float texel = bounds.size.y / Mathf.Max(1f, sprite.rect.height);
                float height = Mathf.Max(texel, bounds.size.y - texel * 2f);
                float halfWidth = Mathf.Max(texel, bounds.extents.x - texel * 2f);
                _measuredTipMargin = texel * 2f;
                Texture2D texture = sprite.texture;
                if (texture != null && texture.isReadable && !sprite.packed)
                {
                    Color32[] pixels = texture.GetPixels32();
                    Rect rect = sprite.rect;
                    int minX = (int)rect.width, minY = (int)rect.height, maxX = -1, maxY = -1;
                    for (int y = 0; y < (int)rect.height; y++)
                        for (int x = 0; x < (int)rect.width; x++)
                        {
                            if (pixels[((int)rect.y + y) * texture.width + (int)rect.x + x].a == 0) continue;
                            minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                            minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                        }
                    if (maxY > minY && maxX > minX)
                    {
                        height = (maxY - minY + 1f) / sprite.pixelsPerUnit;
                        halfWidth = Mathf.Max(1f, maxX - minX - 1f) * .5f / sprite.pixelsPerUnit;
                        _measuredTipMargin = bounds.max.y - (maxY + 1f - sprite.pivot.y) / sprite.pixelsPerUnit;
                    }
                }
                _measuredSlope = halfWidth / height;
                _measuredHead = sprite;
            }
            // A circular cap must clear both sloping triangle edges, including transparent sprite padding.
            float radius = _config.StrokeWidth * .5f;
            _headInset = Mathf.Max(0f, _measuredTipMargin) * _filledHeadBaseScale.y
                + radius * Mathf.Sqrt(1f + _measuredSlope * _measuredSlope) / _measuredSlope;
        }

        private void PrepareStroke()
        {
            if (_curveSteps == 0) _curveSteps = Mathf.Max(2, _body.numCornerVertices);
            _body.numCornerVertices = 0;
            int capacity = _path.Length * (_curveSteps + 1) + 2;
            if (_strokePoints == null || _strokePoints.Length < capacity)
            {
                _strokePoints = new Vector3[capacity];
                _strokeDistances = new float[capacity];
            }
            _strokeCount = 0;
            AddStrokePoint(_path[0], 0f);
            float strokeRadius = _config.StrokeWidth * _headScale * .5f;
            float inset = _usingFilledHead ? _headInset * _headScale : 0f;
            for (int i = 1; i < _path.Length - 1; i++)
            {
                Vector3 incoming = (_path[i] - _path[i - 1]).normalized;
                Vector3 outgoing = (_path[i + 1] - _path[i]).normalized;
                if (Vector3.Dot(incoming, outgoing) > 1f - GeometryEpsilon) continue;
                float radius = Mathf.Min(strokeRadius, .5f);
                // Keep a tangent-aligned head join without restoring a sharp corner when a large head covers it.
                float minimumRadius = GeometryEpsilon * (_curveSteps + 1) * 2f;
                radius = Mathf.Min(radius, Mathf.Max(minimumRadius, (_length - inset - i) * .5f));
                Vector3 center = _path[i] - incoming * radius + outgoing * radius;
                for (int step = 0; step <= _curveSteps; step++)
                {
                    float progress = (float)step / _curveSteps;
                    float angle = progress * Mathf.PI * .5f;
                    Vector3 point = center - outgoing * (Mathf.Cos(angle) * radius)
                        + incoming * (Mathf.Sin(angle) * radius);
                    AddStrokePoint(point, i - radius + progress * radius * 2f);
                }
            }
            AddStrokePoint(_path[_path.Length - 1], _length);
        }

        private void AddStrokePoint(Vector3 point, float distance)
        {
            _strokePoints[_strokeCount] = point;
            _strokeDistances[_strokeCount++] = distance;
        }

        private void RenderStroke(float begin, float end)
        {
            if (end - begin < GeometryEpsilon) { _body.positionCount = 0; return; }
            int count = 2;
            for (int i = 1; i < _strokeCount; i++)
                if (_strokeDistances[i] > begin && _strokeDistances[i] < end) count++;
            _body.positionCount = count;
            int index = 0;
            _body.SetPosition(index++, StrokePointAt(begin));
            for (int i = 1; i < _strokeCount; i++)
                if (_strokeDistances[i] > begin && _strokeDistances[i] < end)
                    _body.SetPosition(index++, _strokePoints[i]);
            _body.SetPosition(index, StrokePointAt(end));
        }

        private Vector3 StrokePointAt(float distance)
        {
            if (distance >= _length) return PointAt(distance);
            for (int i = 1; i < _strokeCount; i++)
                if (distance <= _strokeDistances[i])
                    return Vector3.Lerp(_strokePoints[i - 1], _strokePoints[i],
                        Mathf.InverseLerp(_strokeDistances[i - 1], _strokeDistances[i], distance));
            return _strokePoints[_strokeCount - 1];
        }
    }
}
