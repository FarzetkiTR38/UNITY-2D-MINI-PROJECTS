using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boomerang Projectile - Travels outward then returns to thrower
/// Can hit enemies multiple times (once going, once returning)
/// </summary>
public class BoomerangProjectile : MonoBehaviour
{
    public float rotationSpeed = 720f;
    
    private Transform owner;
    private Vector3 direction;
    private float maxDistance;
    private float speed;
    private int damage;

    private Vector3 startPos;
    private bool returning = false;
    private HashSet<int> hitEnemiesOut = new HashSet<int>();
    private HashSet<int> hitEnemiesBack = new HashSet<int>();

    public void Initialize(Transform owner, Vector3 direction, float maxDistance, float speed, int damage)
    {
        this.owner = owner;
        this.direction = direction.normalized;
        this.maxDistance = maxDistance;
        this.speed = speed;
        this.damage = damage;
        startPos = transform.position;
    }

    void Update()
    {
        // Rotate visually
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        if (!returning)
        {
            // Move outward
            transform.position += direction * speed * Time.deltaTime;

            // Check if reached max distance
            if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            {
                returning = true;
            }
        }
        else
        {
            // Return to owner
            if (owner == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 returnDir = (owner.position - transform.position).normalized;
            transform.position += returnDir * speed * 1.2f * Time.deltaTime;

            // Check if reached owner
            if (Vector3.Distance(transform.position, owner.position) < 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        int enemyId = other.gameObject.GetInstanceID();

        // Check if already hit this enemy in current direction
        if (!returning)
        {
            if (hitEnemiesOut.Contains(enemyId)) return;
            hitEnemiesOut.Add(enemyId);
        }
        else
        {
            if (hitEnemiesBack.Contains(enemyId)) return;
            hitEnemiesBack.Add(enemyId);
        }

        EnemyHealthController hp = other.GetComponent<EnemyHealthController>();
        if (hp != null)
        {
            hp.TakeDamage(damage);

            if (PassiveStats.instance != null)
                PassiveStats.instance.ApplyLifesteal(damage);
        }
    }
}
