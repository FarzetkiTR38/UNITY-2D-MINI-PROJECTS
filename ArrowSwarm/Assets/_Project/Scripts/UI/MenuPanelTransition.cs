namespace ArrowSwarm.UI
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>Plays the menu popup entrance forward or backward from its current progress.</summary>
    internal sealed class MenuPanelTransition
    {
        private Coroutine _routine;
        private RectTransform _dialog;
        private Vector3 _restScale;
        private bool? _visible;
        private float _progress;

        internal void Open(MonoBehaviour owner, RectTransform dialog, CanvasGroup group)
            => ChangeVisibility(owner, dialog, group, true, null);

        internal void RequestClose(MonoBehaviour owner, RectTransform dialog, CanvasGroup group)
        {
            var menu = owner.GetComponentInParent<MainMenuUI>();
            if (menu != null && menu.TryCloseActivePanel(owner.gameObject)) return;
            Close(owner, dialog, group, null);
        }

        internal bool Close(MonoBehaviour owner, RectTransform dialog, CanvasGroup group, Action onClosed)
            => ChangeVisibility(owner, dialog, group, false, onClosed);

        internal void Reset(MonoBehaviour owner)
        {
            Stop(owner);
            if (_dialog != null) _dialog.localScale = _restScale;
            _dialog = null;
            _visible = null;
        }

        private bool ChangeVisibility(MonoBehaviour owner, RectTransform dialog, CanvasGroup group, bool visible, Action onClosed)
        {
            if (!owner.isActiveAndEnabled || _visible == visible) return false;

            Stop(owner);
            if (!_visible.HasValue || _dialog != dialog)
            {
                if (_dialog != null) _dialog.localScale = _restScale;
                _dialog = dialog;
                _restScale = dialog != null ? dialog.localScale : Vector3.one;
                _progress = visible ? 0f : 1f;
            }

            _visible = visible;
            float target = visible ? 1f : 0f;
            if (group == null || Mathf.Approximately(_progress, target))
            {
                Complete(owner, group, visible, onClosed);
                return true;
            }

            group.interactable = false;
            group.blocksRaycasts = true;
            UIPopupAnimator.ApplyOpenProgress(dialog, group, _restScale, _progress);
            _routine = owner.StartCoroutine(Animate(owner, group, visible, target, onClosed));
            return true;
        }

        private IEnumerator Animate(MonoBehaviour owner, CanvasGroup group, bool visible, float target, Action onClosed)
        {
            while (_progress != target)
            {
                yield return null;
                _progress = Mathf.MoveTowards(_progress, target, Time.unscaledDeltaTime / UIPopupAnimator.DefaultOpenDuration);
                UIPopupAnimator.ApplyOpenProgress(_dialog, group, _restScale, _progress);
            }

            Complete(owner, group, visible, onClosed);
        }

        private void Complete(MonoBehaviour owner, CanvasGroup group, bool visible, Action onClosed)
        {
            _routine = null;
            if (_dialog != null) _dialog.localScale = _restScale;
            if (group != null)
            {
                group.alpha = visible ? 1f : 0f;
                group.interactable = visible;
                group.blocksRaycasts = visible;
            }

            if (visible) return;
            owner.gameObject.SetActive(false);
            onClosed?.Invoke();
        }

        private void Stop(MonoBehaviour owner)
        {
            if (_routine == null) return;
            owner.StopCoroutine(_routine);
            _routine = null;
        }
    }
}
