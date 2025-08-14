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
