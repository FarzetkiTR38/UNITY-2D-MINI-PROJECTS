using UnityEngine;

public class MermiController : MonoBehaviour
{


    PlayerHealthController playerHealthController;

    void Awake()
    {
        playerHealthController = Object.FindAnyObjectByType<PlayerHealthController>();
    }

    public float mermiHızı;

    private void Update()
    {
        transform.position += new Vector3(-mermiHızı * Time.deltaTime * transform.localScale.x, 0f, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealthController.HasarAl();
        }

        Destroy(gameObject);
        
    }

}
