namespace ArrowSwarm.Camera
{
    using UnityEngine;

    /// <summary>Reuses four world corners while reading the authored top and bottom HUD boundaries.</summary>
    public partial class CameraController
    {
        private readonly Vector3[] _hudCornerBuffer = new Vector3[4];
    }
}
