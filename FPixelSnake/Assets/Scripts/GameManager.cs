using UnityEngine;
using TMPro;

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




}
