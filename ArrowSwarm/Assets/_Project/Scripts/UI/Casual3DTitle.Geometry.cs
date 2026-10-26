namespace ArrowSwarm.UI
{
    using TMPro;
    using UnityEngine;

    /// <summary>Preserves the authored equal-spacing, optical kerning and per-character arch across the four title layers.</summary>
    public partial class Casual3DTitle
    {
        private void WarpLayer(TextMeshProUGUI tmp, float[] shifts, float centerShift)
        {
            if (tmp == null || string.IsNullOrEmpty(_text)) return;
            tmp.ForceMeshUpdate(true, true);
            TMP_TextInfo textInfo = tmp.textInfo;
            int count = textInfo.characterCount;
            if (count <= 0) return;

            float minX = float.MaxValue, maxX = float.MinValue;
            for (int i = 0; i < count; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;
                int vIdx = textInfo.characterInfo[i].vertexIndex;
                int mIdx = textInfo.characterInfo[i].materialReferenceIndex;
                var v = textInfo.meshInfo[mIdx].vertices;
                float s = shifts[i] + centerShift;
                v[vIdx].x += s; v[vIdx + 1].x += s; v[vIdx + 2].x += s; v[vIdx + 3].x += s;
                minX = Mathf.Min(minX, v[vIdx].x, v[vIdx + 1].x, v[vIdx + 2].x, v[vIdx + 3].x);
                maxX = Mathf.Max(maxX, v[vIdx].x, v[vIdx + 1].x, v[vIdx + 2].x, v[vIdx + 3].x);
            }

            if (!_enableCurve) { tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices); return; }
            float width = maxX - minX;
            if (width <= 0.001f) { tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices); return; }

            float effCurve = count <= 3 ? _curveScale * 0.35f : _curveScale;
            for (int i = 0; i < count; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;
                int vIdx = textInfo.characterInfo[i].vertexIndex;
                int mIdx = textInfo.characterInfo[i].materialReferenceIndex;
                Vector3[] v = textInfo.meshInfo[mIdx].vertices;
                Vector3 mid = new Vector2((v[vIdx].x + v[vIdx + 2].x) * 0.5f, textInfo.characterInfo[i].baseLine);
                v[vIdx] -= mid; v[vIdx + 1] -= mid; v[vIdx + 2] -= mid; v[vIdx + 3] -= mid;

                float x0 = Mathf.Clamp01((mid.x - minX) / width);
                float x1 = Mathf.Clamp01(x0 + 0.001f);
                float y0 = _vertexCurve.Evaluate(x0) * effCurve;
                float y1 = _vertexCurve.Evaluate(x1) * effCurve;

                Vector3 tangent = new Vector3((x1 - x0) * width, y1 - y0, 0f);
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg * _rotationMultiplier;
                Matrix4x4 mat = Matrix4x4.TRS(new Vector3(0f, y0, 0f), Quaternion.Euler(0f, 0f, angle), Vector3.one);

                v[vIdx] = mat.MultiplyPoint3x4(v[vIdx]) + mid;
                v[vIdx + 1] = mat.MultiplyPoint3x4(v[vIdx + 1]) + mid;
                v[vIdx + 2] = mat.MultiplyPoint3x4(v[vIdx + 2]) + mid;
                v[vIdx + 3] = mat.MultiplyPoint3x4(v[vIdx + 3]) + mid;
            }
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }

        private float GetOpticalNudge(char c1, char c2)
        {
            c1 = char.ToUpperInvariant(c1); c2 = char.ToUpperInvariant(c2);
            if (c1 == 'A' && "TVWY".IndexOf(c2) >= 0) return -3.5f;
            if ("VWY".IndexOf(c1) >= 0 && c2 == 'A') return -3.5f;
            if (c1 == 'T' && "EAO".IndexOf(c2) >= 0) return -3.0f;
            return 0f;
        }
    }
}
