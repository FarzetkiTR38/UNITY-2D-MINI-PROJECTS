// Assets/Editor/PlotsCleanup.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class PlotsCleanup
{
    const string ParentName = "Plots";           // Kök parent adı
    const float PosTolerance = 0.0005f;          // Konum toleransı

    static GameObject FindParent()
    {
        var parent = GameObject.Find(ParentName);
        if (parent == null) Debug.LogError($"'{ParentName}' objesi bulunamadı.");
        return parent;
    }

    static IEnumerable<Transform> AllDescendants(Transform root)
    {
        foreach (Transform t in root)
        {
            yield return t;
            foreach (var d in AllDescendants(t)) yield return d;
        }
    }

    static bool SameTRSLocal(Transform a, Transform b)
    {
        return Vector3.Distance(a.localPosition, b.localPosition) <= PosTolerance
            && Quaternion.Angle(a.localRotation, b.localRotation) <= 0.1f
            && Vector3.Distance(a.localScale, b.localScale) <= PosTolerance;
    }

    // -------- 1) İÇ İÇE KOPYALARI BUL (Plot -> Plot) --------
    [MenuItem("Tools/Plots Cleanup/Select Nested Clones")]
    public static void SelectNestedClones()
    {
        var parent = FindParent();
        if (parent == null) return;

        var suspects = new List<Object>();
        foreach (var t in AllDescendants(parent.transform))
        {
            foreach (Transform c in t)
            {
                if (c.name == t.name && SameTRSLocal(c, t))
                    suspects.Add(c.gameObject);
            }
        }

        Selection.objects = suspects.ToArray();
        Debug.Log($"Nested clone adayları seçildi: {suspects.Count} adet.");
    }

    // Daha “güvenli”: daha AZ bileşenli olanı sil (Undo destekli)
    [MenuItem("Tools/Plots Cleanup/Delete Nested Clones (Safe)")]
    public static void DeleteNestedClones()
    {
        var parent = FindParent();
        if (parent == null) return;

        int removed = 0;
        var toDelete = new HashSet<GameObject>();

        foreach (var t in AllDescendants(parent.transform).ToList())
        {
            foreach (Transform c in t)
            {
                if (c.name != t.name || !SameTRSLocal(c, t)) continue;

                int compParent = t.GetComponents<Component>().Length;
                int compChild  = c.GetComponents<Component>().Length;

                // Daha az bileşenli olanı sil
                GameObject victim = (compChild <= compParent) ? c.gameObject : t.gameObject;
                if (!toDelete.Contains(victim))
                    toDelete.Add(victim);
            }
        }

        Undo.IncrementCurrentGroup();
        foreach (var go in toDelete)
        {
            Undo.DestroyObjectImmediate(go);
            removed++;
        }
        Debug.Log($"Nested kopyalardan silinen: {removed}");
    }

    // -------- 2) AYNI POZİSYONDAKİ KOPYALARI BUL/SİL --------
    static string PosKey(Transform t)
    {
        Vector3 p = t.position;
        Vector3 r = t.eulerAngles;
        return $"{t.name}|{Mathf.Round(p.x*1000)/1000f}|{Mathf.Round(p.y*1000)/1000f}|{Mathf.Round(p.z*1000)/1000f}|{Mathf.Round(r.y)}";
    }

    [MenuItem("Tools/Plots Cleanup/Select Position Duplicates")]
    public static void SelectPositionDuplicates()
    {
        var parent = FindParent();
        if (parent == null) return;

        var dict = new Dictionary<string, Transform>();
        var dups = new List<Object>();

        foreach (var t in AllDescendants(parent.transform))
        {
            var key = PosKey(t);
            if (!dict.ContainsKey(key)) dict[key] = t;
            else dups.Add(t.gameObject);
        }

        Selection.objects = dups.ToArray();
        Debug.Log($"Aynı konumdaki kopyalar seçildi: {dups.Count} adet.");
    }

    [MenuItem("Tools/Plots Cleanup/Delete Position Duplicates (Keep First)")]
    public static void DeletePositionDuplicates()
    {
        var parent = FindParent();
        if (parent == null) return;

        var dict = new Dictionary<string, Transform>();
        var toDelete = new List<GameObject>();

        foreach (var t in AllDescendants(parent.transform))
        {
            var key = PosKey(t);
            if (!dict.ContainsKey(key)) dict[key] = t;
            else toDelete.Add(t.gameObject);
        }

        Undo.IncrementCurrentGroup();
        foreach (var go in toDelete)
            Undo.DestroyObjectImmediate(go);

        Debug.Log($"Aynı konumdaki kopyalardan silinen: {toDelete.Count}");
    }
}
