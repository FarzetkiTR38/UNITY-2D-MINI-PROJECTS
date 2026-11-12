namespace ArrowSwarm.Audio
{
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.Tips;
    using UnityEngine;

    /// <summary>Owns one life-loss cue per event and separates surviving mob damage from terminal death.</summary>
    public partial class AudioManager
    {
        private int _previousDefenseLives = -1;
        private PuzzleSession _lifePuzzleSession;
        private int _lifePuzzleVersion = int.MinValue;
        private int _previousPuzzleLives = -1;
        private readonly Dictionary<Mob, FatalDamageMarker> _pendingFatalDamage = new Dictionary<Mob, FatalDamageMarker>();
        private int _fatalMarkerFrame = -1;

        private readonly struct FatalDamageMarker
        {
            public int MobId { get; }
            public int Frame { get; }
            public FatalDamageMarker(Mob mob) { MobId = mob.MobId; Frame = Time.frameCount; }
        }

        private void SubscribeGameplayAudioEvents()
        {
            _previousDefenseLives = GameManager.HasInstance ? GameManager.Instance.CurrentLives : -1;
            GameManager.OnLivesChanged += HandleDefenseLivesChanged;
            LevelManager.OnLevelLoading += HandleAudioLevelLoading;
            PuzzleSession.OnChanged += HandlePuzzleLivesChanged;
            Arrow.OnArrowClicked += HandleArrowClicked;
            Mob.OnMobKilled += HandleMobKilled;
            Mob.OnMobDamaged += HandleMobDamaged;
            Skills.FreezeManager.OnFreezeStarted += HandleFreezeStarted;
            TipManager.OnTipUsed += HandleDefenseTipUsed;
            HandlePuzzleLivesChanged();
        }

        private void UnsubscribeGameplayAudioEvents()
        {
            GameManager.OnLivesChanged -= HandleDefenseLivesChanged;
            LevelManager.OnLevelLoading -= HandleAudioLevelLoading;
            PuzzleSession.OnChanged -= HandlePuzzleLivesChanged;
            Arrow.OnArrowClicked -= HandleArrowClicked;
            Mob.OnMobKilled -= HandleMobKilled;
            Mob.OnMobDamaged -= HandleMobDamaged;
            Skills.FreezeManager.OnFreezeStarted -= HandleFreezeStarted;
            TipManager.OnTipUsed -= HandleDefenseTipUsed;
            _pendingFatalDamage.Clear();
            _lifePuzzleSession = null; _lifePuzzleVersion = int.MinValue; _previousPuzzleLives = -1;
        }

        private void HandleDefenseLivesChanged(int lives)
        {
            if (_previousDefenseLives >= 0 && lives < _previousDefenseLives) PlayHeartBreak();
            _previousDefenseLives = lives;
        }

        private void HandlePuzzleLivesChanged()
        {
            var session = PuzzleSession.Active;
            if (session == null || session.Level == null) return;
            bool sameAttempt = session == _lifePuzzleSession && session.AttemptVersion == _lifePuzzleVersion;
            if (sameAttempt && _previousPuzzleLives >= 0 && session.Lives < _previousPuzzleLives) PlayHeartBreak();
            _lifePuzzleSession = session; _lifePuzzleVersion = session.AttemptVersion; _previousPuzzleLives = session.Lives;
        }

        private void HandleAudioLevelLoading(int level)
        {
            _previousDefenseLives = -1;
            _pendingFatalDamage.Clear();
        }

        private void HandleArrowClicked(Arrow arrow, bool success)
        {
            if (success) PlayArrowFire(arrow != null && arrow.IsRainbow);
            else PlayArrowWrong();
        }

        private void HandleMobKilled(Mob mob)
        {
            if (mob == null) return;
            if (_fatalMarkerFrame != Time.frameCount) { _pendingFatalDamage.Clear(); _fatalMarkerFrame = Time.frameCount; }
            _pendingFatalDamage[mob] = new FatalDamageMarker(mob);
            PlayMobDie();
        }

        private void HandleMobDamaged(Mob mob, int damage)
        {
            if (mob == null) return;
            // ApplyDamage publishes death before OnMobDamaged; the pool can already have restored its HP.
            if (_pendingFatalDamage.TryGetValue(mob, out var marker))
            {
                _pendingFatalDamage.Remove(mob);
                if (marker.Frame == Time.frameCount && marker.MobId == mob.MobId) return;
            }
            if (damage > 0 && mob.isActiveAndEnabled && mob.IsAlive) PlayMobHit();
        }

        private void HandleFreezeStarted(float duration) => PlaySkillFreeze();
        private void HandleDefenseTipUsed(int remaining) => PlaySkillTips();
    }
}
