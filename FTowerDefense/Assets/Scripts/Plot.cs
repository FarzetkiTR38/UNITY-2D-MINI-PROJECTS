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

    void Start()
    {
        startColor = sr.color;
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

        towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);

        turret = towerObj.GetComponent<Turret>();


    }


}
