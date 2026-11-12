namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Targeted scene-authored follow-up; never rebuilds approved panels or writes player data.</summary>
    public static class Ui04RequestedRevisions
    {
        /// <summary>Compacts the existing Boot credit without replacing shared artwork.</summary>
        public static void Boot()
        {
            RequireEdit(); EditorSceneManager.OpenScene("Assets/_Project/Scenes/BootScene.unity");
            foreach (var rect in UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                int side = rect.name.EndsWith("Left") ? -1 : 1;
                if (rect.name == "StudioStarLeft" || rect.name == "StudioStarRight")
                    rect.anchoredPosition = new Vector2(side * 164, rect.anchoredPosition.y);
                if (rect.name == "StudioRuleLeft" || rect.name == "StudioRuleRight")
                    rect.anchoredPosition = new Vector2(side * 109, rect.anchoredPosition.y);
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        /// <summary>Shares the authored campaign card, restores classic tile locks and contains podium accents.</summary>
        public static void Menu()
        {
            RequireEdit(); EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenuScene.unity");
            var owner = UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>(FindObjectsInactive.Include);
            var f = new SerializedObject(owner); var views = f.FindProperty("_modeViews");
            var source = views.GetArrayElementAtIndex(0); var target = views.GetArrayElementAtIndex(2);
            foreach (string field in new[] { "_root", "_dialogBox", "_close", "_previous", "_next", "_page", "_grid" })
                target.FindPropertyRelative(field).objectReferenceValue = source.FindPropertyRelative(field).objectReferenceValue;
            var card = (RectTransform)source.FindPropertyRelative("_dialogBox").objectReferenceValue;
            var identity = card.Find("ColorShiftIdentity");
            if (identity == null)
            {
                identity = K.Rect(card, "ColorShiftIdentity", Vector2.zero, new Vector2(1000, 2300));
                K.Image(identity, "ModeEmblem", K.Sprite("icon_color_shift"), new Vector2(-230,440), new Vector2(135,135));
                U.Text(identity, "ModeLabel", "COLOR SHIFT", new Vector2(105,440), new Vector2(410,108), 67, "mode_color_shift");
            }
            identity.gameObject.SetActive(false);
            target.FindPropertyRelative("_identityRoot").objectReferenceValue = identity.gameObject;
            f.ApplyModifiedPropertiesWithoutUndo();
            owner.transform.Find("Ui04LegacyLevels").gameObject.SetActive(false);
            var prefab = (LevelButtonUI)f.FindProperty("_levelButtonPrefab").objectReferenceValue;
            var old = new SerializedObject(prefab);
            var oldLock = (GameObject)old.FindProperty("_lockIcon").objectReferenceValue;
            foreach (var tile in card.GetComponentsInChildren<LevelButtonUI>(true))
            {
                var tileFields = new SerializedObject(tile);
                var locked = tile.transform.Find("LockedState");
                var face = locked.Find("Face").GetComponent<Image>();
                foreach (var component in face.GetComponents<MonoBehaviour>())
                    if (component.GetType().Name.Contains("Theme")) UnityEngine.Object.DestroyImmediate(component);
                face.sprite = oldLock.GetComponent<Image>().sprite; face.material = null;
                locked.Find("LevelText").gameObject.SetActive(false);
                locked.Find("LockIcon").gameObject.SetActive(false);
                tileFields.FindProperty("_lockedLevelText").objectReferenceValue = null;
                tileFields.FindProperty("_lockIcon").objectReferenceValue = null;
                tileFields.ApplyModifiedPropertiesWithoutUndo();
                var stars = (RectTransform)tile.transform.Find("UnlockedState/StarsContainer");
                stars.anchoredPosition = new Vector2(0,-53); stars.sizeDelta = new Vector2(160,50);
                foreach (RectTransform star in stars)
                {
                    int index = star.name[star.name.Length-1]-'0';
                    star.anchoredPosition = new Vector2((index-1)*47,0); star.sizeDelta = new Vector2(46,48);
                }
            }
            foreach (var row in UnityEngine.Object.FindObjectsByType<LeaderboardEntryUI>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var crown = row.transform.Find("CrownIcon"); if (crown == null) continue;
                var rect = (RectTransform)crown; rect.anchoredPosition = new Vector2(-415,35); rect.sizeDelta = new Vector2(32,32);
                foreach (string path in new[] { "NormalValues/RankText", "HighlightedValues/RankText" })
                {
                    var rank = row.transform.Find(path); if (rank == null) continue;
                    if (crown.gameObject.activeSelf) ((RectTransform)rank).anchoredPosition = new Vector2(-415,-13);
                }
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        private static void RequireEdit()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Use idle Edit Mode.");
        }
    }
}
