using UnityEngine;

public class Health : MonoBehaviour
{


    // nitelikler
    [SerializeField]
    int hitPoints = 2;

    bool isDestroyed = false;

    public void TakeDamage(int dmg)
    {
        hitPoints -= dmg;

        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemySpawner.onEnemyDestroy.Invoke();
            isDestroyed = true;
            Destroy(gameObject);
        }
    }



}



