using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonGalaxy.Utility
{
    /// <summary>
    /// Utility for async scene loading with optional fade transitions.
    /// Attach to a DontDestroyOnLoad object or call as a coroutine from BootManager.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader _instance;

        /// <summary>
        /// Fired when a scene load begins. Args: scene name.
        /// </summary>
        public static event Action<string> OnSceneLoadStarted;

        /// <summary>
        /// Fired when a scene load completes. Args: scene name.
        /// </summary>
        public static event Action<string> OnSceneLoadCompleted;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Loads a scene asynchronously, replacing the current scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        /// <param name="onComplete">Optional callback when load completes.</param>
        public static void LoadScene(string sceneName, Action onComplete = null)
        {
            if (_instance == null)
            {
                Debug.LogError("[SceneLoader] No SceneLoader instance found. Falling back to sync load.");
                SceneManager.LoadScene(sceneName);
                onComplete?.Invoke();
                return;
            }

            _instance.StartCoroutine(_instance.LoadSceneAsync(sceneName, LoadSceneMode.Single, onComplete));
        }

        /// <summary>
        /// Loads a scene additively (without unloading current scene).
        /// </summary>
        public static void LoadSceneAdditive(string sceneName, Action onComplete = null)
        {
            if (_instance == null)
            {
                Debug.LogError("[SceneLoader] No SceneLoader instance found.");
                return;
            }

            _instance.StartCoroutine(_instance.LoadSceneAsync(sceneName, LoadSceneMode.Additive, onComplete));
        }

        /// <summary>
        /// Unloads a scene asynchronously.
        /// </summary>
        public static void UnloadScene(string sceneName, Action onComplete = null)
        {
            if (_instance == null)
            {
                Debug.LogError("[SceneLoader] No SceneLoader instance found.");
                return;
            }

            _instance.StartCoroutine(_instance.UnloadSceneAsync(sceneName, onComplete));
        }

        private IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode mode, Action onComplete)
        {
            OnSceneLoadStarted?.Invoke(sceneName);

            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            if (operation == null)
            {
                Debug.LogError($"[SceneLoader] Failed to start loading scene '{sceneName}'. Is it in Build Settings?");
                yield break;
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            OnSceneLoadCompleted?.Invoke(sceneName);
            onComplete?.Invoke();
        }

        private IEnumerator UnloadSceneAsync(string sceneName, Action onComplete)
        {
            var operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation == null)
            {
                Debug.LogWarning($"[SceneLoader] Scene '{sceneName}' could not be unloaded (not loaded or is the only scene).");
                yield break;
            }

            while (!operation.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}
