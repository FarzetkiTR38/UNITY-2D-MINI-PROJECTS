using UnityEngine;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// A simple utility script to close (disable) a specific panel.
    /// Can be attached to a button's onClick event.
    /// </summary>
    public class ClosePanel : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The panel to close when the action is triggered. (Optional: You can also pass the panel directly to the CloseSpecificPanel method)")]
        public GameObject panelToClose;

        /// <summary>
        /// Closes the panel assigned in the inspector.
        /// Call this method from a UI Button's onClick event.
        /// </summary>
        public void Close()
        {
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
            }
            else
            {
                // If no panel is assigned, we can default to disabling the gameObject this script is attached to.
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Alternative method that allows passing the panel directly via the button event.
        /// </summary>
        public void CloseSpecificPanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
