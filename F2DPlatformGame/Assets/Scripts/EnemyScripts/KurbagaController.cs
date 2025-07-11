using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

public class KurbagaController : MonoBehaviour
{
    public float hareketHizi;

    public Transform solHedef, sagHedef;

    bool sagTarafta;

    Rigidbody2D rb;

    public SpriteRenderer spriteRenderer;

    public float hareketSuresi, beklemeSuresi;
    float hareketSayaci, beklemeSayaci;

    Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        solHedef.parent = null;
        sagHedef.parent = null;

        sagTarafta = true;

        hareketSayaci = hareketSuresi;
    }

    private void Update()
    {

        if (hareketSayaci > 0)
        {

            hareketSayaci -= Time.deltaTime;

            if (sagTarafta)
            {
                spriteRenderer.flipX = true;
                rb.linearVelocity = new Vector2(hareketHizi, rb.linearVelocity.y);

                if (transform.position.x >= sagHedef.position.x)
                {
                    sagTarafta = false;

                }
            }
            else
            {
                spriteRenderer.flipX = false;
                rb.linearVelocity = new Vector2(-hareketHizi, rb.linearVelocity.y);

                if (transform.position.x <= solHedef.position.x)
                {
                    sagTarafta = true;
                }
            }

            if (hareketSayaci <= 0)
            {
                beklemeSayaci = Random.Range(beklemeSuresi * 0.7f, beklemeSuresi * 1.2f);
            }

            animator.SetBool("hareketediyor", true);
        }
        else if (beklemeSayaci > 0)
        {
            beklemeSayaci -= Time.deltaTime;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (beklemeSayaci <= 0)
            {
                hareketSayaci = Random.Range(hareketSuresi * 0.7f, hareketSuresi * 1.2f);
            }

            animator.SetBool("hareketediyor", false);
        }
        
    }





}
