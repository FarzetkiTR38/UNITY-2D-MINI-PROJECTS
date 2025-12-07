using TMPro;
using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fruitText;

    [SerializeField] GameObject pausePanel;


    bool isPaused = false;


    public static UIManager instance;

    private void Awake() 
    {
        instance = this;
    }

    public void UpdateText(int fruitCount)
    {
        fruitText.text = fruitCount.ToString();
    } 

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }    
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            
            // pausePanel.SetActive(true);
            pausePanel.GetComponent<CanvasGroup>().DOFade(1, .5f).SetUpdate(true);
            pausePanel.GetComponent<RectTransform>().DOScale(1, .5F).SetUpdate(true).SetEase(Ease.OutBack);
            Time.timeScale = 0f;
        }
        else
        {
            
            // pausePanel.SetActive(false);
            pausePanel.GetComponent<CanvasGroup>().DOFade(0, .5f).SetUpdate(true);
            pausePanel.GetComponent<RectTransform>().DOScale(0, .5F).SetUpdate(true).SetEase(Ease.InBack);
            Time.timeScale = 1f;
        }
    }

}
