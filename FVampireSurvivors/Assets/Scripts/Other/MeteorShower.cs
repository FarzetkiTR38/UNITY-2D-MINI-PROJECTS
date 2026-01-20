using UnityEngine;

/// <summary>
/// Meteor Shower - Random meteors fall from above dealing AoE damage
/// Level increases: meteor count, damage
/// </summary>
public class MeteorShower : MonoBehaviour
{
    [Header("Meteor Settings")]
    public GameObject meteorPrefab;

    [Header("Stats")]
    public float baseMeteorInterval = 2.5f;
    public int baseDamage = 25;
    public float baseImpactRadius = 1.5f;

    [Header("Spawn Settings")]
    [Tooltip("Minimum height above camera to spawn meteors")]
    public float minSpawnHeight = 6f; // ~540px in world units (depends on camera size)

    private int currentLevel = 0;
    private float meteorTimer = 0f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (currentLevel <= 0) return;

        float interval = PassiveStats.instance != null 
            ? PassiveStats.instance.GetAttackInterval(baseMeteorInterval) 
            : baseMeteorInterval;

        meteorTimer += Time.deltaTime;
        if (meteorTimer >= interval)
        {
            SpawnMeteors();
            meteorTimer = 0f;
        }
    }

    void SpawnMeteors()
    {
        int meteorCount = GetMeteorCount();

        for (int i = 0; i < meteorCount; i++)
        {
            // Get random position WITHIN camera view
            Vector3 targetPos = GetRandomPositionInCamera();

            // Spawn meteor ABOVE camera (outside view)
            Vector3 spawnPos = GetSpawnPositionAboveCamera(targetPos);

            if (meteorPrefab != null)
            {
                GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
                
                MeteorProjectile proj = meteor.GetComponent<MeteorProjectile>();
                if (proj != null)
                {
                    proj.Initialize(targetPos, GetDamage(), GetImpactRadius());
                }
            }
            else
            {
                // No prefab - instant damage at location
                DealImpactDamage(targetPos);
            }
        }
    }

    /// <summary>
    /// Get random position within camera bounds
    /// </summary>
    Vector3 GetRandomPositionInCamera()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Get camera bounds in world space
        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;

        // Random position within camera view (with small margin)
        float margin = 0.5f;
        float randomX = Random.Range(-camWidth / 2f + margin, camWidth / 2f - margin);
        float randomY = Random.Range(-camHeight / 2f + margin, camHeight / 2f - margin);

        // Add camera position (camera follows player)
        Vector3 camPos = mainCamera.transform.position;
        return new Vector3(camPos.x + randomX, camPos.y + randomY, 0f);
    }

    /// <summary>
    /// Get spawn position above camera
    /// </summary>
    Vector3 GetSpawnPositionAboveCamera(Vector3 targetPos)
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Camera top edge in world space
        float cameraTop = mainCamera.transform.position.y + mainCamera.orthographicSize;

        // Spawn at least minSpawnHeight above camera top
        float spawnY = cameraTop + minSpawnHeight;

        return new Vector3(targetPos.x, spawnY, 0f);
    }

    void DealImpactDamage(Vector3 position)
    {
        float radius = GetImpactRadius();
        int damage = GetDamage();

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyHealthController hp = hit.GetComponent<EnemyHealthController>();
            if (hp != null)
            {
                hp.TakeDamage(damage);

                if (PassiveStats.instance != null)
                    PassiveStats.instance.ApplyLifesteal(damage);
            }
        }
    }

    int GetMeteorCount()
    {
        int baseCount = currentLevel;
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetTotalProjectileCount(baseCount) 
            : baseCount;
    }

    float GetImpactRadius()
    {
        // Level 1: 1.5, Level 2: 1.75, Level 3: 2.0, Level 4: 2.25, Level 5: 2.5
        float radius = baseImpactRadius + ((currentLevel - 1) * 0.25f);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.GetScaledArea(radius) 
            : radius;
    }

    int GetDamage()
    {
        int damage = baseDamage + (currentLevel * 15);
        return PassiveStats.instance != null 
            ? PassiveStats.instance.CalculateDamage(damage) 
            : damage;
    }

    public void Upgrade(int level)
    {
        currentLevel = level;
    }
}
