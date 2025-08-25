using UnityEngine;
using Mono.Cecil.Cil;

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
        if (other.gameObject.CompareTag("Enemy"))
        {

            other.gameObject.GetComponent<Health>().TakeDamage(bulletDamage);
            LevelManager.instance.totalDamage += bulletDamage * 100; // canı 2 ise 200 damage ile ölmüş gibi gösterelim 2 çok az :D
                                                                     // 5 canı olan tanka da 500 damage atınca ölüyor vs.
            Destroy(gameObject);
        }

        // mermi sınırın dışında çıktığında sonsuza doğru yol alıyordu bunu sınır koyarak, sınıra deydiği vakit objeyi yok ederek önlemiş oldum.
        if (other.gameObject.CompareTag("Boundaries"))
        {
            Destroy(gameObject);
        }
    }
   


}
