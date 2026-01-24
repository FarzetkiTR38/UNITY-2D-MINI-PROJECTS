using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI killCountText;
    
    [Header("Spin Wheel Initialization")]
    [Tooltip("Assign these panels so their Awake() runs at game start")]
    [SerializeField] GameObject spinWheelPanel;
    [SerializeField] GameObject rewardDisplayPanel;

    private Transform player;          // Player referansı

    float time;
    float dk;
    float saniye;
    
    int level; 
    int currentXP;
    int xpToNextLevel;
    float levelUpPercent;
    
    // Kill Counter
    private int killCount = 0;

    private void Awake() 
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Initialize spin wheel panels - activate briefly to trigger their Awake()
        InitializePanel(spinWheelPanel);
        InitializePanel(rewardDisplayPanel);
    }
    
    /// <summary>
    /// Activates a panel briefly to trigger Awake()/OnEnable(), then deactivates it
    /// </summary>
    void InitializePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
            panel.SetActive(false);
        }
    }

    private void Update() 
    {
       
        Timer();
        TestKeys();
        LevelText();
        UpdateKillCountUI();


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
        // EVOLVED SKILL TEST SHORTCUTS //
        // Her tuş ilgili aktif + pasif skill'i Lv5 yapar
        // Sonra X ile level up → Evolved skill seçeneği çıkar

        // 1 = BeastMode (Fireball + HealthRegen)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            for (int i = 0; i < 5; i++)
            {
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.Fireball);
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.HealthRegen);
            }
            Debug.Log("<color=orange>🔥 BeastMode hazır! X ile level up</color>");
        }

        // 2 = BladeStorm (Sword + AttackSpeed)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            for (int i = 0; i < 5; i++)
            {
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.Sword);
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.AttackSpeed);
            }
            Debug.Log("<color=cyan>⚔️ BladeStorm hazır! X ile level up</color>");
        }

        // 3 = VampiricField (AuraDamage + Lifesteal)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            for (int i = 0; i < 5; i++)
            {
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.AuraDamage);
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.Lifesteal);
            }
            Debug.Log("<color=red>🩸 VampiricField hazır! X ile level up</color>");
        }

        // 4 = FrozenWorld (IceShards + AreaSize)
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            for (int i = 0; i < 5; i++)
            {
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.IceShards);
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.AreaSize);
            }
            Debug.Log("<color=blue>❄️ FrozenWorld hazır! X ile level up</color>");
        }

        // 5 = MeteorFire (MeteorShower + CriticalDamage)
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            for (int i = 0; i < 5; i++)
            {
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.MeteorShower);
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.CriticalDamage);
            }
            Debug.Log("<color=yellow>☄️ MeteorFire hazır! X ile level up</color>");
        }

        // 6 = GreedyOverlord (XPGain + Damage)
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            for (int i = 0; i < 5; i++)
            {
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.XPGain);
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.Damage);
            }
            Debug.Log("<color=yellow>💰 GreedyOverlord hazır! X ile level up</color>");
        }

        // 7 = ImmortalForm (HealthRegen + MaxHealth)
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            for (int i = 0; i < 5; i++)
            {
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.HealthRegen);
                PlayerSkillManager.instance?.UpgradeSkill(SkillType.MaxHealth);
            }
            Debug.Log("<color=white>✨ ImmortalForm hazır! X ile level up</color>");
        }

        // X = Level Up (evolved skill seçeneği çıkar)
        if (Input.GetKeyDown(KeyCode.X))
        {
            player.GetComponent<PlayerExperience>().LevelUp();
        }
        // EVOLVED SKILL TEST SHORTCUTS //

        // C = Spin Wheel (test)
        if (Input.GetKeyDown(KeyCode.C))
        {
            SpinWheelManager.instance?.Show();
            Debug.Log("Test: Spin Wheel opened!");
        }
    }
    
    // ===== KILL COUNTER SYSTEM =====
    
    /// <summary>
    /// Enemy veya Boss öldüğünde çağrılır - killCount'u 1 arttırır
    /// </summary>
    public void IncrementKillCount()
    {
        killCount++;
    }
    
    void UpdateKillCountUI()
    {
        if (killCountText != null)
        {
            killCountText.text = killCount.ToString();
        }
    }
    
    public int GetKillCount() => killCount;
}
