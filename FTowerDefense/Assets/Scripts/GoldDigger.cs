using UnityEngine;

public class GoldDigger : MonoBehaviour
{

    public static GoldDigger instance;

    Turret turret;
    Plot plot;

    void Awake()
    {

        instance = this;


        turret = Object.FindAnyObjectByType<Turret>();
        plot = Object.FindAnyObjectByType<Plot>();
    }



    public void FinishedWave()
    {
        if (plot.CheckForGoldDigger())
        {
            LevelManager.instance.currency += 400 * turret.level;  
        }
        


    }



}
