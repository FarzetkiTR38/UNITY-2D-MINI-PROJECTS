using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class Turret : MonoBehaviour
{

    //referanslar
    [SerializeField]
    Transform turretRotationPoint;

    [SerializeField]
    LayerMask enemyMask;

    [SerializeField]
    GameObject bulletPrefab;

    [SerializeField]
    Transform firingPoint;

    // nitelikler
    [SerializeField]
    float targetingRange = 2f;

    [SerializeField]
    float rotationSpeed = 200f;

    [SerializeField]
    float bps = 1f; // bps -> bullet per second

    Transform target;
    float timeUntilFire;

    void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        if (!CheckTargetIsInRange())
        {
            target = null;
        }
        else
        {
            timeUntilFire += Time.deltaTime;

            if (timeUntilFire >= 1f / bps)
            {
                Shoot();
                timeUntilFire = 0f;
            }

        }
    }

    void Shoot()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);

        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetTarget(target);
    }

    bool CheckTargetIsInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    void RotateTowardsTarget()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
;

    }

    void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            target = hits[0].transform; 
        }
    }

    void OnDrawGizmosSelected()
    {
        Handles.color = Color.cyan;

        Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);
        // turret in reachını ayarlıyoruz 2f dediğimizde sola 2 sağa 2 olacak şekilde yani r'si 2 oluyor, pi.r.r den 4.pi kadarlık alanı görüyor demek.
    }
}
