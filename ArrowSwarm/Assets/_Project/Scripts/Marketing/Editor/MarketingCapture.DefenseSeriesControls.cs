namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Path;
    using ArrowSwarm.Skills;
    using ArrowSwarm.Tips;
    using UnityEngine;
    using Arrow = ArrowSwarm.Arrow.Arrow;
    using Spawner = ArrowSwarm.Arrow.ArrowSpawner;

    /// <summary>Real Tip control and read-only lifecycle evidence for no-Freeze Defense recordings.</summary>
    public static partial class MarketingCapture
    {
        private static readonly HashSet<int> SeriesSpawnIds = new HashSet<int>();

        private static Arrow SeriesSuggestedArrow()
        {
            return TipManager.Instance.HasPendingSuggestion
                ? typeof(TipManager).GetField("_suggestedArrow", Private).GetValue(TipManager.Instance) as Arrow : null;
        }
        private static int SeriesSuggestedIndex()
        {
            var arrow = SeriesSuggestedArrow(); return arrow != null ? DefenseArrowIndex(arrow) : -1;
        }
        private static void InvokeSeriesTip()
        {
            var hud = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.GameHUD>();
            var button = hud == null ? null : typeof(ArrowSwarm.UI.GameHUD).GetField("_skill1TipButton", Private).GetValue(hud) as UnityEngine.UI.Button;
            if (button == null || !button.interactable) throw new InvalidOperationException("Native Tip control unavailable.");
            button.onClick.Invoke(); _seriesTipFrame = _frame; _seriesTipTarget = SeriesSuggestedIndex();
            if (_seriesTipTarget < 0 || _seriesTipTarget == _defenseReservedArrow || DataManager.Instance.PlayerData.tipCount != 0 || _seriesTipEvents != 1)
                throw new InvalidOperationException("Real charged Tip did not identify a separate legal first arrow.");
            DefenseEvent("tip", _seriesTipTarget, "real_button");
        }
        private static int ChooseSeriesLastArrow()
        {
            var arrows = Spawner.Instance.ActiveArrows;
            var tip = _seriesTip ? typeof(TipManager).GetMethod("FindBestArrow", Private).Invoke(TipManager.Instance, null) as Arrow : null;
            int best = -1; float longest = -1;
            for (int index = 0; index < arrows.Count; index++)
            {
                if (arrows[index] == tip || !CanReserveDefenseLast(index)) continue;
                var arrow = arrows[index];
                var route = PathManager.Instance.GetArrowPathWaypoints(GridManager.Instance.GetGridExitPoint(arrow.HeadPoint, arrow.HeadDirection));
                float length = 0;
                for (int i = 1; i < route.Count; i++) length += Vector2.Distance(route[i - 1], route[i]);
                if (length > longest) { best = index; longest = length; }
            }
            if (best < 0) throw new InvalidOperationException("No separate legal final arrow in this procedural roll.");
            return best;
        }
        private static void AttachSeriesEvents()
        {
            SeriesSpawnIds.Clear(); TrackSeriesSpawns("initial_alive");
            MobSpawner.OnMobSpawned += SeriesMobSpawned;
            FreezeManager.OnFreezeStarted += SeriesFreezeStarted;
            TipManager.OnTipUsed += SeriesTipUsed;
        }
        private static void DetachSeriesEvents()
        {
            MobSpawner.OnMobSpawned -= SeriesMobSpawned;
            FreezeManager.OnFreezeStarted -= SeriesFreezeStarted;
            TipManager.OnTipUsed -= SeriesTipUsed;
        }
        private static void TrackSeriesSpawns(string detail)
        {
            foreach (var mob in MobSpawner.Instance.ActiveMobs)
                if (mob != null && mob.IsAlive && mob.gameObject.activeInHierarchy && SeriesSpawnIds.Add(mob.MobId))
                    DefenseEvent("spawn", mob.MobId, detail);
        }
        private static void SeriesMobSpawned() => TrackSeriesSpawns("natural_spawn");
        private static void SeriesTipUsed(int count) { _seriesTipEvents++; }
        private static void SeriesFreezeStarted(float duration)
        {
            _seriesFreezeEvents++; DefenseEvent("freeze_started", -1, "forbidden");
        }
    }
}
