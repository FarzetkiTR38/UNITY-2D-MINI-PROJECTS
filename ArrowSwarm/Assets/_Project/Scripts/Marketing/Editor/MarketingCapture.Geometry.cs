namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEngine;

    /// <summary>Native camera mapping and production-rule evidence for truthful editorial quiz markers.</summary>
    public static partial class MarketingCapture
    {
        [Serializable] private sealed class Geometry
        {
            public string title, levelAsset;
            public int width, height, screenWidth, screenHeight;
            public Vector2 gridZero, gridX, gridY;
            public List<int> witness;
            public List<ArrowEvidence> arrows = new List<ArrowEvidence>();
        }
        [Serializable] private sealed class ArrowEvidence
        {
            public int index;
            public bool initiallyRemovable;
            public float obstacleDistance;
            public Vector2Int direction;
            public List<Vector2Int> points;
        }

        private static void ExportGeometry(PuzzleSession session, PuzzleSolution solution)
        {
            var level = session.Level;
            var camera = UnityEngine.Camera.main;
            if (camera == null) throw new InvalidOperationException("Native puzzle camera missing.");
            var origin = new Vector3(-(level.Width - 1) * .5f, -(level.Height - 1) * .5f, 0);
            var data = new Geometry { title = level.Title, levelAsset = UnityEditor.AssetDatabase.GetAssetPath(level),
                width = level.Width, height = level.Height, screenWidth = Screen.width, screenHeight = Screen.height,
                gridZero = ScreenPoint(camera, origin), gridX = ScreenPoint(camera, origin + Vector3.right),
                gridY = ScreenPoint(camera, origin + Vector3.up), witness = new List<int>(solution.Steps) };
            var removed = new bool[level.Arrows.Count];
            for (int i = 0; i < level.Arrows.Count; i++)
            {
                float distance = PuzzleRules.ObstacleDistance(level, removed, i);
                data.arrows.Add(new ArrowEvidence { index = i,
                    initiallyRemovable = PuzzleRules.CanRemove(level, removed, i, level.InitialOrientation),
                    obstacleDistance = float.IsPositiveInfinity(distance) ? -1 : distance,
                    direction = level.Arrows[i].Direction, points = new List<Vector2Int>(level.Arrows[i].Points) });
            }
            File.WriteAllText(_folder + "/geometry.json", JsonUtility.ToJson(data, true));
        }

        private static Vector2 ScreenPoint(UnityEngine.Camera camera, Vector3 world)
        {
            Vector3 point = camera.WorldToScreenPoint(world);
            return new Vector2(point.x, Screen.height - point.y);
        }
    }
}
