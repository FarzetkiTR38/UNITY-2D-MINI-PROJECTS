using UnityEngine;

/// <summary>
/// Yerden toplanabilir coin. Player temas edince rastgele miktarda gold verir.
/// </summary>
public class CoinPickup : MonoBehaviour
{
    [Header("Gold Settings")]
    [Tooltip("Minimum gold miktarı")]
    public int minGold = 50;
    
    [Tooltip("Maksimum gold miktarı")]
    public int maxGold = 150;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Rastgele gold miktarı hesapla
            int goldAmount = Random.Range(minGold, maxGold + 1);
            
            // GameManager'a gold ekle
            if (GoldManager.instance != null)
            {
                GoldManager.instance.AddGold(goldAmount);
            }
            else
            {
                Debug.LogWarning("[CoinPickup] GoldManager.instance bulunamadı!");
            }

            // Floating gold text göster
            if (DamageTextManager.Instance != null)
            {
                DamageTextManager.Instance.ShowGold(goldAmount, transform.position);
            }
            
            // Efekt varsa spawn et
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}

