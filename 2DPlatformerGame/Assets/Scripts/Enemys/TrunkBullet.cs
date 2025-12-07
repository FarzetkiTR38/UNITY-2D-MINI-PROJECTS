using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TrunkBullet : MonoBehaviour
{
    public float bulletSpeed = 2f;

    public float lifeTime = 5f;

    int direction = 1;

    Rigidbody2D rb;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() 
    {
        rb.linearVelocity = new Vector2(bulletSpeed * direction, 0);

        Destroy(gameObject, lifeTime);    
    }

    public void ChangeDirection(int _direction)
    {
        direction = _direction;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }    

        if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }

}
