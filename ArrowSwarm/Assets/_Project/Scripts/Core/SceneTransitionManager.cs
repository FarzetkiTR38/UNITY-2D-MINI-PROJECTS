namespace ArrowSwarm.Core
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// Global Scene Transition Manager.
    /// Dedicated indestructible singleton providing smooth, glowing Iris Circle Wipe transitions.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager _instance;
        public static SceneTransitionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SceneTransitionManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("SceneTransitionManager");
                        _instance = go.AddComponent<SceneTransitionManager>();
                    }
                }
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (_instance == null)
            {
                var inst = Instance;
            }
        }

        [Header("--- Transition Settings ---")]
        [SerializeField] private Material _transitionMaterial;
        [SerializeField] private float _defaultDuration = 0.45f;

        private Canvas _transitionCanvas;
        private Image _overlayImage;
        private Material _materialInstance;
        private Coroutine _currentTransition;
        private bool _isTransitioning;

        private static readonly int PropProgress = Shader.PropertyToID("_Progress");
        private static readonly int PropCenter = Shader.PropertyToID("_Center");
        private static readonly int PropAspectRatio = Shader.PropertyToID("_AspectRatio");

        /// <summary>
        /// Gets whether a scene transition is currently in progress.
        /// </summary>
        public bool IsTransitioning => _isTransitioning;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            EnsureUI();
        }

        private void EnsureUI()
        {
            if (_transitionCanvas != null && _overlayImage != null && _materialInstance != null) return;

            // 1. Create dedicated top-level Canvas (Order 9999) under this indestructible object
            if (_transitionCanvas == null)
            {
                var canvasGO = new GameObject("Canvas_SceneTransition");
                canvasGO.transform.SetParent(transform, false);

                _transitionCanvas = canvasGO.AddComponent<Canvas>();
                _transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _transitionCanvas.sortingOrder = 9999;

                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGO.AddComponent<GraphicRaycaster>();

                // 2. Fullscreen Overlay Image
                var imageGO = new GameObject("TransitionOverlay");
                imageGO.transform.SetParent(canvasGO.transform, false);

                var rt = imageGO.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                _overlayImage = imageGO.AddComponent<Image>();
                _overlayImage.color = Color.white;
                _overlayImage.raycastTarget = false;
            }

            // 3. Material setup
            if (_materialInstance == null)
            {
                if (_transitionMaterial != null)
                {
                    _materialInstance = new Material(_transitionMaterial);
                }
                else
                {
                    var shader = Shader.Find("UI/IrisCircleWipe");
                    if (shader != null)
                    {
                        _materialInstance = new Material(shader);
                    }
                }
            }

            if (_materialInstance != null && _overlayImage != null)
            {
                _materialInstance.SetFloat(PropProgress, 0f);
                _overlayImage.material = _materialInstance;
                _overlayImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Loads a target scene with an Iris Circle Wipe transition.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        /// <param name="duration">Total transition duration in seconds.</param>
        /// <param name="screenCenter">Normalized screen center (0-1 UV), default is (0.5, 0.5).</param>
        public void LoadScene(string sceneName, float duration = -1f, Vector2? screenCenter = null)
        {
            float d = duration > 0f ? duration : _defaultDuration;
            Vector2 center = screenCenter ?? new Vector2(0.5f, 0.5f);

            if (_currentTransition != null) StopCoroutine(_currentTransition);
            _currentTransition = StartCoroutine(SceneTransitionRoutine(sceneName, d, center));
        }

        /// <summary>
        /// Executes a custom action at the transition midpoint with the Iris Circle Wipe effect.
        /// </summary>
        public void PlayTransition(Action onMidpoint, float duration = -1f, Vector2? screenCenter = null)
        {
            float d = duration > 0f ? duration : _defaultDuration;
            Vector2 center = screenCenter ?? new Vector2(0.5f, 0.5f);

            if (_currentTransition != null) StopCoroutine(_currentTransition);
            _currentTransition = StartCoroutine(ActionTransitionRoutine(onMidpoint, d, center));
        }

        private IEnumerator SceneTransitionRoutine(string sceneName, float duration, Vector2 center)
        {
            _isTransitioning = true;
            EnsureUI();

            float halfDuration = Mathf.Max(0.12f, duration * 0.5f);
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

            try
            {
                if (_materialInstance != null)
                {
                    _materialInstance.SetVector(PropCenter, new Vector4(center.x, center.y, 0f, 0f));
                    _materialInstance.SetFloat(PropAspectRatio, aspect);
                    _materialInstance.SetFloat(PropProgress, 0f);
                }

                if (_overlayImage != null)
                {
                    _overlayImage.gameObject.SetActive(true);
                    _overlayImage.raycastTarget = true;
                }

                // 1. Close Iris Circle (0 -> 1)
                float startTime = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - startTime < halfDuration)
                {
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
                    _materialInstance?.SetFloat(PropProgress, t);
                    yield return null;
                }
                _materialInstance?.SetFloat(PropProgress, 1f);

                // 2. Load target scene while screen is fully closed
                Time.timeScale = 1f;
                Debug.Log($"[ArrowSwarm] SceneTransitionManager: Loading scene '{sceneName}'...");
                SceneManager.LoadScene(sceneName);

                // Wait 1 frame for new scene Awake & Start to resolve
                yield return null;

                // Recalculate aspect in case resolution adjusted
                aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
                if (_materialInstance != null)
                {
                    _materialInstance.SetFloat(PropAspectRatio, aspect);
                    _materialInstance.SetVector(PropCenter, new Vector4(center.x, center.y, 0f, 0f));
                }

                // 3. Open Iris Circle (1 -> 0)
                startTime = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - startTime < halfDuration)
                {
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    float t = Mathf.SmoothStep(1f, 0f, elapsed / halfDuration);
                    _materialInstance?.SetFloat(PropProgress, t);
                    yield return null;
                }
                _materialInstance?.SetFloat(PropProgress, 0f);
            }
            finally
            {
                // 4. Guaranteed Cleanup
                if (_materialInstance != null)
                {
                    _materialInstance.SetFloat(PropProgress, 0f);
                }

                if (_overlayImage != null)
                {
                    _overlayImage.raycastTarget = false;
                    _overlayImage.gameObject.SetActive(false);
                }

                _isTransitioning = false;
                _currentTransition = null;
                Debug.Log($"[ArrowSwarm] SceneTransitionManager: Transition to '{sceneName}' complete!");
            }
        }

        private IEnumerator ActionTransitionRoutine(Action onMidpoint, float duration, Vector2 center)
        {
            _isTransitioning = true;
            EnsureUI();

            float halfDuration = Mathf.Max(0.12f, duration * 0.5f);
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

            try
            {
                if (_materialInstance != null)
                {
                    _materialInstance.SetVector(PropCenter, new Vector4(center.x, center.y, 0f, 0f));
                    _materialInstance.SetFloat(PropAspectRatio, aspect);
                    _materialInstance.SetFloat(PropProgress, 0f);
                }

                if (_overlayImage != null)
                {
                    _overlayImage.gameObject.SetActive(true);
                    _overlayImage.raycastTarget = true;
                }

                // 1. Close Iris Circle (0 -> 1)
                float startTime = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - startTime < halfDuration)
                {
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
                    _materialInstance?.SetFloat(PropProgress, t);
                    yield return null;
                }
                _materialInstance?.SetFloat(PropProgress, 1f);

                // 2. Midpoint action
                onMidpoint?.Invoke();
                yield return null;

                // 3. Open Iris Circle (1 -> 0)
                startTime = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - startTime < halfDuration)
                {
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    float t = Mathf.SmoothStep(1f, 0f, elapsed / halfDuration);
                    _materialInstance?.SetFloat(PropProgress, t);
                    yield return null;
                }
                _materialInstance?.SetFloat(PropProgress, 0f);
            }
            finally
            {
                // 4. Guaranteed Cleanup
                if (_materialInstance != null)
                {
                    _materialInstance.SetFloat(PropProgress, 0f);
                }

                if (_overlayImage != null)
                {
                    _overlayImage.raycastTarget = false;
                    _overlayImage.gameObject.SetActive(false);
                }

                _isTransitioning = false;
                _currentTransition = null;
            }
        }
    }
}
