using UnityEngine;

/// <summary>
/// Meteor Projectile - Falls from above and explodes on impact
/// </summary>
public class MeteorProjectile : MonoBehaviour
{
    public float fallSpeed = 15f;
    public GameObject explosionEffectPrefab;

    private Vector3 targetPosition;
    private int damage;
    private float impactRadius;
    private bool initialized = false;

    public void Initialize(Vector3 target, int damage, float radius)
    {
        targetPosition = target;
        this.damage = damage;
        this.impactRadius = radius;
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        // Move towards target
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * fallSpeed * Time.deltaTime;

        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            Explode();
        }
    }

    void Explode()
    {
        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, targetPosition, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // Deal AoE damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPosition, impactRadius);
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

        Destroy(gameObject);
    }
}
