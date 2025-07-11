using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    float hareketHizi;

    [SerializeField]
    float ziplamaHizi;

    bool yerdemi;


    bool ziplayabilirmi;

    public Transform zeminKontrolNoktasi;
    public LayerMask zeminLayer;


    public float geriTepmeSuresi, geriTepmeGucu;
    float geriTepmeSayaci;
    bool yonSagMi;

    public float DusmandanZiplaGucu;

    Rigidbody2D rb;

    Animator anim;

    public bool hareketEtsinMi;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        hareketEtsinMi = true;
    }
    private void Update()
    {

        if (hareketEtsinMi)
        {
            if (geriTepmeSayaci <= 0)
            {
                HareketEttirFNC();
                ZiplaFNC();
                YonuDegistirFNC();
            }
            else
            {
                geriTepmeSayaci -= Time.deltaTime;

                if (yonSagMi)
                {
                    rb.linearVelocity = new Vector2(-geriTepmeGucu, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(geriTepmeGucu, rb.linearVelocity.y);
                }
            }



            anim.SetFloat("harekethizi", Mathf.Abs(rb.linearVelocity.x)); // math kullanınca vector2 ile çakışıyor. o yüzden mathf kullanıyoruz unity'nin matematik fonksiyonudur.
            anim.SetBool("yerdemi", yerdemi);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetFloat("harekethizi", Mathf.Abs(rb.linearVelocity.x)); // math kullanınca vector2 ile çakışıyor. o yüzden mathf kullanıyoruz unity'nin matematik fonksiyonudur.
            
        }

        

    }

    void HareketEttirFNC()
    {
        float h = Input.GetAxis("Horizontal");

        float hiz = h * hareketHizi;

        rb.linearVelocity = new Vector2(hiz, rb.linearVelocity.y);
    }

    void ZiplaFNC()
    {

        yerdemi = Physics2D.OverlapCircle(zeminKontrolNoktasi.position, .2f, zeminLayer);

        if (Input.GetButtonDown("Jump"))
        {
            if (yerdemi)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, ziplamaHizi);
                ziplayabilirmi = true;
                SesController.instance.SesEffectiCikar(3);
            }
            else if (ziplayabilirmi && Input.GetButtonDown("Jump"))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, ziplamaHizi);
                ziplayabilirmi = false;
                SesController.instance.SesEffectiCikar(3);
            }

        }



    }

    void YonuDegistirFNC()
    {
        Vector2 geciciScale = transform.localScale;
        if (rb.linearVelocity.x > 0)
        {
            yonSagMi = true;
            geciciScale.x = 1f;
        }
        else if (rb.linearVelocity.x < 0)
        {
            yonSagMi = false;
            geciciScale.x = -1f;
        }

        transform.localScale = geciciScale;
    }


    public void GeriTepmeFNC()
    {
        geriTepmeSayaci = geriTepmeSuresi;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        anim.SetTrigger("Hasar");
    }

    public void DusmandanZiplaFNC()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, DusmandanZiplaGucu);
        SesController.instance.SesEffectiCikar(3);
    }

}
