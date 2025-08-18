using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;

public class Plot : MonoBehaviour
{


    // referanslar
    [SerializeField]
    SpriteRenderer sr;

    [SerializeField]
    Color hoverColor;

    GameObject tower;
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
        if (tower != null)
        {
            return;
        }

        Tower towerToBuild = BuildManager.instance.GetSelectedTower();

        if (towerToBuild.cost > LevelManager.instance.currency)
        {
            print("Bu kuleyi satın almak için yeterli paraya sahip değilsin!");
            return;
        }

        LevelManager.instance.SpendCurrency(towerToBuild.cost);

        tower = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);


        print("Build tower here" + name);
    }


}
