namespace ArrowSwarm.Audio
{
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Applies valid zero volumes and the global sound toggle without writing back to player data.</summary>
    public partial class AudioManager
    {
        /// <summary>Applies normalized preferences while retaining the current pause/result and crossfade gain.</summary>
        public void SetVolumes(float music, float sfx)
        {
            _musicVolume = Mathf.Clamp01(music);
            _sfxVolume = Mathf.Clamp01(sfx);
            RefreshMusicMix();
            if ((AudioMuted || _sfxVolume == 0f) && _sfxSources != null)
                foreach (var source in _sfxSources) if (source != null) source.Stop();
        }

        private void LoadVolumeSettings()
        {
            PlayerData data = DataManager.HasInstance ? DataManager.Instance.PlayerData : null;
            if (data != null) SetVolumes(data.musicVolume, data.sfxVolume);
        }
    }
}
