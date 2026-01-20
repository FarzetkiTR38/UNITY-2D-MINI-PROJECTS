using UnityEngine;

/// <summary>
/// Exploding Projectiles - Fires projectiles that explode on hit for AoE damage
/// Level increases: explosion radius, damage
/// </summary>
public class ExplodingProjectiles : MonoBehaviour
{
    [Header("Attack Mode")]
    [Tooltip("true = auto-target nearest enemy, false = shoot towards mouse")]
    public bool useAutoAttack = true;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Stats")]
    public float baseFireRate = 1.2f;
    public int baseDamage = 10;
    public float projectileSpeed = 8f;
    public float attackRange = 10f;
    public float baseExplosionRadius = 1f;

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
        Vector3 direction;

        if (useAutoAttack)
        {
            // Auto mode: target nearest enemy
            Transform target = GetNearestEnemy();
            if (target == null) return;
            direction = (target.position - firePoint.position).normalized;
        }
        else
        {
            // Manual mode: shoot towards mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            direction = (mousePos - firePoint.position).normalized;
        }

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        ExplodingProjectile exploding = proj.GetComponent<ExplodingProjectile>();
        if (exploding != null)
        {
            exploding.SetDirection(direction);
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
        // Level 1: 1, Level 2: 1.25, Level 3: 1.5, Level 4: 1.75, Level 5: 2
        float radius = baseExplosionRadius + ((currentLevel - 1) * 0.25f);
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
