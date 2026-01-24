using UnityEngine;

/// <summary>
/// Directional Projectile - Flies in a fixed direction until it hits something or expires
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class DirectionalProjectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 12f;
    public int damage = 10;

    [Header("Lifetime")]
    public float maxLifetime = 3f;
    public float maxDistance = 15f;

    [Header("Rotation Settings")]
    [Tooltip("Enable rotation to face movement direction")]
    public bool rotateToFaceDirection = true;

    [Tooltip("Sprite's default facing angle (0 if right, 45 if top-right, 90 if up)")]
    public float spriteDefaultAngle = 45f;

    private Vector3 direction;
    private float lifetime = 0f;
    private Vector3 startPosition;
    private bool hasDirection = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

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
            direction = (t.position - transform.position).normalized;
            hasDirection = true;
            RotateToDirection();
        }
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        hasDirection = true;
        RotateToDirection();
    }

    void RotateToDirection()
    {
        if (!rotateToFaceDirection) return;
        if (direction == Vector3.zero) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= spriteDefaultAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        if (!hasDirection) return;

        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        float traveledDistance = Vector3.Distance(startPosition, transform.position);
        if (traveledDistance >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(DamageInfo.Normal(damage, damageable.GetDamageTextPosition()));

            if (PassiveStats.instance != null)
                PassiveStats.instance.ApplyLifesteal(damage);
        }

        Destroy(gameObject);
    }
}
