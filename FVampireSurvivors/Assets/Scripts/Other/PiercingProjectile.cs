using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Piercing Projectile - Flies in a direction and hits multiple enemies
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PiercingProjectile : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 15f;
    public int damage = 10;
    public int maxPierceCount = 3;

    [Header("Lifetime")]
    public float maxLifetime = 3f;
    public float maxDistance = 20f;

    [Header("Rotation Settings")]
    [Tooltip("Enable rotation to face movement direction")]
    public bool rotateToFaceDirection = true;

    [Tooltip("Sprite's default facing angle (0 if sprite faces right, 45 if top-right, 90 if up)")]
    public float spriteDefaultAngle = 0f;

    private Vector3 direction;
    private float lifetime = 0f;
    private Vector3 startPosition;
    private int currentPierceCount = 0;
    private HashSet<int> hitEnemies = new HashSet<int>();
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

        // Prevent hitting same enemy twice
        int enemyId = other.gameObject.GetInstanceID();
        if (hitEnemies.Contains(enemyId)) return;
        hitEnemies.Add(enemyId);

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(DamageInfo.Normal(damage, damageable.GetDamageTextPosition()));

            if (PassiveStats.instance != null)
                PassiveStats.instance.ApplyLifesteal(damage);
        }

        currentPierceCount++;
        if (currentPierceCount >= maxPierceCount)
        {
            Destroy(gameObject);
        }
    }
}
