using UnityEditor.Experimental.Rendering;
using UnityEngine;

public class AScript : MonoBehaviour
{
    // public BScript bScript;

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.B))
    //     {
    //         bScript.BFonksiyonu();
    //     }    
    // }

    BScript bScript;

    private void Awake() 
    {
        // bScript = GameObject.Find("B_Objesi").GetComponent<BScript>();

        bScript = FindAnyObjectByType<BScript>();
    }
    

    // bir diğer yöntem de instance olayı onu zaten biliyorum geçtim
}
