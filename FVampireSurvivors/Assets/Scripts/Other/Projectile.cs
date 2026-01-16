using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 8f;
    public int damage = 10;

    [Tooltip("Maximum lifetime in seconds before auto-destroy")]
    public float maxLifetime = 5f;

    private Transform target;
    private float lifetime = 0f;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void Update()
    {
        lifetime += Time.deltaTime;

        // Auto-destroy after max lifetime
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        // If target is dead/null, destroy projectile
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards target (homing)
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        // Check if reached target
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

        Destroy(gameObject);
    }

    /// <summary>
    /// Trigger collision - ONLY react to Enemy tag
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ONLY hit enemies, ignore everything else (swords, player, etc.)
        if (!other.CompareTag("Enemy")) return;

        EnemyHealthController hp = other.GetComponent<EnemyHealthController>();
        if (hp != null)
            hp.TakeDamage(damage);

        Destroy(gameObject);
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }
}
