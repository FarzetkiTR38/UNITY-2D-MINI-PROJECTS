using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the spin wheel logic and skill rewards.
/// Attach to the SpinWheelPanel UI object.
/// </summary>
public class SpinWheelManager : MonoBehaviour
{
    public static SpinWheelManager instance;
    
    [Header("=== WHEEL SETTINGS ===")]
    [Tooltip("Number of segments on the wheel")]
    [Range(4, 16)]
    public int segmentCount = 8;
    
    [Tooltip("How many skills player wins per spin")]
    [Range(1, 8)]
    public int rewardCount = 3;
    
    [Header("=== SPIN SETTINGS ===")]
    [Tooltip("How long the wheel spins")]
    public float spinDuration = 3f;
    
    [Tooltip("Maximum rotation speed (degrees per second)")]
    public float maxSpinSpeed = 720f;
    
    [Tooltip("Minimum extra rotations before stopping")]
    public int minExtraRotations = 3;
    
    [Tooltip("Radius of the wheel - how far segments are from center")]
    public float wheelRadius = 120f;
    
    [Header("=== SKILL POOL ===")]
    [Tooltip("Leave empty for random selection from all skills. Or specify fixed skills.")]
    public List<SkillType> fixedSkillPool = new List<SkillType>();
    
    [Header("=== UI REFERENCES ===")]
    public GameObject wheelPanel;           // The main panel (this object)
    public RectTransform wheelTransform;    // The rotating wheel container
    public Button spinButton;
    public GameObject segmentPrefab;        // Prefab for each segment
    public Transform segmentHolder;         // Parent for segments
    public TextMeshProUGUI rewardText;                 // Optional: shows rewards
    
    // Runtime data
    private List<SkillType> currentSegmentSkills = new List<SkillType>();
    private bool isSpinning = false;
    private float currentRotation = 0f;
    
    private void Awake()
    {
        // IMPORTANT: Set instance BEFORE disabling the panel!
        instance = this;
    }
    
    private void OnEnable()
    {
        // Also set on enable in case of scene reload
        instance = this;
    }
    
    private void Start()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(Spin);
        
