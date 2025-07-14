using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [SerializeField]
    TextMeshProUGUI currentScoreText;

    [SerializeField]
    TextMeshProUGUI highScoreText;

    int score;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        currentScoreText.text = score.ToString();

        highScoreText.text = PlayerPrefs.GetInt("HighScore", 1).ToString();
        UpdateHeighScore();
    }

    private void UpdateHeighScore()
    {
        if (score > PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", score);
            highScoreText.text = score.ToString();
        }
    }

    public void UpdateScore()
    {
        score++;
        currentScoreText.text = score.ToString();
        UpdateHeighScore();
    }

}
