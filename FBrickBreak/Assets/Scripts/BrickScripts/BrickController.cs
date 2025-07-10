using UnityEngine;

public class BrickController : MonoBehaviour
{

    [SerializeField]
    GameObject brickEffect;

    GameManager gameManager;

    [SerializeField]
    GameObject liveUpPrefab;

    [SerializeField]
    GameObject scoreUpPrefab;
    private void Awake()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();

    }
    private void OnCollisionEnter2D(Collision2D target)
    {
        if (target.gameObject.tag == "Ball")
        {
            Instantiate(brickEffect, transform.position, transform.rotation);

            gameManager.UpdateScore(5);


            int randomChaince = Random.Range(1, 101);

            if (randomChaince > 40)
            {
                Instantiate(liveUpPrefab, transform.position, transform.rotation);
            }
            else if (randomChaince > 20)
            {
                Instantiate(scoreUpPrefab, transform.position, transform.rotation);
            }


            Destroy(gameObject);
        }
    }


}
