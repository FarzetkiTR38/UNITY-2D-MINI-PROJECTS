using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turret / Totem - Stationary structure that fires at enemies
/// Level increases: turret count, damage, fire rate
/// </summary>
public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public GameObject turretPrefab;
    public GameObject projectilePrefab;

    [Header("Stats")]
    public float baseFireRate = 0.8f;
    public int baseDamage = 8;
    public float projectileSpeed = 10f;
    public float attackRange = 8f;
    public float turretPlacementRadius = 3f;

    private List<GameObject> activeTurrets = new List<GameObject>();
    private int currentLevel = 0;

    public void Upgrade(int level)
    {
        // Only spawn new turrets if level increased
        if (level <= currentLevel) return;
        
        int previousLevel = currentLevel;
        currentLevel = level;

        // Add exactly (level - previousLevel) new turrets
        int turretsToAdd = level - previousLevel;
        for (int i = 0; i < turretsToAdd; i++)
        {
            SpawnTurret();
        }

        // Update all turret stats
        foreach (var turret in activeTurrets)
        {
            if (turret == null) continue;

            TurretBehavior behavior = turret.GetComponent<TurretBehavior>();
            if (behavior != null)
            {
                behavior.damage = GetDamage();
                behavior.fireRate = GetFireRate();
            }
        }
    }

    void SpawnTurret()
    {
        float minDistance = 1.5f;
        Vector3 spawnPos = Vector3.zero;
        bool found = false;

        // Try 10 random positions
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * turretPlacementRadius;
            Vector3 testPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            bool tooClose = false;
            foreach (var t in activeTurrets)
            {
                if (t != null && Vector3.Distance(testPos, t.transform.position) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                spawnPos = testPos;
                found = true;
                break;
            }
        }

        // Fallback: evenly spaced circle
        if (!found)
        {
            float angle = activeTurrets.Count * (360f / 5f) * Mathf.Deg2Rad;
            spawnPos = transform.position + new Vector3(
                Mathf.Cos(angle) * turretPlacementRadius,
                Mathf.Sin(angle) * turretPlacementRadius, 0f);
        }

        GameObject turret;
        if (turretPrefab != null)
        {
            turret = Instantiate(turretPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Create basic turret object
            turret = new GameObject("Turret");
            turret.transform.position = spawnPos;
        }

        // Add or configure turret behavior
        TurretBehavior behavior = turret.GetComponent<TurretBehavior>();
        if (behavior == null)
        {
            behavior = turret.AddComponent<TurretBehavior>();
        }

        // Only set projectilePrefab if the behavior doesn't already have one (from prefab)
        if (behavior.projectilePrefab == null && projectilePrefab != null)
        {
            behavior.projectilePrefab = projectilePrefab;
        }
        behavior.attackRange = attackRange;
        behavior.projectileSpeed = projectileSpeed;
        behavior.damage = GetDamage();
        behavior.fireRate = GetFireRate();

        activeTurrets.Add(turret);
    }

    float GetFireRate()
    {
        float rate = baseFireRate * (1f + (currentLevel - 1) * 0.2f);
        return PassiveStats.instance != null 
            ? rate * PassiveStats.instance.attackSpeedMultiplier 
            : rate;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 4);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }
}