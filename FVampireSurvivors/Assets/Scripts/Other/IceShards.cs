using UnityEngine;

/// <summary>
/// Ice Shards - Fires single projectile that creates AoE slow explosion
/// Level increases: damage, explosion radius, slow duration
/// </summary>
public class IceShards : MonoBehaviour
{
    [Header("Shard Settings")]
    public GameObject shardPrefab;
    public Transform firePoint;

    [Header("Stats")]
    public float baseFireRate = 1.2f;
    public int baseDamage = 12;
    public float shardSpeed = 10f;
    public float attackRange = 12f;
    
    [Header("Slow Settings")]
    public float slowPercent = 0.5f; // 50% slow
    public float baseSlowDuration = 1.5f;
    
    [Header("Explosion Settings")]
    public float baseExplosionRadius = 1.5f;
    public float explosionRadiusPerLevel = 0.3f;
    
    [Header("Visual")]
    public GameObject explosionEffectPrefab;

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
            FireShard();
            fireTimer = 0f;
        }
    }

    void FireShard()
    {
        Transform target = GetNearestEnemy();
        if (target == null) return;

        GameObject shard = Instantiate(shardPrefab, firePoint.position, Quaternion.identity);

        IceShardProjectile iceProj = shard.GetComponent<IceShardProjectile>();
        if (iceProj != null)
        {
            iceProj.SetTarget(target);
            iceProj.damage = GetDamage();
            iceProj.speed = shardSpeed;
            iceProj.slowPercent = slowPercent;
            iceProj.slowDuration = GetSlowDuration();
            iceProj.explosionRadius = GetExplosionRadius();
            iceProj.explosionEffectPrefab = explosionEffectPrefab;
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

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 4); // +4 damage per level
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    float GetSlowDuration()
    {
        return baseSlowDuration + (currentLevel * 0.5f); // +0.5s per level
    }

    float GetExplosionRadius()
    {
        float radius = baseExplosionRadius + (currentLevel * explosionRadiusPerLevel);
        
        // FrozenWorld doubles the radius
        if (EvolvedSkillEffects.instance != null)
        {
            radius *= EvolvedSkillEffects.instance.GetFrozenWorldRadiusMultiplier();
        }
        
        return radius;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
        Debug.Log($"<color=blue>❄️ IceShards upgraded to Lv{level} - Radius: {GetExplosionRadius()}, Duration: {GetSlowDuration()}s</color>");
    }
}
