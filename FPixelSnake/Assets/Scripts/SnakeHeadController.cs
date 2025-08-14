using UnityEngine;

public class SnakeHeadController : MonoBehaviour
{




    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tail"))
        {

            print("GameOver < 3");
            // 
        }
    }
}
