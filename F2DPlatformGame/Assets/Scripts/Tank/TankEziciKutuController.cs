using UnityEngine;

public class TankEziciKutuController : MonoBehaviour
{
    PlayerController playerController;
    TankController tankController;
    void Awake()
    {
        playerController = Object.FindAnyObjectByType<PlayerController>();
        tankController = Object.FindAnyObjectByType<TankController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playerController.transform.position.y > transform.position.y)
        {

            playerController.DusmandanZiplaFNC();
            tankController.DarbeAlFNC();

            gameObject.SetActive(false);




        }
    }



}
