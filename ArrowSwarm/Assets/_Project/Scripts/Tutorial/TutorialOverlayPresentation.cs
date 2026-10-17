namespace ArrowSwarm.Tutorial
{
    using System.Collections;
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>Presentation animations and completion navigation for the tutorial overlay.</summary>
    public partial class TutorialOverlayUI
    {
        private IEnumerator PunchBannerRoutine()
        {
            if (_bannerRect == null) yield break;
            CacheOriginalScales();

            Vector3 baseScale = _originalBannerScale;
            Vector3 punchScale = baseScale * 1.08f;
            Vector3 startScale = baseScale * 0.92f;
            float elapsed = 0f;
            float duration = 0.28f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                if (progress < 0.45f)
                {
                    float t = Mathf.SmoothStep(0f, 1f, progress / 0.45f);
                    _bannerRect.localScale = Vector3.Lerp(startScale, punchScale, t);
                }
                else
                {
                    float t = Mathf.SmoothStep(0f, 1f, (progress - 0.45f) / 0.55f);
                    _bannerRect.localScale = Vector3.Lerp(punchScale, baseScale, t);
                }
                yield return null;
            }

            _bannerRect.localScale = baseScale;
            _bannerFadeRoutine = null;
        }

        private IEnumerator PopCompletionRoutine()
        {
            CacheOriginalScales();

            if (_completeGroup != null) _completeGroup.alpha = 0f;
            RectTransform cardRect = _completeCard != null ? _completeCard.transform as RectTransform : null;
            
            Vector3 targetScale = _originalCardScale;
            Vector3 startScale = targetScale * 0.7f;

            if (cardRect != null) cardRect.localScale = startScale;

            float elapsed = 0f;
            float duration = 0.35f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                if (_completeGroup != null) _completeGroup.alpha = t;
                if (cardRect != null) cardRect.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            if (_completeGroup != null)
            {
                _completeGroup.alpha = 1f;
                _completeGroup.interactable = true;
                _completeGroup.blocksRaycasts = true;
            }
            if (cardRect != null) cardRect.localScale = targetScale;
        }

        private void OnSkipClicked()
        {
            TutorialManager.Instance?.SkipTutorial();
        }

        private void SetCompletionBackdrop(bool visible)
        {
            if (_completionBackdrop == null) return;
            _completionBackdrop.alpha = visible ? 1f : 0f;
            _completionBackdrop.blocksRaycasts = visible;
        }

        private void OnContinueClicked()
        {
            Debug.Log("[ArrowSwarm] TutorialOverlayUI: Continue Button Clicked! Returning to MainMenuScene...");
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CompleteAndReturnToMenu();
            }
            else if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene("MainMenuScene");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
            }
        }
    }
}
