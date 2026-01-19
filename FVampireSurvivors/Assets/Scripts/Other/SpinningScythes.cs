using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spinning Scythes - Orbiting scythes that expand over time
/// Level increases: scythe count, damage
/// </summary>
public class SpinningScythes : MonoBehaviour
{
    [Header("Scythe Settings")]
    public GameObject scythePrefab;
    public Transform scytheAnchor;

    [Header("Stats")]
    public float baseRotationSpeed = 200f;
    public float rotationSpeedPerLevel = 30f;
    public float baseRadius = 2f;
    public float radiusPerLevel = 0.3f;
    public int baseDamage = 8;
    public float damageInterval = 0.3f;

    private List<GameObject> activeScythes = new List<GameObject>();
    private int currentLevel = 0;
    private float currentAngle = 0f;

    void Update()
    {
        if (currentLevel <= 0) return;

        float rotationSpeed = baseRotationSpeed + (currentLevel - 1) * rotationSpeedPerLevel;
        currentAngle += rotationSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        UpdateScythePositions();
    }

    void UpdateScythePositions()
    {
        int scytheCount = activeScythes.Count;
        if (scytheCount == 0) return;

        float angleStep = 360f / scytheCount;
        float radius = GetRadius();

        for (int i = 0; i < scytheCount; i++)
        {
            if (activeScythes[i] == null) continue;

            float angle = currentAngle + (i * angleStep);
            float radians = angle * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(
                Mathf.Cos(radians) * radius,
                Mathf.Sin(radians) * radius,
                0f
            );

            activeScythes[i].transform.position = transform.position + pos;
            activeScythes[i].transform.rotation = Quaternion.Euler(0, 0, angle + 45f);
        }
    }

    float GetRadius()
    {
        float radius = baseRadius + (currentLevel - 1) * radiusPerLevel;
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetScaledArea(radius) 
            : radius;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 4);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;

        // Add new scythes if needed
        while (activeScythes.Count < level)
        {
            SpawnScythe();
        }
    }

    void SpawnScythe()
    {
        if (scythePrefab == null) return;

        GameObject scythe = Instantiate(scythePrefab, transform.position, Quaternion.identity);
        
        ScytheDamage dmg = scythe.GetComponent<ScytheDamage>();
        if (dmg != null)
        {
            dmg.SetDamage(GetDamage());
            dmg.hitInterval = damageInterval;
        }

        activeScythes.Add(scythe);
    }
}
