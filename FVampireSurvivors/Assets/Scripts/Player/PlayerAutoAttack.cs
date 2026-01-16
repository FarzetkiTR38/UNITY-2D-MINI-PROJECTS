using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRate = 1f;
    public float attackRange = 10f;
    public Transform attackPoint;
    public GameObject projectilePrefab;

    [Header("Damage Settings")]
    [SerializeField] private int baseDamage = 10;
    private int bonusDamage = 0;

    [Header("Multi-Shot Settings")]
    [Tooltip("Number of projectiles to fire (increases with level)")]
    private int projectileCount = 1;

    [Tooltip("Spawn offset distance from attack point")]
    public float spawnOffsetDistance = 0.5f;

    [Tooltip("Total spread angle in degrees for spawn positions")]
    public float spreadAngle = 45f;

    [Tooltip("Attack rate increase per level")]
    public float attackRatePerLevel = 0.25f;

    private float attackTimer = 0f;
    private int currentLevel = 1;

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < 1f / attackRate)
            return;

        Transform target = GetNearestEnemy();
        if (target == null)
            return;

        Attack(target);
        attackTimer = 0f;
    }

    Transform GetNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
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

    void Attack(Transform target)
    {
        if (projectileCount == 1)
        {
            // Single projectile - spawn at attack point, home to target
            FireProjectile(target, Vector3.zero);
        }
        else
        {
            // Multiple projectiles - spawn at offset positions, all home to same target
            float angleStep = spreadAngle / (projectileCount - 1);
            float startAngle = -spreadAngle / 2f;

            for (int i = 0; i < projectileCount; i++)
            {
                float offsetAngle = startAngle + (angleStep * i);
                
                // Calculate spawn offset position
                float radians = offsetAngle * Mathf.Deg2Rad;
                Vector3 spawnOffset = new Vector3(
                    Mathf.Cos(radians) * spawnOffsetDistance,
                    Mathf.Sin(radians) * spawnOffsetDistance,
                    0f
                );
                
                FireProjectile(target, spawnOffset);
            }
        }
    }

    void FireProjectile(Transform target, Vector3 spawnOffset)
    {
        if (projectilePrefab == null || attackPoint == null) return;

        // Spawn at attack point + offset
        Vector3 spawnPos = attackPoint.position + spawnOffset;
        
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        
        Projectile projectileComponent = proj.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            // All projectiles home to the same target
            projectileComponent.SetTarget(target);
            projectileComponent.SetDamage(baseDamage + bonusDamage);
        }
    }

    /// <summary>
    /// Upgrade the fireball skill.
    /// Level determines projectile count and attack rate.
    /// </summary>
    public void Upgrade(int level)
    {
        currentLevel = level;
        projectileCount = level; // 1 projectile per level
        attackRate = 1f + (level - 1) * attackRatePerLevel;
    }

    /// <summary>
    /// Upgrade damage bonus
    /// </summary>
    public void UpgradeDamage(int level)
    {
        bonusDamage = level * 5;
    }

    /// <summary>
    /// Get current projectile count
    /// </summary>
    public int GetProjectileCount()
    {
        return projectileCount;
    }
}
