namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Core;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Observes actual overlay ownership, closed midpoints, completed input release and rendered frames.</summary>
    public static partial class MenuTransitionVerification
    {
        private static readonly List<CapturedFrame> Captures = new List<CapturedFrame>();
        private static readonly float[] ClosingTargets = { .2f, .4f, .6f, .8f };
        private static readonly float[] OpeningTargets = { .85f, .65f, .45f, .25f, .05f };
        private static SceneTransitionManager _captureManager;
        private static bool _capturing, _midpointReached;
        private static int _closingCapture, _openingCapture, _lastCaptureFrame;
        private static string _captureError;

        private static T Field<T>(object owner, string name) =>
            (T)owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);

        private static void VerifyCovered(string context)
        {
            var manager = SceneTransitionManager.Instance;
            var image = Field<Image>(manager, "_overlayImage");
            Check(manager.IsTransitioning && image.gameObject.activeInHierarchy && image.raycastTarget &&
                image.material.GetFloat("_Progress") == 1f, context + " is fully covered and blocks input");
        }

        private static void VerifyComplete(string context)
        {
            var manager = SceneTransitionManager.Instance;
            var image = Field<Image>(manager, "_overlayImage");
            Check(!manager.IsTransitioning && !image.gameObject.activeSelf && !image.raycastTarget &&
                image.material.GetFloat("_Progress") == 0f && Field<Coroutine>(manager, "_currentTransition") == null,
                context + " clears progress, overlay, coroutine and input blocking");
        }

        private static void VerifyMaterial(string context)
        {
            var image = Field<Image>(SceneTransitionManager.Instance, "_overlayImage");
            Check(image.material == _scope.Runtime && RuntimeCount() == 1, context + " reuses the same single runtime clone");
            Check(EditorJsonUtility.ToJson(_scope.Source) == _scope.SourceJson, context + " preserves exact authored material JSON");
        }

        private static int RuntimeCount()
        {
            int count = 0;
            foreach (var material in Resources.FindObjectsOfTypeAll<Material>())
                if (material.name == "Mat_Runtime_" + _scope.Source.name && material.shader == _scope.Source.shader) count++;
            return count;
        }

        private static void ObserveRender()
        {
            if (!_running || !_capturing || _captureManager == null || !_captureManager.IsTransitioning ||
                _lastCaptureFrame == Time.frameCount) return;
            try
            {
                var image = Field<Image>(_captureManager, "_overlayImage");
                float progress = image.material.GetFloat("_Progress");
                if (!_midpointReached && _closingCapture < ClosingTargets.Length && progress >= ClosingTargets[_closingCapture])
                    Capture("closing-" + (++_closingCapture), progress);
                else if (_midpointReached && _openingCapture < OpeningTargets.Length && progress > 0 && progress <= OpeningTargets[_openingCapture])
                    Capture("opening-" + (++_openingCapture), progress);
            }
            catch (Exception error) { _captureError = error.Message; _capturing = false; }
        }

        private static void Capture(string label, float progress)
        {
            string path = Output + "/transition-" + label + "-" + DateTime.UtcNow.Ticks + ".png";
            _lastCaptureFrame = Time.frameCount;
            ScreenCapture.CaptureScreenshot(path);
            Captures.Add(new CapturedFrame(path, progress, Time.frameCount));
        }

        private static bool CaptureFilesReady()
        {
            if (Time.frameCount <= _lastCaptureFrame) return false;
            foreach (var capture in Captures)
                if (!File.Exists(capture.Path) || new FileInfo(capture.Path).Length == 0) return false;
            return true;
        }

        /// <summary>Records the unique screenshot path and the real rendered transition state.</summary>
        private readonly struct CapturedFrame
        {
            internal readonly string Path;
            internal readonly float Progress;
            internal readonly int Frame;
            internal CapturedFrame(string path, float progress, int frame) { Path = path; Progress = progress; Frame = frame; }
        }
    }
}
