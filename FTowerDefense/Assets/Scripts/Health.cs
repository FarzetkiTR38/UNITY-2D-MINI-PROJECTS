using UnityEngine;

public class Health : MonoBehaviour
{


    // nitelikler
    [SerializeField]
    int hitPoints = 2;

    public void TakeDamage(int dmg)
    {
        hitPoints -= dmg;

        if (hitPoints <= 0)
        {
            EnemySpawner.onEnemyDestroy.Invoke();
            Destroy(gameObject);
        }
    }



}



