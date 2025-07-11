using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class MainMenuController : MonoBehaviour
{


    public string sahneAdi;

    public GameObject resimObje;
    public GameObject baslaButton, cikisButton;

    public GameObject fadeScreen;

    private void Start()
    {
        StartCoroutine(SiraylaAcRoutine());
    }

    IEnumerator SiraylaAcRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        resimObje.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        yield return new WaitForSeconds(0.4f);

        baslaButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        baslaButton.GetComponent<RectTransform>().DOScale(1, 0.5f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.4f);

        cikisButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        cikisButton.GetComponent<RectTransform>().DOScale(1, 0.5f).SetEase(Ease.OutBack);
    }

    public void OyunaBasla()
    {
        StartCoroutine(OyunuAcRoutine());
    }

    public void OyundanCikis()
    {
        Application.Quit();
    }

    IEnumerator OyunuAcRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        fadeScreen.GetComponent<CanvasGroup>().DOFade(1, 1f);

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(sahneAdi);
    }
}
