using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int fruitCount;

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
    }
}
