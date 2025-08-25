using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;

public class Plot : MonoBehaviour
{


    // referanslar
    [SerializeField]
    SpriteRenderer sr;

    [SerializeField]
    Color hoverColor;

    public GameObject towerObj;
    public Turret turret;
    Color startColor;

    public Transform parentObject;

    void Start()
    {
        startColor = sr.color;



        // parentGO adında bir gameobject oluşturup TowersParent adlı empty'i atıyoruz sonrasında onu da parentObject'e atıyor
        // aşağıda da oluşturulan turretleri bu empty objenin altında yani child child olarak instance ediyoruz yoksa inspector panelinde çok yer kaplıyordu.
        GameObject parentGO = GameObject.Find("TowersParent");
        if (parentGO != null)
        {
            parentObject = parentGO.transform;
        }
        else
        {
            Debug.LogWarning("TowersParent objesi sahnede bulunamadı!");
        }
    }


    void OnMouseEnter()
    {
        sr.color = hoverColor;
    }

    void OnMouseExit()
    {
        sr.color = startColor;
    }

    void OnMouseDown()
    {

        if (UIManager.instance.IsHoveringUI())
        {
            return;
        }

        if (towerObj != null)
        {
            turret.OpenUpgradeUI();
            return;
        }

        Tower towerToBuild = BuildManager.instance.GetSelectedTower();



        if (towerToBuild.cost > LevelManager.instance.currency)
        {
            print("Bu kuleyi satın almak için yeterli paraya sahip değilsin!");
            return;
        }

        LevelManager.instance.SpendCurrency(towerToBuild.cost);

        towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity, parentObject);


        turret = towerObj.GetComponent<Turret>();
        SesController.instance.KarisikSesEffectiCikar(1);


    }
    
    public bool hasGoldDigger = false;

    public bool CheckForGoldDigger()
    {
        hasGoldDigger = false; 

        foreach (Transform child in parentObject)
        {
            if (child.name == "GoldDigger(Clone)")
            {
                hasGoldDigger = true;
                break; 
            }
        }

        // FOR DEBUG
        /* 
        if (hasGoldDigger)
        {
            Debug.Log("GoldDigger bulundu!");
        }
        else
        {
            Debug.Log("GoldDigger yok!");
        }
        */

        return hasGoldDigger;
    }


}
