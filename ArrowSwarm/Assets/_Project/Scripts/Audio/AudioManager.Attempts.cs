namespace ArrowSwarm.Audio
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Chooses one looping music track per real gameplay attempt, independently of pause and UI changes.</summary>
    public partial class AudioManager
    {
        private bool _started;
        private AudioClip _attemptBgm;
        private int _nextGameplayTrack;
        private int _gameplayTrackIndex = -1;
        private int _attemptSceneHandle = int.MinValue;
        private PuzzleSession _musicPuzzleSession;
        private int _musicPuzzleVersion = int.MinValue;

        /// <summary>Zero-based playlist position for the current attempt, or -1 before its first level.</summary>
        public int CurrentGameplayTrackIndex => _gameplayTrackIndex;
        /// <summary>Music selected for the current attempt; pausing and settings do not replace it.</summary>
        public AudioClip CurrentGameplayTrack => _attemptBgm;

        private void SubscribeAttemptEvents()
        {
            LevelManager.OnLevelReady += HandleDefenseLevelReady;
            PuzzleSession.OnChanged += HandlePuzzleAttemptChanged;
        }

        private void UnsubscribeAttemptEvents()
        {
            LevelManager.OnLevelReady -= HandleDefenseLevelReady;
            PuzzleSession.OnChanged -= HandlePuzzleAttemptChanged;
        }

        private void HandleDefenseLevelReady(LevelParams parameters)
        {
            // Editor map previews must not consume the player's next music selection.
            if (SceneManager.GetActiveScene().name != "GameScene") return;
            _musicPuzzleSession = null;
            _musicPuzzleVersion = int.MinValue;
            BeginGameplayAttempt(false);
        }

        private void HandlePuzzleAttemptChanged()
        {
            var session = PuzzleSession.Active;
            if (session == null || session.Level == null || !session.gameObject.activeInHierarchy) return;
            RefreshMusicMix();
            if (session == _musicPuzzleSession && session.AttemptVersion == _musicPuzzleVersion && _attemptBgm != null) return;
            if (!BeginGameplayAttempt(session.Preview)) return;
            _musicPuzzleSession = session;
            _musicPuzzleVersion = session.AttemptVersion;
        }

        private bool BeginGameplayAttempt(bool preview)
        {
            if (_sfxLibrary == null) AutoLinkLibrary();
            int count = _sfxLibrary != null ? _sfxLibrary.GameplayTrackCount : 0;
            if (count == 0) return false;
            _gameplayTrackIndex = preview ? 0 : _nextGameplayTrack % count;
            _attemptBgm = _sfxLibrary.GetGameplayTrack(_gameplayTrackIndex);
            _attemptSceneHandle = SceneManager.GetActiveScene().handle;
            if (!preview) _nextGameplayTrack = (_gameplayTrackIndex + 1) % count;
            PlayBGM(_attemptBgm);
            return true;
        }

        private void PlayCurrentGameplayMusic()
        {
            if (_attemptSceneHandle == SceneManager.GetActiveScene().handle) PlayBGM(_attemptBgm);
        }

        private void RecoverGameplayAttempt()
        {
            // Only initial/late activation recovers missed events. sceneLoaded can still contain the old level.
            if (PuzzleSession.Active != null) { HandlePuzzleAttemptChanged(); return; }
            if (_attemptSceneHandle == SceneManager.GetActiveScene().handle) return;
            if (SceneManager.GetActiveScene().name != "GameScene" || !LevelManager.HasInstance || !GameManager.HasInstance) return;
            var state = GameManager.Instance.CurrentState;
            if (state == GameState.Loading || state == GameState.Menu) return;
            if (!LevelManager.Instance.CurrentLevelData.IsValid) return;
            BeginGameplayAttempt(false);
        }
    }
}
