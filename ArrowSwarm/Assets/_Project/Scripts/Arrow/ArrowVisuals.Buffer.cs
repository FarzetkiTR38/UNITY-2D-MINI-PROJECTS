namespace ArrowSwarm.Arrow
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Reuses a growing upload buffer for spawn growth, flight and blocked-bounce body positions.</summary>
    public partial class ArrowVisuals
    {
        private Vector3[] _bodyUploadBuffer = Array.Empty<Vector3>();

        private void UploadBodyPoints(List<Vector3> points)
        {
            int count = points.Count;
            if (_bodyUploadBuffer.Length < count)
                _bodyUploadBuffer = new Vector3[Mathf.NextPowerOfTwo(count)];
            points.CopyTo(_bodyUploadBuffer);
            _lineRenderer.positionCount = count;
            _lineRenderer.SetPositions(_bodyUploadBuffer);
        }
    }
}
