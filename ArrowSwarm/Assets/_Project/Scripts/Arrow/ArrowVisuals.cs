namespace ArrowSwarm.Arrow
{
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages the visual representation of a multi-point arrow:
    /// draws a LineRenderer through path points, places an arrowhead sprite
    /// at the head point, handles color based on weight, rainbow mode,
    /// pulse animation, fire/blocked effects.
    /// </summary>
    [RequireComponent(typeof(Arrow))]
    public class ArrowVisuals : MonoBehaviour
    {
        [SerializeField] private float _lineWidth = 0.08f;
        [SerializeField] private float _headSize = 0.2f;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseAmount = 0.05f;

        private Arrow _arrow;
        private LineRenderer _lineRenderer;
        private SpriteRenderer _headRenderer;
        private Transform _headTransform;
        private BoxCollider2D _boxCollider;
        private TrailRenderer _trailRenderer;
        private float _pulseTimer;
        private bool _isPulsing;
        private bool _isRainbow;
        private bool _isSpawning;
        private Coroutine _spawnCoroutine;
        private readonly List<Vector3> _spawnPointsBuffer = new List<Vector3>();
        private float _baseLineWidth;
        private Vector3 _baseHeadScale;

        /// <summary>Whether this arrow is currently playing its birth/spawn animation.</summary>
        public bool IsSpawning => _isSpawning;

        // Cached rainbow colors
        private static readonly Color[] RainbowColors = new Color[]
        {
            new Color(0.39f, 0.71f, 0.96f, 1f), // Mavi
            new Color(0.51f, 0.78f, 0.52f, 1f), // Yeşil
            new Color(1.00f, 0.72f, 0.30f, 1f), // Turuncu
            new Color(0.73f, 0.41f, 0.78f, 1f), // Mor
            new Color(0.94f, 0.38f, 0.57f, 1f), // Pembe
        };

        private void Awake()
        {
            _arrow = GetComponent<Arrow>();
            
            // Hide the old big arrow sprite from the prefab
            var oldSprite = GetComponent<SpriteRenderer>();
            if (oldSprite != null) oldSprite.enabled = false;
            
            // Ensure we have a BoxCollider2D for accurate path click detection
            _boxCollider = GetComponent<BoxCollider2D>();
            if (_boxCollider == null)
            {
                _boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            
            EnsureLineRenderer();
            EnsureHeadSprite();
            EnsureTrailRenderer();
        }

        /// <summary>
        /// Sets up arrow visuals: draws path with LineRenderer and positions arrowhead.
        /// Supports progressive spawn animation where the head pops in first, followed by body growth.
        /// </summary>
        public void SetupVisuals(Arrow arrow, float spawnDelay = 0f, bool animateSpawn = true)
        {
            _arrow = arrow;
            _isRainbow = arrow.IsRainbow;
            EnsureLineRenderer();
            EnsureHeadSprite();
            EnsureTrailRenderer();

            // Get world positions for all path points
            GridManager grid = GridManager.Instance;
            IReadOnlyList<Vector2Int> pathPoints = arrow.PathPoints;
            float spacing = grid.PointSpacing;
            Vector2 origin = grid.Origin;

            // Position the arrow at the head point for click detection
            Vector2 headWorldPos = arrow.HeadPoint.PointToWorld(spacing, origin);
            transform.position = new Vector3(headWorldPos.x, headWorldPos.y, 0f);

            // Add BoxCollider2D based on path
            if (_boxCollider == null) _boxCollider = gameObject.AddComponent<BoxCollider2D>();

            if (pathPoints.Count == 1)
            {
                _boxCollider.size = new Vector2(grid.PointSpacing * 0.8f, grid.PointSpacing * 0.8f);
                _boxCollider.offset = Vector2.zero;
            }
            else
            {
                // Find min/max local points
                Vector2 min = Vector2.positiveInfinity;
                Vector2 max = Vector2.negativeInfinity;
                
                foreach (var p in pathPoints)
                {
                    Vector2 localPos = p.PointToWorld(spacing, origin) - (Vector2)transform.position;
                    min = Vector2.Min(min, localPos);
                    max = Vector2.Max(max, localPos);
                }
                
                Vector2 size = max - min;
                // Add padding so it has thickness
                size.x += grid.PointSpacing * 0.7f;
                size.y += grid.PointSpacing * 0.7f;
                
                _boxCollider.size = size;
                _boxCollider.offset = (min + max) / 2f;
            }

            // Calculate bold, juicy line width and arrowhead size proportional to grid spacing
            float dynamicLineWidth = Mathf.Clamp(spacing * 0.35f, 0.10f, 0.28f);
            float dynamicHeadSize = Mathf.Clamp(spacing * 0.75f, 0.20f, 0.58f);

            _baseLineWidth = dynamicLineWidth;
            _lineRenderer.startWidth = dynamicLineWidth;
            _lineRenderer.endWidth = dynamicLineWidth;

            // Ensure parent transform has zero rotation so arrowhead is strictly orthogonal
            transform.rotation = Quaternion.identity;

            // Setup arrowhead sprite rotation based on direction (strict 0, 90, 180, 270)
            float zRotation = arrow.HeadDirection switch
            {
                ArrowDirection.Up => 0f,
                ArrowDirection.Right => -90f,
                ArrowDirection.Down => 180f,
                ArrowDirection.Left => 90f,
                _ => 0f
            };
            _headTransform.position = new Vector3(headWorldPos.x, headWorldPos.y, -0.05f);
            _headTransform.localRotation = Quaternion.Euler(0, 0, zRotation);
            _baseHeadScale = Vector3.one * dynamicHeadSize;
            _headTransform.localScale = _baseHeadScale;

            // Random color from palette (independent of weight) or rainbow
            Color arrowColor;
            if (_isRainbow)
            {
                arrowColor = RainbowColors[0];
            }
            else
            {
                arrowColor = GameManager.Instance?.Config?.GetRandomArrowColor() ?? Color.white;
            }

            _lineRenderer.startColor = arrowColor;
            _lineRenderer.endColor = arrowColor;
            _headRenderer.color = arrowColor;

            _isPulsing = false; // Disable continuous idle pulsing for normal arrows
            _pulseTimer = Random.Range(0f, Mathf.PI * 2f);

            _lineRenderer.enabled = true;
            _headRenderer.enabled = true;
            
            if (_trailRenderer != null)
            {
                _trailRenderer.enabled = false; // Disabled until fired
                _trailRenderer.Clear();
            }

            if (animateSpawn && Application.isPlaying)
            {
                if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = StartCoroutine(AnimateSpawnRoutine(spawnDelay, 0.55f));
            }
            else
            {
                // Setup LineRenderer path immediately
                _lineRenderer.positionCount = pathPoints.Count;
                for (int i = 0; i < pathPoints.Count; i++)
                {
                    Vector2 worldPos = pathPoints[i].PointToWorld(spacing, origin);
                    _lineRenderer.SetPosition(i, new Vector3(worldPos.x, worldPos.y, 0f));
                }
            }
        }

        private System.Collections.IEnumerator AnimateSpawnRoutine(float delay, float duration)
        {
            _isSpawning = true;
            _lineRenderer.positionCount = 0;
            if (_headTransform != null)
            {
                _headTransform.localScale = Vector3.zero;
            }

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            GridManager grid = GridManager.Instance;
            IReadOnlyList<Vector2Int> pathPoints = _arrow != null ? _arrow.PathPoints : null;
            if (pathPoints == null || pathPoints.Count == 0)
            {
                _isSpawning = false;
                yield break;
            }

            float spacing = grid.PointSpacing;
            Vector2 origin = grid.Origin;
            int n = pathPoints.Count;

            Vector2[] worldPoints = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                worldPoints[i] = pathPoints[i].PointToWorld(spacing, origin);
            }

            float totalLen = 0f;
            for (int i = 0; i < n - 1; i++)
            {
                totalLen += Vector2.Distance(worldPoints[i], worldPoints[i + 1]);
            }

            // Phase 1: Head Pop in with overshoot bounce curve
            float popDur = 0.15f;
            float popElapsed = 0f;
            while (popElapsed < popDur)
            {
                popElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(popElapsed / popDur);
                float scaleMult = t < 0.7f ? (t / 0.7f) * 1.25f : 1.25f - ((t - 0.7f) / 0.3f) * 0.25f;
                if (_headTransform != null)
                {
                    _headTransform.localScale = _baseHeadScale * scaleMult;
                }
                yield return null;
            }
            if (_headTransform != null) _headTransform.localScale = _baseHeadScale;

            // Phase 2: Progressive Body Growth from Head (P0) to Tail (Pn-1)
            if (n > 1 && totalLen > 0.001f)
            {
                float growDur = Mathf.Max(0.15f, duration - popDur);
                float growElapsed = 0f;

                while (growElapsed < growDur)
                {
                    growElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(growElapsed / growDur);
                    float progress = Mathf.Sin(t * Mathf.PI * 0.5f); // Smooth ease out
                    float currentDist = progress * totalLen;

                    _spawnPointsBuffer.Clear();
                    _spawnPointsBuffer.Add(new Vector3(worldPoints[0].x, worldPoints[0].y, 0f));

                    float accumulated = 0f;
                    for (int i = 0; i < n - 1; i++)
                    {
                        float segLen = Vector2.Distance(worldPoints[i], worldPoints[i + 1]);
                        if (currentDist >= accumulated + segLen)
                        {
                            _spawnPointsBuffer.Add(new Vector3(worldPoints[i + 1].x, worldPoints[i + 1].y, 0f));
                            accumulated += segLen;
                        }
                        else
                        {
                            float remain = currentDist - accumulated;
                            float segT = segLen > 0.0001f ? remain / segLen : 0f;
                            Vector2 tip = Vector2.Lerp(worldPoints[i], worldPoints[i + 1], segT);
                            _spawnPointsBuffer.Add(new Vector3(tip.x, tip.y, 0f));
                            break;
                        }
                    }

                    if (_lineRenderer != null)
                    {
                        _lineRenderer.positionCount = _spawnPointsBuffer.Count;
                        _lineRenderer.SetPositions(_spawnPointsBuffer.ToArray());
                    }

                    yield return null;
                }
            }

            // Phase 3: Final exact resting state
            RestoreRestingVisuals();
            _isSpawning = false;
            _spawnCoroutine = null;
        }

        /// <summary>
        /// Instantly completes the spawn animation (e.g. on early player click).
        /// </summary>
        public void CompleteSpawnImmediately()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
            _isSpawning = false;
            RestoreRestingVisuals();
        }

        /// <summary>
        /// Enables or disables rainbow mode visuals.
        /// </summary>
        public void SetRainbowMode(bool rainbow)
        {
            _isRainbow = rainbow;
            _isPulsing = rainbow;

            if (rainbow)
            {
                _pulseSpeed = 4f;
                _pulseAmount = 0.1f;
            }
            else
            {
                if (_lineRenderer != null)
                {
                    _lineRenderer.startWidth = _baseLineWidth;
                    _lineRenderer.endWidth = _baseLineWidth;
                }
                if (_headTransform != null)
                {
                    _headTransform.localScale = _baseHeadScale;
                }
            }
        }

        private void Update()
        {
            if (_isPulsing)
            {
                UpdatePulse();
            }

            if (_isRainbow)
            {
                UpdateRainbowColor();
            }
        }

        private void UpdatePulse()
        {
            if (!_isPulsing) return;
            _pulseTimer += Time.deltaTime * _pulseSpeed;
            float scale = 1f + Mathf.Sin(_pulseTimer) * _pulseAmount;
            float width = _baseLineWidth * scale;
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;

            if (_headTransform != null)
            {
                _headTransform.localScale = _baseHeadScale * scale;
            }
        }

        private void UpdateRainbowColor()
        {
            float t = (Time.time * 2f) % RainbowColors.Length;
            int index = Mathf.FloorToInt(t);
            int nextIndex = (index + 1) % RainbowColors.Length;
            float lerp = t - index;
            Color color = Color.Lerp(RainbowColors[index], RainbowColors[nextIndex], lerp);

            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color.WithAlpha(0.6f);
            _headRenderer.color = color;
            
            if (_trailRenderer != null && _trailRenderer.enabled)
            {
                _trailRenderer.startColor = color;
                _trailRenderer.endColor = color.WithAlpha(0f);
            }
        }

        /// <summary>ArrowHead child transform.</summary>
        public Transform HeadTransform => _headTransform;

        /// <summary>
        /// Plays the fire visual effect (stops idle pulse, keeps line renderer active for slithering body).
        /// </summary>
        public void PlayFireEffect()
        {
            _isPulsing = false;
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.enabled = false; // Disabled in favor of full arrow body slither
                _trailRenderer.Clear();
            }
        }

        /// <summary>
        /// Updates the slithering body line and head position/rotation while the arrow is moving along the path.
        /// </summary>
        public void UpdateSlitheringBody(List<Vector3> bodyPoints, Vector3 headPos, Quaternion headRotation, bool headVisible)
        {
            if (_lineRenderer != null)
            {
                if (bodyPoints != null && bodyPoints.Count >= 2)
                {
                    _lineRenderer.enabled = true;
                    _lineRenderer.positionCount = bodyPoints.Count;
                    _lineRenderer.SetPositions(bodyPoints.ToArray());
                }
                else
                {
                    _lineRenderer.positionCount = 0;
                }
            }

            if (_headTransform != null)
            {
                _headTransform.position = headPos;
                _headTransform.rotation = headRotation;
                if (_headRenderer != null)
                {
                    _headRenderer.enabled = headVisible;
                }
            }
        }

        /// <summary>
        /// Plays the blocked/error visual effect (red flash).
        /// </summary>
        public void PlayBlockedEffect()
        {
            StartCoroutine(FlashColor(new Color(1f, 0.25f, 0.25f, 1f), 0.25f));
        }

        /// <summary>
        /// Restores resting position and line positions when a blocked arrow finishes its return bounce animation.
        /// </summary>
        public void RestoreRestingVisuals()
        {
            if (_arrow == null) return;

            GridManager grid = GridManager.Instance;
            IReadOnlyList<Vector2Int> pathPoints = _arrow.PathPoints;
            float spacing = grid.PointSpacing;
            Vector2 origin = grid.Origin;

            Vector2 headWorldPos = _arrow.HeadPoint.PointToWorld(spacing, origin);
            transform.position = new Vector3(headWorldPos.x, headWorldPos.y, 0f);
            transform.rotation = Quaternion.identity;

            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = pathPoints.Count;
                for (int i = 0; i < pathPoints.Count; i++)
                {
                    Vector2 worldPos = pathPoints[i].PointToWorld(spacing, origin);
                    _lineRenderer.SetPosition(i, new Vector3(worldPos.x, worldPos.y, 0f));
                }
            }

            if (_headTransform != null)
            {
                float zRotation = _arrow.HeadDirection switch
                {
                    ArrowDirection.Up => 0f,
                    ArrowDirection.Right => -90f,
                    ArrowDirection.Down => 180f,
                    ArrowDirection.Left => 90f,
                    _ => 0f
                };
                _headTransform.position = new Vector3(headWorldPos.x, headWorldPos.y, -0.05f);
                _headTransform.localRotation = Quaternion.Euler(0, 0, zRotation);
                _headTransform.localScale = _baseHeadScale != Vector3.zero ? _baseHeadScale : Vector3.one * _headSize;
                if (_headRenderer != null) _headRenderer.enabled = true;
            }

            // Restore BoxCollider to full path bounds
            if (_boxCollider != null && pathPoints.Count > 1)
            {
                Vector2 min = Vector2.positiveInfinity;
                Vector2 max = Vector2.negativeInfinity;
                foreach (var p in pathPoints)
                {
                    Vector2 localPos = p.PointToWorld(spacing, origin) - (Vector2)transform.position;
                    min = Vector2.Min(min, localPos);
                    max = Vector2.Max(max, localPos);
                }
                Vector2 size = max - min;
                size.x += spacing * 0.7f;
                size.y += spacing * 0.7f;
                _boxCollider.size = size;
                _boxCollider.offset = (min + max) / 2f;
            }
        }

        /// <summary>
        /// Plays a subtle bump reaction shake on the obstacle arrow that was hit.
        /// </summary>
        public void PlayBumpedReactionEffect(Vector2 impactDir)
        {
            StartCoroutine(BumpReactionRoutine(impactDir));
        }

        private System.Collections.IEnumerator BumpReactionRoutine(Vector2 impactDir)
        {
            Vector3 originalPos = transform.position;
            float elapsed = 0f;
            float dur = 0.12f;
            Vector2 normDir = impactDir.normalized;
            
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float offset = Mathf.Sin(t * Mathf.PI) * 0.08f;
                transform.position = originalPos + (Vector3)(normDir * offset);
                yield return null;
            }
            transform.position = originalPos;
        }

        /// <summary>
        /// Highlights this arrow (used by tip system).
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            if (highlighted)
            {
                _pulseAmount = 0.12f;
                _pulseSpeed = 4f;
            }
            else
            {
                _pulseAmount = 0.05f;
                _pulseSpeed = 2f;
            }
        }

        /// <summary>
        /// Resets visuals for pool reuse.
        /// </summary>
        public void ResetVisuals()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
            _isSpawning = false;
            _isPulsing = false;
            _isRainbow = false;
            _pulseTimer = 0f;
            _pulseAmount = 0.05f;
            _pulseSpeed = 2f;

            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
                _lineRenderer.enabled = false;
            }

            if (_headTransform != null)
            {
                _headTransform.localScale = Vector3.one * _headSize;
                _headTransform.rotation = Quaternion.identity;
            }

            if (_headRenderer != null)
            {
                _headRenderer.color = Color.white;
                _headRenderer.enabled = false;
            }
            
            if (_trailRenderer != null)
            {
                _trailRenderer.enabled = false;
                _trailRenderer.Clear();
            }
        }

        private void EnsureLineRenderer()
        {
            if (_lineRenderer != null) return;

            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            _lineRenderer.useWorldSpace = true;
            _lineRenderer.sortingOrder = 5;
            _lineRenderer.textureMode = LineTextureMode.Stretch;
            _lineRenderer.numCapVertices = 8;
            _lineRenderer.numCornerVertices = 8;

            // Use default sprite / URP 2D material with solid white texture via sharedMaterial
            if (_lineRenderer.sharedMaterial == null || _lineRenderer.sharedMaterial.name.Contains("Default") || _lineRenderer.sharedMaterial.mainTexture == null)
            {
                _lineRenderer.sharedMaterial = GetSharedLineMaterial();
            }
        }

        private static Material _sharedLineMaterial;

        private static Material GetSharedLineMaterial()
        {
            if (_sharedLineMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    _sharedLineMaterial = new Material(shader);
                    _sharedLineMaterial.mainTexture = Texture2D.whiteTexture;
                }
            }
            return _sharedLineMaterial;
        }

        private void EnsureHeadSprite()
        {
            if (_headTransform != null) return;

            // Look for existing child named "ArrowHead"
            _headTransform = transform.Find("ArrowHead");
            if (_headTransform == null)
            {
                var headObj = new GameObject("ArrowHead");
                headObj.transform.SetParent(transform, false);
                _headTransform = headObj.transform;
            }

            _headRenderer = _headTransform.GetComponent<SpriteRenderer>();
            if (_headRenderer == null)
            {
                _headRenderer = _headTransform.gameObject.AddComponent<SpriteRenderer>();
            }

            // Use existing sprite or create triangle
            if (_headRenderer.sprite == null)
            {
                _headRenderer.sprite = CreateArrowHeadSprite();
            }

            _headRenderer.sortingOrder = 6;
        }

        private void EnsureTrailRenderer()
        {
            if (_trailRenderer != null) return;

            _trailRenderer = GetComponent<TrailRenderer>();
            if (_trailRenderer == null)
            {
                _trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }

            _trailRenderer.time = 0.5f;
            _trailRenderer.startWidth = _lineWidth * 1.5f;
            _trailRenderer.endWidth = 0f;
            _trailRenderer.sortingOrder = 4;
            
            if (_trailRenderer.sharedMaterial == null || _trailRenderer.sharedMaterial.name.Contains("Default") || _trailRenderer.sharedMaterial.mainTexture == null)
            {
                _trailRenderer.sharedMaterial = GetSharedLineMaterial();
            }
            
            _trailRenderer.enabled = false;
        }

        /// <summary>
        /// Creates a simple triangle sprite for the arrow head.
        /// </summary>
        private static Sprite _cachedArrowHead;

        private static Sprite CreateArrowHeadSprite()
        {
            if (_cachedArrowHead != null) return _cachedArrowHead;

            int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            // Clear to transparent
            Color[] clear = new Color[size * size];
            for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
            texture.SetPixels(clear);

            // Draw a triangle pointing up
            for (int y = 0; y < size; y++)
            {
                float progress = (float)y / size;
                int halfWidth = Mathf.FloorToInt(size * 0.5f * (1f - progress));
                int center = size / 2;

                for (int x = center - halfWidth; x <= center + halfWidth; x++)
                {
                    if (x >= 0 && x < size)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            _cachedArrowHead = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            return _cachedArrowHead;
        }

        private System.Collections.IEnumerator FlashColor(Color flashColor, float duration)
        {
            Color originalLineStart = _lineRenderer.startColor;
            Color originalLineEnd = _lineRenderer.endColor;
            Color originalHead = _headRenderer.color;

            _lineRenderer.startColor = flashColor;
            _lineRenderer.endColor = flashColor;
            _headRenderer.color = flashColor;

            yield return new WaitForSeconds(duration);

            if (!_arrow.IsFired)
            {
                _lineRenderer.startColor = originalLineStart;
                _lineRenderer.endColor = originalLineEnd;
                _headRenderer.color = originalHead;
            }
        }
    }
}
