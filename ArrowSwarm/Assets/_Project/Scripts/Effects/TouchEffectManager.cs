namespace ArrowSwarm.Effects
{
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Utils;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    /// <summary>
    /// Manages modern concentric touch indicator effects across all screen interactions.
    /// Features a fixed semi-transparent outer blue circle and an opaque solid inner blue circle
    /// that shrinks inward into itself on tap/click with zero GC allocations.
    /// </summary>
    public class TouchEffectManager : Singleton<TouchEffectManager>
    {
        [Header("Visual Settings")]
        [SerializeField] private Color _outerCircleColor = new Color(0.20f, 0.72f, 1.0f, 0.35f); // Semi-transparent blue
        [SerializeField] private Color _innerCircleColor = new Color(0.10f, 0.62f, 1.0f, 1.0f);  // Solid opaque blue
        [SerializeField] private float _baseRadius = 20f;                                       // Outer circle radius (~13px diameter)
        [SerializeField] private float _innerRadiusRatio = 0.5f;                                // Inner circle radius ratio (delicate center dot)
        [SerializeField] private float _animationDuration = 0.2f;                               // Super snappy 140ms duration
        [SerializeField] private int _poolSize = 12;

        private Canvas _touchCanvas;
        private RectTransform _canvasRect;
        private Sprite _circleSprite;

        private readonly List<ConcentricTouchInstance> _pool = new List<ConcentricTouchInstance>();

        /// <summary>Base radius in pixels for the outer circle.</summary>
        public float BaseRadius
        {
            get => _baseRadius;
            set => _baseRadius = value;
        }

        /// <summary>Total duration of the touch animation.</summary>
        public float AnimationDuration
        {
            get => _animationDuration;
            set => _animationDuration = value;
        }

        private class ConcentricTouchInstance
        {
            public GameObject RootObj;
            public RectTransform RootRect;
            public Image OuterCircleImage;
            public Image InnerCircleImage;
            public RectTransform OuterCircleRect;
            public RectTransform InnerCircleRect;
            public Coroutine ActiveCoroutine;
        }

        protected override void OnSingletonAwake()
        {
            InitializeCanvasAndSprites();
            InitializePool();
        }

        private void Start()
        {
            if (_pool.Count == 0)
            {
                InitializeCanvasAndSprites();
                InitializePool();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_pool != null && _pool.Count > 0)
            {
                InitializePool();
            }
        }
#endif

        private void Update()
        {
            // Check if VFX is disabled in user settings
            if (Data.DataManager.Instance != null &&
                Data.DataManager.Instance.PlayerData != null &&
                !Data.DataManager.Instance.PlayerData.vfxEnabled)
            {
                return;
            }

            // Mouse click detection
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                SpawnTouchEffect(mouse.position.ReadValue());
                return;
            }

            // Touchscreen tap detection
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.touches.Count > 0)
            {
                for (int i = 0; i < touchscreen.touches.Count; i++)
                {
                    var touch = touchscreen.touches[i];
                    if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        SpawnTouchEffect(touch.position.ReadValue());
                    }
                }
            }
        }

        /// <summary>
        /// Spawns a concentric touch effect (fixed semi-transparent outer + shrinking solid inner) at screen position.
        /// </summary>
        /// <param name="screenPosition">Screen coordinates of the touch/click event.</param>
        public void SpawnTouchEffect(Vector2 screenPosition)
        {
            if (_canvasRect == null) return;

            ConcentricTouchInstance instance = GetAvailableInstance();
            if (instance == null) return;

            // Dynamically update sizes from live _baseRadius
            float outerDiameter = _baseRadius * 2f;
            float innerDiameter = outerDiameter * _innerRadiusRatio;

            instance.RootRect.sizeDelta = new Vector2(outerDiameter, outerDiameter);
            instance.OuterCircleRect.sizeDelta = new Vector2(outerDiameter, outerDiameter);
            instance.InnerCircleRect.sizeDelta = new Vector2(innerDiameter, innerDiameter);

            // Position ripple on screen-space canvas
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPosition, null, out localPoint);

            instance.RootRect.anchoredPosition = localPoint;
            instance.RootObj.SetActive(true);

            if (instance.ActiveCoroutine != null)
            {
                StopCoroutine(instance.ActiveCoroutine);
            }

            instance.ActiveCoroutine = StartCoroutine(AnimateConcentricShrinkRoutine(instance));
        }

        private IEnumerator AnimateConcentricShrinkRoutine(ConcentricTouchInstance instance)
        {
            float elapsed = 0f;

            // Initial state: outer circle fixed, inner circle full size
            instance.OuterCircleRect.localScale = Vector3.one;
            instance.OuterCircleImage.color = _outerCircleColor;

            instance.InnerCircleRect.localScale = Vector3.one;
            instance.InnerCircleImage.color = _innerCircleColor;

            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _animationDuration);

                // Smooth ease-in-out shrink: inner circle collapses into its center (1 -> 0)
                float shrink = 1f - (t * t * (3f - 2f * t)); // Smoothstep curve
                instance.InnerCircleRect.localScale = Vector3.one * Mathf.Max(0f, shrink);

                // Outer circle remains fixed size throughout, then fades out at the very end as inner disappears
                float outerAlpha = (t > 0.85f)
                    ? Mathf.Lerp(_outerCircleColor.a, 0f, (t - 0.85f) / 0.15f)
                    : _outerCircleColor.a;
                instance.OuterCircleImage.color = new Color(_outerCircleColor.r, _outerCircleColor.g, _outerCircleColor.b, outerAlpha);

                yield return null;
            }

            instance.RootObj.SetActive(false);
            instance.ActiveCoroutine = null;
        }

        private ConcentricTouchInstance GetAvailableInstance()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].RootObj.activeSelf)
                {
                    return _pool[i];
                }
            }

            // If all active, reuse the first one
            if (_pool.Count > 0)
            {
                return _pool[0];
            }

            return null;
        }

        private void InitializeCanvasAndSprites()
        {
            if (_touchCanvas == null)
            {
                var existingObj = GameObject.Find("Canvas_TouchEffects");
                if (existingObj != null)
                {
                    _touchCanvas = existingObj.GetComponent<Canvas>();
                    _canvasRect = existingObj.GetComponent<RectTransform>();
                }
                else
                {
                    var canvasObj = new GameObject("Canvas_TouchEffects");
                    canvasObj.transform.SetParent(transform, false);

                    _touchCanvas = canvasObj.AddComponent<Canvas>();
                    _touchCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    _touchCanvas.sortingOrder = 999; // Top-most overlay

                    var scaler = canvasObj.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.matchWidthOrHeight = 0.5f;

                    _canvasRect = canvasObj.GetComponent<RectTransform>();
                }
            }

            if (_circleSprite == null)
            {
                _circleSprite = GenerateCircleSprite(128);
            }
        }

        private void InitializePool()
        {
            if (_touchCanvas == null) return;

            // Clear any old child objects under touch canvas
            for (int i = _touchCanvas.transform.childCount - 1; i >= 0; i--)
            {
                var child = _touchCanvas.transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
            _pool.Clear();

            float outerDiameter = _baseRadius * 2f;
            float innerDiameter = outerDiameter * _innerRadiusRatio;

            for (int i = 0; i < _poolSize; i++)
            {
                var root = new GameObject($"ConcentricTouch_{i}");
                root.transform.SetParent(_touchCanvas.transform, false);
                var rootRect = root.AddComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(outerDiameter, outerDiameter);

                // Layer 1: Outer Semi-Transparent Blue Circle (Fixed Size)
                var outerObj = new GameObject("OuterCircle");
                outerObj.transform.SetParent(root.transform, false);
                var outerRect = outerObj.AddComponent<RectTransform>();
                outerRect.sizeDelta = new Vector2(outerDiameter, outerDiameter);
                var outerImg = outerObj.AddComponent<Image>();
                outerImg.sprite = _circleSprite;
                outerImg.color = _outerCircleColor;
                outerImg.raycastTarget = false;

                // Layer 2: Inner Solid Opaque Blue Circle (Shrinks inward)
                var innerObj = new GameObject("InnerCircle");
                innerObj.transform.SetParent(root.transform, false);
                var innerRect = innerObj.AddComponent<RectTransform>();
                innerRect.sizeDelta = new Vector2(innerDiameter, innerDiameter);
                var innerImg = innerObj.AddComponent<Image>();
                innerImg.sprite = _circleSprite;
                innerImg.color = _innerCircleColor;
                innerImg.raycastTarget = false;

                root.SetActive(false);

                _pool.Add(new ConcentricTouchInstance
                {
                    RootObj = root,
                    RootRect = rootRect,
                    OuterCircleImage = outerImg,
                    OuterCircleRect = outerRect,
                    InnerCircleImage = innerImg,
                    InnerCircleRect = innerRect
                });
            }
        }

        /// <summary>
        /// Procedurally generates a smooth, anti-aliased solid circle sprite.
        /// </summary>
        private static Sprite GenerateCircleSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = (size - 1) * 0.5f;
            float radius = center - 1.2f;

            Color[] colors = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
