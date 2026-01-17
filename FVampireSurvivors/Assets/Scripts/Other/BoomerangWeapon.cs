using UnityEngine;

/// <summary>
/// Boomerang Weapon - Thrown weapon that returns to player
/// Level increases: boomerang count, damage
/// </summary>
public class BoomerangWeapon : MonoBehaviour
{
    [Header("Boomerang Settings")]
    public GameObject boomerangPrefab;
    public Transform throwPoint;

    [Header("Stats")]
    public float baseThrowInterval = 1.5f;
    public int baseDamage = 12;
    public float throwDistance = 6f;
    public float boomerangSpeed = 10f;

    private int currentLevel = 0;
    private float throwTimer = 0f;

    void Update()
    {
        if (currentLevel <= 0) return;

        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseThrowInterval) 
            : baseThrowInterval;

        throwTimer += Time.deltaTime;
        if (throwTimer >= interval)
        {
            ThrowBoomerangs();
            throwTimer = 0f;
        }
    }

    void ThrowBoomerangs()
    {
        int boomerangCount = GetBoomerangCount();
        float angleStep = 360f / boomerangCount;

        for (int i = 0; i < boomerangCount; i++)
        {
            float angle = i * angleStep;
            float radians = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);

            GameObject boomerang = Instantiate(boomerangPrefab, throwPoint.position, Quaternion.identity);

            BoomerangProjectile proj = boomerang.GetComponent<BoomerangProjectile>();
            if (proj != null)
            {
                proj.Initialize(transform, direction, throwDistance, boomerangSpeed, GetDamage());
            }
        }
    }

    int GetBoomerangCount()
    {
        return currentLevel;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 5);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
    }
}
