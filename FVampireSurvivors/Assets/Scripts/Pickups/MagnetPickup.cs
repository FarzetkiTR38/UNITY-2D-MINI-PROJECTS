using UnityEngine;

/// <summary>
/// Yerden toplanabilir mıknatıs. Player temas edince 
/// geçici olarak tüm XP orblarını çeker (magnet radius boost).
/// </summary>
public class MagnetPickup : MonoBehaviour
{
    [Header("Magnet Boost Settings")]
    [Tooltip("Boost aktif olduğunda magnet radius değeri")]
    public float boostRadius = 9999f;
    
    [Tooltip("Boost süresi (saniye)")]
    public float boostDuration = 1f;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // XPOrbGlobalSettings'e geçici boost uygula
            if (XPOrbGlobalSettings.instance != null)
            {
                XPOrbGlobalSettings.instance.ActivateMagnetBoost(boostRadius, boostDuration);
                
                // Efekt varsa spawn et
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[MagnetPickup] XPOrbGlobalSettings.instance bulunamadı!");
            }
        }
    }
}
