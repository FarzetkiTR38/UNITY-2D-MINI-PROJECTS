using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    public float attackRate = 1f;
    public float attackRange = 10f;

    private int baseDamage = 10;

    private int bonusDamage = 0;
    public Transform attackPoint;
    public GameObject projectilePrefab;

    private float attackTimer = 0f;

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < 1f / attackRate)
            return;

        Transform target = GetNearestEnemy();
        if (target == null)
            return;

        Attack(target);
        attackTimer = 0f;
    }

    Transform GetNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
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

    void Attack(Transform target)
    {
        GameObject proj = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
        proj.GetComponent<Projectile>().SetTarget(target);
        proj.GetComponent<Projectile>().SetDamage(baseDamage + bonusDamage);

        
    }

    // 🔥 SKILL LEVEL UPGRADE
    public void Upgrade(int level)
    {
        attackRate = 1f + (level * 0.35f);
    }

    public void UpgradeDamage(int level)
    {
        bonusDamage = level * 5;
    }
}
