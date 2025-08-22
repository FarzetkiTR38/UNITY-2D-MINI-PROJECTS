using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIElements : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI UI_Element_World, UI_Element_Wave, UI_Element_Enemy, UI_Element_Damage, UI_Element_Time, UI_Element_KilledEnemy;

    [SerializeField]
    Image UI_Element_HealthImg1, UI_Element_HealthImg2, UI_Element_HealthImg3, UI_Element_HealthImg4, UI_Element_HealthImg5;

    [SerializeField]
    Sprite doluKalp, bosKalp, yarimKalp;


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
        SaglikDurumunuGuncelle(); // bunu aslında update içinde her an kontrol ettirmek saçma
        // sadece can azaldığında tetikletmeliyiz ama mini projede çok da fark etmez (heralde).
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
    
    public void SaglikDurumunuGuncelle()
    {

        if (LevelManager.instance.health > 10)
        {
            LevelManager.instance.health = 10;
        }

        switch (LevelManager.instance.health)
        {
            case 10:
                UI_Element_HealthImg1.sprite = doluKalp;
                UI_Element_HealthImg2.sprite = doluKalp;
                UI_Element_HealthImg3.sprite = doluKalp;
                UI_Element_HealthImg4.sprite = doluKalp;
                UI_Element_HealthImg5.sprite = doluKalp;
                break;
            case 9:
                UI_Element_HealthImg1.sprite = doluKalp;
                UI_Element_HealthImg2.sprite = doluKalp;
                UI_Element_HealthImg3.sprite = doluKalp;
                UI_Element_HealthImg4.sprite = doluKalp;
                UI_Element_HealthImg5.sprite = yarimKalp;
                break;
            case 8:
                UI_Element_HealthImg1.sprite = doluKalp;
                UI_Element_HealthImg2.sprite = doluKalp;
                UI_Element_HealthImg3.sprite = doluKalp;
                UI_Element_HealthImg4.sprite = doluKalp;
                UI_Element_HealthImg5.sprite = bosKalp;
                break;
            case 7:
                UI_Element_HealthImg1.sprite = doluKalp;
                UI_Element_HealthImg2.sprite = doluKalp;
                UI_Element_HealthImg3.sprite = doluKalp;
                UI_Element_HealthImg4.sprite = yarimKalp;
                UI_Element_HealthImg5.sprite = bosKalp;
                break;
            case 6:
                UI_Element_HealthImg1.sprite = doluKalp;
                UI_Element_HealthImg2.sprite = doluKalp;
                UI_Element_HealthImg3.sprite = doluKalp;
                UI_Element_HealthImg4.sprite = bosKalp;
                UI_Element_HealthImg5.sprite = bosKalp;
                break;
            case 5:
                UI_Element_HealthImg1.sprite = doluKalp;
                UI_Element_HealthImg2.sprite = doluKalp;
                UI_Element_HealthImg3.sprite = yarimKalp;
                UI_Element_HealthImg4.sprite = bosKalp;
                UI_Element_HealthImg5.sprite = bosKalp;
                break;
            case 4:
                UI_Element_HealthImg1.sprite = doluKalp;
                UI_Element_HealthImg2.sprite = doluKalp;
                UI_Element_HealthImg3.sprite = bosKalp;
                UI_Element_HealthImg4.sprite = bosKalp;
                UI_Element_HealthImg5.sprite = bosKalp;
                break;
            case 3:
                UI_Element_HealthImg1.sprite = doluKalp;
                UI_Element_HealthImg2.sprite = yarimKalp;
                UI_Element_HealthImg3.sprite = bosKalp;
                UI_Element_HealthImg4.sprite = bosKalp;
                UI_Element_HealthImg5.sprite = bosKalp;
                break;
            case 2:
                UI_Element_HealthImg1.sprite = doluKalp;
                UI_Element_HealthImg2.sprite = bosKalp;
                UI_Element_HealthImg3.sprite = bosKalp;
                UI_Element_HealthImg4.sprite = bosKalp;
                UI_Element_HealthImg5.sprite = bosKalp;
                break;
            case 1:
                UI_Element_HealthImg1.sprite = yarimKalp;
                UI_Element_HealthImg2.sprite = bosKalp;
                UI_Element_HealthImg3.sprite = bosKalp;
                UI_Element_HealthImg4.sprite = bosKalp;
                UI_Element_HealthImg5.sprite = bosKalp;
                break;
            case 0:
                UI_Element_HealthImg1.sprite = bosKalp;
                UI_Element_HealthImg2.sprite = bosKalp;
                UI_Element_HealthImg3.sprite = bosKalp;
                UI_Element_HealthImg4.sprite = bosKalp;
                UI_Element_HealthImg5.sprite = bosKalp;
                break;

        }
    }




}
