namespace ArrowSwarm.Puzzle
{
    using System.Collections;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using UnityEngine;

    /// <summary>Per-arrow shots, exclusive frame rotation and scene-safe reward callbacks.</summary>
    public sealed partial class PuzzleSession
    {
        /// <summary>Finds the nearest stationary path within the configured touch radius.</summary>
        public void Tap(Vector2 world)
        {
            if (!Ready) return;
            int best = -1; float distance = _config.HitRadius;
            for (int i = 0; i < _views.Length; i++)
            {
                if (_removed[i] || _inFlight[i]) continue;
                float d = _views[i].DistanceTo(world);
                if (d < distance) { distance = d; best = i; }
            }
            if (best >= 0) { if (RocketArmed) FireRocket(best); else Fire(best); }
        }

        /// <summary>Starts a stationary arrow; Shapes accepts other shots while this one is moving.</summary>
        public void Fire(int index)
        {
            if (!Ready || index < 0 || index >= _removed.Length || _removed[index] || _inFlight[index]) return;
            RocketArmed = false;
            for (int i = 0; i < _views.Length; i++) if (!_removed[i] && !_inFlight[i]) _views[i].Highlight(false);
            _inFlight[index] = true; _activeShots++;
            StartCoroutine(Shot(index));
            Notify();
        }

        private IEnumerator Shot(int index)
        {
            int version = _version;
            var arrow = Level.Arrows[index]; var view = _views[index];
            float obstacle = PuzzleRules.ObstacleDistance(Level, _removed, index);
            bool match = PuzzleRules.MatchesWall(Level, index, Orientation);
            bool blocked = !float.IsPositiveInfinity(obstacle) || !match;
            float wallDistance = arrow.Direction.x > 0 ? Level.Width - 1 - arrow.Head.x : arrow.Direction.x < 0 ? arrow.Head.x : arrow.Direction.y > 0 ? Level.Height - 1 - arrow.Head.y : arrow.Head.y;
            float distance = blocked ? Mathf.Max(.1f, Mathf.Min(obstacle - _config.StrokeWidth, wallDistance + _config.WallOffset - _config.StrokeWidth) - view.HeadForwardOffset)
                : wallDistance + view.Length + _config.WallOffset + _config.HeadSize;
            float speed = blocked ? _config.Speed : Mathf.Max(_config.Speed, distance / _config.MaximumFlightDuration);
            if (!blocked) { _removed[index] = true; Remaining--; AudioManager.Instance?.PlayArrowFire(false); }
            float travel = 0;
            while (travel < distance) { travel = Mathf.Min(distance, travel + Time.deltaTime * speed); view.RenderTravel(travel); yield return null; }
            if (blocked)
            {
                Lives = Mathf.Max(0, Lives - 1); AudioManager.Instance?.PlayArrowWrong(); Notify();
                if (version != _version) yield break;
                while (travel > 0) { travel = Mathf.Max(0, travel - Time.deltaTime * _config.Speed); view.RenderTravel(travel); yield return null; }
            }
            else _pool.Release(view);
            _inFlight[index] = false; _activeShots--;
            // Pending collisions must resolve before awarding stars or presenting a terminal popup.
            if (!HasActiveShots)
            {
                if (Lives <= 0) End(false);
                else if (Remaining == 0) End(true);
                else CheckStuck();
            }
            Notify();
        }

