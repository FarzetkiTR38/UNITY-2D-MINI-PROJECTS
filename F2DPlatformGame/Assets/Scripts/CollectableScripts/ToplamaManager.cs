using UnityEngine;

public class ToplamaManager : MonoBehaviour
{

    [SerializeField]
    bool mucevherMi;

    [SerializeField]
    bool canMeyvesiMi;

    [SerializeField]
    GameObject toplamaEffecti;

    bool toplandiMi;

    LevelManager levelManager;

    UIController uiController;

    PlayerHealthController playerHealthController;

    private void Awake()
    {
        levelManager = Object.FindAnyObjectByType<LevelManager>();
        uiController = Object.FindAnyObjectByType<UIController>();
        playerHealthController = Object.FindAnyObjectByType<PlayerHealthController>();
    }







    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !toplandiMi && mucevherMi && !canMeyvesiMi)
        {
            levelManager.toplananMucevherSayisi++;

            toplandiMi = true;

            Destroy(gameObject);

            uiController.MucevherSayisiniGuncelle();

            Instantiate(toplamaEffecti, transform.position, transform.rotation);

            SesController.instance.KarisikSesEffectiCikar(7);
        }


        if (other.CompareTag("Player") && !toplandiMi && !mucevherMi && canMeyvesiMi && playerHealthController.gecerliSaglik < 6)
        {
            playerHealthController.gecerliSaglik++;

            toplandiMi = true;

            Destroy(gameObject);

            uiController.SaglikDurumunuGuncelle();

            Instantiate(toplamaEffecti, transform.position, transform.rotation);

            SesController.instance.SesEffectiCikar(4);
        }


    }
}
