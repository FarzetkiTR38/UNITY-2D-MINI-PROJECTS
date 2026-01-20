using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI timerText;

    private Transform player;          // Player referansı

    float time;
    float dk;
    float saniye;
    
    int level; 
    int currentXP;
    int xpToNextLevel;
    float levelUpPercent;

    private void Awake() 
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

    }

    private void Update() 
    {
       
        Timer();
        TestKeys();
        LevelText();


    }

    void Timer()
    {
        time += Time.deltaTime;
        timerText.text = "00" + ":" + time.ToString("00"); 

        if(time > 60f && time < 3600f)
        {
            dk = time / 60f;
            saniye = time % 60f;
            timerText.text = Mathf.Floor(dk).ToString("00") + ":" + saniye.ToString("00");
        }
    }

    void LevelText()
    {
        currentXP = player.GetComponent<PlayerExperience>().getcurrentXP();   
        xpToNextLevel = player.GetComponent<PlayerExperience>().getxpToNextLevel();   
        levelUpPercent = ((float)currentXP / (float)xpToNextLevel * 100);
        level = player.GetComponent<PlayerExperience>().getLevel();   
        levelText.text = " Level " + level.ToString() + " (" + "%"+ levelUpPercent.ToString("0") + ")";
    }

    void TestKeys()
    {
        // TEST //
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PassiveStats.instance?.UpgradeDamage(1);
        } 
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PassiveStats.instance?.UpgradeDamage(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PassiveStats.instance?.UpgradeDamage(3);
        } 
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PassiveStats.instance?.UpgradeDamage(4);
        } 
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PassiveStats.instance?.UpgradeDamage(5);
        }  
        if (Input.GetKeyDown(KeyCode.X))
        {
            player.GetComponent<PlayerExperience>().LevelUp();
        }  
        // TEST //

        // PlayerSkillsController.instance?.UpgradeSkill(SkillType.MoveSpeed, 5);
    }



    



}
