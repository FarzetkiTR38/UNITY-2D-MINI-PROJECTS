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

    // bir int değer oluşturacağım ve bu değeri 1 2 3 4 olarak değiştireceğim eğer değer 1 ise 2. koşul çalışmayacak eğer 3 ise 4. koşul çalışmayacak:
    // mantık şu şekilde: yılan ters yönde gidemez eğer yönümüz 1 ise yani sol ise 2. koşul yani sağa gitme çalışmamalı:
    
    int yon = 0;

    public void SnakeMovement()
    {
        if (Input.GetKeyDown(KeyCode.W) && yon != 2)
        {
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, hareketHizi);
            yon = 1;
        }
        else if (Input.GetKeyDown(KeyCode.S) && yon != 1)
        {
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -hareketHizi);
            yon = 2;
        }
        else if (Input.GetKeyDown(KeyCode.A) && yon != 4)
        {
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.linearVelocity = new Vector2(-hareketHizi, rb.linearVelocity.y);
            yon = 3;
        }
        else if (Input.GetKeyDown(KeyCode.D) && yon != 3)
        {
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.linearVelocity = new Vector2(hareketHizi, rb.linearVelocity.y);
            yon = 4;
        }
    }
}
