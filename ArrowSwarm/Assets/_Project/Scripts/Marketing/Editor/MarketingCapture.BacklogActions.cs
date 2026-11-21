namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.UI;
    using Arrow = ArrowSwarm.Arrow.Arrow;

    /// <summary>Only real production controls and rule-checked actions for missing capture concepts.</summary>
    public static partial class MarketingCapture
    {
        private static Arrow BacklogBlockedDefense()
        {
            Arrow best = null; float distance = -1;
            foreach (var arrow in ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows)
            {
                if (arrow == null || arrow.IsFired || arrow.IsMarkedBlocked || arrow.IsBlockedAnimating || GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection)) continue;
                var point = GridManager.Instance.GetCollisionPoint(arrow.HeadPoint, arrow.HeadDirection, out Arrow obstacle);
                float length = Vector2.Distance(arrow.transform.position, point);
                if (length > distance) { distance = length; best = arrow; }
            }
            return best;
        }
        private static void BacklogMatchAction(PuzzleSession puzzle, ref int shots, ref int returns, ref bool awaiting, ref int nextShot, ref int step)
        {
            if (puzzle == null || puzzle.Level == null) throw new InvalidOperationException("Native Color Shift required.");
            if (awaiting && !puzzle.HasActiveShots) { awaiting = false; returns++; nextShot = _frame + 45; BacklogEvent("returned", -1, puzzle); }
            if (_frame < nextShot || !puzzle.Idle || awaiting) return;
            if (shots == 0)
            {
                var removed = new bool[puzzle.Level.Arrows.Count]; int target = -1;
                for (int i = 0; i < removed.Length; i++)
                    if (float.IsPositiveInfinity(PuzzleRules.ObstacleDistance(puzzle.Level, removed, i)) && !PuzzleRules.MatchesWall(puzzle.Level, i, puzzle.Orientation)) { target = i; break; }
                if (target < 0)
                {
                    if (puzzle.Turns <= 0) throw new InvalidOperationException("No real mismatch witness.");
                    puzzle.Rotate(1); BacklogEvent("rotate", -1, puzzle); nextShot = _frame + 35; return;
                }
                puzzle.Fire(target); shots++; awaiting = true; BacklogEvent("wrong_fire", target, puzzle); return;
            }
            var currentRemoved = (bool[])typeof(PuzzleSession).GetField("_removed", Private).GetValue(puzzle);
            int legal = -1;
            for (int i = 0; i < currentRemoved.Length; i++)
                if (!currentRemoved[i] && PuzzleRules.CanRemove(puzzle.Level, currentRemoved, i, puzzle.Orientation)) { legal = i; break; }
            if (legal < 0)
            {
                if (shots >= 2) return;
                if (puzzle.Turns <= 0) throw new InvalidOperationException("No valid matching exit witness.");
                puzzle.Rotate(1); BacklogEvent("rotate", -1, puzzle); nextShot = _frame + 35; return;
            }
            int before = puzzle.Remaining; puzzle.Fire(legal);
            if (puzzle.Remaining != before - 1) throw new InvalidOperationException("Matching exit did not remove the expected arrow.");
            shots++; BacklogEvent("fire", legal, puzzle); nextShot = _frame + 20;
        }
        private static void BacklogMenuAction(int frame)
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.MainMenuUI>();
            var modes = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.GameModeSelectUI>();
            if (frame == 45) { modes.Show(true); BacklogEvent("mode_open", -1, null); }
            if (frame == 120 || frame == 180 || frame == 240)
            {
                var choices = (Button[])typeof(ArrowSwarm.UI.GameModeSelectUI).GetField("_choices", Private).GetValue(modes);
                int index = (frame - 120) / 60; choices[index].onClick.Invoke(); BacklogEvent("mode_select", index, null);
            }
            if (frame == 285) modes.Show(false);
            if (frame == 330) { menu.OpenSettings(); BacklogEvent("settings_open", -1, null); }
            if (frame == 390 || frame == 450)
            {
                var settings = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.SettingsUI>();
                string field = frame == 390 ? "_darkThemeButton" : "_lightThemeButton";
                ((Button)typeof(ArrowSwarm.UI.SettingsUI).GetField(field, Private).GetValue(settings)).onClick.Invoke();
                BacklogEvent(frame == 390 ? "dark_theme" : "light_theme", -1, null);
            }
            if (frame == 480)
            {
                var settings = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.SettingsUI>();
                ((Button)typeof(ArrowSwarm.UI.SettingsUI).GetField("_nextLanguageButton", Private).GetValue(settings)).onClick.Invoke();
                BacklogEvent("language_next", -1, null);
            }
            if (frame == 525) { menu.OpenMainPanel(); BacklogEvent("menu_return", -1, null); }
        }
    }
}
