using UnityEngine;

/// <summary>
/// Meteor Projectile - Falls from above and explodes on impact
/// Shows a red warning circle before landing
/// </summary>
public class MeteorProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float fallSpeed = 15f;
    public GameObject explosionEffectPrefab;

    [Header("Warning Indicator")]
    public float warningDuration = 0.5f;
    public Color indicatorColor = new Color(1f, 0f, 0f, 0.35f);

    private Vector3 targetPosition;
    private int damage;
    private float impactRadius;
    private bool initialized = false;
    private bool isFalling = false;
    private float warningTimer = 0f;
    private GameObject indicatorObject;

    public void Initialize(Vector3 target, int dmg, float radius)
    {
        targetPosition = target;
        damage = dmg;
        impactRadius = radius;
        initialized = true;

        // Create warning indicator
        CreateWarningIndicator();
    }

    void CreateWarningIndicator()
    {
        indicatorObject = new GameObject("ImpactWarning");
        indicatorObject.transform.position = targetPosition;
        indicatorObject.transform.localScale = Vector3.one * impactRadius * 2f;

        SpriteRenderer sr = indicatorObject.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = indicatorColor;
        sr.sortingOrder = -1;
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

    void Update()
    {
        if (!initialized) return;

        // Warning phase - show indicator before meteor falls
        if (!isFalling)
        {
            warningTimer += Time.deltaTime;
            if (warningTimer >= warningDuration)
            {
                isFalling = true;
            }
            return;
        }

        // Move towards target
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * fallSpeed * Time.deltaTime;

        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            Explode();
        }
    }

    void Explode()
    {
        // Destroy warning indicator
        if (indicatorObject != null)
            Destroy(indicatorObject);

        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, targetPosition, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // Deal AoE damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPosition, impactRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

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

    void OnDestroy()
    {
        if (indicatorObject != null)
            Destroy(indicatorObject);
    }
}
