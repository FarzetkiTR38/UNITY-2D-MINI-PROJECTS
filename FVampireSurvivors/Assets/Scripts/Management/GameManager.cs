using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI levelText;

    private Transform player;          // Player referansı
    
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
        currentXP = player.GetComponent<PlayerExperience>().getcurrentXP();   
        xpToNextLevel = player.GetComponent<PlayerExperience>().getxpToNextLevel();   
        levelUpPercent = ((float)currentXP / (float)xpToNextLevel * 100);
        level = player.GetComponent<PlayerExperience>().getLevel();   
        levelText.text = " Level " + level.ToString() + " (" + "%"+ levelUpPercent.ToString("0") + ")";
        

        // TEST //
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            
            PlayerSkillsController.instance?.UpgradeSkill(SkillType.Whirlwind, 1);
        } 
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            
            PlayerSkillsController.instance?.UpgradeSkill(SkillType.Whirlwind, 2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            
            PlayerSkillsController.instance?.UpgradeSkill(SkillType.Whirlwind, 3);
            
        } 
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            
            PlayerSkillsController.instance?.UpgradeSkill(SkillType.Whirlwind, 4);
        } 
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            
            PlayerSkillsController.instance?.UpgradeSkill(SkillType.Whirlwind, 5);
        }  
        if (Input.GetKeyDown(KeyCode.X))
        {
            
            player.GetComponent<PlayerExperience>().LevelUp();
        }  
        // TEST //


                
    
    
    }



    



}
