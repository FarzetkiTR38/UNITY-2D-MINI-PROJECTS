using UnityEngine;

/// <summary>
/// Laser Beam - Continuous beam in a fixed direction
/// Level increases: damage, width
/// </summary>
public class LaserBeam : MonoBehaviour
{
    [Header("Laser Settings")]
    public LineRenderer laserLine;
    public float laserLength = 10f;

    [Header("Stats")]
    public float baseDamageInterval = 0.1f;
    public int baseDamage = 3;
    public float baseWidth = 0.2f;

    [Header("Direction")]
    [Tooltip("true = auto-target nearest enemy, false = mouse direction")]
    public bool useAutoAttack = true;
    private Vector3 aimDirection = Vector3.right;

    private int currentLevel = 0;
    private float damageTimer = 0f;

    void Start()
    {
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
    }

    void Update()
    {
        if (currentLevel <= 0)
        {
            if (laserLine != null) laserLine.enabled = false;
            return;
        }

        // Update aim direction
        if (useAutoAttack)
        {
            // Auto mode: target nearest enemy
            Transform target = GetNearestEnemy();
            if (target != null)
            {
                aimDirection = (target.position - transform.position).normalized;
            }
        }
        else
        {
            // Manual mode: mouse direction
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            Vector3 dir = mousePos - transform.position;
            if (dir.sqrMagnitude > 0.01f)
            {
                aimDirection = dir.normalized;
            }
        }

        // Update laser visual
        UpdateLaserVisual();

        // Deal damage
        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseDamageInterval) 
            : baseDamageInterval;

        damageTimer += Time.deltaTime;
        if (damageTimer >= interval)
        {
            DealLaserDamage();
            damageTimer = 0f;
        }
    }

    void UpdateLaserVisual()
    {
        if (laserLine == null) return;

        laserLine.enabled = true;
        laserLine.startWidth = GetWidth();
        laserLine.endWidth = GetWidth();

        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + aimDirection * laserLength;

        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, endPos);
    }

    void DealLaserDamage()
    {
        float width = GetWidth();
        int damage = GetDamage();

        // Raycast along laser
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position, 
            width / 2f, 
            aimDirection, 
            laserLength
        );

        foreach (var hit in hits)
        {
            if (!hit.collider.CompareTag("Enemy")) continue;

            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(DamageInfo.Normal(damage, damageable.GetDamageTextPosition()));

                if (PassiveStats.instance != null)
                    PassiveStats.instance.ApplyLifesteal(damage);
            }
        }
    }

    Transform GetNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    float GetWidth()
    {
        // Level 1: 0.2, Level 2: 0.3, Level 3: 0.4, Level 4: 0.5, Level 5: 0.6
        float width = baseWidth + ((currentLevel - 1) * 0.1f);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetScaledArea(width) 
            : width;
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
    }
}
