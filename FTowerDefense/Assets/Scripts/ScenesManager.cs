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
    string gameSceneName, gameOverSceneName, mainMenuSceneName;



    public void GameOverChecker()
    {
        if (LevelManager.instance.health == 0)
        {
            Time.timeScale = 0f; // zamanı durduruyoruz

            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }


}
