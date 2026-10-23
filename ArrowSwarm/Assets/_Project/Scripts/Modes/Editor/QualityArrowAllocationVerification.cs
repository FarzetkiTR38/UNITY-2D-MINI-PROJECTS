namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Arrow;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Measures real Defense LineRenderer uploads and checks exact coordinates when reusable buffers resize.</summary>
    public static partial class QualityArrowAllocationVerification
    {
        private const string Report = "Captures/QualityPass/arrow-allocation-verification.txt";
        private static readonly List<string> Results = new List<string>();

        /// <summary>Current measured result; Begin also exposes loading progress through the gap-close runner.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Loads a safely isolated real Defense scene, runs the checks and returns to menu.</summary>
        [MenuItem("Arrow Swarm/Quality/Verify Defense Arrow Allocations (Play Mode)")]
        public static void Begin()
        {
            Status = "Loading isolated Defense101";
            QualityGapCloseVerification.BeginWithCheck(Run);
        }

        /// <summary>Checks a real idle arrow in the currently loaded Defense scene, restoring its authored resting appearance.</summary>
        public static void Run()
        {
            Arrow arrow = null;
            if (ArrowSpawner.HasInstance)
                foreach (var candidate in ArrowSpawner.Instance.ActiveArrows)
                    if (candidate != null && !candidate.IsFired && !candidate.IsBlockedAnimating) { arrow = candidate; break; }
            if (!Application.isPlaying || arrow == null) throw new InvalidOperationException("Load a real Defense board before checking allocations.");
            Results.Clear(); Status = "Measuring native Defense uploads"; string outcome;
            var visuals = arrow.Visuals; visuals.CompleteSpawnImmediately();
            var line = visuals.GetComponent<LineRenderer>();
            var snapshot = new VisualSnapshot(visuals, line);
            try
            {
                CheckGeometry(visuals, line);
                CheckAllocations(visuals, line);
                outcome = "PASS: exact Defense spawn/slither geometry and zero warmed managed body-upload allocations";
            }
            catch (Exception exception)
            {
                outcome = "FAIL: " + exception.Message; Status = outcome;
                WriteReport(outcome); throw;
            }
            finally { snapshot.Restore(visuals, line); }
            Check(snapshot.Matches(visuals, line), "Original arrow line, head transform and visibility restored after profiling");
            Status = outcome; WriteReport(outcome);
            Debug.Log("[ArrowSwarm] " + outcome + " (" + Report + ")");
        }

        private static void CheckGeometry(ArrowVisuals visuals, LineRenderer line)
        {
            var points = new List<Vector3>(128);
            foreach (int count in new[] { 2, 3, 7, 33, 5, 65, 2, 0, 1, 17 })
            {
                Fill(points, count); Vector3 head = new Vector3(count * .1f, 2f, -.05f);
                Quaternion rotation = Quaternion.Euler(0f, 0f, count % 4 * 90f); bool visible = count % 2 == 0;
                visuals.UpdateSlitheringBody(points, head, rotation, visible);
                Check(line.positionCount == (count >= 2 ? count : 0), "Slither point count remains exact after resize to " + count);
                for (int index = 0; index < line.positionCount; index++)
                    Check(line.GetPosition(index) == points[index], "Body point " + index + " matches at count " + count);
                Check(visuals.HeadTransform.position == head && Quaternion.Angle(visuals.HeadTransform.rotation, rotation) < .001f &&
                    visuals.HeadTransform.GetComponent<SpriteRenderer>().enabled == visible,
                    "Head position, cardinal rotation and visibility remain unchanged at count " + count);
            }
            var method = typeof(ArrowVisuals).GetMethod("UploadBodyPoints", BindingFlags.NonPublic | BindingFlags.Instance);
            var upload = (Action<List<Vector3>>)Delegate.CreateDelegate(typeof(Action<List<Vector3>>), visuals, method);
            foreach (int count in new[] { 1, 5, 29, 2 })
            {
                Fill(points, count); upload(points);
                Check(line.positionCount == count, "Spawn body uploader preserves growing/shrinking point count " + count);
                for (int index = 0; index < count; index++) Check(line.GetPosition(index) == points[index], "Spawn point remains exact " + count + ":" + index);
            }
        }

        private static void CheckAllocations(ArrowVisuals visuals, LineRenderer line)
        {
            var points = new List<Vector3>(128); Fill(points, 32); var head = points[points.Count - 1];
            var method = typeof(ArrowVisuals).GetMethod("UploadBodyPoints", BindingFlags.NonPublic | BindingFlags.Instance);
            var upload = (Action<List<Vector3>>)Delegate.CreateDelegate(typeof(Action<List<Vector3>>), visuals, method);
            var renderer = visuals.HeadTransform.GetComponent<SpriteRenderer>();
            for (int index = 0; index < 256; index++)
            { visuals.UpdateSlitheringBody(points, head, Quaternion.identity, true); upload(points); LegacyUpload(visuals, line, renderer, points, head); }
            const int iterations = 2048;
            int empty = QualityAllocationEvents.Measure(() => { });
            int legacy = QualityAllocationEvents.Measure(() =>
            { for (int index = 0; index < iterations; index++) LegacyUpload(visuals, line, renderer, points, head); });
            int slither = QualityAllocationEvents.Measure(() =>
            { for (int index = 0; index < iterations; index++) visuals.UpdateSlitheringBody(points, head, Quaternion.identity, true); });
            int spawn = QualityAllocationEvents.Measure(() =>
            { for (int index = 0; index < iterations; index++) upload(points); });
            Results.Add("MEASURED: Unity GC.Alloc events on the current thread; iterations=" + iterations +
                ", points=32, empty bracket=" + empty + ", original ToArray=" + legacy +
                ", current slither=" + slither + ", shared spawn uploader=" + spawn);
            Check(empty == 0, "Empty native recorder bracket observes no measurement overhead allocations");
            Check(legacy >= iterations, "Positive control: original ToArray path emits at least one GC.Alloc event per call");
            Check(slither == 0, "Warmed real slither method emits zero GC.Alloc events across 2048 calls");
            Check(spawn == 0, "Warmed shared spawn uploader emits zero GC.Alloc events across 2048 calls");
            var buffer = typeof(ArrowVisuals).GetField("_bodyUploadBuffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(visuals);
            Fill(points, 2); upload(points); Fill(points, 64); upload(points);
            Check(ReferenceEquals(buffer, typeof(ArrowVisuals).GetField("_bodyUploadBuffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(visuals)),
                "Smaller and previously warmed larger bodies reuse the same allocated buffer");
        }

        private static void LegacyUpload(ArrowVisuals visuals, LineRenderer line, SpriteRenderer renderer, List<Vector3> points, Vector3 head)
        {
            line.enabled = true; line.positionCount = points.Count; line.SetPositions(points.ToArray());
            visuals.HeadTransform.position = head; visuals.HeadTransform.rotation = Quaternion.identity; renderer.enabled = true;
        }

        private static void Fill(List<Vector3> points, int count)
        { points.Clear(); for (int index = 0; index < count; index++) points.Add(new Vector3(index * .2f, index % 3 * .3f, 0f)); }

        private static void WriteReport(string outcome)
        { Directory.CreateDirectory("Captures/QualityPass"); File.WriteAllText(Report, outcome + "\n" + string.Join("\n", Results) + "\n"); }

        private static void Check(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); Results.Add("PASS: " + message); }
    }
}
