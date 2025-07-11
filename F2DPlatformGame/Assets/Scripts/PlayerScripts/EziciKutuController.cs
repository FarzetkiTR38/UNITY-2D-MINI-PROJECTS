using UnityEngine;

public class EziciKutuController : MonoBehaviour
{

    [SerializeField]
    GameObject yokOlmaEffecti;

    PlayerController playerController;

    public float canMeyvesininiCikmaSansi;

    public GameObject canMeyvesi;

    private void Awake()
    {
        playerController = Object.FindAnyObjectByType<PlayerController>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Kurbaga"))
        {
            other.transform.parent.gameObject.SetActive(false);
            // burada önemli 1. kısım normal transform.parent.gameobject... i 
            // false yapınca ezicikutu false oluyor
            // başına other eklememiz gerektiğine dikkat edelim.
            // ikinci konu transform.gameObject dersek kurbağa yok oluyor fakat 
            // görünürde yok oluyor aslında hareket etmeye devam ediyor sadece görüntüsü gidiyor
            // bu yüzden parent yani üst kategorisini false yapıyoruz hareketi de görüntüsü de gidiyor.

            Instantiate(yokOlmaEffecti, transform.position, transform.rotation);

            playerController.DusmandanZiplaFNC();

            float cikmaAraligi = Random.Range(0, 100);

            SesController.instance.SesEffectiCikar(0);

            if (cikmaAraligi <= canMeyvesininiCikmaSansi)
            {
                Instantiate(canMeyvesi, other.transform.position, other.transform.rotation);
            }
        }
    }



}
