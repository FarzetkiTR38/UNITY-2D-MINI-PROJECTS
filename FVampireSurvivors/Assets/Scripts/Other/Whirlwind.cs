using UnityEngine;

/// <summary>
/// Whirlwind / Spin Attack - Continuous AoE damage around the player
/// Level increases: damage, radius
/// </summary>
public class Whirlwind : MonoBehaviour
{
    [Header("Whirlwind Settings")]
    public GameObject whirlwindEffectPrefab;

    [Header("Stats")]
    public float baseDamageInterval = 0.3f; // Damage every X seconds
    public int baseDamage = 5;
    public float baseRadius = 1.5f;

    private int currentLevel = 0;
    private float damageTimer = 0f;
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
        if (activeEffect == null && whirlwindEffectPrefab != null)
        {
            activeEffect = Instantiate(whirlwindEffectPrefab, transform);
            activeEffect.transform.localPosition = Vector3.zero;
        }

        // Update effect scale based on radius
        if (activeEffect != null)
        {
            float scale = GetRadius();
            activeEffect.transform.localScale = Vector3.one * scale;
        }

        // Deal damage at intervals
        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseDamageInterval) 
            : baseDamageInterval;

        damageTimer += Time.deltaTime;
        if (damageTimer >= interval)
        {
            DealDamage();
            damageTimer = 0f;
        }
    }

    void DealDamage()
    {
        float radius = GetRadius();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyHealthController hp = hit.GetComponent<EnemyHealthController>();
            if (hp != null)
            {
                int damage = GetDamage();
                hp.TakeDamage(damage);

                if (PassiveStats.instance != null)
                    PassiveStats.instance.ApplyLifesteal(damage);
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
        int damage = baseDamage + (currentLevel * 3);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, GetRadius());
    }
}
