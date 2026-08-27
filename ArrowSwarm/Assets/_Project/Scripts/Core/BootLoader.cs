namespace ArrowSwarm.Core
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Boot scene loader. Initializes core systems then loads Main Menu rapidly.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private float _splashDuration = 0.2f;
        [SerializeField] private GameObject _coreManagersPrefab;

        private IEnumerator Start()
        {
            if (_coreManagersPrefab != null && Object.FindFirstObjectByType<GameManager>() == null)
            {
                var go = Instantiate(_coreManagersPrefab);
                DontDestroyOnLoad(go);
            }
            
            // Fast transition delay (max 0.2s)
            float waitTime = Mathf.Min(_splashDuration, 0.2f);
            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }

            string targetScene = "MainMenuScene";
            if (Data.DataManager.Instance != null && !Data.DataManager.Instance.IsTutorialCompleted)
            {
                Data.DataManager.Instance.SetCurrentLevel(1);
                targetScene = "GameScene";
            }

            AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
            while (op != null && !op.isDone)
            {
                yield return null;
            }
        }
    }
}
