using System.Collections;
using UnityEngine;
using TMPro;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Visual floating score popup that appears in world space at piece placement positions.
    /// Animates upward, fades out, and destroys itself.
    /// Uses TextMeshPro (world-space variant) for direct 2D rendering.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class ScorePopupView : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float floatDuration = 0.8f;
        [SerializeField] private float moveSpeed = 1.8f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.5f);

        private TextMeshPro _textMesh;

        private void Awake()
        {
            _textMesh = GetComponent<TextMeshPro>();
        }

        /// <summary>
        /// Populates the popup points and begins the float/fade sequence.
        /// </summary>
        public void Setup(int score, Vector3 startPos)
        {
            transform.position = startPos;
            
            if (_textMesh != null)
            {
                _textMesh.text = $"+{score}";
                // Default sorting order layer on top of standard sprites
                _textMesh.sortingOrder = 20;

                // Randomize horizontal drift slightly for natural organic layout feel
                float horizontalDrift = Random.Range(-0.35f, 0.35f);
                Vector3 driftDirection = new Vector3(horizontalDrift, 1f, 0f).normalized;
                
                StartCoroutine(FloatAndFadeRoutine(driftDirection));
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator FloatAndFadeRoutine(Vector3 direction)
        {
            float elapsed = 0f;
            Color baseColor = _textMesh.color;
            Vector3 originalScale = transform.localScale;

            while (elapsed < floatDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / floatDuration;

                // Move in drift direction
                transform.position += direction * (moveSpeed * Time.deltaTime);

                // Scale multiplier
                float scaleMultiplier = scaleCurve != null ? scaleCurve.Evaluate(t) : (1f - t);
                transform.localScale = originalScale * scaleMultiplier;

                // Fade alpha value
                _textMesh.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
