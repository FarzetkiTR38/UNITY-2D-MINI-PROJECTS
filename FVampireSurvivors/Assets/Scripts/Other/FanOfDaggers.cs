using UnityEngine;

/// <summary>
/// Fan of Daggers - Fires multiple daggers in a fan pattern
/// Level increases: dagger count, spread angle
/// </summary>
public class FanOfDaggers : MonoBehaviour
{
    [Header("Dagger Settings")]
    public GameObject daggerPrefab;
    public Transform firePoint;

    [Header("Stats")]
    public float baseFireRate = 1.2f;
    public int baseDamage = 6;
    public float daggerSpeed = 12f;
    public float attackRange = 10f;
    public float baseSpreadAngle = 60f;

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
            FireDaggers();
            fireTimer = 0f;
        }
    }

    void FireDaggers()
    {
        Transform target = GetNearestEnemy();
        if (target == null) return;

        int daggerCount = GetDaggerCount();
        float spreadAngle = baseSpreadAngle + (currentLevel * 10f); // Wider spread at higher levels
        float angleStep = daggerCount > 1 ? spreadAngle / (daggerCount - 1) : 0;
        float startAngle = -spreadAngle / 2f;

        Vector3 baseDirection = (target.position - firePoint.position).normalized;

        for (int i = 0; i < daggerCount; i++)
        {
            float offsetAngle = daggerCount > 1 ? startAngle + (angleStep * i) : 0;
            
            float radians = offsetAngle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            Vector3 direction = new Vector3(
                baseDirection.x * cos - baseDirection.y * sin,
                baseDirection.x * sin + baseDirection.y * cos,
                0f
            ).normalized;

            GameObject dagger = Instantiate(daggerPrefab, firePoint.position, Quaternion.identity);

            DirectionalProjectile dirProj = dagger.GetComponent<DirectionalProjectile>();
            if (dirProj != null)
            {
                dirProj.SetDirection(direction);
                dirProj.damage = GetDamage();
                dirProj.speed = daggerSpeed;
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

    int GetDaggerCount()
    {
        int baseCount = 3 + (currentLevel * 2); // 3 at level 1, up to 13 at level 5
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetTotalProjectileCount(baseCount) 
            : baseCount;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 2);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
    }
}