        /// <summary>Accepts one deliberate swipe while idle; reverse turns consume the same budget.</summary>
        public void Rotate(int direction)
        {
            if (!Ready || _mode != GameModeId.ColorShift || Turns <= 0) return;
            RocketArmed = false;
            _busy = true; Turns--; Orientation ^= 1; Notify();
            StartCoroutine(Turn(direction));
        }
        private IEnumerator Turn(int direction)
        {
            yield return _frame.Rotate(direction, _config.RotationDuration);
            _busy = false; CheckStuck(); Notify();
        }
        private void CheckStuck()
        {
            RocketNeeded = false;
            if (_mode != GameModeId.ColorShift || Turns > 0) return;
            // All normally legal shots remain available, even after the last rotation.
            for (int i = 0; i < _removed.Length; i++) if (PuzzleRules.CanRemove(Level, _removed, i, Orientation)) return;
            int required = 0;
            for (int i = 0; i < _removed.Length; i++)
                if (!_removed[i] && !PuzzleRules.MatchesWall(Level, i, Orientation)) required++;
            if (required == 0 || required > RocketCharges) { SetMessage("puzzle_stuck"); End(false); }
            else { RocketNeeded = true; SetMessage("puzzle_rocket_needed"); }
        }
        private void End(bool won)
        {
            if (Won || Lost) return;
            Won = won; Lost = !won;
            if (won)
            {
                AudioManager.Instance?.PlayLevelWin();
                if (!_awarded && !Preview) { _awarded = true; DataManager.Instance.CompletePuzzleLevel(_mode, LevelNumber, Lives, _catalog.Count); }
                SetMessage(LastLevel ? "puzzle_campaign_complete" : "puzzle_cleared");
            }
            else AudioManager.Instance?.PlayLevelLose();
            Notify();
        }

        /// <summary>Uses the solution witness for a safe arrow or needed frame rotation.</summary>
        public void Hint()
        {
            if (!Idle) return;
            RocketArmed = false;
            if (RocketNeeded)
            {
                RocketArmed = true;
                for (int i = 0; i < _removed.Length; i++)
                    if (!_removed[i] && !PuzzleRules.MatchesWall(Level, i, Orientation)) { _views[i].Highlight(true); break; }
                SetMessage("puzzle_rocket_needed"); Notify(); return;
            }
            var solution = PuzzleSolver.Solve(Level, _removed, Orientation, Turns);
            if (!solution.Solvable || solution.Steps.Count == 0) { SetMessage("puzzle_restart_hint"); Notify(); return; }
            if (!Preview && DataManager.Instance.PlayerData.tipCount <= 0)
            { HintPromptOpen = true; Notify(); return; }
            if (!Preview) DataManager.Instance.ModifyTipCount(-1);
            int action = solution.Steps[0];
            SetMessage(action < 0 ? "puzzle_hint_rotate" : "puzzle_hint_arrow");
            if (action >= 0) _views[action].Highlight(true);
            AudioManager.Instance?.PlaySkillTips(); Notify();
        }
        private void RewardHint()
        {
            int version = _version;
            AdManager.Instance.ShowRewardedAd(success =>
            {
                if (this == null || version != _version) return;
                if (success) { DataManager.Instance.ModifyTipCount(1); SetMessage("puzzle_hint_added"); Notify(); }
            });
        }

        /// <summary>Closes the authored hint popup without spending a hint.</summary>
        public void CloseHintPrompt() { HintPromptOpen = false; Notify(); }

        /// <summary>Requests the existing rewarded ad only after pressing the popup's watch button.</summary>
        public void WatchHintAd()
        {
            if (!HintPromptOpen) return;
            HintPromptOpen = false; Notify(); RewardHint();
        }

        /// <summary>One rewarded continuation restores lives and one turn only when stuck.</summary>
        public void Continue()
        {
            if (!CanContinue) return;
            int version = _version; _continued = true; Notify();
            AdManager.Instance.ShowRewardedAd(success =>
            {
                if (this == null || version != _version) return;
                if (!success) { _continued = false; Notify(); return; }
                Lives = _config.Lives; Lost = false;
                if (_mode == GameModeId.ColorShift && Turns == 0)
                    Turns = Mathf.Max(1,PuzzleSolver.Solve(Level,_removed,Orientation,int.MaxValue).RequiredTurns);
                SetMessage("puzzle_continue"); Notify();
            });
        }
    }
}
