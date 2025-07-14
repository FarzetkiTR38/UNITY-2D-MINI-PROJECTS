using UnityEngine;

public class ScoreArtis : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Flappy"))
        {
            ScoreManager.instance.UpdateScore();
        }
    }
}
