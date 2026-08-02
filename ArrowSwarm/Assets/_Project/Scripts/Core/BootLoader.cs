namespace ArrowSwarm.Core
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Boot scene loader. Initializes core systems then loads Main Menu.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private float _splashDuration = 2f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_splashDuration);
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
