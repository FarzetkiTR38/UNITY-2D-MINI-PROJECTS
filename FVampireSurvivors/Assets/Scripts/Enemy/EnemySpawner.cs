using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Area Borders")]
    public Transform minSpawn;
    public Transform maxSpawn;

    [Header("Spawn Table (ScriptableObject)")]
    public SpawnTableSO spawnTable;

    [Header("Player Level Source")]
    public PlayerExperience playerExperience; // Inspector’dan ver; boşsa otomatik bulur.

    [Header("Rush Wave")]
    [Tooltip("Rush aktifken interval / (multiplier) yapılır. Multiplier bracket'tan okunur.")]
    public bool isRushActive = false;

    [Header("Arena Boss Settings (Optional)")]
    [Tooltip("Arena boss tetiklenince normal spawn dursun mu? (1v1 için genelde true)")]
    public bool pauseSpawningDuringArena = true;

    // Arena modunu dışarıdan yönetmek için (map shrink, 1v1 vb.)
    public System.Action<GameObject> OnArenaBossTriggered;

    private float spawnCounter;
    private bool arenaActive = false;

    // Arena trigger tekrarlarını engellemek için
    private HashSet<int> triggeredArenaLevels = new HashSet<int>();

    private void Awake()
    {
        if (playerExperience == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerExperience = player.GetComponent<PlayerExperience>();
        }
    }

    private void Start()
    {
        spawnCounter = 0f; // hemen spawn edebilsin
    }

    private void Update()
    {
        if (spawnTable == null || spawnTable.brackets == null || spawnTable.brackets.Count == 0)
            return;

        if (playerExperience == null)
            return;

        int level = playerExperience.level;

        // Arena boss tetik kontrolü
        HandleArenaBossTriggers(level);

        // Arena aktifken spawn duracaksa
        if (arenaActive && pauseSpawningDuringArena)
            return;

        // Bracket seç
        if (!spawnTable.TryGetBracket(level, out var bracket) || bracket == null)
            return;

        // Interval hesapla
        float interval = Mathf.Max(0.01f, bracket.baseSpawnInterval);
        if (isRushActive)
        {
            float mult = Mathf.Max(1f, bracket.rushSpeedMultiplier);
            interval = interval / mult;
        }

        spawnCounter -= Time.deltaTime;

        if (spawnCounter <= 0f)
        {
            spawnCounter = interval;

            // Spawn edilecek prefab seç (enemy + boss ayrı listeler ama seçim combined)
            GameObject prefabToSpawn = PickSpawnPrefab(bracket);
            if (prefabToSpawn == null) return;

            Vector3 spawnPos = GetRandomPointOnBorder();
            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        }
    }

    // ----------------------------
    // PUBLIC API (Rush yönetimi)
    // ----------------------------

    public void StartRush(float durationSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(RushRoutine(durationSeconds));
    }

    public void SetRush(bool active)
    {
        isRushActive = active;
    }

    private IEnumerator RushRoutine(float duration)
    {
        isRushActive = true;
        yield return new WaitForSeconds(duration);
        isRushActive = false;
    }

    // ----------------------------
    // Arena Boss (Optional) Hooks
    // ----------------------------

    private void HandleArenaBossTriggers(int currentLevel)
    {
        if (spawnTable.arenaBossTriggers == null || spawnTable.arenaBossTriggers.Count == 0)
            return;

        for (int i = 0; i < spawnTable.arenaBossTriggers.Count; i++)
        {
            var trig = spawnTable.arenaBossTriggers[i];
            if (trig == null) continue;

            if (currentLevel >= trig.triggerLevel)
            {
                // triggerOnce kontrol
                if (trig.triggerOnce && triggeredArenaLevels.Contains(trig.triggerLevel))
                    continue;

                triggeredArenaLevels.Add(trig.triggerLevel);

                if (trig.arenaBossPrefab != null)
                {
                    arenaActive = true; // spawner durdurma seçeneği pauseSpawningDuringArena ile
                    OnArenaBossTriggered?.Invoke(trig.arenaBossPrefab);
                }
            }
        }
    }

    // Arena bittiğinde dışarıdan çağır
    public void EndArenaBoss()
    {
        arenaActive = false;
    }

    // ----------------------------
    // Weighted Selection
    // ----------------------------

    private GameObject PickSpawnPrefab(SpawnTableSO.LevelBracket bracket)
    {
        // Combined weight pool:
        // enemies + bosses = tek roll
        // Örn: enemy1 49.5, enemy2 49.5, boss1 1 => toplam 100
        float totalWeight = 0f;

        totalWeight += SumWeights(bracket.enemies);
        totalWeight += SumWeights(bracket.bosses);

        if (totalWeight <= 0f)
            return null;

        float roll = Random.Range(0f, totalWeight);

        // önce enemies
        var enemyPick = PickFromList(bracket.enemies, ref roll);
        if (enemyPick != null) return enemyPick;

        // sonra bosses
        var bossPick = PickFromList(bracket.bosses, ref roll);
        return bossPick;
    }

    private float SumWeights(List<SpawnTableSO.WeightedPrefab> list)
    {
        if (list == null) return 0f;
        float sum = 0f;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null || list[i].prefab == null) continue;
            if (list[i].weight <= 0f) continue;
            sum += list[i].weight;
        }
        return sum;
    }

    private GameObject PickFromList(List<SpawnTableSO.WeightedPrefab> list, ref float roll)
    {
        if (list == null) return null;

        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item == null || item.prefab == null) continue;
            if (item.weight <= 0f) continue;

            if (roll < item.weight)
                return item.prefab;

            roll -= item.weight;
        }

        return null;
    }

    // ----------------------------
    // Border spawn (your original logic)
    // ----------------------------

    private Vector3 GetRandomPointOnBorder()
    {
        float left = minSpawn.position.x;
        float right = maxSpawn.position.x;
        float bottom = minSpawn.position.y;
        float top = maxSpawn.position.y;

        // 0 = alt, 1 = sağ, 2 = üst, 3 = sol
        int side = Random.Range(0, 4);

        float x = 0f;
        float y = 0f;

        switch (side)
        {
            case 0:
                y = bottom;
                x = Random.Range(left, right);
                break;
            case 1:
                x = right;
                y = Random.Range(bottom, top);
                break;
            case 2:
                y = top;
                x = Random.Range(left, right);
                break;
            case 3:
                x = left;
                y = Random.Range(bottom, top);
                break;
        }

        return new Vector3(x, y, 0f);
    }
}
