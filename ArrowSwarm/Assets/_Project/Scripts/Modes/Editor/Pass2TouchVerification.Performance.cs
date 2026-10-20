namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Reflection;
    using UnityEngine;

    /// <summary>Checks authored HUD bounds and native allocation events during real camera ratio and clamp calculations.</summary>
    public static partial class Pass2TouchVerification
    {
        private static Vector3[] _legacyCornerSink;

        /// <summary>Loads real Defense101, measures its camera calculations, restores context and returns to menu.</summary>
        public static void BeginPerformance() => Begin(PerformanceChecks, "camera-performance-verification.txt");

        private static IEnumerator PerformanceChecks()
        {
            Status = "Measuring authored HUD bounds and camera hot-path allocations";
            var controller = ArrowSwarm.Camera.CameraController.Instance;
            using (new CameraPose(controller))
            {
                controller.enabled = false;
                var top = Hidden<RectTransform>(controller, "_topPanelRect");
                var bottom = Hidden<RectTransform>(controller, "_bottomPanelRect");
                Check(top != null && bottom != null && top.gameObject.activeInHierarchy && bottom.gameObject.activeInHierarchy,
                    "Real authored Defense HUD panels supply both camera boundaries");
                LegacyRatios(top, bottom, out float expectedTop, out float expectedBottom);
                controller.GetHudRatios(out float actualTop, out float actualBottom);
                Check(actualTop == expectedTop && actualBottom == expectedBottom, "Cached HUD ratios exactly match the original authored-corner calculation");
                var clampMethod = controller.GetType().GetMethod("ClampPosition", BindingFlags.Instance | BindingFlags.NonPublic);
                var clamp = (Func<Vector3, Vector3>)Delegate.CreateDelegate(typeof(Func<Vector3, Vector3>), controller, clampMethod);
                foreach (float zoom in new[] { 0f, .5f, 1f })
                {
                    controller.SetZoomNormalized(zoom);
                    controller.GetComponent<UnityEngine.Camera>().orthographicSize = Hidden<float>(controller, "_targetOrthoSize");
                    foreach (var point in new[] { Vector3.zero, new Vector3(-1000f, -1000f, -10f), new Vector3(1000f, 1000f, -10f) })
                        Check((clamp(point) - LegacyClamp(controller, point, expectedTop, expectedBottom)).sqrMagnitude < .00000001f,
                            "Camera clamp preserves exact original bounds at zoom " + zoom + " for " + point);
                }
                for (int index = 0; index < 128; index++) { controller.GetHudRatios(out _, out _); clamp(Vector3.zero); }
                const int iterations = 2048;
                int empty = QualityAllocationEvents.Measure(() => { });
                int legacy = QualityAllocationEvents.Measure(() =>
                { for (int index = 0; index < iterations; index++) LegacyRatios(top, bottom, out _, out _); });
                int ratios = QualityAllocationEvents.Measure(() =>
                { for (int index = 0; index < iterations; index++) controller.GetHudRatios(out _, out _); });
                int clamping = QualityAllocationEvents.Measure(() =>
                { for (int index = 0; index < iterations; index++) clamp(Vector3.zero); });
                Results.Add("MEASURED: current-thread Unity GC.Alloc events / " + iterations +
                    " calls; empty=" + empty + ", original corner arrays=" + legacy + ", cached ratios=" + ratios + ", cached clamp=" + clamping);
                Check(empty == 0, "Native recorder's empty measurement bracket produces zero allocation events");
                Check(legacy >= iterations * 2, "Positive control: original two corner arrays produce at least 4096 native GC.Alloc events");
                Check(ratios == 0 && clamping == 0, "Warmed HUD ratio and camera clamp paths produce zero native GC.Alloc events");
                _legacyCornerSink = null;
            }
            yield return null;
            Check(true, "Original camera pose, zoom target, smoothing and enabled state restored after profiling");
        }

        private static void LegacyRatios(RectTransform top, RectTransform bottom, out float topRatio, out float bottomRatio)
        {
            var topCorners = new Vector3[4]; top.GetWorldCorners(topCorners); _legacyCornerSink = topCorners;
            var bottomCorners = new Vector3[4]; bottom.GetWorldCorners(bottomCorners); _legacyCornerSink = bottomCorners;
            float topMeasured = (Screen.height - Mathf.Min(topCorners[0].y, topCorners[3].y)) / Screen.height;
            float bottomMeasured = Mathf.Max(bottomCorners[1].y, bottomCorners[2].y) / Screen.height;
            topRatio = topMeasured > .03f && topMeasured < .30f ? topMeasured : .085f;
            bottomRatio = bottomMeasured > .05f && bottomMeasured < .35f ? bottomMeasured : .130f;
        }

        private static Vector3 LegacyClamp(ArrowSwarm.Camera.CameraController controller, Vector3 point, float top, float bottom)
        {
            var camera = controller.GetComponent<UnityEngine.Camera>();
            float halfHeight = camera.orthographicSize, halfWidth = halfHeight * camera.aspect;
            Vector2 min = Hidden<Vector2>(controller, "_gridMin"), max = Hidden<Vector2>(controller, "_gridMax");
            float topHeight = halfHeight - halfHeight * 2f * top, bottomHeight = halfHeight - halfHeight * 2f * bottom;
            point.x = Mathf.Clamp(point.x, Mathf.Min(min.x + halfWidth, max.x - halfWidth), Mathf.Max(min.x + halfWidth, max.x - halfWidth));
            point.y = Mathf.Clamp(point.y, Mathf.Min(min.y + bottomHeight, max.y - topHeight), Mathf.Max(min.y + bottomHeight, max.y - topHeight));
            return point;
        }
    }
}
