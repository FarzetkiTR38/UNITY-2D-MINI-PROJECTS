using UnityEngine;
using TMPro;

/// <summary>
/// Individual floating damage text behavior.
/// Handles animation, fade-out, billboard effect, and returns to pool when done.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of the popup animation in seconds")]
    [SerializeField] private float duration = 0.9f;
    
    [Tooltip("How high the text floats up")]
    [SerializeField] private float floatHeight = 1.5f;
    
    [Tooltip("Horizontal spread for random offset")]
    [SerializeField] private float randomOffsetX = 0.5f;
    
    [Tooltip("Vertical spread for random offset")]
    [SerializeField] private float randomOffsetY = 0.3f;

    [Header("Visual Settings")]
    [Tooltip("Base font size for normal damage")]
    [SerializeField] private float baseFontSize = 5f;
    
    [Tooltip("Font size multiplier for critical hits")]
    [SerializeField] private float criticalSizeMultiplier = 1.5f;
    
    [Tooltip("Font size multiplier for DOT damage")]
    [SerializeField] private float dotSizeMultiplier = 0.8f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = new Color(1f, 0.8f, 0f); // Yellow-Orange
    [SerializeField] private Color dotColor = new Color(0.8f, 0.2f, 0.8f); // Purple
    [SerializeField] private Color healColor = new Color(0.2f, 1f, 0.2f); // Green
    [SerializeField] private Color goldColor = new Color(1f, 0.85f, 0f); // Gold/Yellow

    // Components
    private TextMeshPro textMesh;
    private Transform mainCamera;
    
    // Animation state
    private Vector3 startPosition;
    private float timer;
    private bool isActive;
    private Color currentColor;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        
        // Configure TextMeshPro for world space
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.sortingOrder = 100; // Always on top
    }

    private void Start()
    {
        // Cache camera reference
        if (Camera.main != null)
            mainCamera = Camera.main.transform;
    }

    private void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;
        float progress = timer / duration;

        if (progress >= 1f)
        {
            // Animation complete - return to pool
            ReturnToPool();
            return;
        }

        // Move upward with easing (fast start, slow end)
        float easedProgress = 1f - Mathf.Pow(1f - progress, 3f); // Ease out cubic
        Vector3 newPosition = startPosition + Vector3.up * (floatHeight * easedProgress);
        transform.position = newPosition;

        // Fade out in the last 40% of duration
        if (progress > 0.6f)
        {
            float fadeProgress = (progress - 0.6f) / 0.4f;
            float alpha = 1f - fadeProgress;
            textMesh.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }

        // Billboard effect - always face camera
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.rotation;
        }
    }

    /// <summary>
    /// Initialize and show the damage popup with given damage info.
    /// Called by DamageTextManager when spawning from pool.
    /// </summary>
    public void Initialize(DamageInfo damageInfo)
    {
        // Apply random offset to prevent overlapping
        Vector3 offset = new Vector3(
            Random.Range(-randomOffsetX, randomOffsetX),
            Random.Range(-randomOffsetY, randomOffsetY),
            0f
        );
        
        startPosition = damageInfo.Position + offset;
        transform.position = startPosition;
        
        // Reset animation state
        timer = 0f;
        isActive = true;

        // Configure based on damage type
        ConfigureForDamageType(damageInfo);

        // Ensure camera reference is set
        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main.transform;

        // Face camera immediately
        if (mainCamera != null)
            transform.rotation = mainCamera.rotation;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Configure text appearance based on damage type.
    /// </summary>
    private void ConfigureForDamageType(DamageInfo info)
    {
        string displayText;
        float fontSize;
        
        switch (info.Type)
        {
            case DamageType.Critical:
                displayText = info.Amount.ToString() + "!";
                fontSize = baseFontSize * criticalSizeMultiplier;
                currentColor = criticalColor;
                break;
                
            case DamageType.DOT:
                displayText = info.Amount.ToString();
                fontSize = baseFontSize * dotSizeMultiplier;
                currentColor = dotColor;
                break;
                
            case DamageType.Heal:
                displayText = "+" + info.Amount.ToString();
                fontSize = baseFontSize;
                currentColor = healColor;
                break;

            case DamageType.Gold:
                displayText = "+" + info.Amount.ToString();
                fontSize = baseFontSize;
                currentColor = goldColor;
                break;
                
            case DamageType.Normal:
            default:
                displayText = info.Amount.ToString();
                fontSize = baseFontSize;
                currentColor = normalColor;
                break;
        }

        textMesh.text = displayText;
        textMesh.fontSize = fontSize;
        textMesh.color = currentColor;
    }

    /// <summary>
    /// Return this popup to the object pool.
    /// </summary>
    private void ReturnToPool()
    {
        isActive = false;
        gameObject.SetActive(false);
        
        // Notify manager that this popup is available
        DamageTextManager.Instance?.ReturnToPool(this);
    }

    /// <summary>
    /// Force return to pool (for cleanup purposes).
    /// </summary>
    public void ForceReturn()
    {
        ReturnToPool();
    }
}
