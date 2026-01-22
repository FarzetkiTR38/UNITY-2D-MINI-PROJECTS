using UnityEngine;

/// <summary>
/// Ice Shard Projectile - Deals damage and creates AoE slow explosion
/// Goes straight in direction of target (no homing)
/// On impact: explodes and slows ALL enemies in radius
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class IceShardProjectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 8f;
    public int damage = 10;
    public float slowPercent = 0.5f;
    public float slowDuration = 2f;

    [Header("AoE Explosion")]
    [Tooltip("Explosion radius - enemies in this range get slowed")]
    public float explosionRadius = 2f;
    
    [Tooltip("Optional explosion effect prefab")]
    public GameObject explosionEffectPrefab;

    [Header("Lifetime")]
    public float maxLifetime = 5f;
    public float maxDistance = 15f;

    [Header("Rotation Settings")]
    [Tooltip("Enable rotation to face movement direction")]
    public bool rotateToFaceDirection = true;

    [Tooltip("Sprite's default facing angle (45 if sprite faces top-right)")]
    public float spriteDefaultAngle = 45f;

    private Vector3 moveDirection;
    private float lifetime = 0f;
    private Vector3 startPosition;
    private bool hasDirection = false;
    private Rigidbody2D rb;

    void Awake()
    {
        // Ensure Rigidbody2D is set up correctly
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Ensure CircleCollider2D exists and is trigger
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.2f;
    }

    void Start()
    {
        startPosition = transform.position;
    }

    public void SetTarget(Transform t)
    {
        if (t != null)
        {
            moveDirection = (t.position - transform.position).normalized;
            hasDirection = true;
            RotateToDirection();
        }
    }

    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;
        hasDirection = true;
        RotateToDirection();
    }

    void RotateToDirection()
    {
        if (!rotateToFaceDirection) return;
        if (moveDirection == Vector3.zero) return;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        angle -= spriteDefaultAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        if (!hasDirection) return;

        lifetime += Time.deltaTime;

        // Auto-destroy after max lifetime
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        // Auto-destroy after max distance
        float traveledDistance = Vector3.Distance(startPosition, transform.position);
        if (traveledDistance >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        // Move straight (no homing)
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        // Deal damage to hit enemy
        EnemyHealthController hp = other.GetComponent<EnemyHealthController>();
        if (hp != null)
            hp.TakeDamage(damage);

        // Apply lifesteal
        if (PassiveStats.instance != null)
            PassiveStats.instance.ApplyLifesteal(damage);

        // Create AoE slow explosion at impact point
        CreateFrostExplosion();

        Destroy(gameObject);
    }

    /// <summary>
    /// Creates frost explosion that slows all enemies in radius
    /// </summary>
    void CreateFrostExplosion()
    {
        // Get actual explosion radius (FrozenWorld doubles it)
        float actualRadius = explosionRadius;
        if (EvolvedSkillEffects.instance != null)
        {
            actualRadius *= EvolvedSkillEffects.instance.GetFrozenWorldRadiusMultiplier();
        }

        // Create visual frost circle effect (runtime generated)
        CreateFrostCircleVisual(actualRadius);

        // Get actual slow percent (FrozenWorld increases to 80%)
        float actualSlowPercent = slowPercent;
        if (EvolvedSkillEffects.instance != null && EvolvedSkillEffects.instance.frozenWorldActive)
        {
            actualSlowPercent = EvolvedSkillEffects.instance.GetFrozenWorldSlowPercent();
        }

        // Find all enemies in explosion radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, actualRadius);
        
        int slowedCount = 0;
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyController enemy = hit.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.ApplySlow(actualSlowPercent, slowDuration);
                    slowedCount++;
                }
            }
        }

        if (slowedCount > 0)
        {
            Debug.Log($"<color=blue>❄️ Frost Explosion! Slowed {slowedCount} enemies (radius: {actualRadius}, slow: {actualSlowPercent * 100}%)</color>");
        }
    }

    /// <summary>
    /// Creates a semi-transparent cyan circle visual effect at runtime
    /// </summary>
    void CreateFrostCircleVisual(float radius)
    {
        GameObject frostCircle = new GameObject("FrostExplosionVisual");
        frostCircle.transform.position = transform.position;
        frostCircle.transform.localScale = Vector3.one * radius * 2f;

        SpriteRenderer sr = frostCircle.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.5f, 0.9f, 1f, 0.4f); // Light cyan, semi-transparent
        sr.sortingOrder = 5;

        // Add fade-out component
        FrostCircleFade fade = frostCircle.AddComponent<FrostCircleFade>();
        fade.duration = 0.6f;
    }

    /// <summary>
    /// Creates a smooth filled circle sprite at runtime with anti-aliased edges
    /// </summary>
    Sprite CreateCircleSprite()
    {
        int size = 256; // Higher resolution for smooth edges
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float center = size / 2f;
        float radius = size / 2f - 2f;
        float edgeSoftness = 3f; // Anti-aliasing edge width

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                
                if (dist <= radius - edgeSoftness)
                {
                    // Fully inside - white
                    texture.SetPixel(x, y, Color.white);
                }
                else if (dist <= radius + edgeSoftness)
                {
                    // Edge - smooth gradient (anti-aliasing)
                    float t = (dist - (radius - edgeSoftness)) / (edgeSoftness * 2f);
                    float alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    // Outside - transparent
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // Draw explosion radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

/// <summary>
/// Simple component to fade out and destroy the frost circle visual
/// </summary>
public class FrostCircleFade : MonoBehaviour
{
    public float duration = 0.6f;
    private float timer = 0f;
    private SpriteRenderer sr;
    private Color startColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            startColor = sr.color;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Fade out alpha
        if (sr != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            sr.color = c;
        }
    }
}
