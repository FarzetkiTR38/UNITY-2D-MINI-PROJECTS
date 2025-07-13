using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public string gecilecekSahne;

    public void PlayGame()
    {
        SceneManager.LoadScene(gecilecekSahne);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
