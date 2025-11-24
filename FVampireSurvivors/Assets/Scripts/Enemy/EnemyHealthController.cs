using UnityEngine;

public class EnemyHealthController : MonoBehaviour
{
    public static EnemyHealthController instance;

    private void Awake() 
    {
        instance = this;
    }

    public int maxHP = 30;
    private int currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }




}
