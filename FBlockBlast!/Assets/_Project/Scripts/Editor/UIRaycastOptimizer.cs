#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NeonGalaxy.EditorTools
{
    /// <summary>
    /// A tool to optimize UI performance on mobile devices by disabling RaycastTarget
    /// on all UI elements that do not need to receive touch/click events.
    /// This drastically reduces the CPU load of the GraphicRaycaster.
    /// </summary>
    public class UIRaycastOptimizer : EditorWindow
    {
        [MenuItem("Tools/NeonGalaxy/Optimize UI Raycasts")]
        public static void OptimizeRaycasts()
        {
            // Find all Graphic components in the scene (Image, Text, TextMeshProUGUI, RawImage)
            Graphic[] graphics = FindObjectsOfType<Graphic>(true);
            int optimizedCount = 0;

            foreach (Graphic graphic in graphics)
            {
                // If it's already off, skip
                if (!graphic.raycastTarget) continue;

                // Check if this graphic or any of its parents has an interactive component
                // We check parents because a Button might be on a parent object, 
                // and it needs its child Image/Text to have raycastTarget = true to detect clicks.
                bool isPartOfInteractive = graphic.GetComponentInParent<Selectable>(true) != null;

                if (!isPartOfInteractive)
                {
                    Undo.RecordObject(graphic, "Optimize UI Raycast");
                    graphic.raycastTarget = false;
                    
                    // Important for prefab overrides to be saved
                    PrefabUtility.RecordPrefabInstancePropertyModifications(graphic);
                    
                    optimizedCount++;
                }
            }

            Debug.Log($"[UI Optimizer] Successfully disabled RaycastTarget on {optimizedCount} non-interactive UI elements.");
            EditorUtility.DisplayDialog("UI Optimizer", 
                $"Optimization Complete!\n\nDisabled Raycast Target on {optimizedCount} non-interactive UI elements.\n\nMake sure to save your scene.", 
                "OK");
        }
    }
}
#endif
