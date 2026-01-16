using UnityEngine;

public class ArenaBossManager : MonoBehaviour
{
    [SerializeField] Transform bossSpawnPoint;
    EnemySpawner spawner;

    private void Awake()
    {
        spawner = FindAnyObjectByType<EnemySpawner>();

        spawner.OnArenaBossTriggered += SpawnArenaBoss;
    }

    void SpawnArenaBoss(GameObject bossPrefab)
    {
        Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
    }
}

