using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    [SerializeField]
    TMP_Text kalanZamanText;

    [SerializeField]
    TMP_Text kalanHakText;


    public string tryagainlevel;

    public string Level2Scene;


    [SerializeField]
    TMP_Text scoreText;

    [SerializeField]
    GameObject TurSonucPanel;

    [SerializeField]
    GameObject TurKaybedildiPanel;

    public bool turBittiMi;

    public int gecerliPuan = 0;

    [SerializeField]
    GameObject pausePanel;

    Board board;

    [SerializeField]
    TMP_Text mevcutScoreText;

    [SerializeField]
    TMP_Text failedMevcutScoreText;


    public int kalanZaman = 60;

    public int kalanHamle = 25;



    public string anaMenu;
    void Start()
    {
        turBittiMi = false;
        if (SceneManager.GetActiveScene().name == "Level1")
        {
            StartCoroutine(GeriSayRouitine());
        }


    }

    void Awake()
    {
        instance = this;

        board = Object.FindAnyObjectByType<Board>();
    }

    private void Update()
    {
        HamleFıxUI();
        StartCoroutine(HakBittiFNC());
    }



    IEnumerator GeriSayRouitine()
    {
        while (kalanZaman > 0)
        {
            yield return new WaitForSeconds(1f);

            kalanZamanText.text = kalanZaman.ToString() + " saniye";
            kalanZaman--;

            if (kalanZaman <= 0)
            {
                if (gecerliPuan >= 2000)
                {
                    SoundManager.instance.OyunBittiSesiCikar();
                    turBittiMi = true;
                    TurSonucPanel.SetActive(true);
                    GameOverScreenTextsFix();
                }
                else if (gecerliPuan < 2000)
                {
                    SoundManager.instance.OyunBittiSesiCikar();
                    turBittiMi = true;
                    TurKaybedildiPanel.SetActive(true);
                    GameOverFailedScreenTextsFix();
                }

            }
        }
    }

    IEnumerator HakBittiFNC()
    {
        if (kalanHamle <= 0)
        {
            yield return new WaitForSeconds(1.5f);
            if (gecerliPuan >= 2000)
            {
                SoundManager.instance.OyunBittiSesiCikar();
                turBittiMi = true;
                TurSonucPanel.SetActive(true);
                GameOverScreenTextsFix();
            }
            else if (gecerliPuan < 2000)
            {
                SoundManager.instance.OyunBittiSesiCikar();
                turBittiMi = true;
                TurKaybedildiPanel.SetActive(true);
                GameOverFailedScreenTextsFix();
            }
        }
    }

    public void PuaniArttirFNC(int gelenPuan)
    {
        gecerliPuan += gelenPuan;
        scoreText.text = gecerliPuan.ToString() + " puan";
    }

    public void KaristirFNC()
    {
        board.BoardKaristirFNC();
    }

    public void OyunuDurdurAc()
    {
        if (!pausePanel.activeInHierarchy)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f; // zamanı durdurmaya yarıyor
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f; // zamanı tekrar aktif ediyoruz. default değeri 1f dir 0 yaparsak zaman duruyor.
        }
    }

    public void AnaMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(anaMenu);
    }

    public void GameOverScreenTextsFix()
    {
        mevcutScoreText.text = "SENİN YAPTIĞIN SKOR: " + gecerliPuan.ToString();
    }

    public void GameOverFailedScreenTextsFix()
    {
        failedMevcutScoreText.text = "SENİN YAPTIĞIN SKOR: " + gecerliPuan.ToString();
    }

    public void TekrarDeneFNC()
    {
        SceneManager.LoadScene(tryagainlevel);
    }

    public void Level1toLevel2()
    {
        SceneManager.LoadScene(Level2Scene);
    }

    public void HamleYap()
    {
        kalanHamle--;
    }

    public void HamleYapBasarisiz()
    {
        kalanHamle++;
    }

    public void HamleFıxUI()
    {
        kalanHakText.text = kalanHamle.ToString();
    }


    

}
