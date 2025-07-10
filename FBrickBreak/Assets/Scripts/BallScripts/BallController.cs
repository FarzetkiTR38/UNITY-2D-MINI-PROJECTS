using UnityEngine;

public class BallController : MonoBehaviour
{

    [SerializeField]
    float speed = 500f;
    private Rigidbody2D rb;

    [SerializeField]
    bool inPlay;

    [SerializeField]
    Transform ballStartPos;

    GameManager gameManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        gameManager = Object.FindAnyObjectByType<GameManager>();
    }



    void Update()
    {
        // can zaman gameover fonksiyonu çalışacak ve return ile fonksiyondan çıkacak.
        if (gameManager.gameOver == true)
        {
            return;
        }


        if (!inPlay)
        {
            transform.position = ballStartPos.position;

        }

        if (Input.GetButtonDown("Jump") && !inPlay)
        {
            inPlay = true;
            rb.AddForce(Vector2.up * speed);
        }
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.tag == "Bottom")
        {
            rb.linearVelocity = Vector2.zero;

            gameManager.UpdateLives(-1);

            inPlay = false;
        }
    }
}
