namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Sequential recorder for the previously unrecorded original marketing concepts.</summary>
    public static partial class MarketingCapture
    {
        private static bool _backlogBatch, _backlogWaiting;
        private static int _backlogIndex;
        private static readonly string[] BacklogCases = { "Heart", "Advanced", "Budget", "Match", "Tutorial", "Wrong", "Menu" };
        private static readonly List<string> BacklogRows = new List<string>();
        /// <summary>Records all missing witnesses after Arm and initialized offline Play.</summary>
        public static void BeginBacklogBatch(int startAt = 0)
        {
            if (!Application.isPlaying || _recording || !SessionState.GetBool(Armed, false))
                throw new InvalidOperationException("Armed initialized idle Play required.");
            if (startAt < 0 || startAt >= BacklogCases.Length) throw new ArgumentOutOfRangeException(nameof(startAt));
            _backlogIndex = startAt; _backlogWaiting = false; _backlogBatch = true;
            BacklogRows.Clear(); BacklogRows.Add("scenario,source,status");
            string log = "ArrowSwarm_Marketing/05_Reels/ConceptBacklog_20261117/NativeEvidence/capture_batch.csv";
            if (startAt > 0 && File.Exists(log))
                foreach (string line in File.ReadAllLines(log)) if (!line.StartsWith("scenario,")) BacklogRows.Add(line);
            EditorApplication.update -= PumpBacklog; EditorApplication.update += PumpBacklog;
            PumpBacklog();
        }
        private static void PumpBacklog()
        {
            if (!_backlogBatch || !Application.isPlaying) { EditorApplication.update -= PumpBacklog; return; }
            if (_recording) return;
            if (_backlogWaiting)
            {
                if (!Status.StartsWith("PASS:"))
                {
                    if (Status.StartsWith("FAIL:")) { _backlogBatch = false; WriteBacklogRows(); }
                    return;
                }
                BacklogRows.Add(BacklogCases[_backlogIndex] + "," + Status.Substring(6) + ",PASS");
                _backlogIndex++; _backlogWaiting = false; WriteBacklogRows();
            }
            if (_backlogIndex >= BacklogCases.Length)
            {
                _backlogBatch = false; EditorApplication.update -= PumpBacklog; Status = "PASS: Backlog batch7/7"; return;
            }
            string key = BacklogCases[_backlogIndex];
            if (key == "Heart") Begin(1, "Shapes", 18, 0, "tr");
            else if (key == "Advanced") Begin(50, "Shapes", 25, 0, "tr");
            else if (key == "Budget") Begin(5, "ColorShift", 25, 0, "tr");
            else BeginBacklogWitness(key);
            _backlogWaiting = true;
        }
        private static void WriteBacklogRows()
        {
            string folder = "ArrowSwarm_Marketing/05_Reels/ConceptBacklog_20261117/NativeEvidence";
            Directory.CreateDirectory(folder); File.WriteAllLines(folder + "/capture_batch.csv", BacklogRows);
        }
        /// <summary>Records a menu, first-run tutorial, natural Defense loss or Color Shift mismatch witness.</summary>
        public static void BeginBacklogWitness(string kind)
        {
            if (!Application.isPlaying || _recording || !SessionState.GetBool(Armed, false))
                throw new InvalidOperationException("Armed initialized idle Play required.");
            _folder = Root + "/Backlog_" + kind + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            Directory.CreateDirectory(_folder); Events.Clear(); Events.Add("frame,time_seconds,event,arrow,remaining,lives");
            _recording = true; Status = "Loading " + _folder;
            ArrowSwarm.Audio.AudioManager.Instance.StartCoroutine(Drive(RecordBacklogWitness(kind)));
        }
    }
}
