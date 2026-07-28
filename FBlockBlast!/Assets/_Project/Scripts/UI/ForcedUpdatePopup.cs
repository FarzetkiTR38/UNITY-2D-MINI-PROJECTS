using UnityEngine;
using UnityEngine.UI;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Displays a forced update prompt that blocks all input and forces the user to the store.
    /// </summary>
    public class ForcedUpdatePopup : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button updateButton;
        
        private string _storeUrl;

        private void Awake()
        {
            if (updateButton != null)
            {
                updateButton.onClick.AddListener(OnUpdateClicked);
            }
            
            // Start hidden
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (updateButton != null)
            {
                updateButton.onClick.RemoveListener(OnUpdateClicked);
            }
        }

        public void Show(string storeUrl)
        {
            _storeUrl = storeUrl;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        private void OnUpdateClicked()
        {
            if (!string.IsNullOrEmpty(_storeUrl))
            {
                Application.OpenURL(_storeUrl);
            }
        }
    }
}
