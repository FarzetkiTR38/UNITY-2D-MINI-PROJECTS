using UnityEngine;
using UnityEditor;

public class FindNestedPlots
{
    [MenuItem("Tools/Plots Cleanup/Select Nested Plots")]
    static void SelectNestedPlots()
    {
        GameObject parent = GameObject.Find("Plots");
        if (parent == null)
        {
            Debug.LogError("'Plots' bulunamadı!");
            return;
        }

        var nested = new System.Collections.Generic.List<GameObject>();

        foreach (Transform t in parent.transform)
        {
            foreach (Transform child in t)
            {
                if (child.name.StartsWith("Plot")) // Çocuğun adı da Plot ise
                {
                    nested.Add(child.gameObject);
                }
            }
        }

        Selection.objects = nested.ToArray();
        Debug.Log($"Bulunan iç içe Plot sayısı: {nested.Count}");
    }
}
