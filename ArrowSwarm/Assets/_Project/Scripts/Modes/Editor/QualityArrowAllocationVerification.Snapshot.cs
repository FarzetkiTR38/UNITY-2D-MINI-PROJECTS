namespace ArrowSwarm.Modes.Editor
{
    using ArrowSwarm.Arrow;
    using UnityEngine;

    /// <summary>Preserves the profiled arrow's exact existing render state without changing its game data.</summary>
    public static partial class QualityArrowAllocationVerification
    {
        private readonly struct VisualSnapshot
        {
            private readonly Vector3[] _points;
            private readonly Vector3 _headPosition, _headScale;
            private readonly Quaternion _headRotation;
            private readonly bool _headEnabled, _lineEnabled;

            internal VisualSnapshot(ArrowVisuals visuals, LineRenderer line)
            {
                _points = new Vector3[line.positionCount]; line.GetPositions(_points);
                _headPosition = visuals.HeadTransform.position; _headRotation = visuals.HeadTransform.rotation;
                _headScale = visuals.HeadTransform.localScale;
                _headEnabled = visuals.HeadTransform.GetComponent<SpriteRenderer>().enabled; _lineEnabled = line.enabled;
            }

            internal void Restore(ArrowVisuals visuals, LineRenderer line)
            {
                line.positionCount = _points.Length; line.SetPositions(_points); line.enabled = _lineEnabled;
                visuals.HeadTransform.position = _headPosition; visuals.HeadTransform.rotation = _headRotation;
                visuals.HeadTransform.localScale = _headScale; visuals.HeadTransform.GetComponent<SpriteRenderer>().enabled = _headEnabled;
            }

            internal bool Matches(ArrowVisuals visuals, LineRenderer line)
            {
                if (line.positionCount != _points.Length || line.enabled != _lineEnabled ||
                    visuals.HeadTransform.position != _headPosition || visuals.HeadTransform.localScale != _headScale ||
                    Quaternion.Angle(visuals.HeadTransform.rotation, _headRotation) > .001f ||
                    visuals.HeadTransform.GetComponent<SpriteRenderer>().enabled != _headEnabled) return false;
                for (int index = 0; index < _points.Length; index++) if (line.GetPosition(index) != _points[index]) return false;
                return true;
            }
        }
    }
}
