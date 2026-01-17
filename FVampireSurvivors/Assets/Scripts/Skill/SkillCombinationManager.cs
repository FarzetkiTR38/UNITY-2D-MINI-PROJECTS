using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages skill combinations/evolutions
/// When both required skills reach max level, the evolved skill is unlocked
/// </summary>
public class SkillCombinationManager : MonoBehaviour
{
    public static SkillCombinationManager instance;

    [System.Serializable]
    public class SkillCombination
    {
        public SkillType activeSkill;       // Required active skill (must be max level)
        public SkillType passiveSkill;      // Required passive skill (must be max level)
        public SkillType evolvedSkill;      // Result skill
        public bool isUnlocked = false;     // Has this combination been triggered?
    }

    [Header("Combinations")]
    public List<SkillCombination> combinations = new List<SkillCombination>();

    public event Action<SkillCombination> OnCombinationUnlocked;

    private void Awake()
    {
        instance = this;
        SetupDefaultCombinations();
    }

    void SetupDefaultCombinations()
    {
        // Clear and set up default combinations
        combinations.Clear();

        // Fireball + HealthRegen = Beast Mode
        combinations.Add(new SkillCombination
        {
            activeSkill = SkillType.Fireball,
            passiveSkill = SkillType.HealthRegen,
            evolvedSkill = SkillType.BeastMode
        });

        // Sword + AttackSpeed = Blade Storm
        combinations.Add(new SkillCombination
        {
            activeSkill = SkillType.Sword,
            passiveSkill = SkillType.AttackSpeed,
            evolvedSkill = SkillType.BladeStorm
        });

        // AuraDamage + Lifesteal = Vampiric Field
        combinations.Add(new SkillCombination
        {
            activeSkill = SkillType.AuraDamage,
            passiveSkill = SkillType.Lifesteal,
            evolvedSkill = SkillType.VampiricField
        });

        // IceShards + AreaSize = Frozen World
        combinations.Add(new SkillCombination
        {
            activeSkill = SkillType.IceShards,
            passiveSkill = SkillType.AreaSize,
            evolvedSkill = SkillType.FrozenWorld
        });

        // MeteorShower + CriticalDamage = Meteor Fire
        combinations.Add(new SkillCombination
        {
            activeSkill = SkillType.MeteorShower,
            passiveSkill = SkillType.CriticalDamage,
            evolvedSkill = SkillType.MeteorFire
        });

        // XPGain + Damage = Greedy Overlord
        combinations.Add(new SkillCombination
        {
            activeSkill = SkillType.ChainLightning,
            passiveSkill = SkillType.XPGain,
            evolvedSkill = SkillType.GreedyOverlord
        });

        // HealthRegen + MaxHealth = Immortal Form
        combinations.Add(new SkillCombination
        {
            activeSkill = SkillType.Whirlwind,
            passiveSkill = SkillType.MaxHealth,
            evolvedSkill = SkillType.ImmortalForm
        });
    }

    private void Start()
    {
        // Subscribe to skill changes
        if (PlayerSkillManager.instance != null)
        {
            PlayerSkillManager.instance.SkillsChanged += CheckCombinations;
        }
    }

    private void OnDestroy()
    {
        if (PlayerSkillManager.instance != null)
        {
            PlayerSkillManager.instance.SkillsChanged -= CheckCombinations;
        }
    }

    /// <summary>
    /// Check all combinations when skills change
    /// </summary>
    public void CheckCombinations()
    {
        foreach (var combo in combinations)
        {
            if (combo.isUnlocked) continue;

            // Check if both required skills are at max level
            bool activeMaxed = PlayerSkillManager.instance.IsSkillMaxLevel(combo.activeSkill);
            bool passiveMaxed = PlayerSkillManager.instance.IsSkillMaxLevel(combo.passiveSkill);

            if (activeMaxed && passiveMaxed)
            {
                UnlockCombination(combo);
            }
        }
    }

    void UnlockCombination(SkillCombination combo)
    {
        combo.isUnlocked = true;

        Debug.Log($"[Combination] Unlocked: {combo.activeSkill} + {combo.passiveSkill} = {combo.evolvedSkill}");

        // Upgrade the evolved skill
        PlayerSkillManager.instance.UpgradeSkill(combo.evolvedSkill);

        // Fire event for UI
        OnCombinationUnlocked?.Invoke(combo);
    }

    /// <summary>
    /// Get all available (but not yet unlocked) combinations for UI display
    /// </summary>
    public List<SkillCombination> GetAvailableCombinations()
    {
        List<SkillCombination> available = new List<SkillCombination>();

        foreach (var combo in combinations)
        {
            if (!combo.isUnlocked)
            {
                // Show combinations where at least one skill is unlocked
                int activeLevel = PlayerSkillManager.instance.GetSkillLevel(combo.activeSkill);
                int passiveLevel = PlayerSkillManager.instance.GetSkillLevel(combo.passiveSkill);

                if (activeLevel > 0 || passiveLevel > 0)
                {
                    available.Add(combo);
                }
            }
        }

        return available;
    }

    /// <summary>
    /// Get progress towards a specific combination (0.0 to 1.0)
    /// </summary>
    public float GetCombinationProgress(SkillCombination combo)
    {
        if (combo.isUnlocked) return 1f;

        bool activeMaxed = PlayerSkillManager.instance.IsSkillMaxLevel(combo.activeSkill);
        bool passiveMaxed = PlayerSkillManager.instance.IsSkillMaxLevel(combo.passiveSkill);

        if (activeMaxed && passiveMaxed) return 1f;
        if (activeMaxed || passiveMaxed) return 0.5f;
        return 0f;
    }
}
