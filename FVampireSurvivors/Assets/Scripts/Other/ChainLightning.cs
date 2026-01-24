using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chain Lightning - Strikes enemies and chains to nearby targets
/// Level increases: damage, chain count
/// </summary>
public class ChainLightning : MonoBehaviour
{
    [Header("Lightning Settings")]
    public GameObject lightningEffectPrefab;

    [Header("Stats")]
    public float baseStrikeInterval = 1f;
    public int baseDamage = 15;
    public float strikeRange = 10f;
    public float chainRange = 4f;
    public int baseChainCount = 2;

    private int currentLevel = 0;
    private float strikeTimer = 0f;

    void Update()
    {
        if (currentLevel <= 0) return;

        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseStrikeInterval) 
            : baseStrikeInterval;

        strikeTimer += Time.deltaTime;
        if (strikeTimer >= interval)
        {
            StrikeLightning();
            strikeTimer = 0f;
        }
    }

    void StrikeLightning()
    {
        Transform firstTarget = GetNearestEnemy(transform.position, strikeRange, null);
        if (firstTarget == null) return;

        int chainCount = GetChainCount();
        int damage = GetDamage();
        
        HashSet<Transform> hitTargets = new HashSet<Transform>();
        Transform currentTarget = firstTarget;
        Vector3 previousPos = transform.position;

        for (int i = 0; i < chainCount && currentTarget != null; i++)
        {
            hitTargets.Add(currentTarget);

            // Spawn lightning effect
            SpawnLightningEffect(previousPos, currentTarget.position);

            // Deal damage
            IDamageable damageable = currentTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(DamageInfo.Normal(damage, damageable.GetDamageTextPosition()));

                if (PassiveStats.instance != null)
                    PassiveStats.instance.ApplyLifesteal(damage);
            }

            // Reduce damage for chain
            damage = Mathf.Max(1, damage - 2);

            // Find next target
            previousPos = currentTarget.position;
            currentTarget = GetNearestEnemy(currentTarget.position, chainRange, hitTargets);
        }
    }

    void SpawnLightningEffect(Vector3 from, Vector3 to)
    {
        if (lightningEffectPrefab == null) return;

        Vector3 midPoint = (from + to) / 2f;
        GameObject effect = Instantiate(lightningEffectPrefab, midPoint, Quaternion.identity);

        // Rotate to face direction
        Vector3 dir = to - from;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        effect.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Scale to distance
        float distance = dir.magnitude;
        effect.transform.localScale = new Vector3(distance, 0.5f, 1f);

        Destroy(effect, 0.2f);
    }

    Transform GetNearestEnemy(Vector3 position, float range, HashSet<Transform> exclude)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            if (exclude != null && exclude.Contains(enemy.transform)) continue;

            float dist = Vector3.Distance(position, enemy.transform.position);
            if (dist < minDist && dist <= range)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    int GetChainCount()
    {
        return baseChainCount + currentLevel;
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
