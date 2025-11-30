using Unity.VisualScripting;
using UnityEngine;

public enum FruitType
{
    Apple,
    Banana,
    Cherry,
    Kiwi,
    Melon,
    Orange,
    Pinapple,
    Strawberry
} 

public class FruitManager : MonoBehaviour
{
    Animator anim;

    public FruitType fruitType;

    public GameObject collectedPrefab;

    private void Awake() 
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Start() 
    {
        if (GameManager.instance.isRandomFruit)
        {
            RandomFruitSelect();
        }
        else
        {
            SelectFruit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            GameManager.instance.AddFruit();

            // GameObject prefab = Instantiate(collectedPrefab, transform.position, Quaternion.identity);
            // Destroy(prefab, .5f);

            Instantiate(collectedPrefab, transform.position, Quaternion.identity);
        }     

        // AI'A other.gameObject.CompareTag("Player") ile other.CompareTag("Player") arasındaki farkı sorup öğrencem.
    }

    private void RandomFruitSelect()
    {
        int randomIndex = Random.Range(0,7);

        anim.SetFloat("fruitIndex", randomIndex);    
    }

    private void SelectFruit()
    {
        anim.SetFloat("fruitIndex", (int)fruitType);    
    }
}
