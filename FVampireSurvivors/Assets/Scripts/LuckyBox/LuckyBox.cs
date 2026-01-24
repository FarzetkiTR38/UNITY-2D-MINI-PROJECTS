using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LuckyBox - Tek vuruşta kırılan ve içinden şansa bağlı item düşüren kutu.
/// IDamageable implement ederek skill'lerin vurabilmesini sağlar.
/// </summary>
public class LuckyBox : MonoBehaviour, IDamageable
{
    [Header("Drop Table")]
    [Tooltip("Düşürebileceği itemler ve ağırlıkları")]
    public List<LuckyBoxDropItem> dropTable = new List<LuckyBoxDropItem>();

    [Header("Visual Settings")]
    [SerializeField] private float damageTextYOffset = 0.5f;
    [SerializeField] private GameObject destroyEffect;

    [Header("Guaranteed Drop")]
    [Tooltip("Her zaman bir item düşür (false = hiçbir şey düşmeme ihtimali var)")]
    public bool guaranteedDrop = true;

    [Tooltip("Hiçbir şey düşmeme ağırlığı (guaranteedDrop false ise)")]
    public float nothingWeight = 0f;

    private bool isDead = false;

    /// <summary>
    /// IDamageable - Hasar alınca hemen ölür (tek vuruşta kırılır).
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isDead) return;
        isDead = true;

        // Hasar text göster
        if (DamageTextManager.Instance != null)
        {
            Vector3 textPos = damageInfo.Position != Vector3.zero 
                ? damageInfo.Position 
                : GetDamageTextPosition();
            damageInfo.Position = textPos;
            DamageTextManager.Instance.ShowDamage(damageInfo);
        }

        Die();
    }

    /// <summary>
    /// Geriye uyumluluk için basit int hasar.
    /// </summary>
    public void TakeDamage(int damage)
    {
        TakeDamage(DamageInfo.Normal(damage, GetDamageTextPosition()));
    }

    /// <summary>
    /// IDamageable - Damage text pozisyonu.
    /// </summary>
    public Vector3 GetDamageTextPosition()
    {
        return transform.position + Vector3.up * damageTextYOffset;
    }

    private void Die()
    {
        // Efekt varsa spawn et
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // Item drop et
        DropItem();

        Destroy(gameObject);
    }

    private void DropItem()
    {
        if (dropTable == null || dropTable.Count == 0)
        {
            Debug.LogWarning("[LuckyBox] Drop table boş!");
            return;
        }

        // Toplam ağırlığı hesapla
        float totalWeight = 0f;
        foreach (var item in dropTable)
        {
            if (item != null && item.prefab != null && item.weight > 0f)
            {
                totalWeight += item.weight;
            }
        }

        // Hiçbir şey düşmeme ağırlığı ekle
        if (!guaranteedDrop)
        {
            totalWeight += nothingWeight;
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("[LuckyBox] Toplam ağırlık 0!");
            return;
        }

        // Rastgele seç
        float roll = Random.Range(0f, totalWeight);

        foreach (var item in dropTable)
        {
            if (item == null || item.prefab == null || item.weight <= 0f)
                continue;

            if (roll < item.weight)
            {
                // Bu item'ı spawn et
                Instantiate(item.prefab, transform.position, Quaternion.identity);
                return;
            }

            roll -= item.weight;
        }

        // Buraya gelirse hiçbir şey düşmedi (nothingWeight kazandı)
    }
}
