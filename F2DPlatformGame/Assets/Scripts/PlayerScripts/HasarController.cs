using UnityEngine;

public class HasarController : MonoBehaviour
{


    PlayerHealthController playerHealthController;
    private void Awake()
    {
        playerHealthController = Object.FindAnyObjectByType<PlayerHealthController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerHealthController.HasarAl();
        }
    }

}
