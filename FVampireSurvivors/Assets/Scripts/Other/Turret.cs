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
        currentLevel = level;

        // Add new turrets if needed
        while (activeTurrets.Count < level)
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
        // Place turret at random position around player
        Vector2 randomOffset = Random.insideUnitCircle * turretPlacementRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

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

        behavior.projectilePrefab = projectilePrefab;
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
