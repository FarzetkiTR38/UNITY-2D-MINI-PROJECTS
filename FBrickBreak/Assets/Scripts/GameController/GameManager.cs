using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{


    public int lives;
    public int score;

    public Text livesText;
    public Text scoreText;

    public bool gameOver;

    [SerializeField]
    GameObject gameOverPanel;





    private void Start()
    {

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.GetComponent<RectTransform>().localScale = Vector3.zero;
            gameOverPanel.GetComponent<CanvasGroup>().alpha = 0;
        }
        
        livesText.text = "Can: " + lives.ToString();
        scoreText.text = "Puan: " + score.ToString();

        
        
    }

    public void UpdateLives(int countLive)
    {
        lives += countLive;

        if (lives <= 0)
        {
            lives = 0;
            GameOver();
        }

        livesText.text = "Can: " + lives.ToString();
    }

    public void UpdateScore(int _score)
    {

        this.score += _score;

        scoreText.text = "Puan: " + score.ToString();

    }

    public void GameOver()
    {
        gameOver = true;


        Canvas canvas = gameOverPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 10;
        }

        gameOverPanel.GetComponent<CanvasGroup>().DOFade(1, .5f);
        gameOverPanel.GetComponent<RectTransform>().DOScale(1, .5f);

        
    }

    public void ReStart()
    {
        SceneManager.LoadScene("GamePlay");
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

}
