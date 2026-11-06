namespace ArrowSwarm.UI
{
    using System;
    using UnityEngine;

    /// <summary>Gently shimmers only the authored skill accents, independently of the gameplay clock.</summary>
    [DisallowMultipleComponent]
    public sealed class UISkillAccentMotion : MonoBehaviour
    {
        [Serializable]
        private sealed class Accent
        {
            [SerializeField] internal RectTransform _rect;
            [SerializeField] internal CanvasGroup _group;
            [SerializeField] internal float _phase;
            [NonSerialized] internal Vector2 Position;
            [NonSerialized] internal Vector3 Scale;
            [NonSerialized] internal float Alpha;
            [NonSerialized] internal bool Cached;
        }

        [SerializeField] private Accent[] _accents = Array.Empty<Accent>();
        [Min(.1f), SerializeField] private float _period = 3.6f;
        [Min(0), SerializeField] private float _drift = 3f;
        [Range(0, .2f), SerializeField] private float _scaleAmount = .08f;
        [Range(0, 1), SerializeField] private float _minimumAlpha = .72f;
        private float _elapsed;

        private void OnEnable()
        {
            _elapsed = 0;
            for (int i = 0; i < _accents.Length; i++)
            {
                var accent = _accents[i];
                if (accent == null || accent._rect == null) continue;
                accent.Position = accent._rect.anchoredPosition;
                accent.Scale = accent._rect.localScale;
                accent.Alpha = accent._group != null ? accent._group.alpha : 1;
                accent.Cached = true;
            }
        }

        private void Update()
        {
            _elapsed = Mathf.Repeat(_elapsed + Time.unscaledDeltaTime, _period);
            float time = _elapsed * Mathf.PI * 2f / _period;
            for (int i = 0; i < _accents.Length; i++)
            {
                var accent = _accents[i];
                if (accent == null || !accent.Cached || accent._rect == null) continue;
                float wave = Mathf.Sin(time + accent._phase);
                accent._rect.anchoredPosition = accent.Position + new Vector2(0, wave * _drift);
                accent._rect.localScale = accent.Scale * (1 + wave * _scaleAmount);
                if (accent._group != null)
                    accent._group.alpha = accent.Alpha * Mathf.Lerp(_minimumAlpha, 1, (wave + 1) * .5f);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _accents.Length; i++)
            {
                var accent = _accents[i];
                if (accent == null || !accent.Cached) continue;
                if (accent._rect != null)
                {
                    accent._rect.anchoredPosition = accent.Position;
                    accent._rect.localScale = accent.Scale;
                }
                if (accent._group != null) accent._group.alpha = accent.Alpha;
                accent.Cached = false;
            }
        }
    }
}
