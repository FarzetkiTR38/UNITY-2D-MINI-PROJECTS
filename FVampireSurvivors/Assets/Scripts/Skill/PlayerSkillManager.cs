using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public static PlayerSkillManager instance;

    [Header("Skill Database (ScriptableObject)")]
    [SerializeField] private SkillDatabaseSO skillDatabase;

    [Header("Slot Limits")]
    [SerializeField] private int maxActiveSkillSlots = 8;
    [SerializeField] private int maxPassiveSkillSlots = 8;

    // Runtime skill list
    private List<SkillData> allSkills = new List<SkillData>();

    public event Action SkillsChanged;

    private int _unlockCounter = 0;

    // Active skill'ler (silahlar)
    public static readonly HashSet<SkillType> ActiveSkills = new HashSet<SkillType>
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

    // Passive skill'ler
    public static readonly HashSet<SkillType> PassiveSkills = new HashSet<SkillType>
    {
        SkillType.MoveSpeed,
        SkillType.MaxHealth,
        SkillType.Magnet,
        SkillType.Damage,
        SkillType.AttackSpeed,
        SkillType.ProjectileCount,
        SkillType.AreaSize,
        SkillType.XPGain,
        SkillType.CriticalChance,
        SkillType.CriticalDamage,
        SkillType.Lifesteal,
        SkillType.HealthRegen,
        SkillType.Armor
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
        // Active skill - use PlayerSkillsController
        if (IsActiveSkill(skill.skillType))
        {
            PlayerSkillsController.instance?.UpgradeSkill(skill.skillType, skill.currentLevel);
            return;
        }

        // Passive skills
        switch (skill.skillType)
        {
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
            // Evolved skills
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

    // ==================
    // NEW: Slot-based filtering
    // ==================

    /// <summary>
    /// Get random skills for level-up selection
    /// Respects 8-slot limits: if 8 skills selected, only those skills appear
    /// </summary>
    public List<SkillData> GetRandomSkills(int count)
    {
        List<SkillData> pool = new List<SkillData>();

        List<SkillData> unlockedActives = GetUnlockedActiveSkills();
        List<SkillData> unlockedPassives = GetUnlockedPassiveSkills();

        bool activeSlotsFull = unlockedActives.Count >= maxActiveSkillSlots;
        bool passiveSlotsFull = unlockedPassives.Count >= maxPassiveSkillSlots;

        foreach (var skill in allSkills)
        {
            // Skip evolved skills
            if (IsEvolvedSkill(skill.skillType)) continue;

            // Skip max level skills
            if (skill.currentLevel >= skill.maxLevel) continue;

            bool isActive = IsActiveSkill(skill.skillType);
            bool isPassive = IsPassiveSkill(skill.skillType);

            // ACTIVE SKILL LOGIC
            if (isActive)
            {
                if (activeSlotsFull)
                {
                    // Only show already selected active skills
                    if (skill.isUnlocked && skill.currentLevel > 0)
                        pool.Add(skill);
                }
                else
                {
                    // Slots available - show all active skills
                    pool.Add(skill);
                }
            }
            // PASSIVE SKILL LOGIC
            else if (isPassive)
            {
                if (passiveSlotsFull)
                {
                    // Only show already selected passive skills
                    if (skill.isUnlocked && skill.currentLevel > 0)
                        pool.Add(skill);
                }
                else
                {
                    // Slots available - show all passive skills
                    pool.Add(skill);
                }
            }
        }

        // Random selection
        List<SkillData> result = new List<SkillData>();
        while (result.Count < count && pool.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    // ==================
    // HUD Helper Methods
    // ==================

    /// <summary>
    /// Get only unlocked ACTIVE skills (for skill HUD)
    /// </summary>
    public List<SkillData> GetUnlockedActiveSkills()
    {
        List<SkillData> result = new List<SkillData>();
        foreach (var s in allSkills)
        {
            if (s.isUnlocked && s.currentLevel > 0 && IsActiveSkill(s.skillType))
            {
                result.Add(s);
            }
        }
        result.Sort((a, b) => a.unlockOrder.CompareTo(b.unlockOrder));
        return result;
    }

    /// <summary>
    /// Get only unlocked PASSIVE skills (for passive HUD)
    /// </summary>
    public List<SkillData> GetUnlockedPassiveSkills()
    {
        List<SkillData> result = new List<SkillData>();
        foreach (var s in allSkills)
        {
            if (s.isUnlocked && s.currentLevel > 0 && IsPassiveSkill(s.skillType))
            {
                result.Add(s);
            }
        }
        result.Sort((a, b) => a.unlockOrder.CompareTo(b.unlockOrder));
        return result;
    }

    /// <summary>
    /// Get all unlocked skills (skills first, then passives)
    /// </summary>
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

            return a.unlockOrder.CompareTo(b.unlockOrder);
        });

        return unlocked;
    }

    // ==================
    // Utility Methods
    // ==================

    public bool IsActiveSkill(SkillType type) => ActiveSkills.Contains(type);
    public bool IsPassiveSkill(SkillType type) => PassiveSkills.Contains(type);

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

    public bool IsSkillMaxLevel(SkillType type)
    {
        SkillData skill = allSkills.Find(s => s.skillType == type);
        if (skill == null) return false;
        return skill.currentLevel >= skill.maxLevel;
    }

    public int GetSkillLevel(SkillType type)
    {
        SkillData skill = allSkills.Find(s => s.skillType == type);
        return skill?.currentLevel ?? 0;
    }

    /// <summary>
    /// Check if active skill slots are full
    /// </summary>
    public bool AreActiveSlotsFull() => GetUnlockedActiveSkills().Count >= maxActiveSkillSlots;

    /// <summary>
    /// Check if passive skill slots are full
    /// </summary>
    public bool ArePassiveSlotsFull() => GetUnlockedPassiveSkills().Count >= maxPassiveSkillSlots;
}
