using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public static PlayerSkillManager instance;

    [Header("Skill Database (ScriptableObject)")]
    [SerializeField] private SkillDatabaseSO skillDatabase;

    // Runtime'da kullanılacak klonlanmış skill listesi
    private List<SkillData> allSkills = new List<SkillData>();

    public event Action SkillsChanged;

    private int _unlockCounter = 0;

    // Active skill'ler (silahlar)
    private static readonly HashSet<SkillType> ActiveSkills = new HashSet<SkillType>
    {
        SkillType.Fireball,
        SkillType.Sword,
        SkillType.HomingMissiles,
        SkillType.IceShards,
        SkillType.PiercingArrows,
        SkillType.FanOfDaggers,
        SkillType.Whirlwind,
        SkillType.AuraDamage,
        SkillType.ShockwavePulse,
        SkillType.ChainLightning,
        SkillType.Boomerang,
        SkillType.SpinningScythes,
        SkillType.ConeAttack,
        SkillType.MeteorShower,
        SkillType.ExplodingProjectiles,
        SkillType.LaserBeam,
        SkillType.Turret,
        SkillType.BlackHole
    };

    private void Awake()
    {
        instance = this;
        InitializeSkills();
    }

    void InitializeSkills()
    {
        if (skillDatabase == null)
        {
            Debug.LogError("[PlayerSkillManager] SkillDatabase atanmamış!");
            return;
        }

        allSkills = skillDatabase.CloneSkills();

        foreach (var s in allSkills)
            s.unlockOrder = int.MaxValue;

        // Fireball başlangıç açık
        foreach (var skill in allSkills)
        {
            if (skill.skillType == SkillType.Fireball)
            {
                skill.isUnlocked = true;
                skill.currentLevel = 1;
                if (skill.unlockOrder == int.MaxValue)
                    skill.unlockOrder = _unlockCounter++;
            }
            else
            {
                skill.isUnlocked = false;
                skill.currentLevel = 0;
            }
        }

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

        if (!skill.isUnlocked)
        {
            skill.isUnlocked = true;
            if (skill.unlockOrder == int.MaxValue)
                skill.unlockOrder = _unlockCounter++;
        }

        skill.currentLevel++;
        ApplySkillEffect(skill);
        SkillsChanged?.Invoke();
    }

    void ApplySkillEffect(SkillData skill)
    {
        // Check if it's an active skill - use PlayerSkillsController
        if (IsActiveSkill(skill.skillType))
        {
            PlayerSkillsController.instance?.UpgradeSkill(skill.skillType, skill.currentLevel);
            return;
        }

        // Handle passive and other skills
        switch (skill.skillType)
        {
            // ==================
            // PASSIVE SKILLS
            // ==================
            case SkillType.MoveSpeed:
                FindAnyObjectByType<PlayerController>()?.UpgradeSpeed(skill.currentLevel);
                break;

            case SkillType.MaxHealth:
                FindAnyObjectByType<PlayerHealthController>()?.UpgradeHealth(skill.currentLevel);
                break;

            case SkillType.Magnet:
                XPOrbGlobalSettings.instance?.UpgradeMagnet(skill.currentLevel);
                break;

            case SkillType.Damage:
                PassiveStats.instance?.UpgradeDamage(skill.currentLevel);
                break;

            case SkillType.AttackSpeed:
                PassiveStats.instance?.UpgradeAttackSpeed(skill.currentLevel);
                break;

            case SkillType.ProjectileCount:
                PassiveStats.instance?.UpgradeProjectileCount(skill.currentLevel);
                break;

            case SkillType.AreaSize:
                PassiveStats.instance?.UpgradeAreaSize(skill.currentLevel);
                break;

            case SkillType.XPGain:
                PassiveStats.instance?.UpgradeXPGain(skill.currentLevel);
                break;

            case SkillType.CriticalChance:
                PassiveStats.instance?.UpgradeCriticalChance(skill.currentLevel);
                break;

            case SkillType.CriticalDamage:
                PassiveStats.instance?.UpgradeCriticalDamage(skill.currentLevel);
                break;

            case SkillType.Lifesteal:
                PassiveStats.instance?.UpgradeLifesteal(skill.currentLevel);
                break;

            case SkillType.HealthRegen:
                PassiveStats.instance?.UpgradeHealthRegen(skill.currentLevel);
                break;

            case SkillType.Armor:
                PassiveStats.instance?.UpgradeArmor(skill.currentLevel);
                break;

            // ==================
            // COMBINED/EVOLVED SKILLS
            // ==================
            case SkillType.BeastMode:
            case SkillType.BladeStorm:
            case SkillType.VampiricField:
            case SkillType.FrozenWorld:
            case SkillType.MeteorFire:
            case SkillType.GreedyOverlord:
            case SkillType.ImmortalForm:
                Debug.Log($"Evolved skill {skill.skillType} activated!");
                break;
        }
    }

    bool IsActiveSkill(SkillType type)
    {
        return ActiveSkills.Contains(type);
    }

    public List<SkillData> GetRandomSkills(int count)
    {
        List<SkillData> pool = new List<SkillData>();

        foreach (var skill in allSkills)
        {
            // Don't show evolved skills in random pool (they unlock via combination)
            if (IsEvolvedSkill(skill.skillType)) continue;
            
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

    bool IsEvolvedSkill(SkillType type)
    {
        return type == SkillType.BeastMode ||
               type == SkillType.BladeStorm ||
               type == SkillType.VampiricField ||
               type == SkillType.FrozenWorld ||
               type == SkillType.MeteorFire ||
               type == SkillType.GreedyOverlord ||
               type == SkillType.ImmortalForm;
    }

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

            if (aActive != bActive)
                return aActive ? -1 : 1;

            int cmp = a.unlockOrder.CompareTo(b.unlockOrder);
            if (cmp != 0) return cmp;

            return a.skillType.CompareTo(b.skillType);
        });

        return unlocked;
    }

    /// <summary>
    /// Check if a skill is at max level
    /// </summary>
    public bool IsSkillMaxLevel(SkillType type)
    {
        SkillData skill = allSkills.Find(s => s.skillType == type);
        if (skill == null) return false;
        return skill.currentLevel >= skill.maxLevel;
    }

    /// <summary>
    /// Get current level of a skill
    /// </summary>
    public int GetSkillLevel(SkillType type)
    {
        SkillData skill = allSkills.Find(s => s.skillType == type);
        return skill?.currentLevel ?? 0;
    }
}
