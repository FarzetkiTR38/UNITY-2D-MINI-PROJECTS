using System;
using System.Collections;
using UnityEngine;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Visual representation of an individual grid cell.
    /// Handles visual state changes (empty, occupied, preview/ghost) and clear animations.
    /// Works with World-Space SpriteRenderers.
    /// </summary>
    public class CellView : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private SpriteRenderer blockRenderer;

        [Header("Animations")]
        [SerializeField] private float punchScale = 1.25f;
        [SerializeField] private float punchDuration = 0.1f;
        [SerializeField] private float fadeDuration = 0.2f;

        private Vector3 _originalScale;
        private Color _originalBlockColor;
        private Coroutine _activeAnimation;

        private void Awake()
        {
            _originalScale = transform.localScale;
            if (blockRenderer != null)
            {
                _originalBlockColor = blockRenderer.color;
                blockRenderer.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the cell to the occupied state with a specific sprite and tint color.
        /// </summary>
        public void SetOccupied(Sprite sprite, Color tintColor)
        {
            StopActiveAnimation();
            transform.localScale = _originalScale;

            if (blockRenderer != null)
            {
                blockRenderer.gameObject.SetActive(true);
                if (sprite != null) blockRenderer.sprite = sprite;
                blockRenderer.color = new Color(tintColor.r, tintColor.g, tintColor.b, 1.0f);
            }
        }

        /// <summary>
        /// Sets the cell to the empty state.
        /// </summary>
        public void SetEmpty()
        {
            StopActiveAnimation();
            transform.localScale = _originalScale;

            if (blockRenderer != null)
            {
                blockRenderer.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the cell to a semi-transparent ghost preview state.
        /// </summary>
        public void SetGhostPreview(Sprite sprite, Color color, bool isValid)
        {
            StopActiveAnimation();
            transform.localScale = _originalScale;

            if (blockRenderer != null)
            {
                blockRenderer.gameObject.SetActive(true);
                if (sprite != null) blockRenderer.sprite = sprite;
                // 50% opacity for preview. If valid, tint green or keep color, otherwise maybe apply error hue.
                float alpha = 0.5f;
                Color previewColor = isValid ? color : Color.red;
                blockRenderer.color = new Color(previewColor.r, previewColor.g, previewColor.b, alpha);
            }
        }

        /// <summary>
        /// Plays the line clear cell animation (scale punch + fade out) and notifies on completion.
        /// </summary>
        public void PlayClearAnimation(float delay, Action onComplete)
        {
            StopActiveAnimation();
            _activeAnimation = StartCoroutine(ClearRoutine(delay, onComplete));
        }

        private IEnumerator ClearRoutine(float delay, Action onComplete)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            if (blockRenderer == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            Color baseColor = blockRenderer.color;

            // Phase 1: Scale Punch Up
            float elapsed = 0f;
            while (elapsed < punchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / punchDuration;
                transform.localScale = Vector3.Lerp(_originalScale, _originalScale * punchScale, t);
                yield return null;
            }
            transform.localScale = _originalScale * punchScale;

            // Fire per-cell VFX event at the peak of the punch (most impactful moment)
            GameEvents.InvokeCellClearing(transform.position, baseColor);

            // Phase 2: Shrink and Fade Out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                transform.localScale = Vector3.Lerp(_originalScale * punchScale, Vector3.zero, t);
                blockRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(1f, 0f, t));
                yield return null;
            }

            // Cleanup & Reset visual state
            SetEmpty();
            onComplete?.Invoke();
            _activeAnimation = null;
        }

        private void StopActiveAnimation()
        {
            if (_activeAnimation != null)
            {
                StopCoroutine(_activeAnimation);
                _activeAnimation = null;
            }
        }

        private void OnDisable()
        {
            StopActiveAnimation();
        }
    }
}
