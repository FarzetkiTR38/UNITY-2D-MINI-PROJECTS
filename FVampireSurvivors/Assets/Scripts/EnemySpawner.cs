using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    
    public GameObject enemyToSpawn;

    public float timeToSpawn;
    private float spawnCounter;

    public Transform minSpawn, maxSpawn;

    void Start()
    {
        spawnCounter = timeToSpawn;
    }

    
    void Update()
    {
        spawnCounter -= Time.deltaTime;
        if(spawnCounter <= 0)
        {
            spawnCounter = timeToSpawn;

            Instantiate(enemyToSpawn, transform.position, transform.rotation);
        }
    }
}
