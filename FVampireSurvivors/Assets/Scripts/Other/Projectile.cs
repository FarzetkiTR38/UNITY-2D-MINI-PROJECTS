using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 10;
    private Transform target;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Hedef yönüne ilerle
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        // Çok yaklaştıysa vur
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            // Damage uygula
            EnemyHealthController hp = target.GetComponent<EnemyHealthController>();
            if (hp != null)
                hp.TakeDamage(damage);

            Destroy(gameObject);
        }
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }
}
