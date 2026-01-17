using UnityEngine;

/// <summary>
/// Cone Attack / Flame Breath - Damages enemies in a cone in front of player
/// Level increases: damage, cone size
/// </summary>
public class ConeAttack : MonoBehaviour
{
    [Header("Cone Settings")]
    public GameObject flameEffectPrefab;

    [Header("Stats")]
    public float baseDamageInterval = 0.2f;
    public int baseDamage = 4;
    public float baseConeAngle = 45f; // Total angle
    public float baseConeRange = 4f;

    [Header("Direction")]
    public bool useMouseDirection = false;
    private Vector3 aimDirection = Vector3.right;

    private int currentLevel = 0;
    private float damageTimer = 0f;
    private GameObject activeEffect;

    void Update()
    {
        if (currentLevel <= 0)
        {
            if (activeEffect != null) Destroy(activeEffect);
            return;
        }

        // Update aim direction
        if (useMouseDirection)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            aimDirection = (mousePos - transform.position).normalized;
            aimDirection.z = 0;
        }
        else
        {
            // Use player's facing direction (based on movement)
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (h != 0 || v != 0)
            {
                aimDirection = new Vector3(h, v, 0).normalized;
            }
        }

        // Update effect rotation
        if (activeEffect != null)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            activeEffect.transform.rotation = Quaternion.Euler(0, 0, angle);
            float scale = GetConeRange() / baseConeRange;
            activeEffect.transform.localScale = new Vector3(scale, scale, 1f);
        }

        // Deal damage
        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseDamageInterval) 
            : baseDamageInterval;

        damageTimer += Time.deltaTime;
        if (damageTimer >= interval)
        {
            DealConeDamage();
            damageTimer = 0f;
        }
    }

    void DealConeDamage()
    {
        float coneAngle = GetConeAngle();
        float coneRange = GetConeRange();
        int damage = GetDamage();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            float angle = Vector3.Angle(aimDirection, dirToEnemy);

            // Check if enemy is within cone
            if (distance <= coneRange && angle <= coneAngle / 2f)
            {
                EnemyHealthController hp = enemy.GetComponent<EnemyHealthController>();
                if (hp != null)
                {
                    hp.TakeDamage(damage);

                    if (PassiveStats.instance != null)
                        PassiveStats.instance.ApplyLifesteal(damage);
                }
            }
        }
    }

    float GetConeAngle()
    {
        return baseConeAngle + (currentLevel * 10f);
    }

    float GetConeRange()
    {
        float range = baseConeRange + (currentLevel * 0.5f);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetScaledArea(range) 
            : range;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 2);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;

        if (activeEffect == null && flameEffectPrefab != null)
        {
            activeEffect = Instantiate(flameEffectPrefab, transform);
            activeEffect.transform.localPosition = Vector3.zero;
        }
    }
}
