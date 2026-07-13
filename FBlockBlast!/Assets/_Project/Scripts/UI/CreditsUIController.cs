using UnityEngine;
using UnityEngine.UI;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Controls the Credits panel UI.
    /// Handles back navigation and social media external links.
    /// </summary>
    public class CreditsUIController : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button closeButton;

        [Header("Social Buttons")]
        [SerializeField] private Button websiteButton;
        [SerializeField] private Button twitterButton;
        [SerializeField] private Button discordButton;
        [SerializeField] private Button instagramButton;
        [SerializeField] private Button youtubeButton;

        [Header("URLs")]
        [SerializeField] private string websiteUrl = "https://farzetkigames.com";
        [SerializeField] private string twitterUrl = "https://twitter.com/FarzetkiGames";
        [SerializeField] private string discordUrl = "https://discord.gg/yourinvite";
        [SerializeField] private string instagramUrl = "https://instagram.com/FarzetkiGames";
        [SerializeField] private string youtubeUrl = "https://youtube.com/c/FarzetkiGames";

        public event System.Action OnCloseClicked;

        private void Awake()
        {
            // Back Button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    OnCloseClicked?.Invoke();
                    gameObject.SetActive(false); // Panels generally disable themselves or let HomeScreenController do it
                });
            }

            // Social Links
            if (websiteButton != null)
                websiteButton.onClick.AddListener(() => OpenURL(websiteUrl));
                
            if (twitterButton != null)
                twitterButton.onClick.AddListener(() => OpenURL(twitterUrl));
                
            if (discordButton != null)
                discordButton.onClick.AddListener(() => OpenURL(discordUrl));
                
            if (instagramButton != null)
                instagramButton.onClick.AddListener(() => OpenURL(instagramUrl));
                
            if (youtubeButton != null)
                youtubeButton.onClick.AddListener(() => OpenURL(youtubeUrl));
        }

        private void OpenURL(string url)
        {
            NeonGalaxy.VFX.AudioManager.Instance?.PlayUIClick();
            if (string.IsNullOrEmpty(url)) return;
            
            Debug.Log($"[CreditsUI] Opening URL: {url}");
            Application.OpenURL(url);
        }
    }
}
