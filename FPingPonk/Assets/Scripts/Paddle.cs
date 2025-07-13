using UnityEngine;

public class Paddle : MonoBehaviour
{
    public string paddleName;

    public float hareketHizi;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HareketFNC();
    }

    void HareketFNC()
    {
        float moveY = 0f;

        if (paddleName == "leftPaddle")
        {
            moveY = Input.GetAxis("LeftPaddle");
        }
        else if (paddleName == "rightPaddle")
        {
            moveY = Input.GetAxis("RightPaddle");
        }

        Vector2 hiz = rb.linearVelocity;

        hiz.y = moveY * hareketHizi;

        rb.linearVelocity = hiz;
    }
}
