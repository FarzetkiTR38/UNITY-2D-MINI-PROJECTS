using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spinning Shuriken - Orbiting shurikens that expand over time
/// Level increases: shuriken count, damage
/// </summary>
public class SpinningShuriken : MonoBehaviour
{
    [Header("Shuriken Settings")]
    public GameObject shurikenPrefab;
    public Transform shurikenAnchor;

    [Header("Stats")]
    public float baseRotationSpeed = 200f;
    public float rotationSpeedPerLevel = 30f;
    public float baseRadius = 2f;
    public float radiusPerLevel = 0.3f;
    public int baseDamage = 8;
    public float damageInterval = 0.3f;

    private List<GameObject> activeShurikens = new List<GameObject>();
    private int currentLevel = 0;
    private float currentAngle = 0f;

    void Update()
    {
        if (currentLevel <= 0) return;

        float rotationSpeed = baseRotationSpeed + (currentLevel - 1) * rotationSpeedPerLevel;
        currentAngle += rotationSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        UpdateShurikenPositions();
    }

    void UpdateShurikenPositions()
    {
        int shurikenCount = activeShurikens.Count;
        if (shurikenCount == 0) return;

        float angleStep = 360f / shurikenCount;
        float radius = GetRadius();

        for (int i = 0; i < shurikenCount; i++)
        {
            if (activeShurikens[i] == null) continue;

            float angle = currentAngle + (i * angleStep);
            float radians = angle * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(
                Mathf.Cos(radians) * radius,
                Mathf.Sin(radians) * radius,
                0f
            );

            activeShurikens[i].transform.position = transform.position + pos;
            // Shuriken is symmetrical, so we can use any rotation
            activeShurikens[i].transform.rotation = Quaternion.Euler(0, 0, angle);
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

        // Add new shurikens if needed
        while (activeShurikens.Count < level)
        {
            SpawnShuriken();
        }
    }

    void SpawnShuriken()
    {
        if (shurikenPrefab == null) return;

        GameObject shuriken = Instantiate(shurikenPrefab, transform.position, Quaternion.identity);
        
        ShurikenDamage dmg = shuriken.GetComponent<ShurikenDamage>();
        if (dmg != null)
        {
            dmg.SetDamage(GetDamage());
            dmg.hitInterval = damageInterval;
        }

        activeShurikens.Add(shuriken);
    }
}
