using UnityEngine;

/// <summary>
/// Ice Shard Projectile - Deals damage and slows enemy
/// Goes straight in direction of target (no homing)
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

        EnemyHealthController hp = other.GetComponent<EnemyHealthController>();
        if (hp != null)
            hp.TakeDamage(damage);

        // Apply slow effect
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
            enemy.ApplySlow(slowPercent, slowDuration);

        // Apply lifesteal
        if (PassiveStats.instance != null)
            PassiveStats.instance.ApplyLifesteal(damage);

        Destroy(gameObject);
    }
}
