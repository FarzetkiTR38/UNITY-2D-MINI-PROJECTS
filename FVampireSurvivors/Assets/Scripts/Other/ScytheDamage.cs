using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scythe Damage - Attached to scythe projectile, deals periodic damage
/// </summary>
public class ScytheDamage : MonoBehaviour
{
    public int damage = 10;
    public float hitInterval = 0.3f;

    private Dictionary<int, float> hitTimers = new Dictionary<int, float>();

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        int enemyId = other.gameObject.GetInstanceID();

        // Check cooldown
        if (hitTimers.ContainsKey(enemyId))
        {
            hitTimers[enemyId] -= Time.deltaTime;
            if (hitTimers[enemyId] > 0) return;
        }

        // Deal damage
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(DamageInfo.Normal(damage, damageable.GetDamageTextPosition()));

            if (PassiveStats.instance != null)
                PassiveStats.instance.ApplyLifesteal(damage);
        }

        // Reset cooldown
        hitTimers[enemyId] = hitInterval;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        
        int enemyId = other.gameObject.GetInstanceID();
        if (hitTimers.ContainsKey(enemyId))
        {
            hitTimers.Remove(enemyId);
        }
    }
}
