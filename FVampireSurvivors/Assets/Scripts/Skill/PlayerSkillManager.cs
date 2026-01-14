using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public static PlayerSkillManager instance;

    public List<SkillData> allSkills = new List<SkillData>();

    private void Awake()
    {
        instance = this;
        InitializeSkills();
    }

    void InitializeSkills()
    {
        foreach (var skill in allSkills)
        {
            if (skill.skillType == SkillType.Fireball)
            {
                skill.isUnlocked = true;
                skill.currentLevel = 1;
            }
            else
            {
                skill.isUnlocked = false;
                skill.currentLevel = 0;
            }
        }
    }

    public void UpgradeSkill(SkillType type)
    {
        SkillData skill = allSkills.Find(s => s.skillType == type);

        if (skill == null) return;
        if (skill.currentLevel >= skill.maxLevel) return;

        skill.isUnlocked = true;
        skill.currentLevel++;

        ApplySkillEffect(skill);
    }

void ApplySkillEffect(SkillData skill)
{
    switch (skill.skillType)
    {
        case SkillType.Fireball:
            // FindObjectOfType<PlayerAutoAttack>().Upgrade(skill.currentLevel);
            FindAnyObjectByType<PlayerAutoAttack>().Upgrade(skill.currentLevel);
            break;

        case SkillType.Sword:
            // FindObjectOfType<PlayerSwordSkill>().Upgrade(skill.currentLevel);
            FindAnyObjectByType<PlayerSwordSkill>().Upgrade(skill.currentLevel);
            break;

        case SkillType.MoveSpeed:
            // FindObjectOfType<PlayerController>().UpgradeSpeed(skill.currentLevel);
            FindAnyObjectByType<PlayerController>().UpgradeSpeed(skill.currentLevel);
            break;

        case SkillType.MaxHealth:
            // FindObjectOfType<PlayerHealthController>().UpgradeHealth(skill.currentLevel);
            FindAnyObjectByType<PlayerHealthController>().UpgradeHealth(skill.currentLevel);
            break;

        case SkillType.Magnet:
            XPOrbGlobalSettings.instance.UpgradeMagnet(skill.currentLevel);
            break;

        case SkillType.Damage:
            // FindObjectOfType<PlayerAutoAttack>().UpgradeDamage(skill.currentLevel);
            FindAnyObjectByType<PlayerAutoAttack>().UpgradeDamage(skill.currentLevel);
            break;
    }
}

    public List<SkillData> GetRandomSkills(int count)
    {
        List<SkillData> pool = new List<SkillData>();

        foreach (var skill in allSkills)
        {
            if (skill.currentLevel < skill.maxLevel)
                pool.Add(skill);
        }

        List<SkillData> result = new List<SkillData>();

        while (result.Count < count && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}
