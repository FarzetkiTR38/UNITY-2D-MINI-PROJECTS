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

            LoadStartingScene();
        }

        private void LoadStartingScene()
        {
            string targetScene = "MainMenuScene";
            if (Data.DataManager.Instance != null && !Data.DataManager.Instance.IsTutorialCompleted)
            {
                Data.DataManager.Instance.SetCurrentLevel(1);
                targetScene = "GameScene";
            }
            else
            {
                GameManager.Instance?.SetState(GameState.Menu);
            }

            var transition = SceneTransitionManager.Instance;
            if (transition != null)
            {
                transition.LoadStartupScene(targetScene, _loadingUI != null ? _loadingUI.SetProgress : null);
            }
            else
            {
                StartCoroutine(LoadWithoutTransition(targetScene));
            }
        }

        private IEnumerator LoadWithoutTransition(string sceneName)
        {
            Time.timeScale = 1f;
            var load = SceneManager.LoadSceneAsync(sceneName);
            while (!load.isDone)
            {
                if (_loadingUI != null) _loadingUI.SetProgress(load.progress);
                yield return null;
            }
        }
    }
}
