using UnityEngine;

/// <summary>
/// Exploding Projectile - Travels towards target and explodes on impact
/// If target dies, continues straight and explodes after max distance
/// </summary>
public class ExplodingProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 8f;
    public int damage = 10;
    public float explosionRadius = 1f;
    public float maxDistance = 15f;
    public GameObject explosionEffectPrefab;

    [Header("Rotation")]
    [Tooltip("Sprite's default facing angle (45 for top-right facing)")]
    public float spriteDefaultAngle = 45f;

    [Header("Explosion Visual")]
    public Color explosionColor = new Color(1f, 0.3f, 0f, 0.4f); // Orange-red
    public float explosionVisualDuration = 0.3f;

    private Vector3 moveDirection;
    private Vector3 startPosition;
    private bool hasDirection = false;

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

    void Start()
    {
        startPosition = transform.position;
    }

    void RotateToDirection()
    {
        if (moveDirection == Vector3.zero) return;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        angle -= spriteDefaultAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        if (!hasDirection) return;

        // Move in direction
        transform.position += moveDirection * speed * Time.deltaTime;

        // Check max distance
        float traveled = Vector3.Distance(startPosition, transform.position);
        if (traveled >= maxDistance)
        {
            Explode();
        }
    }

    void Explode()
    {
        // Create explosion radius visual
        CreateExplosionVisual();

        // Spawn effect prefab if exists
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // AoE damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            // IDamageable kullan - hem EnemyHealthController hem LuckyBox vurulabilir
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(DamageInfo.Normal(damage, damageable.GetDamageTextPosition()));

                if (PassiveStats.instance != null)
                    PassiveStats.instance.ApplyLifesteal(damage);
            }
        }

        Destroy(gameObject);
    }

    void CreateExplosionVisual()
    {
        GameObject visual = new GameObject("ExplosionRadius");
        visual.transform.position = transform.position;
        visual.transform.localScale = Vector3.one * explosionRadius * 2f;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = explosionColor;
        sr.sortingOrder = 10;

        Destroy(visual, explosionVisualDuration);
    }

    Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;

        Color transparent = new Color(0, 0, 0, 0);
        float center = size / 2f;
        float radius = size / 2f - 1f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                    texture.SetPixel(x, y, Color.white);
                else
                    texture.SetPixel(x, y, transparent);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Explode();
        }
    }
}
