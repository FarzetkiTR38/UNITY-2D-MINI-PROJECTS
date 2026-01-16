using UnityEngine;

/// <summary>
/// Meteor Shower - Random meteors fall from above dealing AoE damage
/// Level increases: meteor count, damage
/// </summary>
public class MeteorShower : MonoBehaviour
{
    [Header("Meteor Settings")]
    public GameObject meteorPrefab;

    [Header("Stats")]
    public float baseMeteorInterval = 1.5f;
    public int baseDamage = 25;
    public float impactRadius = 1.5f;
    public float spawnRadius = 8f; // Random spawn area around player

    private int currentLevel = 0;
    private float meteorTimer = 0f;

    void Update()
    {
        if (currentLevel <= 0) return;

        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseMeteorInterval) 
            : baseMeteorInterval;

        meteorTimer += Time.deltaTime;
        if (meteorTimer >= interval)
        {
            SpawnMeteors();
            meteorTimer = 0f;
        }
    }

    void SpawnMeteors()
    {
        int meteorCount = GetMeteorCount();

        for (int i = 0; i < meteorCount; i++)
        {
            // Random position around player
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 targetPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            // Spawn meteor above target
            Vector3 spawnPos = targetPos + Vector3.up * 10f;

            if (meteorPrefab != null)
            {
                GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
                
                MeteorProjectile proj = meteor.GetComponent<MeteorProjectile>();
                if (proj != null)
                {
                    proj.Initialize(targetPos, GetDamage(), GetImpactRadius());
                }
            }
            else
            {
                // No prefab - instant damage at location
                DealImpactDamage(targetPos);
            }
        }
    }

    void DealImpactDamage(Vector3 position)
    {
        float radius = GetImpactRadius();
        int damage = GetDamage();

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
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
    }

    int GetMeteorCount()
    {
        int baseCount = currentLevel;
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetTotalProjectileCount(baseCount) 
            : baseCount;
    }

    float GetImpactRadius()
    {
        float radius = impactRadius + (currentLevel * 0.3f);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetScaledArea(radius) 
            : radius;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 15);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
    }
}
