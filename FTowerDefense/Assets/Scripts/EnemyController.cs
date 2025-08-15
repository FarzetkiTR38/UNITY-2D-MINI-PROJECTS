using UnityEngine;

public class EnemyController : MonoBehaviour
{


    [SerializeField]
    Rigidbody2D rb;


    [SerializeField]
    float moveSpeed = 2f;

    private Transform target;
    private int pathIndex = 0;
    void Start()
    {
        target = LevelManager.instance.path[pathIndex];
    }

    void Update()
    {
        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            pathIndex++;


            if (pathIndex == LevelManager.instance.path.Length)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                target = LevelManager.instance.path[pathIndex];
            }
        }


    }

    void FixedUpdate()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        // .normalized, bir vektörün yönünü koruyarak uzunluğunu (magnitude) 1’e eşitleyen bir işlemdir.
        rb.linearVelocity = direction * moveSpeed;
    }
}
