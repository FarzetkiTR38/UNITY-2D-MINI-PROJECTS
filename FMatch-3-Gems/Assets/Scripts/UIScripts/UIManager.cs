using UnityEngine;
using TMPro;
using System.Collections;
public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    [SerializeField]
    TMP_Text kalanZamanText;


    [SerializeField]
    TMP_Text scoreText;

    [SerializeField]
    GameObject TurSonucPanel;

    public bool turBittiMi;

    public int gecerliPuan = 0;
    void Start()
    {
        turBittiMi = false;

        StartCoroutine(GeriSayRouitine());
    }

    void Awake()
    {
        instance = this;
    }

    public int kalanZaman = 60;

    IEnumerator GeriSayRouitine()
    {
        while (kalanZaman > 0)
        {
            yield return new WaitForSeconds(1f);

            kalanZamanText.text = kalanZaman.ToString() + " saniye";
            kalanZaman--;

            if (kalanZaman <= 0)
            {
                turBittiMi = true;
                TurSonucPanel.SetActive(true);
            }
        }
    }

    public void PuaniArttirFNC(int gelenPuan)
    {
        gecerliPuan += gelenPuan;
        scoreText.text = gecerliPuan.ToString() + " puan";
    }


}
