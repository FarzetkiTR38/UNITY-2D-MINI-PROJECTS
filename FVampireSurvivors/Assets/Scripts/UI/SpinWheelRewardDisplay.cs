using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays won skills after wheel spin with icons, similar to level up panel.
/// </summary>
public class SpinWheelRewardDisplay : MonoBehaviour
{
    public static SpinWheelRewardDisplay instance;
    
    [Header("UI References")]
    public GameObject rewardPanel;           // Main panel
    public Transform rewardContainer;        // Parent for reward items
    public GameObject rewardItemPrefab;      // Prefab with icon + text
    public TextMeshProUGUI titleText;        // "Kazanılan Ödüller!"
    public Button closeButton;
    
    [Header("Settings")]
    public float displayDuration = 3f;       // Auto-close after X seconds (0 = manual close only)
    public float itemSpacing = 120f;         // Horizontal spacing between items
    
    [Header("Skill Database")]
    public SkillDatabaseSO skillDatabase;    // To get skill icons
    
    private List<GameObject> spawnedItems = new List<GameObject>();
    
    private void Awake()
    {
        // Set instance in Awake (if panel starts active)
        instance = this;
    }
    
    private void OnEnable()
    {
        // Also set instance on enable - this ensures instance is set
        // even if the GameObject starts disabled in the scene
        instance = this;
    }
    
    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }
    
    /// <summary>
    /// Show the reward panel with won skills
    /// </summary>
    public void ShowRewards(List<SkillType> wonSkills)
    {
        if (rewardPanel == null) return;
        
        Debug.Log($"[RewardDisplay] Showing {wonSkills.Count} rewards");
        
        // Clear previous items
        ClearItems();
        
        // Set title
        if (titleText != null)
        {
            titleText.text = wonSkills.Count > 1 
                ? $"🎉 {wonSkills.Count} Skill Kazandın!" 
                : "🎉 Skill Kazandın!";
        }
        
        // Calculate starting position for centering
        float totalWidth = (wonSkills.Count - 1) * itemSpacing;
        float startX = -totalWidth / 2f;
        
        // Spawn reward items
        for (int i = 0; i < wonSkills.Count; i++)
        {
            if (rewardItemPrefab == null || rewardContainer == null) continue;
            
            GameObject item = Instantiate(rewardItemPrefab, rewardContainer);
            spawnedItems.Add(item);
            
            // Position horizontally
            RectTransform rt = item.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(startX + (i * itemSpacing), 0);
            }
            
            // Setup item visuals
            SetupRewardItem(item, wonSkills[i]);
        }
        
        // Show panel
        rewardPanel.SetActive(true);
        
        // Auto-close after delay
        if (displayDuration > 0)
        {
            StartCoroutine(AutoCloseRoutine());
        }
    }
    
    void SetupRewardItem(GameObject item, SkillType skillType)
    {
        // Find "Icon" child specifically (not just any Image)
        Transform iconTransform = item.transform.Find("Icon");
        Image iconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
        
        // Find "LevelText" child for level display
        Transform levelTransform = item.transform.Find("LevelText");
        TextMeshProUGUI levelText = levelTransform != null ? levelTransform.GetComponent<TextMeshProUGUI>() : null;
        
        // Find name text (first TMP that's not LevelText)
        TextMeshProUGUI nameText = null;
        foreach (var tmp in item.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (tmp != levelText)
            {
                nameText = tmp;
                break;
            }
        }
        
        // Try to get skill data from database
        SkillData skillData = null;
        if (skillDatabase != null)
        {
            skillData = skillDatabase.GetSkill(skillType);
        }
        
        // Set icon on the "Icon" child
        if (iconImage != null && skillData != null && skillData.icon != null)
        {
            iconImage.sprite = skillData.icon;
            iconImage.enabled = true;
        }
        else if (iconImage == null)
        {
            Debug.LogWarning($"[RewardDisplay] Could not find 'Icon' child in RewardItem prefab!");
        }
        
        // Set skill name
        if (nameText != null)
        {
            nameText.text = GetSkillDisplayName(skillType);
        }
        
        // Set level (skill was already upgraded, so this shows current level)
        if (levelText != null)
        {
            int currentLevel = 1;
            if (PlayerSkillManager.instance != null)
            {
                currentLevel = PlayerSkillManager.instance.GetSkillLevel(skillType);
            }
            levelText.text = $"Level {currentLevel}";
        }
    }
    
    string GetSkillDisplayName(SkillType skill)
    {
        string name = skill.ToString();
        string result = "";
        foreach (char c in name)
        {
            if (char.IsUpper(c) && result.Length > 0)
                result += " ";
            result += c;
        }
        return result;
    }
    
    void ClearItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }
        spawnedItems.Clear();
    }
    
    IEnumerator AutoCloseRoutine()
    {
        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Hide();
    }
    
    public void Hide()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(false);
        
        ClearItems();
        
        // Resume game
        Time.timeScale = 1f;
    }
}
