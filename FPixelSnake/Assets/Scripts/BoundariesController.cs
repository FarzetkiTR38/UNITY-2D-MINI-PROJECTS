using UnityEditor.Rendering;
using UnityEngine;

public class BoundariesController : MonoBehaviour
{

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    float anlikX;
    float anlikY;

    /*
        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("LeftBoundaries") || other.gameObject.CompareTag("S"))
            {
                anlikX = rb.transform.position.x;
                anlikY = rb.transform.position.y;
                rb.transform.position = new Vector3(Mathf.Abs(anlikX), anlikY, rb.transform.position.z);

            }
            else if (other.gameObject.CompareTag("RightBoundaries"))
            {

            }
            else if (other.gameObject.CompareTag("UpBoundaries"))
            {

            }
            else if (other.gameObject.CompareTag("DownBoundaries"))
            {

            }
        }
    */

    // bir int değer oluşturacağım ve bu değeri 1 2 3 4 olarak değiştireceğim eğer değer 1 ise 2. koşul çalışmayacak eğer 3 ise 4. koşul çalışmayacak:
    // mantık şu şekilde: yılan ters yönde gidemez eğer yönümüz 1 ise yani sol ise 2. koşul yani sağa gitme çalışmamalı:
     

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LeftBoundaries") || other.CompareTag("Snake"))
        {
            anlikX = rb.transform.position.x;
            anlikY = rb.transform.position.y;
            rb.transform.position = new Vector3(Mathf.Abs(anlikX) - 0.5f, anlikY, rb.transform.position.z);
        }
        else if (other.CompareTag("RightBoundaries") || other.CompareTag("Snake"))
        {
            anlikX = rb.transform.position.x;
            anlikY = rb.transform.position.y;
            rb.transform.position = new Vector3(-anlikX + 0.5f, anlikY, rb.transform.position.z);

        }
        else if (other.CompareTag("UpBoundaries") || other.CompareTag("Snake"))
        {
            anlikX = rb.transform.position.x;
            anlikY = rb.transform.position.y;
            rb.transform.position = new Vector3(anlikX, -anlikY + 0.5f, rb.transform.position.z);

        }
        else if (other.CompareTag("DownBoundaries") || other.CompareTag("Snake"))
        {
            anlikX = rb.transform.position.x;
            anlikY = rb.transform.position.y;
            rb.transform.position = new Vector3(anlikX, Mathf.Abs(anlikY) - 0.5f, rb.transform.position.z);

        }
    }
}
