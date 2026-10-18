namespace ArrowSwarm.Puzzle
{
    using UnityEngine;

    /// <summary>One scene-authored reusable rocket sprite; creates no runtime projectiles.</summary>
    public sealed class PuzzleRocketEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _icon;
        [SerializeField] private AnimationCurve _fade = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        private void Awake() => Hide();

        /// <summary>Moves the authored sprite above the selected arrow while it is extracted.</summary>
        public void Show(Vector3 origin, Vector3 lift, float progress, float size)
        {
            if (_icon == null || _icon.sprite == null) return;
            _icon.enabled = true;
            transform.position = origin + lift * progress * progress;
            transform.localScale = Vector3.one * (size / Mathf.Max(.01f, _icon.sprite.bounds.size.x));
            Color tint = Color.white; tint.a = _fade.Evaluate(progress); _icon.color = tint;
        }

        /// <summary>Clears the visual immediately on completion, retry or scene exit.</summary>
        public void Hide() { if (_icon != null) _icon.enabled = false; }
    }
}
