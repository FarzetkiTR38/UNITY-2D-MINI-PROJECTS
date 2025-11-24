using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    public float attackRate = 1f;         // saniyede 1 atış
    public float attackRange = 10f;       // hedef arama mesafesi
    public Transform attackPoint;         // projectile çıkış noktası
    public GameObject projectilePrefab;   // fırlatılan mermi
    private float attackTimer = 0f;

    private void Update()
    {
        attackTimer += Time.deltaTime;

        // Attack süresi dolmadıysa çık
        if (attackTimer < 1f / attackRate)
            return;

        // Hedef bul
        Transform target = GetNearestEnemy();
        if (target == null)
            return;

        // Atak yap
        Attack(target);
        attackTimer = 0f;
    }

    Transform GetNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(currentPos, enemy.transform.position);
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

        // Projectile scriptine hedefi gönder
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.SetTarget(target);
        }
    }
}
