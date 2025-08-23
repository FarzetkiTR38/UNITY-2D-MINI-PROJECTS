using UnityEngine;
using UnityEngine.SceneManagement;


public class ScenesManager : MonoBehaviour
{

    public static ScenesManager instance;


    private void Awake()
    {
        instance = this;
    }

    // ! bir daha dosyanın adını SceneManager yapmayacağım.. SceneManager.LoadScene yapınca scripte bakıyormuş... Öğrenmiş olduk :d

    [SerializeField]
    string gameSceneName, mainMenuSceneName;

    [SerializeField]
    GameObject GameOverCanvas;

    [SerializeField]
    GameObject CreditsText;
    bool creditsBool = false;



    public void GameOverChecker()
    {
        if (LevelManager.instance.health == 0)
        {
            Time.timeScale = 0f; // zamanı durduruyoruz

            GameOverCanvas.SetActive(true);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);

        Time.timeScale = 1f; // zamanı tekrar normalde çeviriyoruz

        GameOverCanvas.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowCredits()
    {

        CreditsText.SetActive(!creditsBool);

        creditsBool = !creditsBool;
    }


}
