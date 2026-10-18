namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using ArrowSwarm.Puzzle;
    using UnityEngine;

    /// <summary>Independent occupancy, dependency and camera metrics accompanying real initial-board screenshots.</summary>
    public static partial class ShapesRevision3Verification
    {
        private static readonly List<MetricRow> Rows = new List<MetricRow>();
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        private static MetricRow Analyze(PuzzleLevel level, int number)
        {
            Require(level.Mode == GameModeId.Shapes, "unexpected mode in Shapes catalog");
            Require(PuzzleRules.Validate(level) == null, level.Title + ": invalid path geometry");
            Require(level.CoverageMask != null && level.CoverageMask.Count > 0, "missing authored silhouette mask");
            var intended = new HashSet<Vector2Int>(level.CoverageMask);
            Require(intended.Count == level.CoverageMask.Count, "duplicate authored coverage cells");
            var occupied = new HashSet<Vector2Int>(); var colors = new HashSet<Color32>();
            int minX = level.Width, minY = level.Height, maxX = 0, maxY = 0, maxPath = 0;
            foreach (var arrow in level.Arrows)
            {
                maxPath = Mathf.Max(maxPath, arrow.Points.Count); colors.Add((Color32)arrow.Color);
                foreach (var p in arrow.Points)
                {
                    Require(PuzzleRules.Inside(level, p), "path outside board");
                    Require(intended.Contains(p) && occupied.Add(p), "overlap or path outside intended silhouette");
                    minX = Mathf.Min(minX, p.x); minY = Mathf.Min(minY, p.y);
                    maxX = Mathf.Max(maxX, p.x); maxY = Mathf.Max(maxY, p.y);
                }
            }
            Require(occupied.SetEquals(intended), "silhouette has unoccupied cells");
            var graph = DenseDependencyGraph.Analyze(level.Arrows, level.Width, level.Height);
            Require(graph.Solvable && graph.MaximumDepth > 1, "missing acyclic dependencies");
            Require(graph.DirectionCount >= 3 && graph.InwardHeads > 0, "missing inward heads or directional variety");
            return new MetricRow(number, level, occupied.Count, maxX - minX + 1, maxY - minY + 1, colors.Count, maxPath, graph);
        }

        private static void WriteMetrics()
        {
            Directory.CreateDirectory(Folder);
            var csv = new StringBuilder("level,title,grid_width,grid_height,shape_width,shape_height,covered_cells,arrows,average_path_cells,max_path_cells,colors,blocked_arrows,dependency_depth,inward_heads,directions,stroke_world,stroke_pixels,capture_width,capture_height,peak_concurrent_shots,actual_taps,light_capture,dark_capture\n");
            var json = new StringBuilder("[\n");
            for (int i = 0; i < Rows.Count; i++)
            {
                csv.Append(Rows[i].Csv()).Append('\n');
                if (i > 0) json.Append(",\n"); json.Append(Rows[i].Json());
            }
            json.Append("\n]\n");
            File.WriteAllText(Folder + "/level-metrics.csv", csv.ToString());
            File.WriteAllText(Folder + "/level-metrics.json", json.ToString());
        }

        /// <summary>Private immutable level metrics plus observed camera and runtime verification values.</summary>
        private sealed class MetricRow
        {
            private readonly int _number, _width, _height, _shapeWidth, _shapeHeight, _cells, _arrows, _colors, _maxPath;
            private readonly int _blocked, _depth, _inward, _directions;
            private readonly string _title;
            private float _stroke, _strokePixels;
            private int _captureWidth, _captureHeight, _peak, _taps;
            internal MetricRow(int number, PuzzleLevel level, int cells, int shapeWidth, int shapeHeight, int colors,
                int maxPath, DenseDependencyMetrics graph)
            {
                _number = number; _title = level.Title; _width = level.Width; _height = level.Height;
                _shapeWidth = shapeWidth; _shapeHeight = shapeHeight; _cells = cells; _arrows = level.Arrows.Count;
                _colors = colors; _maxPath = maxPath; _blocked = graph.BlockedArrows; _depth = graph.MaximumDepth;
                _inward = graph.InwardHeads; _directions = graph.DirectionCount;
            }
            internal void SetDisplay(PuzzleSession session)
            {
                _stroke = session.Config.StrokeWidth; _captureWidth = Screen.width; _captureHeight = Screen.height;
                var camera = UnityEngine.Camera.main;
                _strokePixels = camera == null ? 0 : _stroke * _captureHeight / (camera.orthographicSize * 2);
            }
            internal void SetResult(int peak, int taps) { _peak = peak; _taps = taps; }
            private string[] Values() => new[]
            {
                N(_number), _title, N(_width), N(_height), N(_shapeWidth), N(_shapeHeight), N(_cells), N(_arrows),
                ((float)_cells / _arrows).ToString("F2", Invariant), N(_maxPath), N(_colors), N(_blocked), N(_depth),
                N(_inward), N(_directions), _stroke.ToString("F3", Invariant), _strokePixels.ToString("F2", Invariant),
                N(_captureWidth), N(_captureHeight), N(_peak), N(_taps),
                CaptureRelative(_number, "light"), CaptureDark(_number) ? CaptureRelative(_number, "dark") : ""
            };
            internal string Csv()
            {
                var values = Values(); for (int i = 0; i < values.Length; i++) values[i] = "\"" + values[i].Replace("\"", "\"\"") + "\"";
                return string.Join(",", values);
            }
            internal string Json()
            {
                var values = Values(); var text = new StringBuilder("  {");
                string[] keys = { "level", "title", "grid_width", "grid_height", "shape_width", "shape_height", "covered_cells", "arrows", "average_path_cells", "max_path_cells", "colors", "blocked_arrows", "dependency_depth", "inward_heads", "directions", "stroke_world", "stroke_pixels", "capture_width", "capture_height", "peak_concurrent_shots", "actual_taps", "light_capture", "dark_capture" };
                for (int i = 0; i < values.Length; i++)
                {
                    if (i > 0) text.Append(','); text.Append('"').Append(keys[i]).Append("\":");
                    if (i == 1 || i >= 21) text.Append('"').Append(values[i].Replace("\\", "\\\\").Replace("\"", "\\\"")).Append('"');
                    else text.Append(values[i]);
                }
                return text.Append('}').ToString();
            }
            private static string N(int number) => number.ToString(Invariant);
        }
    }
}
