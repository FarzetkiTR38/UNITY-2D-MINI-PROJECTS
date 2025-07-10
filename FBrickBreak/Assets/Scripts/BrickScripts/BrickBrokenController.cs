using UnityEngine;

public class BrickBrokenController : MonoBehaviour
{


    [SerializeField]
    private Sprite brokenImage;

    int count;

    GameManager gameManager;
    
    [SerializeField]
    GameObject liveUpPrefab;

    [SerializeField]
    GameObject scoreUpPrefab;

    private void Awake()
    {

        gameManager = Object.FindAnyObjectByType<GameManager>();

    }

    private void Start()
    {
        count = 0;
    }


    [SerializeField]
    GameObject brickBrokenEffect;
    void OnCollisionEnter2D(Collision2D target)
    {
        if (target.gameObject.tag == "Ball")
        {
            count++;

            if (count == 1)
            {
                GetComponent<SpriteRenderer>().sprite = brokenImage;
                gameManager.UpdateScore(5);
            }
            else if (count == 2)
            {
                Instantiate(brickBrokenEffect, transform.position, transform.rotation);

                gameManager.UpdateScore(10);

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


}
