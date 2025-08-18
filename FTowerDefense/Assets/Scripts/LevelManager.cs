using System.Runtime;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public Transform startPoint;
    public Transform[] path;

    public int currency;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currency = 100;
    }

    public void IncreaseCurrency(int amount)
    {
        currency += amount;
    }

    public bool SpendCurrency(int amount)
    {
        if (amount <= currency)
        {
            // buy tower 
            currency -= amount;
            return true;

        }
        else
        {
            print("Satın alma başarısız! (Para yetersiz)");
            return false;
        }

    }   
}
