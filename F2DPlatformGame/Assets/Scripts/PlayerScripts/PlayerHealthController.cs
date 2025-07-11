using TreeEditor;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public int maxSaglik, gecerliSaglik;

    UIController uiController;

    public float yenilmezlikSuresi;
    float yenilmezlikSayaci;

    SpriteRenderer spriteRenderer;

    PlayerController playerController;

    [SerializeField]
    GameObject yokOlmaEfffecti;


    private void Awake()
    {
        uiController = Object.FindAnyObjectByType<UIController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = Object.FindAnyObjectByType<PlayerController>();
    }

    private void Start()
    {
        gecerliSaglik = maxSaglik;
    }

    private void Update()
    {
        yenilmezlikSayaci -= Time.deltaTime;

        if (yenilmezlikSayaci <= 0)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }

    }


    public void HasarAl()
    {
        if (yenilmezlikSayaci <= 0)
        {
            gecerliSaglik--;

            if (gecerliSaglik <= 0)
            {
                gecerliSaglik = 0;
                Instantiate(yokOlmaEfffecti, transform.position, transform.rotation);
                gameObject.SetActive(false);
                SesController.instance.SesEffectiCikar(2);
            }
            else
            {
                yenilmezlikSayaci = yenilmezlikSuresi;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.6f);

                playerController.GeriTepmeFNC();
                SesController.instance.SesEffectiCikar(1);
            }


            uiController.SaglikDurumunuGuncelle();
        }
    }


}
