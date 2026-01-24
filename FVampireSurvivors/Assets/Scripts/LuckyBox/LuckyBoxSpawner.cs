using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LuckyBox spawn yöneticisi. Harita üzerinde rastgele konumlarda LuckyBox spawn eder.
/// </summary>
public class LuckyBoxSpawner : MonoBehaviour
{
    [Header("LuckyBox Prefab")]
    [Tooltip("Spawn edilecek LuckyBox prefabı")]
    public GameObject luckyBoxPrefab;

    [Header("Spawn Area")]
    [Tooltip("Spawn alanının minimum köşesi (sol alt)")]
    public Transform minSpawn;
    
    [Tooltip("Spawn alanının maksimum köşesi (sağ üst)")]
    public Transform maxSpawn;

    [Header("Spawn Settings")]
    [Tooltip("Spawn aralığı (saniye)")]
    public float spawnInterval = 15f;
    
    [Tooltip("Aynı anda maksimum aktif kutu sayısı")]
    public int maxActiveBoxes = 5;
    
    [Tooltip("Spawn başlamadan önce gecikme (saniye)")]
    public float initialDelay = 10f;

    [Header("Player Distance Check (Optional)")]
    [Tooltip("Player'a minimum mesafe (0 = kontrol yok)")]
    public float minDistanceFromPlayer = 3f;
    
    private Transform player;
    private float spawnTimer;
    private List<GameObject> activeBoxes = new List<GameObject>();
    private bool isInitialized = false;

    private void Start()
    {
        // Player'ı bul
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // İlk spawn için gecikme
        spawnTimer = initialDelay;
    }

    private void Update()
    {
        if (luckyBoxPrefab == null || minSpawn == null || maxSpawn == null)
            return;

        // Aktif kutuları temizle (destroy edilmişleri kaldır)
        CleanupDestroyedBoxes();

        // Spawn timer
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            spawnTimer = spawnInterval;
            TrySpawnBox();
        }
    }

    private void CleanupDestroyedBoxes()
    {
        activeBoxes.RemoveAll(box => box == null);
    }

    private void TrySpawnBox()
    {
        // Maksimum kutu sayısı kontrolü
        if (activeBoxes.Count >= maxActiveBoxes)
            return;

        // Spawn pozisyonu bul
        Vector3 spawnPos = GetRandomSpawnPosition();

        // Player mesafe kontrolü (opsiyonel)
        if (minDistanceFromPlayer > 0f && player != null)
        {
            int maxAttempts = 10;
            int attempts = 0;

            while (Vector2.Distance(spawnPos, player.position) < minDistanceFromPlayer && attempts < maxAttempts)
            {
                spawnPos = GetRandomSpawnPosition();
                attempts++;
            }
        }

        // Spawn et
        GameObject newBox = Instantiate(luckyBoxPrefab, spawnPos, Quaternion.identity);
        activeBoxes.Add(newBox);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(minSpawn.position.x, maxSpawn.position.x);
        float y = Random.Range(minSpawn.position.y, maxSpawn.position.y);
        
        return new Vector3(x, y, 0f);
    }

    /// <summary>
    /// Tüm aktif kutuları temizler.
    /// </summary>
    public void ClearAllBoxes()
    {
        foreach (var box in activeBoxes)
        {
            if (box != null)
            {
                Destroy(box);
            }
        }
        activeBoxes.Clear();
    }

    /// <summary>
    /// Spawn'u durdurur/başlatır.
    /// </summary>
    public void SetSpawningEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    /// <summary>
    /// Aktif kutu sayısını döndürür.
    /// </summary>
    public int GetActiveBoxCount()
    {
        CleanupDestroyedBoxes();
        return activeBoxes.Count;
    }
}
