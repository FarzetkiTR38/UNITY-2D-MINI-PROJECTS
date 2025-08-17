using UnityEngine;

public class Bullet : MonoBehaviour
{

    // referanslar
    [SerializeField]
    Rigidbody2D rb;



    [SerializeField]
    float bulletSpeed = 5f;

    [SerializeField]
    int bulletDamage = 1;

    Transform target;

    public void SetTarget(Transform _target)
    {
        target = _target;
    }


    void FixedUpdate()
    {
        if (!target)
        {
            return;
        }


        Vector2 direction = (target.position - transform.position).normalized;

        rb.linearVelocity = direction * bulletSpeed;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        other.gameObject.GetComponent<Health>().TakeDamage(bulletDamage);
        Destroy(gameObject); 
    }


}
