using UnityEngine;
using System.Collections.Generic;

public class FoodController : MonoBehaviour
{

    [Header("Kuyruk Ayarları")]
    [SerializeField] GameObject tailPrefab;
    [SerializeField] GameObject foodPrefab;
    [SerializeField] float gap = 3; // Kuyruk parçaları arasındaki pozisyon adımı

    Rigidbody2D rb;


    List<Transform> tailParts = new List<Transform>();
    List<Vector3> positions = new List<Vector3>();

    public int tailAmount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        positions.Add(transform.position);
        SpawnFood();
    }

    void FixedUpdate() // Fizik güncellemesi — daha stabil takip sağlar
    {

        // Pozisyon kaydı — her FixedUpdate'te baş pozisyonunu sakla
        if (positions.Count == 0 || Vector3.Distance(positions[0], transform.position) > 0.01f)
        {
            positions.Insert(0, transform.position);
        }

        MoveTail();
    }



    void MoveTail()
    {
        for (int i = 0; i < tailParts.Count; i++)
        {
            float index = Mathf.Min((i + 1) * gap, positions.Count - 1);
            tailParts[i].position = positions[(int)index];
        }
    }

    void AddTail()
    {
        GameObject newPart = Instantiate(tailPrefab, positions[positions.Count - 1], Quaternion.identity);
        tailParts.Add(newPart.transform);
        // debug
        /*
        print(tailParts);
        print(positions.Count);
        */
        tailAmount++;
    }

    void SpawnFood()
    {
        int randomX = Random.Range(-17, 17);
        int randomY = Random.Range(-9, 9);
        Instantiate(foodPrefab, new Vector3(randomX, randomY, 0), Quaternion.identity);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Destroy(other.gameObject);
            AddTail();
            SpawnFood();
        }
    }
    

}
