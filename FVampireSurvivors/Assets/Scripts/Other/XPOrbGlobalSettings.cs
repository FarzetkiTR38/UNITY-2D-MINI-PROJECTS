using UnityEngine;

public class XPOrbGlobalSettings : MonoBehaviour
{
    public static XPOrbGlobalSettings instance;

    public float baseMagnetRadius = 3f;
    public float currentMagnetRadius = 3f;

    private void Awake()
    {
        instance = this;
    }

    public void UpgradeMagnet(int level)
    {
        currentMagnetRadius = baseMagnetRadius + level * 1.2f;
    }
}
