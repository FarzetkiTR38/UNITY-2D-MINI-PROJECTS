using UnityEngine;
using TMPro;

public class GameOverSceneStats : MonoBehaviour
{

    UIElements uiElements;
    EnemySpawner enemySpawner;


    private void Awake()
    {
        uiElements = Object.FindAnyObjectByType<UIElements>();
        enemySpawner = Object.FindAnyObjectByType<EnemySpawner>();
    }

    [SerializeField]
    TextMeshProUGUI totalDamageText, killedEnemyText, earnedCurrencyText, playingTimeText, worldAndWaveText;



    private void GameOverSceneStatsFixUI()
    {
        
        totalDamageText.text = "Total Damage: " + LevelManager.instance.totalDamage.ToString();
        killedEnemyText.text = "Killed Enemy: " + enemySpawner.totalKilledEnemy.ToString();
        earnedCurrencyText.text = "Earned Money: " + LevelManager.instance.earnedCurrency.ToString();
        playingTimeText.text = "Playing Time: " + uiElements.time.ToString("F2");
        worldAndWaveText.text = "World & Wave: " + LevelManager.instance.world.ToString() + " & " + LevelManager.instance.wave.ToString();

    }


    void Update()
    {
        GameOverSceneStatsFixUI();
    }
}
