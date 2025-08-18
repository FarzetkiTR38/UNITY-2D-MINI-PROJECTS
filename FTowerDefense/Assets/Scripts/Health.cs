using UnityEngine;

public class Health : MonoBehaviour
{


    // nitelikler
    [SerializeField]
    int hitPoints = 2;

    [SerializeField]
    int currencyWorth = 50;

    bool isDestroyed = false;

    public void TakeDamage(int dmg)
    {
        hitPoints -= dmg;

        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemySpawner.onEnemyDestroy.Invoke();
            LevelManager.instance.IncreaseCurrency(currencyWorth);
            isDestroyed = true;
            Destroy(gameObject);
        }
    }



}



