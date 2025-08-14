using UnityEngine;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] float hareketHizi = 5f;

    [SerializeField] Transform SnakeHead;

    [SerializeField] Transform Snake;

    Rigidbody2D rb;
    int yon = 0; // 1=Up, 2=Down, 3=Left, 4=Right

    [SerializeField]
    private Sprite SnakeLeft, SnakeRight, SnakeUp, SnakeDown;
    

    List<Transform> tailParts = new List<Transform>();
    List<Vector3> positions = new List<Vector3>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    void FixedUpdate() // Fizik güncellemesi — daha stabil takip sağlar
    {
        SnakeMovement();
    }

    // bir int değer oluşturacağım ve bu değeri 1 2 3 4 olarak değiştireceğim eğer değer 1 ise 2. koşul çalışmayacak eğer 3 ise 4. koşul çalışmayacak:
    // mantık şu şekilde: yılan ters yönde gidemez eğer yönümüz 1 ise yani sol ise 2. koşul yani sağa gitme çalışmamalı:

    void SnakeMovement()
    {
        if (Input.GetKey(KeyCode.W) && yon != 2)
        {
            rb.linearVelocity = new Vector2(0, hareketHizi);
            yon = 1;
            SnakeHead.rotation = Quaternion.Euler(0, 0, 90);
            SnakeHead.position = rb.position + new Vector2(0f, 0.55f);


            GetComponent<SpriteRenderer>().sprite = SnakeUp;
        }
        else if (Input.GetKey(KeyCode.S) && yon != 1)
        {
            rb.linearVelocity = new Vector2(0, -hareketHizi);
            yon = 2;
            SnakeHead.rotation = Quaternion.Euler(0, 0, -90);
            SnakeHead.position = rb.position + new Vector2(0f, -0.55f);

            GetComponent<SpriteRenderer>().sprite = SnakeDown;
        }
        else if (Input.GetKey(KeyCode.A) && yon != 4)
        {
            rb.linearVelocity = new Vector2(-hareketHizi, 0);
            yon = 3;
            SnakeHead.rotation = Quaternion.Euler(0, 0, 180);
            SnakeHead.position = rb.position + new Vector2(-0.55f, 0f);

            GetComponent<SpriteRenderer>().sprite = SnakeLeft;
        }
        else if (Input.GetKey(KeyCode.D) && yon != 3)
        {
            rb.linearVelocity = new Vector2(hareketHizi, 0);
            yon = 4;

            SnakeHead.rotation = Quaternion.Euler(0, 0, 0);
            SnakeHead.position = rb.position + new Vector2(0.55f, 0f);

            GetComponent<SpriteRenderer>().sprite = SnakeRight;
        }
        

    }
    

}
