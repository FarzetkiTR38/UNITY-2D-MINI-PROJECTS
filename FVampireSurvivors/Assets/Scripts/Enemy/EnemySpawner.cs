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

        if (spawnCounter <= 0f)
        {
            spawnCounter = timeToSpawn;

            Vector3 spawnPos = GetRandomPointOnBorder();
            Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
        }
    }

    Vector3 GetRandomPointOnBorder()
    {
        float left   = minSpawn.position.x;
        float right  = maxSpawn.position.x;
        float bottom = minSpawn.position.y;
        float top    = maxSpawn.position.y;

        // 0 = alt, 1 = sağ, 2 = üst, 3 = sol kenar
        int side = Random.Range(0, 4);

        float x = 0f;
        float y = 0f;

        switch (side)
        {
            case 0: // ALT kenar
                y = bottom;
                x = Random.Range(left, right);
                break;

            case 1: // SAĞ kenar
                x = right;
                y = Random.Range(bottom, top);
                break;

            case 2: // ÜST kenar
                y = top;
                x = Random.Range(left, right);
                break;

            case 3: // SOL kenar
                x = left;
                y = Random.Range(bottom, top);
                break;
        }

        return new Vector3(x, y, 0f);
    }






}
