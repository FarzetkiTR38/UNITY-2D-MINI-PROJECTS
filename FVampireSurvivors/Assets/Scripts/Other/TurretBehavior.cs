using UnityEngine;

/// <summary>
/// Turret Behavior - Auto-fires at nearby enemies
/// </summary>
public class TurretBehavior : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireRate = 1f;
    public int damage = 10;
    public float attackRange = 8f;
    public float projectileSpeed = 10f;

    private float fireTimer = 0f;

    void Update()
    {
        // Always rotate towards nearest enemy
        Transform target = GetNearestEnemy();
        if (target != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // Fire timer
        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / fireRate)
        {
            TryFire();
            fireTimer = 0f;
        }
    }

    void TryFire()
    {
        Transform target = GetNearestEnemy();
        if (target == null) return;

        if (projectilePrefab == null) return;

        // Rotate turret to face target (sprite faces UP, so subtract 90)
        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetTarget(target);
            projectile.SetDamage(damage);
            projectile.speed = projectileSpeed;
        }
    }

    Transform GetNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist && dist <= attackRange)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }
}