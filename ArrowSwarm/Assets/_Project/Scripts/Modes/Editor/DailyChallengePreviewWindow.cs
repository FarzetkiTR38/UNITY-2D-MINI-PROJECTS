namespace ArrowSwarm.Modes.Editor
{
    using System;
    using ArrowSwarm.Core;
    using ArrowSwarm.Marketing.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    internal sealed class DailyChallengePreviewWindow : EditorWindow
    {
        [MenuItem("Arrow Swarm/Playtest/Daily Challenge")]
        private static void Open() => GetWindow<DailyChallengePreviewWindow>("Daily Challenge");

        private void OnInspectorUpdate() => Repaint();

        private void OnGUI()
        {
            var active = DailyChallengePreview.Active;
            var today = DailyChallengePreview.Select(DateTimeOffset.UtcNow);
            var board = active ?? today;
            EditorGUILayout.LabelField("Günün Bulmacası", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("UTC tarihi", board.Date.ToString("yyyy-MM-dd"));
            EditorGUILayout.LabelField("Tahta", $"{GameModeSelection.DisplayName(board.Mode)} · Bölüm {board.Level}");
            EditorGUILayout.LabelField("Kişisel rekor", $"{board.BestStars} / 3 yıldız");
            EditorGUILayout.Space();

            if (active.HasValue)
            {
                using (new EditorGUI.DisabledScope(DailyChallengePreview.Transitioning))
                {
                    if (GUILayout.Button("Tekrar dene")) DailyChallengePreview.Retry();
                    if (GUILayout.Button("Ana menüye dön")) GameManager.Instance.GoToMainMenu();
                }
            }
            else if (DailyChallengePreview.Ready)
            {
                if (GUILayout.Button("Günün tahtasını aç")) DailyChallengePreview.Start(DateTimeOffset.UtcNow);
            }
            else if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox(DailyChallengePreview.IsIsolated ? "Ana menünün hazır olmasını bekle." :
                    "Yalıtılmış oturumu başlatmak için Play Mode’dan çık.", MessageType.Info);
            }
            else
            {
                var scene = SceneManager.GetActiveScene();
                bool canPlay = scene.name == "BootScene" && !scene.isDirty && !DailyChallengePreview.IsIsolated;
                if (!canPlay)
                    EditorGUILayout.HelpBox("Başlamak için kaydedilmiş BootScene’i aç ve varsa önceki denemeyi bitir.", MessageType.Info);
                using (new EditorGUI.DisabledScope(!canPlay))
                {
                    if (GUILayout.Button("Yalıtılmış Play Mode’u başlat"))
                    {
                        MarketingCapture.Arm();
                        EditorApplication.EnterPlaymode();
                    }
                }
            }
        }
    }
}
