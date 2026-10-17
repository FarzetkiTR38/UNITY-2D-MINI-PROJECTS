namespace ArrowSwarm.Puzzle
{
    using System.Collections;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using UnityEngine;

    /// <summary>Atomic shot/rotation actions and scene-safe reward callbacks.</summary>
    public sealed partial class PuzzleSession
    {
        /// <summary>Finds the nearest stationary path within the configured touch radius.</summary>
        public void Tap(Vector2 world)
        {
            if (!Ready) return;
            int best = -1; float distance = _config.HitRadius;
            for (int i = 0; i < _views.Length; i++)
            {
                if (_removed[i]) continue;
                float d = _views[i].DistanceTo(world);
                if (d < distance) { distance = d; best = i; }
            }
            if (best >= 0) Fire(best);
        }

        /// <summary>Starts exactly one shot; blocked shots keep occupancy until the player retries.</summary>
        public void Fire(int index)
        {
            if (!Ready || index < 0 || index >= _removed.Length || _removed[index]) return;
            _busy = true;
            for (int i = 0; i < _views.Length; i++) if (!_removed[i]) _views[i].Highlight(false);
            StartCoroutine(Shot(index));
            Notify();
        }

        private IEnumerator Shot(int index)
        {
            var arrow = Level.Arrows[index]; var view = _views[index];
            float obstacle = PuzzleRules.ObstacleDistance(Level, _removed, index);
            bool match = PuzzleRules.MatchesWall(Level, index, Orientation);
            bool blocked = !float.IsPositiveInfinity(obstacle) || !match;
            float wallDistance = arrow.Direction.x > 0 ? Level.Width - 1 - arrow.Head.x : arrow.Direction.x < 0 ? arrow.Head.x : arrow.Direction.y > 0 ? Level.Height - 1 - arrow.Head.y : arrow.Head.y;
            float distance = blocked ? Mathf.Max(.1f, Mathf.Min(obstacle - _config.StrokeWidth, wallDistance + _config.WallOffset - _config.StrokeWidth))
                : wallDistance + view.Length + Mathf.Max(Level.Width, Level.Height);
            if (!blocked) { _removed[index] = true; Remaining--; AudioManager.Instance?.PlayArrowFire(false); }
            float travel = 0;
            while (travel < distance) { travel = Mathf.Min(distance, travel + Time.deltaTime * _config.Speed); view.RenderTravel(travel); yield return null; }
            if (blocked)
            {
                Lives--; AudioManager.Instance?.PlayArrowWrong(); Notify();
                while (travel > 0) { travel = Mathf.Max(0, travel - Time.deltaTime * _config.Speed); view.RenderTravel(travel); yield return null; }
            }
            else _pool.Release(view);
            _busy = false;
            if (Lives <= 0) End(false);
            else if (Remaining == 0) End(true);
            else CheckStuck();
            Notify();
        }

        /// <summary>Accepts one deliberate swipe while idle; reverse turns consume the same budget.</summary>
        public void Rotate(int direction)
        {
            if (!Ready || _mode != GameModeId.ColorShift || Turns <= 0) return;
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
            if (_mode == GameModeId.ColorShift && Turns == 0 && !PuzzleSolver.Solve(Level, _removed, Orientation, 0).Solvable)
            {
                // Allow every remaining legal shot before presenting the exhausted-frame failure.
                for (int i = 0; i < _removed.Length; i++) if (PuzzleRules.CanRemove(Level, _removed, i, Orientation)) return;
                SetMessage("puzzle_stuck"); End(false);
            }
        }
        private void End(bool won)
        {
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
            if (!Ready) return;
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
