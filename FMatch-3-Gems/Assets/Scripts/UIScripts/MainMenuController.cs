using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{

    public string levelAdi;

    public void OyunaBasla()
    {
        SceneManager.LoadScene(levelAdi);
    }

    public void OyundanCikis()
    {
        Application.Quit();
    }



}
