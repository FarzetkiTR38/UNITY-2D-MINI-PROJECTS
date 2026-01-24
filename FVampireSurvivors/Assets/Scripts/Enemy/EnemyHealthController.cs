using UnityEngine;

/// <summary>
/// Health controller for enemies and bosses.
/// Implements IDamageable for floating damage text integration.
/// </summary>
public class EnemyHealthController : MonoBehaviour, IDamageable
{
    public static EnemyHealthController instance;

    private void Awake() 
    {
        instance = this;
    }

    public int maxHP = 30;
    private int currentHP;

    public GameObject xpOrbPrefab;
    public int xpAmount = 5;
    // buradaki mimari biraz kötü ama işe yarar:
    // xporb da zaten xpvalue var ama buradaki xpamount'u oraya atıyoruz 
    // bu sayede farklı enemylerde farklı xp vermesini sağlayabilcez

    [Header("Boss Settings")]
    public bool isBoss = false;
    public GameObject chestPrefab;

    [Header("Damage Text Settings")]
    [Tooltip("Y offset for damage text spawn position")]
    [SerializeField] private float damageTextYOffset = 0.5f;
    
    [Tooltip("Enable floating damage text")]
    [SerializeField] private bool showDamageText = true;

    private void Start()
    {
        currentHP = maxHP;
    }

    /// <summary>
    /// Take damage using DamageInfo (IDamageable interface).
    /// Shows floating damage text.
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        // Apply damage
        currentHP -= damageInfo.Amount;

        // Show floating damage text
        if (showDamageText && DamageTextManager.Instance != null)
        {
            // Use provided position or calculate from entity
            Vector3 textPosition = damageInfo.Position != Vector3.zero 
                ? damageInfo.Position 
                : GetDamageTextPosition();
            
            damageInfo.Position = textPosition;
            DamageTextManager.Instance.ShowDamage(damageInfo);
        }

        if (currentHP <= 0)
            Die();
    }

    /// <summary>
    /// Original TakeDamage method for backward compatibility.
    /// Existing skills can still call this without changes.
    /// </summary>
    public void TakeDamage(int dmg)
    {
        TakeDamage(DamageInfo.Normal(dmg, GetDamageTextPosition()));
    }

    /// <summary>
    /// Take critical damage (for skills with critical hit mechanics).
    /// </summary>
    public void TakeCriticalDamage(int dmg)
    {
        TakeDamage(DamageInfo.Critical(dmg, GetDamageTextPosition()));
    }

    /// <summary>
    /// Take DOT damage (poison, burn, etc.).
    /// </summary>
    public void TakeDOTDamage(int dmg)
    {
        TakeDamage(DamageInfo.DOT(dmg, GetDamageTextPosition()));
    }

    /// <summary>
    /// Get the world position where damage text should appear.
    /// </summary>
    public Vector3 GetDamageTextPosition()
    {
        return transform.position + Vector3.up * damageTextYOffset;
    }

    void Die()
    {
        // XP orb düşür
        if (xpOrbPrefab != null)
        {
            GameObject orb = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);

            XPOrb xp = orb.GetComponent<XPOrb>();
            if (xp != null)
            {
                xp.xpValue = xpAmount;
            }
        }

        // Boss ise chest düşür
        if (isBoss && chestPrefab != null)
        {
            Instantiate(chestPrefab, transform.position, Quaternion.identity);
            Debug.Log("[Boss] Chest dropped!");
        }

        Destroy(gameObject);
    }
}

