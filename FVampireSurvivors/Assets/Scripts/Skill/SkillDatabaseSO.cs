using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VS/Skill Database", fileName = "SkillDatabase")]
public class SkillDatabaseSO : ScriptableObject
{
    [Header("All Available Skills")]
    public List<SkillData> skills = new List<SkillData>();

    /// <summary>
    /// Belirtilen SkillType'a sahip skill'i döndürür.
    /// </summary>
    public SkillData GetSkill(SkillType type)
    {
        foreach (var skill in skills)
        {
            if (skill.skillType == type)
                return skill;
        }
        return null;
    }

    /// <summary>
    /// Tüm skill'lerin kopyasını döndürür (runtime değişiklikler için).
    /// </summary>
    public List<SkillData> CloneSkills()
    {
        List<SkillData> clones = new List<SkillData>();
        
        foreach (var skill in skills)
        {
            clones.Add(new SkillData
            {
                skillType = skill.skillType,
                currentLevel = skill.currentLevel,
                maxLevel = skill.maxLevel,
                isUnlocked = skill.isUnlocked,
                icon = skill.icon,
                unlockOrder = skill.unlockOrder
            });
        }
        
        return clones;
    }
}
