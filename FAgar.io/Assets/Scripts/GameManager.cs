using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public GameObject foodPrefab;


    public Vector2 xRange;
    public Vector2 yRange;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        for (int i = 0; i < 2500; i++)
        {
            SpawnFood();
        }
    }


    public void SpawnFood()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(xRange.x, xRange.y), Random.Range(yRange.x, yRange.y), 1);

        GameObject _food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);

        _food.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0f, 1f, 0.9f, 1f, 0.9f, 1f);
    }

    

}
