using UnityEngine;

/// <summary>
/// Shockwave Pulse - Periodically releases a damaging wave in all directions
/// Level increases: damage, radius
/// </summary>
public class ShockwavePulse : MonoBehaviour
{
    [Header("Shockwave Settings")]
    public GameObject shockwaveEffectPrefab;

    [Header("Stats")]
    public float basePulseInterval = 2f;
    public int baseDamage = 20;
    public float baseRadius = 1.5f;

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.25f;

    private int currentLevel = 0;
    private float pulseTimer = 0f;
    private GameObject activeEffect;

    void Update()
    {
        if (currentLevel <= 0)
        {
            if (activeEffect != null)
            {
                Destroy(activeEffect);
                activeEffect = null;
            }
            return;
        }

        // Spawn visual effect if not exists
        if (activeEffect == null && shockwaveEffectPrefab != null)
        {
            activeEffect = Instantiate(shockwaveEffectPrefab, transform);
            activeEffect.transform.localPosition = Vector3.zero;
        }

        // Update effect scale based on radius
        if (activeEffect != null)
        {
            float scale = GetRadius();
            activeEffect.transform.localScale = Vector3.one * scale;
        }

        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(basePulseInterval) 
            : basePulseInterval;

        pulseTimer += Time.deltaTime;
        if (pulseTimer >= interval)
        {
            ReleaseShockwave();
            pulseTimer = 0f;
        }
    }

    void ReleaseShockwave()
    {
        // Deal damage in radius
        float radius = GetRadius();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                int damage = GetDamage();
                damageable.TakeDamage(DamageInfo.Normal(damage, damageable.GetDamageTextPosition()));

                if (PassiveStats.instance != null)
                    PassiveStats.instance.ApplyLifesteal(damage);
            }

            // Apply knockback via EnemyController
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                enemy.ApplyKnockback(knockbackDir, knockbackForce, knockbackDuration);
            }
        }
    }

    float GetRadius()
    {
        // Level 1: 1.5, Level 2: 1.75, Level 3: 2.0, Level 4: 2.25, Level 5: 2.5
        float radius = baseRadius + ((currentLevel - 1) * 0.25f);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetScaledArea(radius) 
            : radius;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 10);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
    }
}
