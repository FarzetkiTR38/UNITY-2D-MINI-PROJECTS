using UnityEngine;

/// <summary>
/// Ice Shards - Fires projectiles that slow enemies
/// Level increases: projectile count, slow duration
/// </summary>
public class IceShards : MonoBehaviour
{
    [Header("Shard Settings")]
    public GameObject shardPrefab;
    public Transform firePoint;

    [Header("Stats")]
    public float baseFireRate = 1f;
    public int baseDamage = 8;
    public float shardSpeed = 8f;
    public float attackRange = 10f;
    public float slowPercent = 0.5f; // 50% slow
    public float baseSlowDuration = 2f;

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
            FireShards();
            fireTimer = 0f;
        }
    }

    void FireShards()
    {
        Transform target = GetNearestEnemy();
        if (target == null) return;

        int shardCount = GetShardCount();
        float spreadAngle = 30f;
        float angleStep = shardCount > 1 ? spreadAngle / (shardCount - 1) : 0;
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < shardCount; i++)
        {
            float offsetAngle = shardCount > 1 ? startAngle + (angleStep * i) : 0;
            Vector3 spawnOffset = Quaternion.Euler(0, 0, offsetAngle) * Vector3.right * 0.3f;

            GameObject shard = Instantiate(shardPrefab, firePoint.position + spawnOffset, Quaternion.identity);

            IceShardProjectile iceProj = shard.GetComponent<IceShardProjectile>();
            if (iceProj != null)
            {
                iceProj.SetTarget(target);
                iceProj.damage = GetDamage();
                iceProj.speed = shardSpeed;
                iceProj.slowPercent = slowPercent;
                iceProj.slowDuration = GetSlowDuration();
            }
            else
            {
                // Fallback to regular projectile
                Projectile proj = shard.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.SetTarget(target);
                    proj.SetDamage(GetDamage());
                    proj.speed = shardSpeed;
                }
            }
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

    int GetShardCount()
    {
        int baseCount = currentLevel;
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetTotalProjectileCount(baseCount) 
            : baseCount;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 3);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    float GetSlowDuration()
    {
        return baseSlowDuration + (currentLevel * 0.5f);
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
    }
}
