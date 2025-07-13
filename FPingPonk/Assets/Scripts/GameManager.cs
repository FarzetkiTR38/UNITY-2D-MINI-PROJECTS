using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextMeshProUGUI leftText, rightText;

    public int leftScore, rightScore;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        leftScore = 0;
        rightScore = 0;
    }

    public void LeftPuanArttir()
    {
        leftScore++;

        rightText.text = leftScore.ToString();
    }

    public void RightPuanArttir()
    {
        rightScore++;

        leftText.text = rightScore.ToString();
    } 


    
}
