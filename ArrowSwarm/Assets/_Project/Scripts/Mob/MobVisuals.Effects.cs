namespace ArrowSwarm.Mob
{
    using System.Collections;
    using UnityEngine;

    /// <summary>Owns interruptible damage feedback without changing the sprite animation.</summary>
    public partial class MobVisuals
    {
        private static readonly WaitForEndOfFrame FlashFrame = new WaitForEndOfFrame();

        private void HandleDamageTaken(int damage, int remainingHP)
        {
            UpdateHPDisplay(remainingHP);
            if (!_isShaking && _spriteRenderer != null && gameObject.activeInHierarchy)
            {
                _flashCoroutine = StartCoroutine(FlashCoroutine());
            }
        }

        private void StopDamageFlash()
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
            _isShaking = false;
        }

        private IEnumerator FlashCoroutine()
        {
            _isShaking = true;
            float elapsed = 0f;
            _spriteRenderer.color = Color.red;
            while (elapsed < _flashDuration)
            {
                float progress = elapsed / _flashDuration;
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = Color.Lerp(Color.red, RestingTint, progress);
                    float squash = Mathf.Sin(progress * Mathf.PI);
                    _spriteRenderer.transform.localScale = Vector3.Scale(_originalLocalScale,
                        new Vector3(1f + squash * 0.22f, 1f - squash * 0.18f, 1f));
                }
                elapsed += Time.deltaTime;
                yield return FlashFrame;
            }
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = RestingTint;
                _spriteRenderer.transform.localScale = _originalLocalScale;
            }
            _isShaking = false;
            _flashCoroutine = null;
        }
    }
}
