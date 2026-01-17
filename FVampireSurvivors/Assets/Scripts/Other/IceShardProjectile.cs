using UnityEngine;

/// <summary>
/// Ice Shard Projectile - Deals damage and slows enemy
/// </summary>
public class IceShardProjectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 10;
    public float slowPercent = 0.5f;
    public float slowDuration = 2f;
    public float maxLifetime = 5f;

    private Transform target;
    private float lifetime = 0f;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (target == null) return;

        EnemyHealthController hp = target.GetComponent<EnemyHealthController>();
        if (hp != null)
            hp.TakeDamage(damage);

        // Apply slow effect
        EnemyController enemy = target.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.ApplySlow(slowPercent, slowDuration);
        }

        // Apply lifesteal
        if (PassiveStats.instance != null)
            PassiveStats.instance.ApplyLifesteal(damage);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyHealthController hp = other.GetComponent<EnemyHealthController>();
        if (hp != null)
            hp.TakeDamage(damage);

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
            enemy.ApplySlow(slowPercent, slowDuration);

        if (PassiveStats.instance != null)
            PassiveStats.instance.ApplyLifesteal(damage);

        Destroy(gameObject);
    }
}
