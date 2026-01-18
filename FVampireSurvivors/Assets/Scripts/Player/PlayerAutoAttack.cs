using System.Collections;
using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 2f;  // Time between bursts
    public float attackRange = 10f;
    public Transform attackPoint;
    public GameObject projectilePrefab;

    [Header("Damage Settings")]
    [SerializeField] private int baseDamage = 10;
    private int bonusDamage = 0;

    [Header("Burst Fire Settings")]
    [Tooltip("Number of projectiles per burst (set by skill level)")]
    private int projectileCount = 1;

    [Tooltip("Delay between each projectile in burst")]
    public float delayBetweenShots = 0.05f;

    [Tooltip("Spawn offset distance from attack point")]
    public float spawnOffsetDistance = 0.3f;

    [Tooltip("Total spread angle in degrees")]
    public float spreadAngle = 30f;

    [Tooltip("Attack rate increase per level")]
    public float attackRatePerLevel = 0.15f;

    private float cooldownTimer = 0f;
    private int currentLevel = 0;
    private bool isFiring = false;

    void Update()
    {
        // Don't attack if level is 0 (skill not unlocked)
        if (currentLevel <= 0) return;
        if (isFiring) return; // Don't start new burst while firing

        cooldownTimer += Time.deltaTime;

        // Apply attack speed from PassiveStats
        float effectiveCooldown = attackCooldown;
        if (PassiveStats.instance != null)
        {
            effectiveCooldown /= PassiveStats.instance.attackSpeedMultiplier;
        }

        if (cooldownTimer < effectiveCooldown)
            return;

        Transform target = GetNearestEnemy();
        if (target == null)
            return;

        // Start burst fire
        StartCoroutine(BurstFire(target));
        cooldownTimer = 0f;
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

    /// <summary>
    /// Fire projectiles one by one with delay
    /// </summary>
    IEnumerator BurstFire(Transform target)
    {
        isFiring = true;

        int effectiveCount = projectileCount;

        if (effectiveCount <= 1)
        {
            // Single projectile
            FireProjectile(target, Vector3.zero);
        }
        else
        {
            // Multiple projectiles with delay between each
            float halfSpread = spreadAngle / 2f;
            float angleStep = spreadAngle / (effectiveCount - 1);

            for (int i = 0; i < effectiveCount; i++)
            {
                // Recalculate target direction each shot (in case player moved)
                Transform currentTarget = target;
                if (currentTarget == null)
                {
                    currentTarget = GetNearestEnemy();
                }

                float offsetAngle = -halfSpread + (angleStep * i);

                // Calculate spawn offset perpendicular to target direction
                Vector3 spawnOffset = Vector3.zero;
                if (currentTarget != null)
                {
                    Vector3 toTarget = (currentTarget.position - transform.position).normalized;
                    Vector3 perpendicular = new Vector3(-toTarget.y, toTarget.x, 0f);
                    float radians = offsetAngle * Mathf.Deg2Rad;
                    spawnOffset = perpendicular * (Mathf.Sin(radians) * spawnOffsetDistance);
                }

                FireProjectile(currentTarget, spawnOffset);

                // Wait between shots (except after last one)
                if (i < effectiveCount - 1)
                {
                    yield return new WaitForSeconds(delayBetweenShots);
                }
            }
        }

        isFiring = false;
    }

    void FireProjectile(Transform target, Vector3 spawnOffset)
    {
        if (projectilePrefab == null || attackPoint == null) return;
        if (target == null) return; // No target, skip this shot

        // Spawn at attack point + offset
        Vector3 spawnPos = attackPoint.position + spawnOffset;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Projectile projectileComponent = proj.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            // Set direction towards target (projectile goes straight)
            projectileComponent.SetTarget(target);

            // Calculate damage with PassiveStats
            int finalDamage = baseDamage + bonusDamage;
            if (PassiveStats.instance != null)
            {
                finalDamage = PassiveStats.instance.CalculateDamage(finalDamage);
            }
            projectileComponent.SetDamage(finalDamage);
        }
    }

    /// <summary>
    /// Upgrade the fireball skill.
    /// Level determines projectile count per burst.
    /// </summary>
    public void Upgrade(int level)
    {
        currentLevel = level;
        projectileCount = level; // Level 1 = 1 projectile, Level 2 = 2, etc.
        
        // Reduce cooldown slightly with level
        attackCooldown = 2f - (level - 1) * attackRatePerLevel;
        attackCooldown = Mathf.Max(attackCooldown, 0.5f); // Minimum 0.5s cooldown
    }

    /// <summary>
    /// Upgrade damage bonus (from passive skill)
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
