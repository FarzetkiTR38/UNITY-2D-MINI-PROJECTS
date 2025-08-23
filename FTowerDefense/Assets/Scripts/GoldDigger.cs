using UnityEngine;

public class GoldDigger : MonoBehaviour
{

    public static GoldDigger instance;

    Turret turret;

    void Awake()
    {

        instance = this;


        turret = Object.FindAnyObjectByType<Turret>();
    }





    public void FinishedWave()
    {
        LevelManager.instance.currency += 200 * turret.level;
    }



}
