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

    public GameObject xpOrbPrefab;
    public int xpAmount = 5;
    // buradaki mimari biraz kötü ama işe yarar:
    // xporb da zaten xpvalue var ama buradaki xpamount'u oraya atıyoruz 
    // bu sayede farklı enemylerde farklı xp vermesini sağlayabilcez

    [Header("Boss Settings")]
    public bool isBoss = false;
    public GameObject chestPrefab;

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
        // XP orb düşür
        if (xpOrbPrefab != null)
        {
            GameObject orb = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);

            XPOrb xp = orb.GetComponent<XPOrb>();
            if (xp != null)
            {
                xp.xpValue = xpAmount;
            }
        }

        // Boss ise chest düşür
        if (isBoss && chestPrefab != null)
        {
            Instantiate(chestPrefab, transform.position, Quaternion.identity);
            Debug.Log("[Boss] Chest dropped!");
        }

        Destroy(gameObject);
    }





}
