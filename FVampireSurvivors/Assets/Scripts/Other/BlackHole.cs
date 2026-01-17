using UnityEngine;

/// <summary>
/// Black Hole - Creates a vortex that pulls enemies in and deals damage
/// Level increases: pull force, damage, radius
/// </summary>
public class BlackHole : MonoBehaviour
{
    [Header("Black Hole Settings")]
    public GameObject blackHoleEffectPrefab;

    [Header("Stats")]
    public float baseSpawnInterval = 4f;
    public float baseDuration = 3f;
    public int baseDamagePerTick = 5;
    public float damageInterval = 0.3f;
    public float baseRadius = 3f;
    public float basePullForce = 5f;
    public float spawnRadius = 6f;

    private int currentLevel = 0;
    private float spawnTimer = 0f;

    void Update()
    {
        if (currentLevel <= 0) return;

        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseSpawnInterval) 
            : baseSpawnInterval;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= interval)
        {
            SpawnBlackHole();
            spawnTimer = 0f;
        }
    }

    void SpawnBlackHole()
    {
        // Find best spawn location (where most enemies are)
        Vector3 spawnPos = FindBestSpawnLocation();

        GameObject blackHole;
        if (blackHoleEffectPrefab != null)
        {
            blackHole = Instantiate(blackHoleEffectPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            blackHole = new GameObject("BlackHole");
            blackHole.transform.position = spawnPos;
        }

        BlackHoleBehavior behavior = blackHole.GetComponent<BlackHoleBehavior>();
        if (behavior == null)
        {
            behavior = blackHole.AddComponent<BlackHoleBehavior>();
        }

        behavior.Initialize(
            GetDuration(),
            GetDamage(),
            damageInterval,
            GetRadius(),
            GetPullForce()
        );
    }

    Vector3 FindBestSpawnLocation()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (enemies.Length == 0)
        {
            // Random location around player
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            return transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
        }

        // Find center of enemy cluster
        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= spawnRadius)
            {
                center += enemy.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            return center / count;
        }

        // Fallback to nearest enemy
        return enemies[0].transform.position;
    }

    float GetDuration()
    {
        return baseDuration + (currentLevel * 0.5f);
    }

    float GetRadius()
    {
        float radius = baseRadius + (currentLevel * 0.5f);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetScaledArea(radius) 
            : radius;
    }

    float GetPullForce()
    {
        return basePullForce + (currentLevel * 2f);
    }

    int GetDamage()
    {
        int damage = baseDamagePerTick + (currentLevel * 3);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
    }
}
