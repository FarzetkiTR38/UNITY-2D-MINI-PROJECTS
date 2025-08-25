using UnityEngine;

public class Health : MonoBehaviour
{


    // nitelikler
    [SerializeField]
    int hitPoints = 2;

    [SerializeField]
    int currencyWorth = 50;

    bool isDestroyed = false;

    [SerializeField]
    GameObject EnemyDestoryEffect;

    public void TakeDamage(int dmg)
    {
        hitPoints -= dmg;
        SesController.instance.KarisikSesEffectiCikar(0);
        
        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemySpawner.onEnemyDestroy.Invoke();
            LevelManager.instance.IncreaseCurrency(currencyWorth);
            isDestroyed = true;
            Destroy(gameObject);
            Instantiate(EnemyDestoryEffect, transform.position, Quaternion.identity);

            print("instantiate çalışmış olmalı");
        }
    }



}



