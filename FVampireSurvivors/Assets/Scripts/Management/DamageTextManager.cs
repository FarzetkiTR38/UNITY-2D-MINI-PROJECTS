using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central manager for spawning and pooling floating damage text.
/// Uses object pooling for performance - no Instantiate/Destroy during gameplay.
/// </summary>
public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    [Header("Pool Settings")]
    [Tooltip("Prefab for damage popup - must have DamagePopup component")]
    [SerializeField] private GameObject damagePopupPrefab;
    
    [Tooltip("Initial pool size - pre-instantiated at start")]
    [SerializeField] private int initialPoolSize = 50;
    
    [Tooltip("Maximum pool size - prevents memory issues")]
    [SerializeField] private int maxPoolSize = 200;

    [Header("Spawn Settings")]
    [Tooltip("Default Y offset above the damage position")]
    [SerializeField] private float defaultYOffset = 0.5f;

    // Object pool
    private Queue<DamagePopup> pool = new Queue<DamagePopup>();
    private List<DamagePopup> activePopups = new List<DamagePopup>();
    private Transform poolContainer;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Create container for pooled objects
        poolContainer = new GameObject("DamagePopupPool").transform;
        poolContainer.SetParent(transform);

        // Pre-warm the pool
        InitializePool();
    }

    /// <summary>
    /// Pre-instantiate pool objects at start.
    /// </summary>
    private void InitializePool()
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogError("[DamageTextManager] Damage Popup Prefab is not assigned!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledObject();
        }

        Debug.Log($"[DamageTextManager] Pool initialized with {initialPoolSize} objects");
    }

    /// <summary>
    /// Create a new pooled object and add to pool.
    /// </summary>
    private DamagePopup CreatePooledObject()
    {
        GameObject obj = Instantiate(damagePopupPrefab, poolContainer);
        obj.SetActive(false);
        
        DamagePopup popup = obj.GetComponent<DamagePopup>();
        if (popup == null)
        {
            Debug.LogError("[DamageTextManager] Prefab is missing DamagePopup component!");
            Destroy(obj);
            return null;
        }

        pool.Enqueue(popup);
        return popup;
    }

    /// <summary>
    /// Get a popup from the pool, or create new one if needed.
    /// </summary>
    private DamagePopup GetFromPool()
    {
        // Try to get from pool
        while (pool.Count > 0)
        {
            DamagePopup popup = pool.Dequeue();
            if (popup != null && popup.gameObject != null)
            {
                activePopups.Add(popup);
                return popup;
            }
        }

        // Pool is empty - create new if under max
        int totalCount = activePopups.Count + pool.Count;
        if (totalCount < maxPoolSize)
        {
            DamagePopup newPopup = CreatePooledObject();
            if (newPopup != null)
            {
                pool.Dequeue(); // Remove from pool since we're using it
                activePopups.Add(newPopup);
                return newPopup;
            }
        }

        // At max capacity - reuse oldest active popup
        if (activePopups.Count > 0)
        {
            DamagePopup oldest = activePopups[0];
            activePopups.RemoveAt(0);
            oldest.ForceReturn();
            activePopups.Add(oldest);
            return oldest;
        }

        Debug.LogWarning("[DamageTextManager] Unable to get popup from pool!");
        return null;
    }

    /// <summary>
    /// Return a popup to the pool.
    /// Called by DamagePopup when animation is complete.
    /// </summary>
    public void ReturnToPool(DamagePopup popup)
    {
        if (popup == null) return;

        activePopups.Remove(popup);
        pool.Enqueue(popup);
    }

    /// <summary>
    /// Show damage text at the specified position.
    /// Main public API - call this from any damage source.
    /// </summary>
    public void ShowDamage(DamageInfo damageInfo)
    {
        // Apply default Y offset if position wasn't set
        if (damageInfo.Position.y == 0)
        {
            damageInfo.Position += Vector3.up * defaultYOffset;
        }

        DamagePopup popup = GetFromPool();
        if (popup != null)
        {
            popup.Initialize(damageInfo);
        }
    }

    /// <summary>
    /// Convenience method: Show normal damage at position.
    /// </summary>
    public void ShowDamage(int amount, Vector3 position)
    {
        ShowDamage(DamageInfo.Normal(amount, position + Vector3.up * defaultYOffset));
    }

    /// <summary>
    /// Convenience method: Show critical damage at position.
    /// </summary>
    public void ShowCritical(int amount, Vector3 position)
    {
        ShowDamage(DamageInfo.Critical(amount, position + Vector3.up * defaultYOffset));
    }

    /// <summary>
    /// Convenience method: Show DOT damage at position.
    /// </summary>
    public void ShowDOT(int amount, Vector3 position)
    {
        ShowDamage(DamageInfo.DOT(amount, position + Vector3.up * defaultYOffset));
    }

    /// <summary>
    /// Convenience method: Show heal at position.
    /// </summary>
    public void ShowHeal(int amount, Vector3 position)
    {
        ShowDamage(DamageInfo.Heal(amount, position + Vector3.up * defaultYOffset));
    }

    /// <summary>
    /// Convenience method: Show gold pickup at position.
    /// </summary>
    public void ShowGold(int amount, Vector3 position)
    {
        ShowDamage(DamageInfo.Gold(amount, position + Vector3.up * defaultYOffset));
    }

    /// <summary>
    /// Get current pool statistics for debugging.
    /// </summary>
    public string GetPoolStats()
    {
        return $"Pool: {pool.Count} available, {activePopups.Count} active, {pool.Count + activePopups.Count} total";
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
