using UnityEngine;

/// <summary>
/// Chest that drops from bosses. Player clicks to open spin wheel.
/// </summary>
public class ChestDrop : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Player must be within this distance to interact")]
    public float interactionRange = 2f;
    
    [Header("Visual Feedback")]
    public GameObject highlightEffect;  // Optional: glow effect when in range
    public GameObject openEffect;       // Optional: particle effect on open
    
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private bool isPlayerInRange = false;
    private bool isOpened = false;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (highlightEffect != null)
            highlightEffect.SetActive(false);
    }
    
    private void Update()
    {
        if (isOpened || player == null) return;
        
        // Check distance to player
        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRange;
        
        // Visual feedback when entering/exiting range
        if (isPlayerInRange != wasInRange)
        {
            OnRangeChanged(isPlayerInRange);
        }
        
        // Check for click/tap input
        if (isPlayerInRange && Input.GetMouseButtonDown(0))
        {
            // Check if click is on this chest
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            
            if (hit != null && hit.gameObject == gameObject)
            {
                OpenChest();
            }
        }
    }
    
    void OnRangeChanged(bool inRange)
    {
        // Highlight effect
        if (highlightEffect != null)
            highlightEffect.SetActive(inRange);
        
        // Optional: scale pulse
        if (spriteRenderer != null)
        {
            transform.localScale = inRange ? Vector3.one * 1.1f : Vector3.one;
        }
    }
    
    void OpenChest()
    {
        if (isOpened) return;
        isOpened = true;
        
        Debug.Log("[Chest] Opening spin wheel!");
        
        // Spawn open effect
        if (openEffect != null)
        {
            Instantiate(openEffect, transform.position, Quaternion.identity);
        }
        
        // Show spin wheel
        if (SpinWheelManager.instance != null)
        {
            SpinWheelManager.instance.Show();
        }
        else
        {
            Debug.LogError("[Chest] SpinWheelManager.instance is null!");
        }
        
        // Destroy chest
        Destroy(gameObject, 0.1f);
    }
    
    // Visual debug in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
