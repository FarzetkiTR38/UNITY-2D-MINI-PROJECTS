using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    Rigidbody2D rb;

    [SerializeField]
    float hareketHizi = 4f;

    [SerializeField]
    float maxBaslangicAcisi = 0.6f;

    float hizArtisCarpani = 1.2f;

    public ParticleSystem particleEffect;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        ResetPosition();
    }

    void HareketFNC()
    {
        YaymayiGerceklestir(20);

        float rastgeleDeger = Random.value;

        Vector2 direction = Vector2.zero;

        direction.y = Random.Range(-maxBaslangicAcisi, maxBaslangicAcisi);

        if (rastgeleDeger > 0.5f)
        {
            direction = Vector2.left;
        }
        else
        {
            direction = Vector2.right;
        }


        direction.y = Random.Range(-maxBaslangicAcisi, maxBaslangicAcisi);


        rb.linearVelocity = direction * hareketHizi;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Paddle"))
        {
            YaymayiGerceklestir(10);
            rb.linearVelocity *= hizArtisCarpani;
            
        }
        else if (other.gameObject.CompareTag("altustDuvarlar"))
        {
            YaymayiGerceklestir(15);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("solDuvar"))
        {
            GameManager.instance.LeftPuanArttir();
            ResetPosition();
        }
        else if (other.CompareTag("sagDuvar"))
        {
            GameManager.instance.RightPuanArttir();
            ResetPosition();
        }
    }

    void ResetPosition()
    {
        rb.linearVelocity = Vector2.zero;

        transform.localPosition = Vector2.zero;

        HareketFNC();
    }


    void YaymayiGerceklestir(int yaymaAdeti)
    {
        particleEffect.Emit(yaymaAdeti);
    }
}
