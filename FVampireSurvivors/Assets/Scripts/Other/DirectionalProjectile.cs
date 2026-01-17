using UnityEngine;

/// <summary>
/// Directional Projectile - Flies in a fixed direction until it hits something or expires
/// </summary>
public class DirectionalProjectile : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 10;
    public float maxLifetime = 3f;

    private Vector3 direction;
    private float lifetime = 0f;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        
        // Rotate sprite to face direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyHealthController hp = other.GetComponent<EnemyHealthController>();
        if (hp != null)
            hp.TakeDamage(damage);

        if (PassiveStats.instance != null)
            PassiveStats.instance.ApplyLifesteal(damage);

        Destroy(gameObject);
    }
}
