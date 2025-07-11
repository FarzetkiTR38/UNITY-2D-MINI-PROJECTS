using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    PlayerController playerController;

    UIController uicontroller;

    public string sahneAdi;

    private void Awake()
    {
        instance = this;

        playerController = Object.FindAnyObjectByType<PlayerController>();
        uicontroller = Object.FindAnyObjectByType<UIController>();
    }
    public int toplananMucevherSayisi;

    public void SahneyiBitir()
    {
        StartCoroutine(SahneyiBitirRoutine());
    }

    IEnumerator SahneyiBitirRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        playerController.hareketEtsinMi = false;

        yield return new WaitForSeconds(0.6f);

        uicontroller.FadeEkraniAc();

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(sahneAdi);


    }

}
