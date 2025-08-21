using System.Runtime;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public Transform startPoint;
    public Transform[] path;

    public int currency;

    public int world;
    public int wave;

    public int totalDamage;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currency = 1000;
        world = 1;
        wave = 1;
    }

    public void IncreaseCurrency(int amount)
    {
        currency += amount;
    }

    public void WaveUp()
    {
        wave++;
    }

    public void WorldChecker()
    {
        if (wave == 11) // 10. wave son olarak ayarlanmış oldu, 11. wave geçtiği gibi world 1 artacak wave tekrar 1 olacak.
        {
            wave = 1;
            world++;
        }
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
