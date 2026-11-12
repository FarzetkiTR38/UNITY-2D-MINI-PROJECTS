namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Modes.Editor;
    using ArrowSwarm.Tips;
    using UnityEngine;
    using Arrow = ArrowSwarm.Arrow.Arrow;
    using ArrowSpawner = ArrowSwarm.Arrow.ArrowSpawner;

    /// <summary>Exercises actual hint accounting in disposable live boards while preserving data and preferences.</summary>
    public static partial class HintGuardVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>Checks live Defense repeats, zero inventory, pause, unrelated/suggested fire, retry and inactive pool targets.</summary>
        public static string VerifyDefense()
        {
            if (!Application.isPlaying || !LevelManager.HasInstance || !TipManager.HasInstance ||
                GameManager.Instance.CurrentState != GameState.Playing || LevelManager.Instance.CurrentParams.Level < 2)
                throw new InvalidOperationException("Use a disposable, ready Defense board after tutorial (e.g.101).");
            var checks = new List<string>(); var tips = TipManager.Instance; var data = DataManager.Instance;
            int level = LevelManager.Instance.CurrentParams.Level, used = 0, empty = 0;
            float time = Time.timeScale; var random = UnityEngine.Random.state;
            var snapshot = new QualityDataSnapshot(data);
            Action<int> onUsed = count => used++; Action onEmpty = () => empty++;
            TipManager.OnTipUsed += onUsed; TipManager.OnNoTipsAvailable += onEmpty;
            try
            {
                var clone = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(data.PlayerData)); snapshot.SetData(clone);
                Time.timeScale = 0; Call(tips, "ClearSuggestion"); clone.tipCount = 3; tips.UseTip();
                var suggested = Field<Arrow>(tips, "_suggestedArrow");
                Check(suggested != null && tips.HasPendingSuggestion && clone.tipCount == 2 && used == 1,
                    "First hint selects a real legal arrow and charges exactly once", checks);
                for (int i = 0; i < 12; i++) tips.UseTip();
                Check(clone.tipCount == 2 && used == 1 && Field<Arrow>(tips, "_suggestedArrow") == suggested,
                    "Twelve repeated manager calls reuse the suggestion without another charge/event", checks);
                clone.tipCount = 0; tips.UseTip();
                Check(tips.HasPendingSuggestion && empty == 0 && used == 1 && clone.tipCount == 0,
                    "Outstanding last-charge hint suppresses exhausted-hint events", checks);
                GameManager.Instance.SetState(GameState.Paused); GameManager.Instance.SetState(GameState.Playing); tips.UseTip();
                Check(tips.HasPendingSuggestion && clone.tipCount == 0 && empty == 0,
                    "Pause and resume preserve the paid suggestion", checks);
                var highlighter = Field<TipHighlighter>(tips, "_highlighter");
                highlighter.ClearHighlight(); tips.UseTip();
                Check(Field<Arrow>(highlighter, "_highlightedArrow") == suggested && clone.tipCount == 0,
                    "A cleared visual can be restored without spending another charge", checks);
                Arrow unrelated = null;
                foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
                    if (arrow != null && arrow != suggested && !arrow.IsFired &&
                        GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection)) { unrelated = arrow; break; }
                Check(unrelated != null, "Fixture has an unrelated legal arrow", checks);
                unrelated.OnPlayerClick();
                Check(tips.HasPendingSuggestion && Field<Arrow>(highlighter, "_highlightedArrow") == suggested,
                    "Firing a different arrow preserves the highlighted paid suggestion", checks);
                suggested.OnPlayerClick();
                Check(!tips.HasPendingSuggestion && Field<Arrow>(highlighter, "_highlightedArrow") == null,
                    "Firing the suggested arrow consumes its lock and visual immediately", checks);
                clone.tipCount = 2; tips.UseTip();
                Check(tips.HasPendingSuggestion && clone.tipCount == 1 && used == 2,
                    "A new hint can be bought after consuming the previous suggestion", checks);
                var pooled = Field<Arrow>(tips, "_suggestedArrow"); pooled.gameObject.SetActive(false);
                Check(!tips.HasPendingSuggestion, "An inactive pooled target invalidates its suggestion", checks);
                pooled.gameObject.SetActive(true);
                tips.UseTip(); Check(clone.tipCount == 0 && tips.HasPendingSuggestion,
                    "A fresh valid hint is available after target invalidation", checks);
                LevelManager.Instance.LoadLevel(level);
                Check(!tips.HasPendingSuggestion && Field<Arrow>(highlighter, "_highlightedArrow") == null,
                    "Retry with reused pooled arrows resets the old attempt's hint", checks);
                snapshot.Dispose(); snapshot.VerifyRestored(checks); return Report(checks);
            }
            finally
            {
                TipManager.OnTipUsed -= onUsed; TipManager.OnNoTipsAvailable -= onEmpty;
                snapshot.Dispose(); Call(tips, "ClearSuggestion");
                LevelManager.Instance.LoadLevel(level); Time.timeScale = time; UnityEngine.Random.state = random;
            }
        }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
        private static void Set(object owner, string name, object value) => owner.GetType().GetField(name, Private).SetValue(owner, value);
        private static void Call(object owner, string name) => owner.GetType().GetMethod(name, Private).Invoke(owner, null);
        private static void Check(bool value, string label, List<string> checks)
        { if (!value) throw new InvalidOperationException(label); checks.Add(label); }
        private static string Report(List<string> checks) => JsonUtility.ToJson(new HintReport(checks), true);

        /// <summary>Serializable result uses private fields to avoid exposing mutable test state.</summary>
        [Serializable]
        private sealed class HintReport
        {
            [SerializeField] private string status = "PASS";
            [SerializeField] private string[] checks;
            internal HintReport(List<string> values) { checks = values.ToArray(); }
        }
    }
}
