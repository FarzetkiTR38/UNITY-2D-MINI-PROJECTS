using System.Collections;
using UnityEngine;
using UnityEditor;

public class SlowmoTower : MonoBehaviour
{

    //referanslar
    [SerializeField]
    LayerMask enemyMask;


    // nitelikler
    [SerializeField]
    float targetingRange = 2f;

    [SerializeField]
    float aps = 2f; // aps -> attack per second

    [SerializeField]
    float freezeTime = 1f;

    float timeUntilFire;



    void Update()
    {

        timeUntilFire += Time.deltaTime;

        if (timeUntilFire >= 1f / aps)
        {
            FreezeEnimies();
            timeUntilFire = 0f;
        }

    }

    void FreezeEnimies()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];

                EnemyController em = hit.transform.GetComponent<EnemyController>();
                em.UpdateSpeed(.5f);

                StartCoroutine(ResetEnemySpeed(em));
            }
        }
    }

    private IEnumerator ResetEnemySpeed(EnemyController em)
    {
        yield return new WaitForSeconds(freezeTime);

        em.ResetSpeed();
    }
    
    void OnDrawGizmosSelected()
    {
        Handles.color = Color.cyan;

        Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);
        // turret in reachını ayarlıyoruz 2f dediğimizde sola 2 sağa 2 olacak şekilde yani r'si 2 oluyor, pi.r.r den 4.pi kadarlık alanı görüyor demek.
    }


}


