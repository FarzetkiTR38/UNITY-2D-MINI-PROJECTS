using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    [SerializeField]
    Image kalp1_img, kalp2_img, kalp3_img;

    [SerializeField]
    Sprite doluKalp, bosKalp, yarimKalp;



    [SerializeField]
    TMP_Text mucevherText;

    PlayerHealthController playerHealthController;
    LevelManager levelManager;

    public GameObject fadeScreen;


    void Awake()
    {
        playerHealthController = Object.FindAnyObjectByType<PlayerHealthController>();
        levelManager = Object.FindAnyObjectByType<LevelManager>();
    }

    public void SaglikDurumunuGuncelle()
    {

        if (playerHealthController.gecerliSaglik > 6)
        {
            playerHealthController.gecerliSaglik = 6;
        }

        switch (playerHealthController.gecerliSaglik)
        {
            case 6:
                kalp1_img.sprite = doluKalp;
                kalp2_img.sprite = doluKalp;
                kalp3_img.sprite = doluKalp;
                break;
            case 5:
                kalp1_img.sprite = doluKalp;
                kalp2_img.sprite = doluKalp;
                kalp3_img.sprite = yarimKalp;
                break;
            case 4:
                kalp1_img.sprite = doluKalp;
                kalp2_img.sprite = doluKalp;
                kalp3_img.sprite = bosKalp;
                break;
            case 3:
                kalp1_img.sprite = doluKalp;
                kalp2_img.sprite = yarimKalp;
                kalp3_img.sprite = bosKalp;
                break;
            case 2:
                kalp1_img.sprite = doluKalp;
                kalp2_img.sprite = bosKalp;
                kalp3_img.sprite = bosKalp;
                break;
            case 1:
                kalp1_img.sprite = yarimKalp;
                kalp2_img.sprite = bosKalp;
                kalp3_img.sprite = bosKalp;
                break;
            case 0:
                kalp1_img.sprite = bosKalp;
                kalp2_img.sprite = bosKalp;
                kalp3_img.sprite = bosKalp;
                break;

        }
    }

    public void MucevherSayisiniGuncelle()
    {
        mucevherText.text = levelManager.toplananMucevherSayisi.ToString();
    }

    public void FadeEkraniAc()
    {
        fadeScreen.GetComponent<CanvasGroup>().DOFade(1, 1f);
    }

    
}
