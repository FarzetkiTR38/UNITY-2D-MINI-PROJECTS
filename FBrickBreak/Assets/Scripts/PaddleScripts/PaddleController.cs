using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [SerializeField]
    float speed;

    [SerializeField]
    float leftTarget, rightTarget;

    GameManager gameManager;

    private void Awake()
    {
        gameManager = Object.FindAnyObjectByType<GameManager>();

    }


    void Update()
    {
        float h = Input.GetAxis("Horizontal");

        if (gameManager.gameOver == false)
        {
            transform.Translate(Vector2.right * h * Time.deltaTime * speed);
        }
        else
        {
            return;
        }

        /*       sol sınırı geçersek pozisyonu sol sınıra atıyor yani geçemiyoruz bu sayede */
        if (transform.position.x < leftTarget)
        {
            transform.position = new Vector2(leftTarget, transform.position.y);
        }
        /*       sağ sınırı geçersek pozisyonu sağ sınıra atıyor yani geçemiyoruz bu sayede */
        if (transform.position.x > rightTarget)
        {
            transform.position = new Vector2(rightTarget, transform.position.y);
        }

        /*
                Vector2 temp = transform.position;
                temp.x = Mathf.Clamp(temp.x, leftTarget, rightTarget);
                transform.position = temp;
                // bu 3 satır kod ile de aynı şekilde paddleyi sınırlandırabilirdik.
        */

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "LiveUpBall")
        {
            gameManager.UpdateLives(1);
            Destroy(other.gameObject);
        }
        
        if (other.gameObject.tag == "ScoreUpBall")
        {
            gameManager.UpdateScore(10);
            Destroy(other.gameObject);
        }
    }

}
