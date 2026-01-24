using UnityEngine;

/// <summary>
/// LuckyBox drop tablosu için serializable item class.
/// Inspector'dan kolayca ayarlanabilir.
/// </summary>
[System.Serializable]
public class LuckyBoxDropItem
{
    [Tooltip("Drop edilecek prefab (HealthPickup, MagnetPickup, Gold vb.)")]
    public GameObject prefab;
    
    [Tooltip("Ağırlık değeri (yüksek = daha sık drop)")]
    [Range(0f, 100f)]
    public float weight = 1f;
}
