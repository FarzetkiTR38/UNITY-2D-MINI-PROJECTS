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
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(DamageInfo.Normal(damage, damageable.GetDamageTextPosition()));

                lastHitTime = Time.time;
                lastHitEnemy = other;
            }
        }
    }
}
