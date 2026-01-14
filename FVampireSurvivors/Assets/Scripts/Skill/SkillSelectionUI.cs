using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionUI : MonoBehaviour
{
    public static SkillSelectionUI instance;
    public SkillButton[] buttons;

    private void Awake()
    {
        instance = this;
    }

    public void ShowSkills(List<SkillData> skills)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].Setup(skills[i]);
        }
    }

    public void OnSkillSelected(SkillType type)
    {
        PlayerSkillManager.instance.UpgradeSkill(type);

        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
