using UnityEngine;
using TMPro;

public class GameOverSceneStats : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI scoreText, totalDamageText, killedEnemyText, playingTimeText, worldAndWaveText;



    private void GameOverSceneStatsFixUI()
    {
        scoreText.text  = "Your Score: ";
        totalDamageText.text  = "Total Damage: " + LevelManager.instance.totalDamage;
        killedEnemyText.text = "Killed Enemy: "; 
        playingTimeText.text = "Playing Time: ";
        worldAndWaveText.text = "World & Wave: " + LevelManager.instance.world + " & " + LevelManager.instance.wave; 

    }
    
}
