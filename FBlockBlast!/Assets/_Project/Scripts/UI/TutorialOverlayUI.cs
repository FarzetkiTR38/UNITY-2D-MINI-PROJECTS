using UnityEngine;
using TMPro;
using System.Collections;

namespace NeonGalaxy.UI
{
    public class TutorialOverlayUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private CanvasGroup canvasGroup;

        private Coroutine _fadeCoroutine;
        private Coroutine _scaleCoroutine;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            
            // Initial state
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        public void SetStep(int step)
        {
            string text = "";
            switch (step)
            {
                case 0:
                    text = "Hoş geldin! Önce yatay bloğu sürükle ve satırı tamamla.";
                    break;
                case 1:
                    text = "Harika! Şimdi dikey bloğu sürükle ve sütunu tamamla.";
                    break;
                case 2:
                    text = "Mükemmel! Şimdi tekli bloğu köşeye yerleştir ve NOVA CROSS yap!";
                    break;
            }

            if (instructionText != null)
            {
                instructionText.text = text;
            }

            if (canvasGroup != null)
            {
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                canvasGroup.alpha = 0f;
                _fadeCoroutine = StartCoroutine(FadeRoutine(1f, 0.5f, 0f));
            }

            if (instructionText != null)
            {
                if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
                _scaleCoroutine = StartCoroutine(ScaleRoutine(instructionText.transform, 0.8f, 1f, 0.5f));
            }
        }

        public void ShowCompletion()
        {
            if (instructionText != null)
            {
                instructionText.text = "Tebrikler! Öğreticiyi tamamladın. \nİyi oyunlar!";
                if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
                _scaleCoroutine = StartCoroutine(ScaleRoutine(instructionText.transform, 0.8f, 1.2f, 0.5f));
            }

            if (canvasGroup != null)
            {
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeRoutine(0f, 1f, 2f));
            }
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, float delay)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        private IEnumerator ScaleRoutine(Transform target, float startScale, float endScale, float duration)
        {
            target.localScale = Vector3.one * startScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // Simple ease-out
                float t = elapsed / duration;
                t = 1f - (1f - t) * (1f - t);
                target.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);
                yield return null;
            }

            target.localScale = Vector3.one * endScale;
        }
    }
}
