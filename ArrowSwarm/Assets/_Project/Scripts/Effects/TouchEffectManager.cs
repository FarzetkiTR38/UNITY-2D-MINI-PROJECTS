namespace ArrowSwarm.Effects
{
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Utils;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    /// <summary>
    /// Manages modern, responsive touch ripple effects across all screen interactions.
    /// Spawns a glowing blue double-wave ripple on tap/click with zero GC allocations.
    /// </summary>
    public class TouchEffectManager : Singleton<TouchEffectManager>
    {
        [Header("Visual Settings")]
        [SerializeField] private Color _innerDotColor = new Color(0.40f, 0.78f, 1.0f, 0.85f);
        [SerializeField] private Color _outerWaveColor = new Color(0.20f, 0.85f, 1.0f, 0.80f);
        [SerializeField] private float _baseRadius = 16f;                                  // Compact touch radius (32px diameter)
        [SerializeField] private float _animationDuration = 0.28f;                         // Snappy 280ms duration
        [SerializeField] private int _poolSize = 12;

        private Canvas _touchCanvas;
        private RectTransform _canvasRect;
        private Sprite _circleSprite;
        private Sprite _ringSprite;

        private readonly List<RippleInstance> _pool = new List<RippleInstance>();

        /// <summary>Base radius in pixels for the touch ripple effect.</summary>
        public float BaseRadius
        {
            get => _baseRadius;
            set => _baseRadius = value;
        }

        /// <summary>Total duration of the ripple animation.</summary>
        public float AnimationDuration
        {
            get => _animationDuration;
            set => _animationDuration = value;
        }

        private class RippleInstance
        {
            public GameObject RootObj;
            public RectTransform RootRect;
            public Image InnerDotImage;
            public Image OuterWaveImage;
            public RectTransform InnerDotRect;
            public RectTransform OuterWaveRect;
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
        /// Spawns a glowing touch ripple wave at the specified screen position.
        /// </summary>
        /// <param name="screenPosition">Screen coordinates of the touch/click event.</param>
        public void SpawnTouchEffect(Vector2 screenPosition)
        {
            if (_canvasRect == null) return;

            RippleInstance ripple = GetAvailableRipple();
            if (ripple == null) return;

            // Dynamically update sizes from live _baseRadius
            float diameter = _baseRadius * 2f;
            ripple.RootRect.sizeDelta = new Vector2(diameter, diameter);
            ripple.OuterWaveRect.sizeDelta = new Vector2(diameter, diameter);
            ripple.InnerDotRect.sizeDelta = new Vector2(diameter * 0.5f, diameter * 0.5f);

            // Position ripple on screen-space canvas
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPosition, null, out localPoint);

            ripple.RootRect.anchoredPosition = localPoint;
            ripple.RootObj.SetActive(true);

            if (ripple.ActiveCoroutine != null)
            {
                StopCoroutine(ripple.ActiveCoroutine);
            }

            ripple.ActiveCoroutine = StartCoroutine(AnimateRippleRoutine(ripple));
        }

        private IEnumerator AnimateRippleRoutine(RippleInstance ripple)
        {
            float elapsed = 0f;

            // Initial setup
            ripple.InnerDotRect.localScale = Vector3.one * 0.25f;
            ripple.OuterWaveRect.localScale = Vector3.one * 0.20f;

            ripple.InnerDotImage.color = _innerDotColor;
            ripple.OuterWaveImage.color = _outerWaveColor;

            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _animationDuration);

                // Smooth cubic ease-out
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);
                float easeOutFast = 1f - Mathf.Pow(1f - Mathf.Clamp01(t * 1.8f), 2f);

                // Outer wave expands slightly and fades out
                float waveScale = Mathf.Lerp(0.20f, 1.05f, easeOut);
                float waveAlpha = Mathf.Lerp(_outerWaveColor.a, 0f, easeOut);
                ripple.OuterWaveRect.localScale = Vector3.one * waveScale;
                ripple.OuterWaveImage.color = new Color(_outerWaveColor.r, _outerWaveColor.g, _outerWaveColor.b, waveAlpha);

                // Inner dot expands slightly and fades fast
                float dotScale = Mathf.Lerp(0.25f, 0.70f, easeOutFast);
                float dotAlpha = Mathf.Lerp(_innerDotColor.a, 0f, easeOutFast);
                ripple.InnerDotRect.localScale = Vector3.one * dotScale;
                ripple.InnerDotImage.color = new Color(_innerDotColor.r, _innerDotColor.g, _innerDotColor.b, dotAlpha);

                yield return null;
            }

            ripple.RootObj.SetActive(false);
            ripple.ActiveCoroutine = null;
        }

        private RippleInstance GetAvailableRipple()
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
            if (_ringSprite == null)
            {
                _ringSprite = GenerateRingSprite(128, 0.76f);
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

            float diameter = _baseRadius * 2f;

            for (int i = 0; i < _poolSize; i++)
            {
                var root = new GameObject($"TouchRipple_{i}");
                root.transform.SetParent(_touchCanvas.transform, false);
                var rootRect = root.AddComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(diameter, diameter);

                // Outer wave ring
                var waveObj = new GameObject("OuterWave");
                waveObj.transform.SetParent(root.transform, false);
                var waveRect = waveObj.AddComponent<RectTransform>();
                waveRect.sizeDelta = new Vector2(diameter, diameter);
                var waveImg = waveObj.AddComponent<Image>();
                waveImg.sprite = _ringSprite;
                waveImg.color = _outerWaveColor;
                waveImg.raycastTarget = false;

                // Inner dot
                var dotObj = new GameObject("InnerDot");
                dotObj.transform.SetParent(root.transform, false);
                var dotRect = dotObj.AddComponent<RectTransform>();
                dotRect.sizeDelta = new Vector2(diameter * 0.5f, diameter * 0.5f);
                var dotImg = dotObj.AddComponent<Image>();
                dotImg.sprite = _circleSprite;
                dotImg.color = _innerDotColor;
                dotImg.raycastTarget = false;

                root.SetActive(false);

                _pool.Add(new RippleInstance
                {
                    RootObj = root,
                    RootRect = rootRect,
                    OuterWaveImage = waveImg,
                    OuterWaveRect = waveRect,
                    InnerDotImage = dotImg,
                    InnerDotRect = dotRect
                });
            }
        }

        /// <summary>
        /// Procedurally generates a smooth, anti-aliased filled circle sprite.
        /// </summary>
        private static Sprite GenerateCircleSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = (size - 1) * 0.5f;
            float radius = center - 1.5f;

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

        /// <summary>
        /// Procedurally generates a smooth, anti-aliased hollow ring sprite.
        /// </summary>
        private static Sprite GenerateRingSprite(int size, float innerRatio)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = (size - 1) * 0.5f;
            float outerRadius = center - 1.5f;
            float innerRadius = outerRadius * innerRatio;

            Color[] colors = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float outerAlpha = Mathf.Clamp01(outerRadius - dist + 0.5f);
                    float innerAlpha = Mathf.Clamp01(dist - innerRadius + 0.5f);
                    float alpha = Mathf.Min(outerAlpha, innerAlpha);
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
