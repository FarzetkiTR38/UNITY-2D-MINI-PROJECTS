using UnityEngine;
using FishNet.Object;

public class PlayerCameraLink : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            CameraFollow.Instance.SetTarget(transform);
        }
    }
}
