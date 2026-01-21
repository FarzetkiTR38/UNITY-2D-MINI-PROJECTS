using UnityEngine;

/// <summary>
/// Global passive stats that affect all skills and player.
/// Singleton - access via PassiveStats.instance
/// </summary>
public class PassiveStats : MonoBehaviour
{
    public static PassiveStats instance;

    [Header("Attack Modifiers")]
    [Tooltip("Multiplier for attack speed (1.0 = normal, 1.5 = 50% faster)")]
    public float attackSpeedMultiplier = 1f;

    [Tooltip("Bonus projectiles added to all multi-projectile skills")]
    public int bonusProjectileCount = 0;

    [Tooltip("Multiplier for all area/radius effects (1.0 = normal, 1.5 = 50% larger)")]
    public float areaSizeMultiplier = 1f;

    [Tooltip("Bonus damage added to all attacks")]
    public int bonusDamage = 0;

    [Tooltip("Damage multiplier (1.0 = normal, 1.5 = 50% more damage)")]
    public float damageMultiplier = 1f;

    [Header("Critical Hit")]
    [Tooltip("Critical hit chance (0.0 to 1.0)")]
    public float criticalChance = 0f;

    [Tooltip("Critical damage multiplier (2.0 = double damage on crit)")]
    public float criticalDamageMultiplier = 2f;

    [Header("Survivability")]
    [Tooltip("Lifesteal percentage (0.0 to 1.0, 0.1 = 10% of damage as health)")]
    public float lifestealPercent = 0f;

    [Tooltip("Health regeneration per second")]
    public float healthRegenPerSecond = 0f;

    [Tooltip("Damage reduction percentage (0.0 to 1.0, 0.2 = 20% less damage taken)")]
    public float damageReduction = 0f;

    [Header("Experience")]
    [Tooltip("XP gain multiplier (1.0 = normal, 1.5 = 50% more XP)")]
    public float xpGainMultiplier = 1f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // ==================
    // UPGRADE METHODS
    // ==================

    public void UpgradeAttackSpeed(int level)
    {
        // Each level adds 10% attack speed
        attackSpeedMultiplier = 1f + (level * 0.1f);
    }

    public void UpgradeProjectileCount(int level)
    {
        // Each level adds 1 projectile
        bonusProjectileCount = level;
    }

    public void UpgradeAreaSize(int level)
    {
        // Each level adds 15% area size
        areaSizeMultiplier = 1f + (level * 0.15f);
    }

    public void UpgradeDamage(int level)
    {
        // Each level adds 5 damage and 10% multiplier
        bonusDamage = level * 5;
        damageMultiplier = 1f + (level * 0.1f);
        Debug.Log($"[PassiveStats] UpgradeDamage Level {level}: bonusDamage={bonusDamage}, multiplier={damageMultiplier}");
    }

    public void UpgradeCriticalChance(int level)
    {
        // Each level adds 5% crit chance (max 25% at level 5)
        criticalChance = level * 0.05f;
    }

    public void UpgradeCriticalDamage(int level)
    {
        // Each level adds 25% crit damage (2.0 base + 1.25 at max)
        criticalDamageMultiplier = 2f + (level * 0.25f);
    }

    public void UpgradeLifesteal(int level)
    {
        // Each level adds 3% lifesteal
        lifestealPercent = level * 0.03f;
    }

    public void UpgradeHealthRegen(int level)
    {
        // Each level adds 1 HP/sec
        healthRegenPerSecond = level * 1f;
    }

    public void UpgradeArmor(int level)
    {
        // Each level adds 5% damage reduction (max 25%)
        damageReduction = level * 0.05f;
    }

    public void UpgradeXPGain(int level)
    {
        // Each level adds 10% XP gain
        xpGainMultiplier = 1f + (level * 0.1f);
    }

    // ==================
    // HELPER METHODS
    // ==================

    /// <summary>
    /// Calculate final damage with all modifiers
    /// </summary>
    public int CalculateDamage(int baseDamage)
    {
        float damage = (baseDamage + bonusDamage) * damageMultiplier;
        float preCritDamage = damage;

        // Check for critical hit
        float roll = Random.value;
        bool isCrit = roll < criticalChance;
        
        if (isCrit)
        {
            damage *= criticalDamageMultiplier;
            Debug.Log($"<color=yellow>💥 CRITICAL HIT!</color> Chance: {criticalChance * 100:F1}%, Roll: {roll * 100:F1}%, Damage: {preCritDamage:F0} → {damage:F0} (x{criticalDamageMultiplier:F2})");
        }
        else
        {
            Debug.Log($"Normal Hit - Chance: {criticalChance * 100:F1}%, Roll: {roll * 100:F1}%, Damage: {damage:F0}");
        }

        return Mathf.RoundToInt(damage);
    }

    /// <summary>
    /// Apply lifesteal healing based on damage dealt
    /// </summary>
    public void ApplyLifesteal(int damageDealt)
    {
        if (lifestealPercent <= 0f) return;

        float healAmount = damageDealt * lifestealPercent;
        if (healAmount > 0 && PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.Heal(healAmount);
        }
    }

    /// <summary>
    /// Calculate damage taken after armor reduction
    /// </summary>
    public float CalculateDamageTaken(float incomingDamage)
    {
        return incomingDamage * (1f - damageReduction);
    }

    /// <summary>
    /// Calculate XP with bonus
    /// </summary>
    public int CalculateXP(int baseXP)
    {
        return Mathf.RoundToInt(baseXP * xpGainMultiplier);
    }

    /// <summary>
    /// Get total projectile count (base + bonus)
    /// </summary>
    public int GetTotalProjectileCount(int baseCount)
    {
        return baseCount + bonusProjectileCount;
    }

    public float GetScaledArea(float baseArea)
    {
        return baseArea * areaSizeMultiplier;
    }

    /// <summary>
    /// Get attack interval with speed modifier
    /// </summary>
    public float GetAttackInterval(float baseInterval)
    {
        return baseInterval / attackSpeedMultiplier;
    }
}
