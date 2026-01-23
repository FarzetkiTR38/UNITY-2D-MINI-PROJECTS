using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual segment on the spin wheel.
/// Uses PieSlice for pizza-slice shape!
/// </summary>
public class SpinWheelSegment : MonoBehaviour
{
    [Header("UI References")]
    public PieSlice pieSlice;           // Pizza slice background
    public TextMeshProUGUI skillNameText;
    public Image iconImage;             // Optional skill icon
    
    [Header("Text Settings")]
    [Tooltip("How far from center the text is positioned (higher = closer to edge)")]
    public float textRadius = 120f;
    
    [Header("Skill Type Colors")]
    public Color[] activeSkillColors = new Color[]
    {
        new Color(0.86f, 0.20f, 0.20f, 1f),  // Red
        new Color(0.20f, 0.60f, 0.86f, 1f),  // Blue
        new Color(0.20f, 0.75f, 0.40f, 1f),  // Green
        new Color(0.95f, 0.70f, 0.20f, 1f),  // Orange
        new Color(0.70f, 0.30f, 0.80f, 1f),  // Purple
        new Color(0.95f, 0.85f, 0.20f, 1f),  // Yellow
        new Color(0.20f, 0.85f, 0.85f, 1f),  // Cyan
        new Color(0.90f, 0.40f, 0.60f, 1f),  // Pink
    };
    
    public Color[] passiveSkillColors = new Color[]
    {
        new Color(0.40f, 0.70f, 0.40f, 1f),  // Light Green
        new Color(0.50f, 0.50f, 0.80f, 1f),  // Light Purple
        new Color(0.70f, 0.70f, 0.70f, 1f),  // Gray
        new Color(0.60f, 0.80f, 0.95f, 1f),  // Light Blue
    };
    
    private SkillType skillType;
    private static int colorIndex = 0;
    
    /// <summary>
    /// Setup the segment with a skill
    /// </summary>
    public void Setup(SkillType skill, float angleSize, float rotationAngle, int segmentIndex)
    {
        skillType = skill;
        
        // Setup pie slice shape
        if (pieSlice != null)
        {
            // Pick color based on segment index for variety
            Color sliceColor;
            if (IsActiveSkill(skill))
            {
                sliceColor = activeSkillColors[segmentIndex % activeSkillColors.Length];
            }
            else
            {
                sliceColor = passiveSkillColors[segmentIndex % passiveSkillColors.Length];
            }
            
            Debug.Log($"[Segment {segmentIndex}] Skill={skill}, Angle={angleSize}, Rotation={rotationAngle}");
            pieSlice.SetSlice(angleSize, rotationAngle, sliceColor);
        }
        else
        {
            Debug.LogWarning($"[Segment {segmentIndex}] pieSlice is NULL! Make sure to assign it in prefab.");
        }
        
        // Set name text
        if (skillNameText != null)
        {
            skillNameText.text = GetSkillDisplayName(skill);
            
            // Position text on the slice (uses Inspector textRadius value)
            float textAngle = rotationAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(textAngle) * textRadius;
            float y = Mathf.Sin(textAngle) * textRadius;
            
            skillNameText.rectTransform.anchoredPosition = new Vector2(x, y);
            
            // Rotate text to be readable
            float textRotation = rotationAngle - 90f;
            if (rotationAngle > 90f && rotationAngle < 270f)
            {
                textRotation += 180f; // Flip text on bottom half
            }
            skillNameText.rectTransform.localRotation = Quaternion.Euler(0, 0, textRotation);
        }
        
        // Icon (optional)
        if (iconImage != null)
        {
            iconImage.enabled = false; // Disable for now
        }
    }
    
    // Overload for backwards compatibility
    public void Setup(SkillType skill, float angleSize)
    {
        Setup(skill, angleSize, 0f, 0);
    }
    
    string GetSkillDisplayName(SkillType skill)
    {
        string name = skill.ToString();
        string result = "";
        foreach (char c in name)
        {
            if (char.IsUpper(c) && result.Length > 0)
                result += "\n"; // Line break instead of space for vertical text
            result += c;
        }
        return result;
    }
    
    bool IsActiveSkill(SkillType skill)
    {
        return skill <= SkillType.BlackHole;
    }
    
    public SkillType GetSkillType()
    {
        return skillType;
    }
}
