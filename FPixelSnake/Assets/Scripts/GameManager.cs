using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public TextMeshProUGUI currentScore;
    public TextMeshProUGUI bestScore;

    int score_;
    int bestScore_;

    FoodController foodController;

    SnakeController snakeController;
    private void Awake()
    {
        foodController = Object.FindAnyObjectByType<FoodController>();
        snakeController = Object.FindAnyObjectByType<SnakeController>();
    }

    void Update()
    {
        if (score_ != foodController.tailAmount)
        {
            score_ = foodController.tailAmount;
        }

        if (score_ > bestScore_)
        {
            bestScore_ = score_;
        }

        currentScore.text = "Score: " + score_;
        bestScore.text = "Best Score: " + bestScore_;

    }

    public string mainSahneAdi;
    public void yesButton()
    {
        gameOverCanvas.SetActive(false);
        SceneManager.LoadScene(mainSahneAdi);

    }

    public void noButton()
    {
        Application.Quit();
    }

    [SerializeField]
    GameObject gameOverCanvas;
    public void GameOverScreen()
    {
        gameOverCanvas.SetActive(true);
    }



}
