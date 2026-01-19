using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shuriken Damage - Attached to shuriken projectile, deals periodic damage
/// </summary>
public class ShurikenDamage : MonoBehaviour
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

        if (!hitTimers.ContainsKey(enemyId))
        {
            hitTimers[enemyId] = 0f;
        }

        hitTimers[enemyId] -= Time.deltaTime;

        if (hitTimers[enemyId] <= 0f)
        {
            EnemyHealthController hp = other.GetComponent<EnemyHealthController>();
            if (hp != null)
            {
                hp.TakeDamage(damage);

                if (PassiveStats.instance != null)
                    PassiveStats.instance.ApplyLifesteal(damage);
            }

            hitTimers[enemyId] = hitInterval;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        int enemyId = other.gameObject.GetInstanceID();
        hitTimers.Remove(enemyId);
    }
}
