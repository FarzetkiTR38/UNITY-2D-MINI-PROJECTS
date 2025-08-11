using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic; // List kullanmak için şartmış yoksa hata veriyor...

public class FoodController : MonoBehaviour
{
    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    [SerializeField]
    GameObject foodPrefab;

    [SerializeField]
    GameObject tailPrefab; 
    private List<Transform> tailParts = new List<Transform>();

    void AddTail()
    {
        // Son parçanın pozisyonunu al (veya başın pozisyonunu)
        Vector3 newPos = tailParts.Count == 0 ? transform.position : tailParts[tailParts.Count - 1].position;

        // Yeni parça oluştur
        GameObject newPart = Instantiate(tailPrefab, newPos, Quaternion.identity);
        tailParts.Add(newPart.transform);
    }

    //Instantiate(myPrefab, Vector3.zero, Quaternion.identity);

    int randomX;
    int randomY;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            //rb.transform.localScale = new Vector3(rb.transform.localScale.x + 1, rb.transform.localScale.y, rb.transform.localScale.z);
            Destroy(other.gameObject);

            // random x y değerleri oluşturup instantiate edelim.
            // instantiate içinde de random sayılar oluşturulabilir fakat ben böyle tercih ediyorum.

            randomX = Random.Range(-17, 17);
            randomY = Random.Range(-9, 9);


            Instantiate(foodPrefab, new Vector3(randomX, randomY, 1f), Quaternion.identity);

            AddTail();
        }
    }

    void Start()
    {
        randomX = Random.Range(-17, 17);
        randomY = Random.Range(-9, 9);
        Instantiate(foodPrefab, new Vector3(randomX, randomY, 1f), Quaternion.identity);
    }
    



}
