using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 8f;
    public int damage = 10;

    [Header("Lifetime Settings")]
    [Tooltip("Maximum lifetime in seconds before auto-destroy")]
    public float maxLifetime = 5f;

    [Tooltip("Maximum travel distance before auto-destroy")]
    public float maxDistance = 15f;

    private Vector3 moveDirection;
    private float lifetime = 0f;
    private Vector3 startPosition;
    private bool hasDirection = false;
    private Rigidbody2D rb;

    void Awake()
    {
        // Ensure Rigidbody2D is set up correctly for trigger detection
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

    /// <summary>
    /// Set initial target - projectile will calculate direction and go STRAIGHT
    /// </summary>
    public void SetTarget(Transform t)
    {
        if (t != null)
        {
            moveDirection = (t.position - transform.position).normalized;
            hasDirection = true;
        }
    }

    /// <summary>
    /// Set direction directly (for skills that don't use targets)
    /// </summary>
    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;
        hasDirection = true;
    }

    private void Update()
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

        // Move in straight direction (NO homing, NO retargeting)
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    /// <summary>
    /// Trigger collision - hit enemy and destroy
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ONLY hit enemies
        if (!other.CompareTag("Enemy")) return;

        EnemyHealthController hp = other.GetComponent<EnemyHealthController>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            Debug.Log($"[Projectile] Hit {other.name} for {damage} damage!");

            // Apply lifesteal
            if (PassiveStats.instance != null)
                PassiveStats.instance.ApplyLifesteal(damage);
        }

        Destroy(gameObject);
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }
}
