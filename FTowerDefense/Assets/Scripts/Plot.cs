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

        GameObject towerToBuild = BuildManager.instance.GetSelectedTower();
        Instantiate(towerToBuild, transform.position, Quaternion.identity);


        print("Build tower here" + name);
    }


}
