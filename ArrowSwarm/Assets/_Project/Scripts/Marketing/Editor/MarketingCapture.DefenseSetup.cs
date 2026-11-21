namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Path;
    using UnityEditor;
    using UnityEngine;
    using Arrow = ArrowSwarm.Arrow.Arrow;

    /// <summary>Private debug-state isolation and production-only shot/control selection.</summary>
    public static partial class MarketingCapture
    {
        private static ArrowSwarm.Debug.DebugManager _defenseDebug;
        private static string _defenseDebugJson;
        private static bool _defenseHadDebug;
        private static void PrepareDefenseDebug()
        {
            _defenseHadDebug = ArrowSwarm.Debug.DebugManager.HasInstance;
            _defenseDebug = ArrowSwarm.Debug.DebugManager.Instance;
            _defenseDebugJson = EditorJsonUtility.ToJson(_defenseDebug);
            typeof(ArrowSwarm.Debug.DebugManager).GetField("_useDebugLevel", Private).SetValue(_defenseDebug, false);
            typeof(ArrowSwarm.Debug.DebugManager).GetField("_infiniteLives", Private).SetValue(_defenseDebug, false);
            typeof(ArrowSwarm.Debug.DebugManager).GetField("_infiniteTips", Private).SetValue(_defenseDebug, false);
            typeof(ArrowSwarm.Debug.DebugManager).GetField("_gameSpeedMultiplier", Private).SetValue(_defenseDebug, 1f);
        }
        private static void RestoreDefenseDebug()
        {
            if (_defenseDebug == null) return;
            if (_defenseHadDebug) EditorJsonUtility.FromJsonOverwrite(_defenseDebugJson, _defenseDebug);
            else UnityEngine.Object.DestroyImmediate(_defenseDebug);
            _defenseDebug = null; _defenseDebugJson = null;
        }
        private static void InvokeDefenseFreeze()
        {
            var hud = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.GameHUD>();
            var field = typeof(ArrowSwarm.UI.GameHUD).GetField("_skill2FreezeButton", Private);
            var button = hud == null ? null : field.GetValue(hud) as UnityEngine.UI.Button;
            if (button == null || !button.interactable) throw new InvalidOperationException("Native Freeze button unavailable.");
            button.onClick.Invoke();
        }
        private static Arrow NextDefenseArrow()
        {
            Arrow result = null; float longest = -1;
            foreach (var arrow in ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows)
            {
                if (ArrowSwarm.Arrow.ArrowSpawner.Instance.RemainingArrows > 1 && DefenseArrowIndex(arrow) == _defenseReservedArrow) continue;
                if (arrow == null || arrow.IsFired || arrow.IsBlockedAnimating ||
                    !GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection)) continue;
                var route = PathManager.Instance.GetArrowPathWaypoints(GridManager.Instance.GetGridExitPoint(arrow.HeadPoint, arrow.HeadDirection));
                float length = 0;
                for (int i = 1; i < route.Count; i++) length += Vector2.Distance(route[i - 1], route[i]);
                if (length > longest) { longest = length; result = arrow; }
            }
            return result;
        }
        [Serializable] private sealed class DefenseGeometry
        {
            public string mapAsset;
            public int level, totalArrows;
            public float pathLength, baseMobSpeed, configuredFreezeDuration;
            public Vector2 freezeButton;
            public List<DefenseArrowGeometry> arrows = new List<DefenseArrowGeometry>();
        }
        [Serializable] private sealed class DefenseArrowGeometry
        {
            public int index, weight;
            public string direction;
            public List<Vector2Int> points;
            public Vector2 screenHead;
        }
        private static void ExportDefenseGeometry(int level)
        {
            var hud = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.GameHUD>();
            var button = (UnityEngine.UI.Button)typeof(ArrowSwarm.UI.GameHUD).GetField("_skill2FreezeButton", Private).GetValue(hud);
            var screen = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
            var result = new DefenseGeometry { level = level, totalArrows = ArrowSwarm.Arrow.ArrowSpawner.Instance.TotalArrows,
                mapAsset = AssetDatabase.GetAssetPath(LevelManager.Instance.CurrentLevelData.Map), pathLength = PathManager.Instance.TotalPathLength,
                baseMobSpeed = ArrowSwarm.Mob.MobSpawner.Instance.BaseMobSpeed, configuredFreezeDuration = GameManager.Instance.Config.FreezeDuration,
                freezeButton = new Vector2(screen.x, Screen.height - screen.y) };
            var arrows = ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows;
            for (int i = 0; i < arrows.Count; i++) result.arrows.Add(new DefenseArrowGeometry { index = i, weight = arrows[i].Weight,
                direction = arrows[i].HeadDirection.ToString(), points = new List<Vector2Int>(arrows[i].PathPoints),
                screenHead = DefenseScreenHead(arrows[i]) });
            File.WriteAllText(_folder + "/geometry.json", JsonUtility.ToJson(result, true));
        }
    }
}
