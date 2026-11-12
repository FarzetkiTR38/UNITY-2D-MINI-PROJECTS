namespace ArrowSwarm.Audio
{
    using System;
    using UnityEngine;

    /// <summary>Inspector-authored limits and gain for each effect, including reserved feedback capacity.</summary>
    public partial class SFXLibrary
    {
        [Header("--- Effect Voice Budget ---")]
        [SerializeField, Range(4, 16)] private int _sfxVoiceBudget = 8;
        [SerializeField, Min(0)] private int _reservedFeedbackVoices = 2;
        [SerializeField, Range(0f, 1f)] private float _starPitchStep = .15f;
        [SerializeField] private SfxClipMix _defaultSfxMix = new SfxClipMix();
        [SerializeField] private SfxClipMix[] _sfxClipMix;

        /// <summary>Total number of simultaneous effect sources, separate from the two music sources.</summary>
        public int SfxVoiceBudget => Mathf.Clamp(_sfxVoiceBudget, 4, 16);
        /// <summary>Sources unavailable to generic combat, preserving capacity for controls and feedback.</summary>
        public int ReservedFeedbackVoices => Mathf.Clamp(_reservedFeedbackVoices, 0, SfxVoiceBudget - 1);
        /// <summary>Pitch increase between consecutive result stars.</summary>
        public float StarPitchStep => Mathf.Clamp01(_starPitchStep);

        /// <summary>Returns per-clip settings or the serialized default for compatible external callers.</summary>
        public SfxClipMix GetSfxMix(AudioClip clip)
        {
            if (_sfxClipMix != null)
                foreach (var entry in _sfxClipMix)
                    if (entry != null && entry.Clip == clip && clip != null) return entry;
            return _defaultSfxMix ?? (_defaultSfxMix = new SfxClipMix());
        }

        /// <summary>One effect's gain, repetition limits, pitch and non-destructive voice priority.</summary>
        [Serializable]
        public sealed class SfxClipMix
        {
            [SerializeField] private AudioClip _clip;
            [SerializeField, Range(0f, 1f)] private float _gain = .8f;
            [SerializeField, Min(0f)] private float _minimumInterval = .025f;
            [Tooltip("Zero allows any free voice within the global budget.")]
            [SerializeField, Min(0)] private int _maximumVoices = 2;
            [SerializeField, Range(0f, .2f)] private float _pitchVariation;
            [SerializeField] private bool _useReservedVoices;
            [Tooltip("Only critical feedback may replace a lower-priority generic combat cue. Protected voices are never replaced.")]
            [SerializeField] private bool _canReclaimCombatVoice;
            [SerializeField, Range(0, 256)] private int _priority = 160;

            /// <summary>Clip controlled by this entry.</summary>
            public AudioClip Clip => _clip;
            /// <summary>Gain applied without modifying the original waveform.</summary>
            public float Gain => Mathf.Clamp01(_gain);
            /// <summary>Minimum unscaled interval between accepted starts of this same clip.</summary>
            public float MinimumInterval => Mathf.Max(0f, _minimumInterval);
            /// <summary>Per-clip simultaneous limit; zero uses only the global budget.</summary>
            public int MaximumVoices => Mathf.Max(0, _maximumVoices);
            /// <summary>Symmetric pitch variation applied once, when an idle voice starts.</summary>
            public float PitchVariation => Mathf.Clamp(_pitchVariation, 0f, .2f);
            /// <summary>Whether this cue may use the capacity reserved from combat bursts.</summary>
            public bool UseReservedVoices => _useReservedVoices;
            /// <summary>Whether critical feedback may reclaim a lower-priority generic combat voice.</summary>
            public bool CanReclaimCombatVoice => _canReclaimCombatVoice;
            /// <summary>Unity voice priority: lower numbers are more important.</summary>
            public int Priority => Mathf.Clamp(_priority, 0, 256);
        }
    }
}
