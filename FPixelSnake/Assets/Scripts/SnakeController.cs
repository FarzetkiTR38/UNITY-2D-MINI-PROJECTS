using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class SnakeController : MonoBehaviour
{

    [SerializeField]
    float hareketHizi;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        SnakeMovement();
    }

    public void SnakeMovement()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {   
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, hareketHizi);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -hareketHizi);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.linearVelocity = new Vector2(-hareketHizi, rb.linearVelocity.y);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.linearVelocity = new Vector2(hareketHizi, rb.linearVelocity.y);
        }
    }
}
