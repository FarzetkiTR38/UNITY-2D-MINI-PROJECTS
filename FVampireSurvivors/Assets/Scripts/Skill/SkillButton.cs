using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI levelText;
    public Image iconImage;

    private SkillData skill;

    public void Setup(SkillData data)
    {
        skill = data;

        titleText.text = data.skillType.ToString();
        levelText.text = "Level " + (data.currentLevel + 1);
        iconImage.sprite = data.icon;
    }

    public void Click()
    {
        SkillSelectionUI.instance.OnSkillSelected(skill.skillType);
    }
}
