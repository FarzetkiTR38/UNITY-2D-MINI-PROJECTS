using UnityEngine;

/// <summary>
/// Cone Attack / Flame Breath - Damages enemies in a cone in front of player
/// Level increases: damage, cone size
/// </summary>
public class ConeAttack : MonoBehaviour
{
    [Header("Cone Settings")]
    public GameObject flameEffectPrefab;

    [Header("Stats")]
    public float baseDamageInterval = 0.2f;
    public int baseDamage = 4;
    public float baseConeAngle = 45f; // Fixed angle matching sprite
    public float baseConeRange = 2f;

    [Header("Anchor Settings")]
    [Tooltip("The flame effect starts from this transform position")]
    public Transform flameAnchor;

    [Header("Direction")]
    public bool useMouseDirection = false;
    private Vector3 aimDirection = Vector3.right;

    private int currentLevel = 0;
    private float damageTimer = 0f;
    private GameObject activeEffect;
    private SpriteRenderer effectSpriteRenderer;

    void Start()
    {
        // Auto-find FlameAnchor if not assigned
        if (flameAnchor == null)
        {
            Transform anchor = transform.Find("FlameAnchor");
            if (anchor != null)
            {
                flameAnchor = anchor;
            }
            else
            {
                // Create one if it doesn't exist
                GameObject anchorObj = new GameObject("FlameAnchor");
                anchorObj.transform.SetParent(transform);
                anchorObj.transform.localPosition = new Vector3(0.5f, 0f, 0f);
                flameAnchor = anchorObj.transform;
            }
        }
    }

    void Update()
    {
        if (currentLevel <= 0)
        {
            if (activeEffect != null) Destroy(activeEffect);
            return;
        }

        // Update aim direction
        if (useMouseDirection)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            
            // Only use direction from player to mouse, ignore distance
            Vector3 dirToMouse = mousePos - transform.position;
            if (dirToMouse.sqrMagnitude > 0.01f) // Avoid zero vector when mouse is on player
            {
                aimDirection = dirToMouse.normalized;
            }
        }
        else
        {
            // Use player's facing direction (based on movement)
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (h != 0 || v != 0)
            {
                aimDirection = new Vector3(h, v, 0).normalized;
            }
        }

        // Update effect position and rotation
        if (activeEffect != null)
        {
            float scale = GetConeRange();
            
            // Get anchor position
            Vector3 anchorPos = flameAnchor != null ? flameAnchor.position : transform.position;
            
            // First apply scale
            activeEffect.transform.localScale = new Vector3(scale, scale, 1f);
            
            // Calculate sprite width using UNROTATED sprite bounds
            float spriteWidth = scale; // default fallback
            if (effectSpriteRenderer != null && effectSpriteRenderer.sprite != null)
            {
                // Use sprite's local bounds (not affected by rotation)
                // sprite.bounds is in local space, multiply by scale
                spriteWidth = effectSpriteRenderer.sprite.bounds.size.x * scale;
            }
            
            // Position so left edge of sprite is at anchor
            // Sprite center = anchor + (spriteWidth / 2) in aim direction
            activeEffect.transform.position = anchorPos + aimDirection * (spriteWidth / 2f);
            
            // Rotate to face aim direction AFTER positioning
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            activeEffect.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Deal damage
        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseDamageInterval) 
            : baseDamageInterval;

        damageTimer += Time.deltaTime;
        if (damageTimer >= interval)
        {
            DealConeDamage();
            damageTimer = 0f;
        }
    }

    void DealConeDamage()
    {
        float coneAngle = GetConeAngle();
        float coneRange = GetConeRange();
        int damage = GetDamage();

        // Cone origin is at anchor position (where flame starts)
        Vector3 anchorPos = flameAnchor != null ? flameAnchor.position : transform.position;

        // Damage range matches sprite width
        float damageRange = coneRange;
        if (effectSpriteRenderer != null)
        {
            damageRange = effectSpriteRenderer.bounds.size.x;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Vector3 dirToEnemy = (enemy.transform.position - anchorPos).normalized;
            float distance = Vector3.Distance(anchorPos, enemy.transform.position);
            float angle = Vector3.Angle(aimDirection, dirToEnemy);

            // Check if enemy is within cone
            if (distance <= damageRange && angle <= coneAngle / 2f)
            {
                EnemyHealthController hp = enemy.GetComponent<EnemyHealthController>();
                if (hp != null)
                {
                    hp.TakeDamage(damage);

                    if (PassiveStats.instance != null)
                        PassiveStats.instance.ApplyLifesteal(damage);
                }
            }
        }
    }

    float GetConeAngle()
    {
        // Fixed angle - matches sprite shape
        return baseConeAngle;
    }

    float GetConeRange()
    {
        // Level 1: 2, Level 2: 2.5, Level 3: 3, Level 4: 3.5, Level 5: 4
        float range = baseConeRange + ((currentLevel - 1) * 0.5f);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetScaledArea(range) 
            : range;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 2);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;

        if (activeEffect == null && flameEffectPrefab != null)
        {
            activeEffect = Instantiate(flameEffectPrefab);
            
            // Cache sprite renderer for bounds calculation
            effectSpriteRenderer = activeEffect.GetComponent<SpriteRenderer>();
            if (effectSpriteRenderer == null)
            {
                effectSpriteRenderer = activeEffect.GetComponentInChildren<SpriteRenderer>();
            }
        }
    }
}
