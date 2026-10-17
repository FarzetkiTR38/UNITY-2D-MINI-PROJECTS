namespace ArrowSwarm.UI
{
    using System.Collections;
    using ArrowSwarm.Audio;
    using UnityEngine;

    /// <summary>Preserves authored popup scale and safely cancels transitions when attempts change.</summary>
    public sealed partial class PuzzlePopups
    {
        private readonly bool[] _visible = new bool[4];
        private readonly Coroutine[] _animations = new Coroutine[4];
        private readonly Vector3[] _dialogScales = new Vector3[4];
        private Vector3[] _starScales;
        private Coroutine _starsRoutine;
        private bool _initialized;

        private void InitializePanels()
        {
            if (_initialized) return;
            _initialized = true;
            for (int i = 0; i < 4; i++)
                _dialogScales[i] = _dialogs != null && i < _dialogs.Length && _dialogs[i] != null
                    ? _dialogs[i].localScale : Vector3.one;
            _starScales = new Vector3[_winStars != null ? _winStars.Length : 0];
            for (int i = 0; i < _starScales.Length; i++)
                _starScales[i] = _winStars[i] != null ? _winStars[i].transform.localScale : Vector3.one;
            ResetPanels();
        }

        private void ResetPanels()
        {
            if (!_initialized) return;
            StopAllCoroutines(); _starsRoutine = null;
            for (int i = 0; i < 4; i++)
            {
                _animations[i] = null; _visible[i] = false;
                if (_panels != null && i < _panels.Length && _panels[i] != null)
                {
                    _panels[i].alpha = 0f;
                    _panels[i].interactable = _panels[i].blocksRaycasts = false;
                }
                if (_dialogs != null && i < _dialogs.Length && _dialogs[i] != null)
                    _dialogs[i].localScale = _dialogScales[i];
            }
            for (int i = 0; _winStars != null && i < _winStars.Length; i++)
                if (_winStars[i] != null)
                {
                    _winStars[i].SetActive(false);
                    _winStars[i].transform.localScale = _starScales[i];
                }
        }

        private void SetPanelVisible(int index, bool visible)
        {
            InitializePanels();
            if (_visible[index] == visible || _panels == null || index >= _panels.Length || _panels[index] == null) return;
            _visible[index] = visible;
            if (_animations[index] != null) StopCoroutine(_animations[index]);
            if (visible)
            {
                AudioManager.Instance?.PlayPopupOpen();
                _animations[index] = StartCoroutine(UIPopupAnimator.AnimateOpen(_dialogs[index], _panels[index],
                    targetScale: _dialogScales[index]));
            }
            else
            {
                AudioManager.Instance?.PlayPopupClose();
                _animations[index] = StartCoroutine(ClosePanel(index));
            }
        }

        private IEnumerator ClosePanel(int index)
        {
            yield return UIPopupAnimator.AnimateClose(_dialogs[index], _panels[index]);
            if (_dialogs[index] != null) _dialogs[index].localScale = _dialogScales[index];
            _animations[index] = null;
        }

        private void AnimateWinStars(int stars)
        {
            if (_starsRoutine != null) StopCoroutine(_starsRoutine);
            _starsRoutine = StartCoroutine(WinStars(stars));
            if (ArrowSwarm.Effects.ParticleManager.HasInstance)
                ArrowSwarm.Effects.ParticleManager.Instance.SpawnConfetti();
        }

        private IEnumerator WinStars(int stars)
        {
            for (int i = 0; _winStars != null && i < _winStars.Length; i++)
                if (_winStars[i] != null) _winStars[i].SetActive(false);
            yield return new WaitForSecondsRealtime(.2f);
            for (int i = 0; _winStars != null && i < stars && i < _winStars.Length; i++)
            {
                var star = _winStars[i];
                if (star == null) continue;
                star.SetActive(true); AudioManager.Instance?.PlayStarEarn(i);
                float elapsed = 0f;
                while (elapsed < .22f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / .22f) - 1f;
                    float scale = 1f + 2.70158f * t * t * t + 1.70158f * t * t;
                    star.transform.localScale = _starScales[i] * Mathf.Max(0f, scale);
                    yield return null;
                }
                star.transform.localScale = _starScales[i];
                yield return new WaitForSecondsRealtime(.12f);
            }
            _starsRoutine = null;
        }
    }
}
