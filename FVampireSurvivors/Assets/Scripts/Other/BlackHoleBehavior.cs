using UnityEngine;

/// <summary>
/// Black Hole Behavior - Pulls enemies in and damages them
/// </summary>
public class BlackHoleBehavior : MonoBehaviour
{
    private float duration;
    private int damage;
    private float damageInterval;
    private float radius;
    private float pullForce;

    private float lifetime = 0f;
    private float damageTimer = 0f;

    public void Initialize(float duration, int damage, float damageInterval, float radius, float pullForce)
    {
        this.duration = duration;
        this.damage = damage;
        this.damageInterval = damageInterval;
        this.radius = radius;
        this.pullForce = pullForce;

        // Scale visual effect
        transform.localScale = Vector3.one * (radius / 3f);
    }

    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= duration)
        {
            Destroy(gameObject);
            return;
        }

        // Pull enemies
        PullEnemies();

        // Damage at intervals
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            DealDamage();
            damageTimer = 0f;
        }
    }

    void PullEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 pullDirection = (transform.position - hit.transform.position).normalized;
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                float forceMagnitude = pullForce * (1f - distance / radius); // Stronger when closer
                rb.AddForce(pullDirection * forceMagnitude);
            }
        }
    }

    void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyHealthController hp = hit.GetComponent<EnemyHealthController>();
            if (hp != null)
            {
                hp.TakeDamage(damage);

                if (PassiveStats.instance != null)
                    PassiveStats.instance.ApplyLifesteal(damage);
            }
        }
    }
}
