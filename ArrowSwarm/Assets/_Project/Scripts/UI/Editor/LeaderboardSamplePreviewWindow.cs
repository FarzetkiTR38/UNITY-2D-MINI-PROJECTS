namespace ArrowSwarm.UI.Editor
{
    using System;
    using ArrowSwarm.Data;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Discoverable opt-in Editor controls for sample standings, kept out of player builds.</summary>
    public sealed class LeaderboardSamplePreviewWindow : EditorWindow
    {
        private bool _onBoard = true;
        private string _error;

        /// <summary>Opens sample controls without automatically replacing any existing live leaderboard.</summary>
        [MenuItem("ArrowSwarm/Preview/Leaderboard Samples")]
        public static void Open() => GetWindow<LeaderboardSamplePreviewWindow>("Leaderboard Samples");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Leaderboard • Sample Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Örnek rakipler yalnızca Editor önizlemesidir. Gerçek skor, cache veya oyuncu ilerlemesi olarak kaydedilmez.", MessageType.Info);
            EditorGUILayout.LabelField("1. BootScene üzerinden yeni Play başlat.\n2. Gerçek sıralamayı açmadan örnekleri göster.", EditorStyles.wordWrappedLabel);
            bool next = EditorGUILayout.Toggle("Player inside top 10 (#7)", _onBoard);
            if (next != _onBoard)
            {
                _onBoard = next;
                if (LeaderboardSamplePreview.IsActive) Invoke(() => LeaderboardSamplePreview.SetScenario(_onBoard));
            }
            using (new EditorGUI.DisabledScope(!Application.isPlaying || LeaderboardSamplePreview.IsActive))
                if (GUILayout.Button("Show 10 sample players")) Invoke(() => LeaderboardSamplePreview.Begin(_onBoard));
            using (new EditorGUI.DisabledScope(!LeaderboardSamplePreview.IsActive))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Light")) Invoke(() => LeaderboardSamplePreview.SetTheme(ThemeMode.Light));
                if (GUILayout.Button("Dark")) Invoke(() => LeaderboardSamplePreview.SetTheme(ThemeMode.Dark));
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("End Preview / Gerçek veriye dön")) Invoke(LeaderboardSamplePreview.End);
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(LeaderboardSamplePreview.Status, EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("Pencereyi veya sıralamayı kapatınca, sahne değişince ya da Play durunca önizleme sonlanır.", EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrEmpty(_error)) EditorGUILayout.HelpBox(_error, MessageType.Warning);
        }

        private void Invoke(Action action)
        { try { action(); _error = null; } catch (Exception error) { _error = error.Message; } Repaint(); }

        private void OnDisable() => LeaderboardSamplePreview.End();
    }
}
