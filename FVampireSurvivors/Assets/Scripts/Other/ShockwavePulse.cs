using UnityEngine;

/// <summary>
/// Shockwave Pulse - Periodically releases a damaging wave in all directions
/// Level increases: damage, radius, wave count
/// </summary>
public class ShockwavePulse : MonoBehaviour
{
    [Header("Shockwave Settings")]
    public GameObject shockwaveEffectPrefab;

    [Header("Stats")]
    public float basePulseInterval = 2f;
    public int baseDamage = 20;
    public float baseRadius = 4f;
    public float waveSpeed = 8f;

    private int currentLevel = 0;
    private float pulseTimer = 0f;

    void Update()
    {
        if (currentLevel <= 0) return;

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
        // Spawn visual effect
        if (shockwaveEffectPrefab != null)
        {
            GameObject effect = Instantiate(shockwaveEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f); // Destroy after animation
        }

        // Deal damage in radius
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

            // Knockback effect
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                rb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
            }
        }
    }

    float GetRadius()
    {
        float radius = baseRadius + (currentLevel * 1f);
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
