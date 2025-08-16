using System.Collections;
using UnityEngine;
using UnityEngine.Events;


public class EnemySpawner : MonoBehaviour
{
    // referanslar
    [SerializeField]
    GameObject[] enemyPrefabs;


    // nitelikler
    [SerializeField]
    int baseEnemies = 8;

    [SerializeField]
    float enemiesPerSeconds = 0.5f;

    [SerializeField]
    float timeBetweenWaves = 5f;

    [SerializeField]
    float difficultScalingFactor = 0.75f;

    int currentWave = 1;
    float timeSinceLastSpawn;
    int enemiesAlive;
    int enemiesLeftToSpawn;
    bool isSpawning = false;


    // events
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestoryed);

    }
    void Start()
    {
        StartCoroutine(StartWave());
    }

    void Update()
    {

        if (!isSpawning)
        {
            return;
        }



        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= (1f / enemiesPerSeconds) && enemiesLeftToSpawn > 0)
        {
            SpawnEnemy();
            enemiesLeftToSpawn--;
            enemiesAlive++;
            timeSinceLastSpawn = 0f;
        }

        if (enemiesAlive == 0 && enemiesLeftToSpawn == 0)
        {
            EndWave();
        }
    }

    void EnemyDestoryed()
    {
        enemiesAlive--;
    }

    IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        isSpawning = true;
        enemiesLeftToSpawn = EnemiesPerWave();
    }

    void EndWave()
    {
        isSpawning = false;
        timeSinceLastSpawn = 0f;
        currentWave++;
        StartCoroutine(StartWave());
    }

    int EnemiesPerWave()
    {
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficultScalingFactor));
        //Mathf.Pow Unity’de bir sayının üsünü (kuvvetini) almak için kullanılır.
    }

    void SpawnEnemy()
    {
        GameObject prefabToSpawn = enemyPrefabs[0];
        Instantiate(prefabToSpawn, LevelManager.instance.startPoint.position, Quaternion.identity);


    }




}
