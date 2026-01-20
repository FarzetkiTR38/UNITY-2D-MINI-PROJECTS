using UnityEngine;

/// <summary>
/// Black Hole Behavior - Pulls enemies in and damages them
/// </summary>
public class BlackHoleBehavior : MonoBehaviour
{
    private float duration;
    private int damage;
    private float damageInterval;
    private float radius;
    private float pullForce;

    private float lifetime = 0f;
    private float damageTimer = 0f;

    public void Initialize(float duration, int damage, float damageInterval, float radius, float pullForce)
    {
        this.duration = duration;
        this.damage = damage;
        this.damageInterval = damageInterval;
        this.radius = radius;
        this.pullForce = pullForce;

        // Scale visual effect (1.5x bigger than before)
        transform.localScale = Vector3.one * (radius / 3f) * 1.5f;

        // Create radius indicator
        CreateRadiusIndicator();
    }

    void CreateRadiusIndicator()
    {
        GameObject indicator = new GameObject("RadiusIndicator");
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.6f, 0.1f, 0.9f, 0.3f); // Purple, semi-transparent
        sr.sortingOrder = -1; // Behind other objects

        // Scale to match ACTUAL damage radius (not affected by 1.5x prefab scale)
        float spriteSize = 1f;
        indicator.transform.localScale = Vector3.one * (radius * 2f / spriteSize) / transform.localScale.x;
    }

    Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        
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
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= duration)
        {
            Destroy(gameObject);
            return;
        }

        // Pull enemies
        PullEnemies();

        // Damage at intervals
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            DealDamage();
            damageTimer = 0f;
        }
    }

    void PullEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 pullDirection = (transform.position - hit.transform.position).normalized;
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                float forceMagnitude = pullForce * (1f - distance / radius); // Stronger when closer
                rb.AddForce(pullDirection * forceMagnitude);
            }
        }
    }

    void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyHealthController hp = hit.GetComponent<EnemyHealthController>();
            if (hp != null)
            {
                hp.TakeDamage(damage);

                if (PassiveStats.instance != null)
                    PassiveStats.instance.ApplyLifesteal(damage);
            }
        }
    }
}
