namespace ArrowSwarm.Effects
{
    using System;
    using System.Collections;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Represents an individual pooled floating text item that animates upward,
    /// punches scale, and fades out cleanly with zero garbage collection allocations.
    /// </summary>
    public class FloatingTextItem : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _duration = 0.65f;
        [SerializeField] private float _floatDistance = 0.85f;

        private Action<FloatingTextItem> _returnToPool;
        private Coroutine _animRoutine;

        private void Awake()
        {
            if (_text == null)
            {
                _text = GetComponent<TextMeshPro>() ?? gameObject.AddComponent<TextMeshPro>();
                _text.alignment = TextAlignmentOptions.Center;
                _text.fontSize = 4.5f;
                _text.fontStyle = FontStyles.Bold;
                _text.sortingOrder = 30; // Render above mobs (20) and HP text (25)
            }
        }

        /// <summary>
        /// Initializes and starts the floating text animation.
        /// </summary>
        public void Spawn(string content, Vector3 startPos, Color color, Action<FloatingTextItem> returnCallback)
        {
            if (_text == null) Awake();

            _returnToPool = returnCallback;
            transform.position = startPos;
            transform.localScale = Vector3.one;

            _text.text = content;
            _text.color = color;

            gameObject.SetActive(true);

            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _animRoutine = StartCoroutine(AnimateRoutine(startPos, color));
        }

        private IEnumerator AnimateRoutine(Vector3 startPos, Color baseColor)
        {
            float elapsed = 0f;
            float randomX = UnityEngine.Random.Range(-0.35f, 0.35f);
            Vector3 targetPos = startPos + new Vector3(randomX, _floatDistance, 0f);

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);

                // Smooth upward float with gentle deceleration
                float moveProgress = Mathf.Sin(t * Mathf.PI * 0.5f);
                transform.position = Vector3.Lerp(startPos, targetPos, moveProgress);

                // Punch scale: snap to 1.35x then ease to 1.0x
                float scaleMult = t < 0.25f
                    ? Mathf.Lerp(0.7f, 1.35f, t / 0.25f)
                    : Mathf.Lerp(1.35f, 0.95f, (t - 0.25f) / 0.75f);
                transform.localScale = Vector3.one * scaleMult;

                // Alpha fade out during second half
                float alpha = t < 0.45f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.45f) / 0.55f);
                _text.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                yield return null;
            }

            gameObject.SetActive(false);
            _animRoutine = null;
            _returnToPool?.Invoke(this);
        }
    }
}
