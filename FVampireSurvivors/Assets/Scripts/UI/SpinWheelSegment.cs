using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual segment on the spin wheel.
/// Attach to segment prefab.
/// </summary>
public class SpinWheelSegment : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public Image iconImage;
    public TextMeshProUGUI skillNameText;
    
    [Header("Colors")]
    public Color activeSkillColor = new Color(0.2f, 0.6f, 1f, 1f);   // Blue for active
    public Color passiveSkillColor = new Color(0.2f, 0.8f, 0.4f, 1f); // Green for passive
    
    private SkillType skillType;
    
    /// <summary>
    /// Setup the segment with a skill
    /// </summary>
    public void Setup(SkillType skill, float angleSize)
    {
        skillType = skill;
        
        // Set name text
        if (skillNameText != null)
        {
            skillNameText.text = GetSkillDisplayName(skill);
        }
        
        // Set background color based on skill type
        if (backgroundImage != null)
        {
            backgroundImage.color = IsActiveSkill(skill) ? activeSkillColor : passiveSkillColor;
        }
        
        // Try to get icon from skill database
        if (iconImage != null && PlayerSkillManager.instance != null)
        {
            var skillData = GetSkillData(skill);
            if (skillData != null && skillData.icon != null)
            {
                iconImage.sprite = skillData.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
    }
    
    string GetSkillDisplayName(SkillType skill)
    {
        // Convert enum to readable name
        string name = skill.ToString();
        
        // Add spaces before capital letters
        string result = "";
        foreach (char c in name)
        {
            if (char.IsUpper(c) && result.Length > 0)
                result += " ";
            result += c;
        }
        
        return result;
    }
    
    bool IsActiveSkill(SkillType skill)
    {
        return skill <= SkillType.BlackHole;
    }
    
    SkillData GetSkillData(SkillType skill)
    {
        // Try to find skill data from PlayerSkillManager
        if (PlayerSkillManager.instance == null) return null;
        
        // Access internal skills list via reflection or public method
        // For now, return null - icon will be disabled
        return null;
    }
    
    public SkillType GetSkillType()
    {
        return skillType;
    }
}
