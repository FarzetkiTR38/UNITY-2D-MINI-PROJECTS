using UnityEngine;

/// <summary>
/// Exploding Projectiles - Fires projectiles that explode on hit for AoE damage
/// Level increases: explosion radius, damage
/// </summary>
public class ExplodingProjectiles : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Stats")]
    public float baseFireRate = 1.2f;
    public int baseDamage = 10;
    public float projectileSpeed = 8f;
    public float attackRange = 10f;
    public float baseExplosionRadius = 1.5f;

    private int currentLevel = 0;
    private float fireTimer = 0f;

    void Update()
    {
        if (currentLevel <= 0) return;

        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseFireRate) 
            : baseFireRate;

        fireTimer += Time.deltaTime;
        if (fireTimer >= interval)
        {
            FireProjectile();
            fireTimer = 0f;
        }
    }

    void FireProjectile()
    {
        Transform target = GetNearestEnemy();
        if (target == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        ExplodingProjectile exploding = proj.GetComponent<ExplodingProjectile>();
        if (exploding != null)
        {
            exploding.SetTarget(target);
            exploding.damage = GetDamage();
            exploding.explosionRadius = GetExplosionRadius();
            exploding.speed = projectileSpeed;
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

    float GetExplosionRadius()
    {
        float radius = baseExplosionRadius + (currentLevel * 0.3f);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetScaledArea(radius) 
            : radius;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 8);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
    }
}
