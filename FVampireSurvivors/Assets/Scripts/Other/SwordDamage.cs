using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public float hitCooldown = 0.3f;

    private float lastHitTime = -999f;
    private Collider2D lastHitEnemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < lastHitTime + hitCooldown && other == lastHitEnemy)
            return;

        if (other.CompareTag("Enemy"))
        {
            EnemyHealthController enemy = other.GetComponent<EnemyHealthController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                lastHitTime = Time.time;
                lastHitEnemy = other;
            }
        }
    }
}
