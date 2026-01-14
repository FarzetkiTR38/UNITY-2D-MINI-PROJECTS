using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public static PlayerSkillManager instance;

    [Header("All skills (set in Inspector)")]
    public List<SkillData> allSkills = new List<SkillData>();

    // HUD ve diğer UI'lar dinlesin diye event
    public event Action SkillsChanged;

    private int _unlockCounter = 0;

    // Active skill önceliği (önce bunlar gösterilecek)
    private static readonly HashSet<SkillType> ActiveSkills = new HashSet<SkillType>
    {
        SkillType.Fireball,
        SkillType.Sword
    };

    private void Awake()
    {
        instance = this;
        InitializeSkills();
    }

    void InitializeSkills()
    {
        // tüm skill runtime unlock order reset
        foreach (var s in allSkills)
            s.unlockOrder = int.MaxValue;

        // Fireball başlangıç açık (Level 1)
        foreach (var skill in allSkills)
        {
            if (skill.skillType == SkillType.Fireball)
            {
                skill.isUnlocked = true;

                // Eğer inspector’da 0 verdin diye burada +1 mantığı karışmasın:
                // Fireball'u net olarak 1 yapıyoruz.
                skill.currentLevel = 1;

                // unlockOrder ver
                if (skill.unlockOrder == int.MaxValue)
                    skill.unlockOrder = _unlockCounter++;
            }
            else
            {
                skill.isUnlocked = false;
                skill.currentLevel = 0;
            }
        }

        // Başlangıç effect uygulansın (Fireball Level 1)
        ApplyAllUnlockedEffects();

        SkillsChanged?.Invoke();
    }

    void ApplyAllUnlockedEffects()
    {
        foreach (var s in allSkills)
        {
            if (s.isUnlocked && s.currentLevel > 0)
                ApplySkillEffect(s);
        }
    }

    public void UpgradeSkill(SkillType type)
    {
        SkillData skill = allSkills.Find(s => s.skillType == type);
        if (skill == null) return;

        if (skill.currentLevel >= skill.maxLevel) return;

        // İlk defa açılıyorsa unlockOrder ata
        if (!skill.isUnlocked)
        {
            skill.isUnlocked = true;
            if (skill.unlockOrder == int.MaxValue)
                skill.unlockOrder = _unlockCounter++;
        }

        // Level arttır
        skill.currentLevel++;

        ApplySkillEffect(skill);

        SkillsChanged?.Invoke();
    }

    void ApplySkillEffect(SkillData skill)
    {
        switch (skill.skillType)
        {
            case SkillType.Fireball:
                FindAnyObjectByType<PlayerAutoAttack>().Upgrade(skill.currentLevel);
                break;

            case SkillType.Sword:
                FindAnyObjectByType<PlayerSwordSkill>().Upgrade(skill.currentLevel);
                break;

            case SkillType.MoveSpeed:
                FindAnyObjectByType<PlayerController>().UpgradeSpeed(skill.currentLevel);
                break;

            case SkillType.MaxHealth:
                FindAnyObjectByType<PlayerHealthController>().UpgradeHealth(skill.currentLevel);
                break;

            case SkillType.Magnet:
                XPOrbGlobalSettings.instance.UpgradeMagnet(skill.currentLevel);
                break;

            case SkillType.Damage:
                FindAnyObjectByType<PlayerAutoAttack>().UpgradeDamage(skill.currentLevel);
                break;
        }
    }

    // LevelUp panelinde 3 random seçim için (max level olanlar çıkmaz)
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
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    // HUD için: Açık olanları "aktif önce, sonra pasif" + açılma sırası
    public List<SkillData> GetOrderedUnlockedSkills()
    {
        List<SkillData> unlocked = new List<SkillData>();

        foreach (var s in allSkills)
        {
            if (s.isUnlocked && s.currentLevel > 0)
                unlocked.Add(s);
        }

        unlocked.Sort((a, b) =>
        {
            bool aActive = ActiveSkills.Contains(a.skillType);
            bool bActive = ActiveSkills.Contains(b.skillType);

            // aktifler önce
            if (aActive != bActive)
                return aActive ? -1 : 1;

            // aynı gruptaysa unlockOrder’a göre
            int cmp = a.unlockOrder.CompareTo(b.unlockOrder);
            if (cmp != 0) return cmp;

            // fallback (stabil)
            return a.skillType.CompareTo(b.skillType);
        });

        return unlocked;
    }
}
