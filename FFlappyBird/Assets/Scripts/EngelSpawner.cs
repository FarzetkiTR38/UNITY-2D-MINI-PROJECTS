using System.Threading;
using UnityEngine;

public class EngelSpawner : MonoBehaviour
{

    [SerializeField]
    float maxTime = 1.5f;

    [SerializeField]
    float heightRange = 0.45f;

    [SerializeField]
    GameObject engelObje;

    float timer;

    void Start()
    {
        EngelSpawn();
    }


    void Update()
    {
        if (timer > maxTime)
        {
            EngelSpawn();
            timer = 0;
        }

        timer += Time.deltaTime;
    }

    private void EngelSpawn()
    {
        Vector3 spawnPos = transform.position + new Vector3(0, Random.Range(-heightRange, heightRange), 0);

        GameObject engel = Instantiate(engelObje, spawnPos, Quaternion.identity);

        Destroy(engel, 10f);
    } 
}
