using UnityEngine;

public class TankController : MonoBehaviour
{

    public enum tankDurumlari { atesEtme, darbeAlma, hareketEtme, tankYokOldu};
    public tankDurumlari gecerliDurum;

    [SerializeField]
    Transform tankObje;
    public Animator anim;

    [Header("Hareket")]
    public float hareketHizi;
    public Transform solHedef, sagHedef;
    bool yonSagmi;
    public GameObject mayinObje;
    public Transform mayinMerkezNoktasi;
    public float mayinBirakmaSuresi;
    float mayinBirakmaSayac;

    [Header("AtesEtme")]
    public GameObject mermi;
    public Transform mermiMerkezi;
    public float mermiAtmaSuresi;
    float mermiAtmaSayaci;

    [Header("Darbe")]
    public float darbeSuresi;
    float darbeSayaci;

    [Header("CanDurumu")]
    public int canDurumu = 5;
    public GameObject tankPatlamaEffecti;
    bool yenildiMi;
    public float mermiSuresiArtir, mayinBirakmaSuresiArtir;


    public GameObject tankEziciKutu;

    private void Start()
    {
        gecerliDurum = tankDurumlari.atesEtme; // 
    }

    private void Update()
    {
        switch (gecerliDurum)
        {
            case tankDurumlari.atesEtme:
                // tank ates edildiğinde olacak durumlar
                mermiAtmaSayaci -= Time.deltaTime;

                if (mermiAtmaSayaci <= 0)
                {
                    mermiAtmaSayaci = mermiAtmaSuresi;

                    var yeniMermi = Instantiate(mermi, mermiMerkezi.position, mermiMerkezi.rotation);
                    yeniMermi.transform.localScale = tankObje.localScale;
                } 

                break;

            case tankDurumlari.darbeAlma:
                // tank darbe aldığında olacak durumlar
                if (darbeSayaci > 0)
                {
                    darbeSayaci -= Time.deltaTime;

                    if (darbeSayaci <= 0)
                    {
                        gecerliDurum = tankDurumlari.hareketEtme;

                        mayinBirakmaSayac = 0;

                        if (yenildiMi)
                        {
                            tankObje.gameObject.SetActive(false);
                            Instantiate(tankPatlamaEffecti, transform.position, transform.rotation);

                            gecerliDurum = tankDurumlari.tankYokOldu;
                        }
                    }
                }


                break;

            case tankDurumlari.hareketEtme:
                // tank hareket ettiğinde olacak durumlar
                if (yonSagmi)
                {
                    tankObje.position += new Vector3(hareketHizi * Time.deltaTime, 0f, 0f);

                    if (tankObje.position.x > sagHedef.position.x)
                    {
                        tankObje.localScale = new Vector3(1, 1, 1);

                        yonSagmi = false;

                        HareketiDurdurFNC();
                    }
                }
                else
                {
                    tankObje.position -= new Vector3(hareketHizi * Time.deltaTime, 0f, 0f);

                    if (tankObje.position.x < solHedef.position.x)
                    {
                        tankObje.localScale = new Vector3(-1, 1, 1);
                        yonSagmi = true;

                        HareketiDurdurFNC();
                    }
                }

                mayinBirakmaSayac -= Time.deltaTime;

                if (mayinBirakmaSayac <= 0)
                {
                    mayinBirakmaSayac = mayinBirakmaSuresi;

                    Instantiate(mayinObje, mayinMerkezNoktasi.position, mayinMerkezNoktasi.rotation);
                }

                break;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            DarbeAlFNC();
        }
    }

    public void DarbeAlFNC()
    {



        gecerliDurum = tankDurumlari.darbeAlma;

        darbeSayaci = darbeSuresi;

        anim.SetTrigger("Vur");

        //  MayinController[] mayinlar = FindObjectsOfType<MayinController>(); yerine alttakini kullanıyoruz:
        // unity 6 da artık bu şekilde kullanılıyormuş:
        MayinController[] mayinlar = FindObjectsByType<MayinController>(FindObjectsSortMode.None);


        if (mayinlar.Length > 0)
        {
            foreach (MayinController bulunanMayin in mayinlar)
            {
                bulunanMayin.PatlamaFNC();
            }
        }

        canDurumu--;

        if (canDurumu <= 0)
        {
            yenildiMi = true;
        }
        else
        {
            mermiAtmaSuresi /= mermiSuresiArtir;
            mayinBirakmaSuresi /= mayinBirakmaSuresiArtir;
        }
    }

    void HareketiDurdurFNC()
    {

        tankEziciKutu.SetActive(true);

        gecerliDurum = tankDurumlari.atesEtme;
        mermiAtmaSayaci = mermiAtmaSuresi;

        anim.SetTrigger("HareketiDurdur");
    }

}
