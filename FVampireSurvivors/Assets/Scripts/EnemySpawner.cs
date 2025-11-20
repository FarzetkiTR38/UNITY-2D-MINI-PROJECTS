using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    
    public GameObject enemyToSpawn;

    public float timeToSpawn;
    private float spawnCounter;


    void Start()
    {
        spawnCounter = timeToSpawn;
    }

    // Update is called once per frame
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
