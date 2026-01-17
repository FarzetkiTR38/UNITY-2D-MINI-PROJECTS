using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Piercing Projectile - Flies in a direction and hits multiple enemies
/// </summary>
public class PiercingProjectile : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 10;
    public int maxPierceCount = 3;
    public float maxLifetime = 3f;

    private Vector3 direction;
    private float lifetime = 0f;
    private int currentPierceCount = 0;
    private HashSet<int> hitEnemies = new HashSet<int>();

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

        // Prevent hitting same enemy twice
        int enemyId = other.gameObject.GetInstanceID();
        if (hitEnemies.Contains(enemyId)) return;
        hitEnemies.Add(enemyId);

        EnemyHealthController hp = other.GetComponent<EnemyHealthController>();
        if (hp != null)
            hp.TakeDamage(damage);

        if (PassiveStats.instance != null)
            PassiveStats.instance.ApplyLifesteal(damage);

        currentPierceCount++;
        if (currentPierceCount >= maxPierceCount)
        {
            Destroy(gameObject);
        }
    }
}
