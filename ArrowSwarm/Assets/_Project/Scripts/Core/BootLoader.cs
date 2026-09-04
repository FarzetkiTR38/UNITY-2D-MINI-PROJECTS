namespace ArrowSwarm.Core
{
    using System.Collections;
    using ArrowSwarm.UI;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Boot scene loader. Initializes core systems, coordinates the animated
    /// BootLoadingUI progress, and seamlessly transitions into MainMenuScene or GameScene.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private GameObject _coreManagersPrefab;
        [SerializeField] private BootLoadingUI _loadingUI;

        private void Awake()
        {
            // Eagerly resolve localization so saved language is active before UI displays
            var loc = ArrowSwarm.Localization.LocalizationManager.Instance;
        }

        private void Start()
        {
            // Ensure CoreManagers are instantiated
            if (_coreManagersPrefab != null && Object.FindFirstObjectByType<GameManager>() == null)
            {
                var go = Instantiate(_coreManagersPrefab);
                DontDestroyOnLoad(go);
            }

            if (_loadingUI == null)
            {
                _loadingUI = Object.FindFirstObjectByType<BootLoadingUI>();
            }

            if (_loadingUI != null)
            {
                _loadingUI.StartLoading(OnLoadingCompleted);
            }
            else
            {
                StartCoroutine(FallbackLoadRoutine());
            }
        }

        private void OnLoadingCompleted()
        {
            string targetScene = "MainMenuScene";
            if (Data.DataManager.Instance != null && !Data.DataManager.Instance.IsTutorialCompleted)
            {
                Data.DataManager.Instance.SetCurrentLevel(1);
                targetScene = "GameScene";
            }

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(targetScene);
            }
            else
            {
                SceneManager.LoadScene(targetScene);
            }
        }

        private IEnumerator FallbackLoadRoutine()
        {
            yield return new WaitForSeconds(1.0f);
            OnLoadingCompleted();
        }
    }
}
