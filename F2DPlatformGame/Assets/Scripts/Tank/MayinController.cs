using UnityEngine;

public class MayinController : MonoBehaviour
{


    public GameObject patlamaEffecti;

    PlayerHealthController playerHealthController;

    private void Awake()
    {
        playerHealthController = Object.FindAnyObjectByType<PlayerHealthController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PatlamaFNC();

            playerHealthController.HasarAl();
        }
    }

    public void PatlamaFNC()
    {
        Destroy(this.gameObject);

        Instantiate(patlamaEffecti, transform.position, transform.rotation);
    }


}
