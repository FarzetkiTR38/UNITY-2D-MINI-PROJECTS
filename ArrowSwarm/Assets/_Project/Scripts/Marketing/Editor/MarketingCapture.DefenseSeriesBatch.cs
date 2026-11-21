namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Sequential isolated captures of the five requested episodes, stopping immediately on failed evidence.</summary>
    public static partial class MarketingCapture
    {
        private static readonly Queue<int> SeriesQueue = new Queue<int>();
        private static readonly List<string> SeriesBatchRows = new List<string>();
        private static bool _seriesBatchWaiting;

        /// <summary>Records full framed sources for levels3/6/8/12/15; level8 alone uses Tip.</summary>
        public static void BeginDefenseSeriesBatch()
        {
            if (!Application.isPlaying || _recording || SeriesQueue.Count > 0 || _seriesBatchWaiting)
                throw new InvalidOperationException("Begin batch only from idle initialized armed Play.");
            foreach (int level in new[] { 3, 6, 8, 12, 15 }) SeriesQueue.Enqueue(level);
            SeriesBatchRows.Clear(); SeriesBatchRows.Add("level,source,status");
            EditorApplication.update -= SeriesBatchTick; EditorApplication.update += SeriesBatchTick;
            SeriesBatchTick();
        }
        private static void SeriesBatchTick()
        {
            if (!Application.isPlaying) { StopSeriesBatch(); return; }
            if (_recording) return;
            if (_seriesBatchWaiting)
            {
                _seriesBatchWaiting = false;
                SeriesBatchRows.Add(Path.GetFileName(_folder).Split('_')[1].Substring(1) + "," + Path.GetFileName(_folder) + "," +
                    (Status.StartsWith("PASS:") ? "PASS" : "RETRY"));
                Directory.CreateDirectory("ArrowSwarm_Marketing/05_Reels/DefenseSeries01_20261115");
                File.WriteAllLines("ArrowSwarm_Marketing/05_Reels/DefenseSeries01_20261115/capture_batch.csv", SeriesBatchRows);
                if (!Status.StartsWith("PASS:")) { StopSeriesBatch(); return; }
            }
            if (SeriesQueue.Count == 0) { StopSeriesBatch(); Status = "PASS: five framed Defense sources"; return; }
            int level = SeriesQueue.Dequeue();
            int seed = level == 3 || level == 6 ? 20261115 : 20261115 + level;
            try
            {
                BeginDefenseSeries(level, level == 8, true, level == 8 ? .4f : .5f, 10, 0, seed, 45, 75);
                _seriesBatchWaiting = true;
            }
            catch (Exception error) { StopSeriesBatch(); Status = "FAIL: batch " + error.Message; }
        }
        private static void StopSeriesBatch()
        {
            EditorApplication.update -= SeriesBatchTick; SeriesQueue.Clear(); _seriesBatchWaiting = false;
        }
    }
}
