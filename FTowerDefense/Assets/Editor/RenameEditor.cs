using UnityEngine;
using UnityEditor;

public class RenameChildren : MonoBehaviour
{
    [MenuItem("Tools/Rename Plots Sequentially")]
    static void RenamePlots()
    {
        GameObject parent = GameObject.Find("Plots"); // Parent objenin adı
        if (parent == null)
        {
            Debug.LogError("Parent 'Plots' bulunamadı!");
            return;
        }

        int index = 1;
        foreach (Transform child in parent.transform)
        {
            child.name = "Plot " + index;
            index++;
        }

        Debug.Log("Plots yeniden isimlendirildi!");
    }
}