        // Hide panel at start (after instance is set)
        if (wheelPanel != null)
            wheelPanel.SetActive(false);
    }
    
    /// <summary>
    /// Show the spin wheel UI
    /// </summary>
    public void Show()
    {
        if (isSpinning) return;
        
        Debug.Log("[SpinWheel] Showing wheel...");
        
        // Pause game
        Time.timeScale = 0f;
        
        // Generate random skills for segments
        GenerateSegmentSkills();
        
        // Update UI visuals
        UpdateWheelVisuals();
        
        // Show panel
        if (wheelPanel != null)
            wheelPanel.SetActive(true);
        
        // Reset wheel rotation
        currentRotation = 0f;
        if (wheelTransform != null)
            wheelTransform.rotation = Quaternion.identity;
        
        // Enable spin button
        if (spinButton != null)
            spinButton.interactable = true;
        
        // Clear reward text
        if (rewardText != null)
            rewardText.text = "";
    }
    
    /// <summary>
    /// Hide the spin wheel UI
    /// </summary>
    public void Hide()
    {
        if (wheelPanel != null)
            wheelPanel.SetActive(false);
        
        // Resume game
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Generate random skills for each segment
    /// </summary>
    void GenerateSegmentSkills()
    {
        currentSegmentSkills.Clear();
        
        List<SkillType> pool = new List<SkillType>();
        
        if (fixedSkillPool != null && fixedSkillPool.Count > 0)
        {
            // Use fixed pool
            pool.AddRange(fixedSkillPool);
        }
        else
        {
            // Use all active + passive skills
            pool.AddRange(GetAllActiveSkills());
            pool.AddRange(GetAllPassiveSkills());
        }
        
        // Shuffle pool
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = pool[i];
            pool[i] = pool[j];
            pool[j] = temp;
        }
        
        // Pick skills for segments
        for (int i = 0; i < segmentCount && i < pool.Count; i++)
        {
            currentSegmentSkills.Add(pool[i]);
        }
        
        // If not enough skills, repeat from start
        while (currentSegmentSkills.Count < segmentCount && pool.Count > 0)
        {
            currentSegmentSkills.Add(pool[currentSegmentSkills.Count % pool.Count]);
        }
        
        Debug.Log($"[SpinWheel] Generated {currentSegmentSkills.Count} segments");
    }
    
    /// <summary>
    /// Update the wheel segment visuals
    /// </summary>
    void UpdateWheelVisuals()
    {
        if (segmentHolder == null || segmentPrefab == null) return;
        
        // Clear existing segments
        foreach (Transform child in segmentHolder)
        {
            Destroy(child.gameObject);
        }
        
        // Create pizza-style pie slices from center
        float anglePerSegment = 360f / segmentCount;
        
        for (int i = 0; i < currentSegmentSkills.Count; i++)
        {
            GameObject seg = Instantiate(segmentPrefab, segmentHolder);
            
            // Calculate rotation angle for this slice (starting from top)
            // 90 degrees offset so first slice is at top
            float rotationAngle = 90f - (i * anglePerSegment) - (anglePerSegment / 2f);
            
            // Position at center (all slices share the same center)
            RectTransform rectTransform = seg.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(wheelRadius * 2f, wheelRadius * 2f);
            }
            
            // Set skill info with angle parameters
            SpinWheelSegment segScript = seg.GetComponent<SpinWheelSegment>();
            if (segScript != null)
            {
                segScript.Setup(currentSegmentSkills[i], anglePerSegment, rotationAngle, i);
            }
        }
    }
    
    /// <summary>
    /// Start the wheel spin
    /// </summary>
    public void Spin()
    {
        if (isSpinning) return;
        
        Debug.Log("[SpinWheel] Spinning!");
        
        if (spinButton != null)
            spinButton.interactable = false;
        
        StartCoroutine(SpinRoutine());
    }
    
    IEnumerator SpinRoutine()
    {
        isSpinning = true;
        
        // Calculate target rotation
        float randomOffset = Random.Range(0f, 360f);
        float totalRotation = (360f * minExtraRotations) + randomOffset;
        
        float elapsed = 0f;
        float startRotation = currentRotation;
        float targetRotation = startRotation + totalRotation;
        
        while (elapsed < spinDuration)
        {
            // Use unscaled time since game is paused
            elapsed += Time.unscaledDeltaTime;
            
            // Ease out curve
            float t = elapsed / spinDuration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease out
            
            currentRotation = Mathf.Lerp(startRotation, targetRotation, easeT);
            
            if (wheelTransform != null)
                wheelTransform.rotation = Quaternion.Euler(0, 0, -currentRotation);
            
            yield return null;
        }
        
        // Ensure final rotation
        currentRotation = targetRotation;
        if (wheelTransform != null)
            wheelTransform.rotation = Quaternion.Euler(0, 0, -currentRotation);
        
        isSpinning = false;
        
        // Calculate winners
        OnSpinComplete();
    }
    
    /// <summary>
    /// Calculate winning segments and award skills
    /// </summary>
    void OnSpinComplete()
    {
        Debug.Log("[SpinWheel] Spin complete!");
        
        // Calculate which segment the pointer is on
        // Pointer is at top (0 degrees), wheel rotated by currentRotation
        float normalizedRotation = currentRotation % 360f;
        float anglePerSegment = 360f / segmentCount;
        
        List<SkillType> wonSkills = new List<SkillType>();
        
        // Get 'rewardCount' consecutive segments starting from pointer position
        int startIndex = Mathf.FloorToInt(normalizedRotation / anglePerSegment);
        
        for (int i = 0; i < rewardCount; i++)
        {
            int segIndex = (startIndex + i) % currentSegmentSkills.Count;
            wonSkills.Add(currentSegmentSkills[segIndex]);
        }
        
        // Award skills
        string rewardLog = "[SpinWheel] Won skills: ";
        foreach (var skill in wonSkills)
        {
            rewardLog += skill.ToString() + ", ";
            
            if (PlayerSkillManager.instance != null)
            {
                PlayerSkillManager.instance.UpgradeSkill(skill);
            }
        }
        Debug.Log(rewardLog);
        
        // Show reward text
        if (rewardText != null)
        {
            string text = "Kazanılan:\n";
            foreach (var skill in wonSkills)
            {
                text += "• " + skill.ToString() + "\n";
            }
            rewardText.text = text;
        }
        
        // Auto-close after delay
        StartCoroutine(AutoCloseRoutine());
    }
    
    IEnumerator AutoCloseRoutine()
    {
        // Wait 2 seconds (unscaled)
        float wait = 2f;
        float elapsed = 0f;
        while (elapsed < wait)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        Hide();
    }
    
    // ==================
    // HELPER METHODS
    // ==================
    
    List<SkillType> GetAllActiveSkills()
    {
        return new List<SkillType>
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
            SkillType.SpinningShuriken,
            SkillType.ConeAttack,
            SkillType.MeteorShower,
            SkillType.ExplodingProjectiles,
            SkillType.LaserBeam,
            SkillType.Turret,
            SkillType.BlackHole
        };
    }
    
    List<SkillType> GetAllPassiveSkills()
    {
        return new List<SkillType>
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
    }
}
