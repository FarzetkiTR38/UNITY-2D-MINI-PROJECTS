using UnityEngine;

/// <summary>
/// Piercing Arrows - Fires arrows in a straight line that pierce through enemies
/// Level increases: arrow count, damage
/// </summary>
public class PiercingArrows : MonoBehaviour
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("Stats")]
    public float baseFireRate = 0.6f;
    public int baseDamage = 12;
    public float arrowSpeed = 15f;
    public float attackRange = 15f;
    public int pierceCount = 3; // How many enemies it can hit

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
            FireArrows();
            fireTimer = 0f;
        }
    }

    void FireArrows()
    {
        Transform target = GetNearestEnemy();
        if (target == null) return;

        int arrowCount = GetArrowCount();
        float spreadAngle = 20f;
        float angleStep = arrowCount > 1 ? spreadAngle / (arrowCount - 1) : 0;
        float startAngle = -spreadAngle / 2f;

        Vector3 baseDirection = (target.position - firePoint.position).normalized;

        for (int i = 0; i < arrowCount; i++)
        {
            float offsetAngle = arrowCount > 1 ? startAngle + (angleStep * i) : 0;
            
            float radians = offsetAngle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            Vector3 direction = new Vector3(
                baseDirection.x * cos - baseDirection.y * sin,
                baseDirection.x * sin + baseDirection.y * cos,
                0f
            ).normalized;

            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

            PiercingProjectile piercingProj = arrow.GetComponent<PiercingProjectile>();
            if (piercingProj != null)
            {
                piercingProj.SetDirection(direction);
                piercingProj.damage = GetDamage();
                piercingProj.speed = arrowSpeed;
                piercingProj.maxPierceCount = pierceCount + currentLevel;
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

    int GetArrowCount()
    {
        int baseCount = Mathf.Max(1, currentLevel);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetTotalProjectileCount(baseCount) 
            : baseCount;
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
    }
}
