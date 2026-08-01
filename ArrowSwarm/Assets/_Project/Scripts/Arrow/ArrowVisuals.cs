namespace ArrowSwarm.Arrow
{
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>
    /// Manages the visual representation of an arrow:
    /// color based on weight, size scaling, idle pulse animation,
    /// fire effect, blocked effect, and rainbow mode.
    /// </summary>
    [RequireComponent(typeof(Arrow))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ArrowVisuals : MonoBehaviour
    {
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseAmount = 0.05f;
        [SerializeField] private float _baseSizeMin = 0.6f;
        [SerializeField] private float _baseSizeMax = 1.2f;

        private SpriteRenderer _spriteRenderer;
        private Arrow _arrow;
        private Vector3 _baseScale;
        private float _pulseTimer;
        private bool _isPulsing;
        private bool _isRainbow;

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
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _arrow = GetComponent<Arrow>();
        }

        /// <summary>
        /// Sets up arrow visuals based on direction, weight, and rainbow state.
        /// </summary>
        public void SetupVisuals(ArrowDirection direction, int weight, bool isRainbow)
        {
            _isRainbow = isRainbow;

            // Rotation based on direction
            float zRotation = direction switch
            {
                ArrowDirection.Up => 0f,
                ArrowDirection.Right => -90f,
                ArrowDirection.Down => 180f,
                ArrowDirection.Left => 90f,
                _ => 0f
            };
            transform.rotation = Quaternion.Euler(0, 0, zRotation);

            // Size based on weight (1-10 mapped to baseSizeMin-baseSizeMax)
            float size = Mathf.Lerp(_baseSizeMin, _baseSizeMax, (weight - 1) / 9f);
            _baseScale = Vector3.one * size;
            transform.localScale = _baseScale;

            // Color based on weight or rainbow
            if (isRainbow)
            {
                _spriteRenderer.color = RainbowColors[0];
            }
            else
            {
                Color color = GameManager.Instance?.Config?.GetArrowColor(weight) ?? Color.white;
                _spriteRenderer.color = color;
            }

            _isPulsing = true;
            _pulseTimer = Random.Range(0f, Mathf.PI * 2f); // Random phase offset
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
            _pulseTimer += Time.deltaTime * _pulseSpeed;
            float scale = 1f + Mathf.Sin(_pulseTimer) * _pulseAmount;
            transform.localScale = _baseScale * scale;
        }

        private void UpdateRainbowColor()
        {
            float t = (Time.time * 2f) % RainbowColors.Length;
            int index = Mathf.FloorToInt(t);
            int nextIndex = (index + 1) % RainbowColors.Length;
            float lerp = t - index;
            _spriteRenderer.color = Color.Lerp(RainbowColors[index], RainbowColors[nextIndex], lerp);
        }

        /// <summary>
        /// Plays the fire visual effect (stops pulse, could add trail).
        /// </summary>
        public void PlayFireEffect()
        {
            _isPulsing = false;
            // Trail and particle effects will be added in Phase 8
        }

        /// <summary>
        /// Plays the blocked/error visual effect (red flash).
        /// </summary>
        public void PlayBlockedEffect()
        {
            // Flash red briefly then return to normal color
            StartCoroutine(FlashColor(Color.red, 0.2f));
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
            _isPulsing = false;
            _isRainbow = false;
            _pulseTimer = 0f;
            _pulseAmount = 0.05f;
            _pulseSpeed = 2f;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
            }
        }

        private System.Collections.IEnumerator FlashColor(Color flashColor, float duration)
        {
            Color originalColor = _spriteRenderer.color;
            _spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            if (!_arrow.IsFired)
            {
                _spriteRenderer.color = originalColor;
            }
        }
    }
}
