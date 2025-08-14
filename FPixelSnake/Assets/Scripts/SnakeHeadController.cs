using UnityEngine;

public class SnakeHeadController : MonoBehaviour
{

    GameManager gameManager;
    private void Awake()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tail"))
        {

            print("GameOver < 3");
            gameManager.GameOverScreen();
        }
    }
}
