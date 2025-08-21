using UnityEngine;
using TMPro;

public class UIElements : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI UI_Element_World, UI_Element_Wave, UI_Element_Enemy, UI_Element_Damage, UI_Element_Time, UI_Element_KilledEnemy;

    public float time;

    EnemySpawner enemySpawner;

    void Awake()
    {
        enemySpawner = Object.FindAnyObjectByType<EnemySpawner>();
    }

    void Update()
    {
        time += Time.deltaTime;
        FixUI();
    }

    public void FixUI()
    {
        UI_Element_World.text = "World: " + LevelManager.instance.world.ToString();
        UI_Element_Wave.text = "Wave: " + LevelManager.instance.wave.ToString();
        UI_Element_Enemy.text = "Enemy: " + enemySpawner.enemiesAlive.ToString();
        UI_Element_Damage.text = "Damage: " + LevelManager.instance.totalDamage.ToString(); 
        UI_Element_Time.text = "Time: " + time.ToString("F2"); // fixleyeceğim dk saniye diye ayrılacak..
        UI_Element_KilledEnemy.text = "KilledEnemy: " + enemySpawner.totalKilledEnemy.ToString();
    }




}
