namespace ArrowSwarm.Puzzle
{
    using System.Collections;
    using ArrowSwarm.Audio;
    using UnityEngine;

    /// <summary>Per-attempt single-arrow recovery, isolated from Defense skills and saved inventory.</summary>
    public sealed partial class PuzzleSession
    {
        [SerializeField] private PuzzleRocketEffect _rocketEffect;
        public int RocketCharges { get; private set; }
        public bool RocketArmed { get; private set; }
        public bool RocketNeeded { get; private set; }

        /// <summary>Arms or cancels selection without consuming a charge.</summary>
        public void ToggleRocket()
        {
            if (!Idle || RocketCharges <= 0) return;
            RocketArmed = !RocketArmed;
            for (int i = 0; i < _views.Length; i++) if (!_removed[i]) _views[i].Highlight(false);
            Notify();
        }

        private void ResetRocket()
        {
            RocketArmed = RocketNeeded = false; RocketCharges = _config.RocketCharges;
            if (_rocketEffect != null) _rocketEffect.Hide();
        }

        /// <summary>Extracts exactly one valid selected arrow without checking blockers or wall colors.</summary>
        public void FireRocket(int index)
        {
            if (!Idle || !RocketArmed || RocketCharges <= 0 || index < 0 || index >= _removed.Length || _removed[index]) return;
            _busy = true; RocketArmed = RocketNeeded = false; RocketCharges--; _removed[index] = true; Remaining--;
            for (int i = 0; i < _views.Length; i++) if (_views[i] != null) _views[i].Highlight(false);
            AudioManager.Instance?.PlayArrowFire(false);
            StartCoroutine(Extract(index)); Notify();
        }

        private IEnumerator Extract(int index)
        {
            var view = _views[index];
            float extent = Mathf.Max(Level.Width, Level.Height);
            Vector3 lift = Vector3.up * extent * _config.RocketLiftFraction;
            Vector3 head = new Vector3(Level.Arrows[index].Head.x - (Level.Width - 1) * .5f,
                Level.Arrows[index].Head.y - (Level.Height - 1) * .5f, 0f);
            float elapsed = 0f;
            while (elapsed < _config.RocketDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / _config.RocketDuration);
                view.RenderRocket(progress, lift);
                if (_rocketEffect != null) _rocketEffect.Show(head, lift, progress, extent * _config.RocketIconFraction);
                yield return null;
            }
            if (_rocketEffect != null) _rocketEffect.Hide();
            _pool.Release(view); _busy = false;
            if (Remaining == 0) End(true); else CheckStuck();
            Notify();
        }
    }
}
