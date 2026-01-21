using UnityEngine;

/// <summary>
/// Manages evolved skill effects and bonuses
/// Added to Player when an evolution is activated
/// </summary>
public class EvolvedSkillEffects : MonoBehaviour
{
    public static EvolvedSkillEffects instance;

    // Evolution states
    [Header("Active Evolutions")]
    public bool beastModeActive;      // Fireball + HealthRegen
    public bool bladeStormActive;     // Sword + AttackSpeed
    public bool vampiricFieldActive;  // AuraDamage + Lifesteal
    public bool frozenWorldActive;    // IceShards + AreaSize
    public bool meteorFireActive;     // MeteorShower + CriticalDamage
    public bool greedyOverlordActive; // XPGain + Damage
    public bool immortalFormActive;   // HealthRegen + MaxHealth

    [Header("ImmortalForm Settings")]
    public float immortalDuration = 3f;
    public float immortalCooldown = 60f;
    private float immortalTimer = 0f;
    private bool isImmortal = false;
    private float immortalCooldownTimer = 0f;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // Handle ImmortalForm cooldown and duration
        if (immortalFormActive)
        {
            HandleImmortalForm();
        }
    }

    /// <summary>
    /// Activate an evolution effect
    /// </summary>
    public void ActivateEvolution(SkillType evolvedType)
    {
        // Swap prefab first (if available)
        PlayerSkillsController.instance?.SwapToEvolvedPrefab(evolvedType);

        switch (evolvedType)
        {
            case SkillType.BeastMode:
                ActivateBeastMode();
                break;
            case SkillType.BladeStorm:
                ActivateBladeStorm();
                break;
            case SkillType.VampiricField:
                ActivateVampiricField();
                break;
            case SkillType.FrozenWorld:
                ActivateFrozenWorld();
                break;
            case SkillType.MeteorFire:
                ActivateMeteorFire();
                break;
            case SkillType.GreedyOverlord:
                ActivateGreedyOverlord();
                break;
            case SkillType.ImmortalForm:
                ActivateImmortalForm();
                break;
        }
    }

    // ==================
    // BEAST MODE
    // Fireball + HealthRegen
    // Effect: +2 HP per fireball hit
    // ==================
    void ActivateBeastMode()
    {
        beastModeActive = true;
        Debug.Log("<color=orange>🔥 BEAST MODE ACTIVATED! Fireballs heal +2 HP per hit</color>");
    }

    public int GetBeastModeHeal()
    {
        return beastModeActive ? 2 : 0;
    }

    // ==================
    // BLADE STORM
    // Sword + AttackSpeed
    // Effect: 2x sword speed, +50% damage
    // ==================
    void ActivateBladeStorm()
    {
        bladeStormActive = true;
        Debug.Log("<color=cyan>⚔️ BLADE STORM ACTIVATED! 2x sword speed, +50% damage</color>");
    }

    public float GetBladeStormSpeedMultiplier()
    {
        return bladeStormActive ? 2f : 1f;
    }

    public float GetBladeStormDamageMultiplier()
    {
        return bladeStormActive ? 1.5f : 1f;
    }

    // ==================
    // VAMPIRIC FIELD
    // AuraDamage + Lifesteal
    // Effect: Aura damage gives 30% lifesteal
    // ==================
    void ActivateVampiricField()
    {
        vampiricFieldActive = true;
        Debug.Log("<color=red>🩸 VAMPIRIC FIELD ACTIVATED! Aura damage gives 30% lifesteal</color>");
    }

    public float GetVampiricFieldLifesteal()
    {
        return vampiricFieldActive ? 0.3f : 0f;
    }

    // ==================
    // FROZEN WORLD
    // IceShards + AreaSize
    // Effect: 2x radius, 80% slow
    // ==================
    void ActivateFrozenWorld()
    {
        frozenWorldActive = true;
        Debug.Log("<color=blue>❄️ FROZEN WORLD ACTIVATED! 2x ice radius, 80% slow</color>");
    }

    public float GetFrozenWorldRadiusMultiplier()
    {
        return frozenWorldActive ? 2f : 1f;
    }

    public float GetFrozenWorldSlowPercent()
    {
        return frozenWorldActive ? 0.8f : 0.5f; // 80% vs default 50%
    }

    // ==================
    // METEOR FIRE
    // MeteorShower + CriticalDamage
    // Effect: Meteors always crit
    // ==================
    void ActivateMeteorFire()
    {
        meteorFireActive = true;
        Debug.Log("<color=yellow>☄️ METEOR FIRE ACTIVATED! All meteors are critical hits!</color>");
    }

    public bool IsMeteorAlwaysCrit()
    {
        return meteorFireActive;
    }

    // ==================
    // GREEDY OVERLORD
    // XPGain + Damage
    // Effect: 2x XP, 1.5x damage
    // ==================
    void ActivateGreedyOverlord()
    {
        greedyOverlordActive = true;
        
        // Apply permanent bonuses
        if (PassiveStats.instance != null)
        {
            PassiveStats.instance.xpGainMultiplier *= 2f;
            PassiveStats.instance.damageMultiplier *= 1.5f;
        }
        
        Debug.Log("<color=gold>💰 GREEDY OVERLORD ACTIVATED! 2x XP, 1.5x damage</color>");
    }

    // ==================
    // IMMORTAL FORM
    // HealthRegen + MaxHealth
    // Effect: 3s invincibility every 60s
    // ==================
    void ActivateImmortalForm()
    {
        immortalFormActive = true;
        immortalCooldownTimer = 0f; // Ready to use immediately
        Debug.Log("<color=white>✨ IMMORTAL FORM ACTIVATED! 3s invincibility every 60s</color>");
    }

    void HandleImmortalForm()
    {
        if (isImmortal)
        {
            immortalTimer -= Time.deltaTime;
            if (immortalTimer <= 0f)
            {
                isImmortal = false;
                immortalCooldownTimer = immortalCooldown;
                Debug.Log("<color=white>✨ Immortal Form ended - Cooldown started</color>");
            }
        }
        else
        {
            if (immortalCooldownTimer > 0f)
            {
                immortalCooldownTimer -= Time.deltaTime;
            }
            else
            {
                // Activate immortality
                TriggerImmortality();
            }
        }
    }

    void TriggerImmortality()
    {
        isImmortal = true;
        immortalTimer = immortalDuration;
        Debug.Log("<color=white>✨ IMMORTAL! Invincible for 3 seconds!</color>");
    }

    public bool IsImmortal()
    {
        return isImmortal;
    }
}
