using UnityEngine;

/// <summary>
/// Yerden toplanabilir kalp. Player temas edince belirtilen miktarda can doldurur.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    [Header("Heal Settings")]
    [Tooltip("Dolduracağı HP miktarı")]
    public float healAmount = 50f;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
            
            if (playerHealth != null)
            {
                // Can dolu değilse iyileştir
                if (playerHealth.currentHealth < playerHealth.maxHealth)
                {
                    playerHealth.Heal(healAmount);
                    
                    // Efekt varsa spawn et
                    if (pickupEffect != null)
                    {
                        Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    }
                    
                    Destroy(gameObject);
                }
                // Can doluysa pickup almaz (isteğe bağlı: yorum satırını kaldırırsan her zaman alır)
                // else { Destroy(gameObject); }
            }
        }
    }
}
