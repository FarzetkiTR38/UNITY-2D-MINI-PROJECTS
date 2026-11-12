namespace ArrowSwarm.Audio
{
    using UnityEngine;

    /// <summary>Inspector-authored playback gains and transitions; original audio assets remain unchanged.</summary>
    public partial class SFXLibrary
    {
        [Header("--- Music Mix ---")]
        [SerializeField, Range(0f, 1f)] private float _menuMusicGain = 1f;
        [Tooltip("Gains use the original playlist slot order, including any empty slots.")]
        [SerializeField] private float[] _gameBGMTrackGains;
        [SerializeField, Min(0f)] private float _musicCrossfadeSeconds = .35f;
        [SerializeField, Range(0f, 1f)] private float _pausedMusicMultiplier = .3f;
        [SerializeField, Range(0f, 1f)] private float _resultMusicMultiplier = .3f;

        /// <summary>Duration of a music change, measured in unscaled seconds.</summary>
        public float MusicCrossfadeSeconds => Mathf.Max(0f, _musicCrossfadeSeconds);
        /// <summary>Music gain while the current gameplay attempt is paused.</summary>
        public float PausedMusicMultiplier => Mathf.Clamp01(_pausedMusicMultiplier);
        /// <summary>Music gain behind a completed or failed attempt.</summary>
        public float ResultMusicMultiplier => Mathf.Clamp01(_resultMusicMultiplier);

        /// <summary>Returns a non-destructive clip gain, using unity gain for legacy libraries.</summary>
        public float GetMusicGain(AudioClip clip)
        {
            if (clip == _menuBGM) return Mathf.Clamp01(_menuMusicGain);
            if (_gameBGMPlaylist != null && _gameBGMTrackGains != null)
                for (int i = 0; i < _gameBGMPlaylist.Length && i < _gameBGMTrackGains.Length; i++)
                    if (clip != null && clip == _gameBGMPlaylist[i]) return Mathf.Clamp01(_gameBGMTrackGains[i]);
            return 1f;
        }
    }
}
