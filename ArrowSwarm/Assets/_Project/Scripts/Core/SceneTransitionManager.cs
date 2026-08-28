namespace ArrowSwarm.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// Available visual styles for scene transitions.
    /// </summary>
    public enum TransitionStyle
    {
        [Tooltip("45-degree diagonal sweep with a glowing neon cyan/pink blade.")]
        NeonCyberBlade,

        [Tooltip("High-tech neon hexagon honeycomb grid wave.")]
        HexagonHoneycomb,

        [Tooltip("Geometric diamond tile matrix expansion.")]
        DiamondGrid,

        [Tooltip("Modern circular iris wipe with neon ring.")]
        IrisCircle,

        [Tooltip("Minimalist dark navy fade with subtle cyan vignette.")]
        SmoothFade
    }

    /// <summary>
    /// Centralized Scene Transition Manager.
    /// Manages shader-based screen transitions between scenes and UI states.
    /// Fully configurable via Inspector on CoreManagers or runtime script.
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

        [Header("--- Transition Style & Preset ---")]
        [Tooltip("Select the visual transition effect to use across all scene loads.")]
        [SerializeField] private TransitionStyle _activeStyle = TransitionStyle.NeonCyberBlade;

        [Header("--- Timing Settings ---")]
        [Tooltip("Total duration of the transition in seconds (0.70s = 0.35s close + 0.35s open).")]
        [Range(0.2f, 2.0f)]
        [SerializeField] private float _defaultDuration = 0.70f;

        [Header("--- Theme Colors ---")]
        [Tooltip("Deep background color behind the transition.")]
        [SerializeField] private Color _backgroundColor = new Color(0.063f, 0.078f, 0.145f, 1.0f); // #101425

        [Tooltip("Primary neon glow blade/border color.")]
        [SerializeField] private Color _neonGlowColor = new Color(0.400f, 0.880f, 1.000f, 1.0f); // #66E0FF (Cyan)

        [Tooltip("Secondary accent glow color.")]
        [SerializeField] private Color _secondaryGlowColor = new Color(1.000f, 0.400f, 0.700f, 1.0f); // #FF66B2 (Pink)

        [Header("--- Custom Material Override (Optional) ---")]
        [Tooltip("Optional custom material override. If null, automatically loads the shader for the selected Active Style.")]
        [SerializeField] private Material _customMaterialOverride;

        private Canvas _transitionCanvas;
        private Image _overlayImage;
        private Coroutine _currentTransition;
        private bool _isTransitioning;

        // Cached runtime materials per style
        private readonly Dictionary<TransitionStyle, Material> _materialCache = new Dictionary<TransitionStyle, Material>();

        private static readonly int PropProgress = Shader.PropertyToID("_Progress");
        private static readonly int PropCenter = Shader.PropertyToID("_Center");
        private static readonly int PropAspectRatio = Shader.PropertyToID("_AspectRatio");
        private static readonly int PropColor = Shader.PropertyToID("_Color");
        private static readonly int PropBorderColor = Shader.PropertyToID("_BorderColor");
        private static readonly int PropSecondaryGlow = Shader.PropertyToID("_SecondaryGlow");

        /// <summary>Gets whether a scene transition is currently in progress.</summary>
        public bool IsTransitioning => _isTransitioning;

        /// <summary>Gets or sets the current active transition style.</summary>
        public TransitionStyle ActiveStyle
        {
            get => _activeStyle;
            set
            {
                _activeStyle = value;
                ApplyActiveMaterial();
            }
        }

        /// <summary>Total duration of the transition.</summary>
        public float DefaultDuration
        {
            get => _defaultDuration;
            set => _defaultDuration = Mathf.Max(0.1f, value);
        }

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

        private void OnValidate()
        {
            if (Application.isPlaying && _overlayImage != null)
            {
                ApplyActiveMaterial();
            }
        }

        private void EnsureUI()
        {
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

            ApplyActiveMaterial();
        }

        private Material GetOrCreateMaterial(TransitionStyle style)
        {
            if (_customMaterialOverride != null)
            {
                return _customMaterialOverride;
            }

            if (_materialCache.TryGetValue(style, out Material cached) && cached != null)
            {
                return cached;
            }

            string shaderName = style switch
            {
                TransitionStyle.NeonCyberBlade => "UI/NeonDiamondWipe",
                TransitionStyle.HexagonHoneycomb => "UI/HexagonHoneycombWipe",
                TransitionStyle.DiamondGrid => "UI/DiamondGridWipe",
                TransitionStyle.IrisCircle => "UI/IrisCircleWipe",
                TransitionStyle.SmoothFade => "UI/SmoothFadeWipe",
                _ => "UI/NeonDiamondWipe"
            };

            Shader shader = Shader.Find(shaderName) ?? Shader.Find("UI/NeonDiamondWipe") ?? Shader.Find("UI/IrisCircleWipe");
            if (shader == null)
            {
                Debug.LogError($"[ArrowSwarm] SceneTransitionManager: Could not find shader '{shaderName}'!");
                return null;
            }

            var mat = new Material(shader)
            {
                name = $"Mat_Runtime_{style}"
            };

            UpdateMaterialParameters(mat);
            _materialCache[style] = mat;
            return mat;
        }

        private void UpdateMaterialParameters(Material mat)
        {
            if (mat == null) return;
            if (mat.HasProperty(PropColor)) mat.SetColor(PropColor, _backgroundColor);
            if (mat.HasProperty(PropBorderColor)) mat.SetColor(PropBorderColor, _neonGlowColor);
            if (mat.HasProperty(PropSecondaryGlow)) mat.SetColor(PropSecondaryGlow, _secondaryGlowColor);
        }

        private void ApplyActiveMaterial()
        {
            Material mat = GetOrCreateMaterial(_activeStyle);
            if (mat != null && _overlayImage != null)
            {
                UpdateMaterialParameters(mat);
                mat.SetFloat(PropProgress, 0f);
                _overlayImage.material = mat;
                _overlayImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Loads a target scene using the currently configured transition style and timing.
        /// </summary>
        /// <param name="sceneName">Name of the target scene.</param>
        /// <param name="duration">Custom total duration (<=0 uses default 0.70s).</param>
        /// <param name="screenCenter">Optional focus center (0-1 UV).</param>
        public void LoadScene(string sceneName, float duration = -1f, Vector2? screenCenter = null)
        {
            float d = duration > 0f ? duration : _defaultDuration;
            Vector2 center = screenCenter ?? new Vector2(0.5f, 0.5f);

            if (_currentTransition != null) StopCoroutine(_currentTransition);
            _currentTransition = StartCoroutine(SceneTransitionRoutine(sceneName, d, center));
        }

        /// <summary>
        /// Executes a custom callback action at the transition midpoint (e.g. reload board or popup transition).
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

            Material mat = GetOrCreateMaterial(_activeStyle);
            float halfDuration = Mathf.Max(0.15f, duration * 0.5f);
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

            try
            {
                if (mat != null)
                {
                    UpdateMaterialParameters(mat);
                    mat.SetVector(PropCenter, new Vector4(center.x, center.y, 0f, 0f));
                    mat.SetFloat(PropAspectRatio, aspect);
                    mat.SetFloat(PropProgress, 0f);
                    _overlayImage.material = mat;
                }

                if (_overlayImage != null)
                {
                    _overlayImage.gameObject.SetActive(true);
                    _overlayImage.raycastTarget = true;
                }

                // 1. Close / Wipe In (0 -> 1) over halfDuration (0.35s)
                float startTime = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - startTime < halfDuration)
                {
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
                    mat?.SetFloat(PropProgress, t);
                    yield return null;
                }
                mat?.SetFloat(PropProgress, 1f);

                // 2. Load target scene while screen is fully covered
                Time.timeScale = 1f;
                Debug.Log($"[ArrowSwarm] SceneTransitionManager: Loading '{sceneName}' with {_activeStyle}...");
                SceneManager.LoadScene(sceneName);

                // Wait 1 frame for new scene Awake & Start to resolve
                yield return null;

                // Recompute aspect ratio in case screen geometry adjusted
                aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
                if (mat != null)
                {
                    mat.SetFloat(PropAspectRatio, aspect);
                    mat.SetVector(PropCenter, new Vector4(center.x, center.y, 0f, 0f));
                }

                // 3. Open / Wipe Out (1 -> 0) over halfDuration (0.35s)
                startTime = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - startTime < halfDuration)
                {
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    float t = Mathf.SmoothStep(1f, 0f, elapsed / halfDuration);
                    mat?.SetFloat(PropProgress, t);
                    yield return null;
                }
                mat?.SetFloat(PropProgress, 0f);
            }
            finally
            {
                // 4. Guaranteed Cleanup
                if (mat != null) mat.SetFloat(PropProgress, 0f);

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

            Material mat = GetOrCreateMaterial(_activeStyle);
            float halfDuration = Mathf.Max(0.15f, duration * 0.5f);
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

            try
            {
                if (mat != null)
                {
                    UpdateMaterialParameters(mat);
                    mat.SetVector(PropCenter, new Vector4(center.x, center.y, 0f, 0f));
                    mat.SetFloat(PropAspectRatio, aspect);
                    mat.SetFloat(PropProgress, 0f);
                    _overlayImage.material = mat;
                }

                if (_overlayImage != null)
                {
                    _overlayImage.gameObject.SetActive(true);
                    _overlayImage.raycastTarget = true;
                }

                float startTime = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - startTime < halfDuration)
                {
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / halfDuration);
                    mat?.SetFloat(PropProgress, t);
                    yield return null;
                }
                mat?.SetFloat(PropProgress, 1f);

                onMidpoint?.Invoke();
                yield return null;

                startTime = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - startTime < halfDuration)
                {
                    float elapsed = Time.realtimeSinceStartup - startTime;
                    float t = Mathf.SmoothStep(1f, 0f, elapsed / halfDuration);
                    mat?.SetFloat(PropProgress, t);
                    yield return null;
                }
                mat?.SetFloat(PropProgress, 0f);
            }
            finally
            {
                if (mat != null) mat.SetFloat(PropProgress, 0f);

                if (_overlayImage != null)
                {
                    _overlayImage.raycastTarget = false;
                    _overlayImage.gameObject.SetActive(false);
                }

                _isTransitioning = false;
                _currentTransition = null;
            }
        }

        [ContextMenu("⚡ Test Transition Preview (Play Mode)")]
        private void TestTransitionContext()
        {
            if (Application.isPlaying)
            {
                PlayTransition(() => Debug.Log("[ArrowSwarm] SceneTransitionManager: Preview midpoint reached!"));
            }
            else
            {
                Debug.LogWarning("[ArrowSwarm] SceneTransitionManager: Please enter Play mode to preview transitions in real-time.");
            }
        }
    }
}
