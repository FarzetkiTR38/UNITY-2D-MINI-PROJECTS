using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Fruit Settings")]
    public int fruitCount;
    public bool isRandomFruit;

    [Header("Player Settings")]
    [SerializeField] GameObject player;
    [SerializeField] float respawnDelay = 1.5f;
    [SerializeField] Transform spawnPoint;

    Vector3 currentPosition;

    private void Start() 
    {
        currentPosition = spawnPoint.position;
    }
    

    private void Awake() 
    {
        if(instance != null && instance != this)
        {
            Debug.Log(instance);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        
    }

    public void AddFruit()
    {
        fruitCount++;
        UIManager.instance.UpdateText(fruitCount);
    }

    

    public void RespawnPlayer()
    {
        StartCoroutine(RoutineSpawnPlayer());
    }

    IEnumerator RoutineSpawnPlayer()
    {
        yield return new WaitForSeconds(1f);

        Instantiate(player, currentPosition, Quaternion.identity);

    }

    public void ChangePosition(Transform checkTransform)
    {
        currentPosition = checkTransform.position;
    }
}
