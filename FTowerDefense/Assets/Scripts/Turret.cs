using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


public class Turret : MonoBehaviour
{

    //referanslar
    [SerializeField]
    Transform turretRotationPoint;

    [SerializeField]
    LayerMask enemyMask;

    [SerializeField]
    GameObject bulletPrefab;

    [SerializeField]
    Transform firingPoint;

    [SerializeField]
    GameObject upgradeUI;

    [SerializeField]
    Button upgradeButton;





    // nitelikler
    [SerializeField]
    float targetingRange = 2f;

    [SerializeField]
    float rotationSpeed = 200f;

    [SerializeField]
    float bps = 1f; // bps -> bullet per second

    public int baseUpgradeCost = 100;

    public int totalSpentMoney;

    float bpsBase;
    float targetingRangeBase;

    Transform target;
    float timeUntilFire;

    public int level = 1;

    void Start()
    {
        bpsBase = bps;
        targetingRangeBase = targetingRange;

        upgradeButton.onClick.AddListener(Upgrade);

        totalSpentMoney = baseUpgradeCost;
        print("totalSpentMoney:" + totalSpentMoney);
    }

    void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        if (!CheckTargetIsInRange())
        {
            target = null;
        }
        else
        {
            timeUntilFire += Time.deltaTime;

            if (timeUntilFire >= 1f / bps)
            {
                Shoot();
                timeUntilFire = 0f;
            }

        }
    }

    void Shoot()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);

        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetTarget(target);
    }

    bool CheckTargetIsInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    void RotateTowardsTarget()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        ;

    }

    void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    void OnDrawGizmosSelected()
    {
        Handles.color = Color.cyan;

        Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);
        // turret in reachını ayarlıyoruz 2f dediğimizde sola 2 sağa 2 olacak şekilde yani r'si 2 oluyor, pi.r.r den 4.pi kadarlık alanı görüyor demek.
    }


    public void OpenUpgradeUI()
    {
        upgradeUI.SetActive(true);
    }

    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        UIManager.instance.SetHoveringState(false);
    }

    public void Upgrade()
    {
        if (CalculateCost() > LevelManager.instance.currency)
        {
            print("Yükseltme yapabilmek için gereken para: " + CalculateCost() + ", siz de bulunan para: " + LevelManager.instance.currency + ", yani " + (CalculateCost() - LevelManager.instance.currency) + " paraya ihtiyacın var.");
            return;
        }
        else if (level == 10)
        {
            print("Max levele (10) ulaştınız!");
            return;
        }
        else
        {
            LevelManager.instance.SpendCurrency(CalculateCost());
            totalSpentMoney += CalculateCost();
            print("totalSpentMoney:" + totalSpentMoney);

            level++;

            bps = CalculateBPS();

            targetingRange = CalculateRange();

            CloseUpgradeUI();

            print("BPS: " + bps);
            print("targetingRange: " + targetingRange);
            print("Cost: " + CalculateCost());
        }
    }

    int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    }

    float CalculateBPS()
    {
        return bpsBase * Mathf.Pow(level, 0.6f);
    }

    float CalculateRange()
    {
        return targetingRangeBase * Mathf.Pow(level, 0.4f);
    }

    public void SellTurret()
    {

        CloseUpgradeUI();

        Destroy(gameObject);
        LevelManager.instance.currency += totalSpentMoney / 2;
        totalSpentMoney = 0;

    }
    
    
}
