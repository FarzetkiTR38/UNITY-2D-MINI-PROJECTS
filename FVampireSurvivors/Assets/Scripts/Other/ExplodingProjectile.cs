using UnityEngine;

/// <summary>
/// Exploding Projectile - Homes to target and explodes on impact
/// </summary>
public class ExplodingProjectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 10;
    public float explosionRadius = 1.5f;
    public float maxLifetime = 5f;
    public GameObject explosionEffectPrefab;

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
            Explode();
            return;
        }

        if (target == null)
        {
            Explode();
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            Explode();
        }
    }

    void Explode()
    {
        // Spawn effect
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // AoE damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Explode();
        }
    }
}
