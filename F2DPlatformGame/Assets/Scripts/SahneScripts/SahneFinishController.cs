using UnityEngine;

public class SahneFinishController : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.instance.SahneyiBitir();
        }
    }




}
