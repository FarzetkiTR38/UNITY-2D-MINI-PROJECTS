using Unity.VisualScripting;
using UnityEngine;

public class FruitManager : MonoBehaviour
{
    Animator anim;

    private void Awake() 
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Start() 
    {
        int randomIndex = Random.Range(0,7);

        anim.SetFloat("fruitIndex", randomIndex);    
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            GameManager.instance.AddFruit();
        }     

        // AI'A other.gameObject.CompareTag("Player") ile other.CompareTag("Player") arasındaki farkı sorup öğrencem.
    }
}
