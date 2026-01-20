using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Auto Attack Settings - Toggle auto vs manual attack mode
/// Attach to a UI Button for toggle functionality
/// </summary>
public class AutoAttackSettings : MonoBehaviour
{
    public static AutoAttackSettings instance;

    [Header("Current State")]
    [Tooltip("true = auto-target enemies, false = mouse targeting")]
    public bool isAutoAttackEnabled = true;

    [Header("UI (Optional)")]
    public Button toggleButton;
    public Image buttonIcon;
    public Sprite autoIcon;
    public Sprite manualIcon;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Setup button listener
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleAutoAttack);
        }

        // Apply initial state
        ApplyAutoAttackState();
        UpdateUI();
    }

    /// <summary>
    /// Toggle between auto and manual attack modes
    /// </summary>
    public void ToggleAutoAttack()
    {
        isAutoAttackEnabled = !isAutoAttackEnabled;
        ApplyAutoAttackState();
        UpdateUI();
        Debug.Log($"[AutoAttackSettings] Toggled to: {(isAutoAttackEnabled ? "AUTO" : "MOUSE")}");
    }

    /// <summary>
    /// Set specific auto attack state
    /// </summary>
    public void SetAutoAttack(bool enabled)
    {
        isAutoAttackEnabled = enabled;
        ApplyAutoAttackState();
        UpdateUI();
    }

    /// <summary>
    /// Apply the current state to all relevant skills
    /// </summary>
    void ApplyAutoAttackState()
    {
        // Access skills via PlayerSkillsController
        if (PlayerSkillsController.instance != null)
        {
            // Update Fireball (PlayerAutoAttack)
            PlayerAutoAttack fireball = PlayerSkillsController.instance.GetFireball();
            if (fireball != null)
            {
                fireball.useAutoAttack = isAutoAttackEnabled;
            }

            // Update Cone Attack (Flame Breath)
            ConeAttack coneAttack = PlayerSkillsController.instance.GetConeAttack();
            if (coneAttack != null)
            {
                coneAttack.useMouseDirection = !isAutoAttackEnabled;
            }

            // Update Exploding Projectiles
            ExplodingProjectiles exploding = PlayerSkillsController.instance.GetExplodingProjectiles();
            if (exploding != null)
            {
                exploding.useAutoAttack = isAutoAttackEnabled;
            }

            // Update Laser Beam
            LaserBeam laser = PlayerSkillsController.instance.GetLaserBeam();
            if (laser != null)
            {
                laser.useAutoAttack = isAutoAttackEnabled;
            }

            Debug.Log($"[AutoAttackSettings] All skills set to: {(isAutoAttackEnabled ? "AUTO" : "MOUSE")}");
        }
    }

    /// <summary>
    /// Update button visual
    /// </summary>
    void UpdateUI()
    {
        if (buttonIcon != null)
        {
            if (isAutoAttackEnabled && autoIcon != null)
                buttonIcon.sprite = autoIcon;
            else if (!isAutoAttackEnabled && manualIcon != null)
                buttonIcon.sprite = manualIcon;
        }
    }

    /// <summary>
    /// Get current auto attack state
    /// </summary>
    public bool IsAutoAttackEnabled()
    {
        return isAutoAttackEnabled;
    }
}
