using UnityEngine;

/// <summary>
/// Homing Missiles - Fires rockets that lock onto nearest enemies
/// Level increases: projectile count & damage
/// </summary>
public class HomingMissiles : MonoBehaviour
{
    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public Transform firePoint;

    [Header("Stats")]
    public float baseFireRate = 0.8f;
    public int baseDamage = 15;
    public float missileSpeed = 10f;
    public float attackRange = 12f;

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
            FireMissiles();
            fireTimer = 0f;
        }
    }

    void FireMissiles()
    {
        Transform[] targets = GetNearestEnemies(GetMissileCount());
        
        foreach (var target in targets)
        {
            if (target == null) continue;
            
            GameObject missile = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);
            
            Projectile proj = missile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetTarget(target);
                proj.SetDamage(GetDamage());
                proj.speed = missileSpeed;
            }
        }
    }

    Transform[] GetNearestEnemies(int count)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        // Sort by distance
        System.Array.Sort(enemies, (a, b) =>
        {
            float distA = Vector3.Distance(transform.position, a.transform.position);
            float distB = Vector3.Distance(transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });

        Transform[] targets = new Transform[Mathf.Min(count, enemies.Length)];
        for (int i = 0; i < targets.Length; i++)
        {
            if (Vector3.Distance(transform.position, enemies[i].transform.position) <= attackRange)
                targets[i] = enemies[i].transform;
        }

        return targets;
    }

    int GetMissileCount()
    {
        int baseCount = currentLevel;
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetTotalProjectileCount(baseCount) 
            : baseCount;
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
