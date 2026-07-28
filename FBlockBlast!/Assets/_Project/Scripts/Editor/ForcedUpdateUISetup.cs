#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.UI;
using NeonGalaxy.Boot;
using UnityEditor.SceneManagement;

namespace NeonGalaxy.EditorScripts
{
    public static class ForcedUpdateUISetup
    {
        [MenuItem("NeonGalaxy/Setup Forced Update UI")]
        public static void SetupUI()
        {
            var bootManager = Object.FindFirstObjectByType<BootManager>();
            if (bootManager == null)
            {
                EditorUtility.DisplayDialog("Error", "BootManager not found. Lütfen önce Boot sahnesini açın.", "OK");
                return;
            }

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "Canvas bulunamadı. Lütfen Boot sahnesine bir Canvas ekleyin.", "OK");
                return;
            }

            // Create ForcedUpdatePopup
            GameObject popupObj = new GameObject("ForcedUpdatePopup");
            popupObj.transform.SetParent(canvas.transform, false);

            RectTransform popupRect = popupObj.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.sizeDelta = Vector2.zero;
            popupRect.anchoredPosition = Vector2.zero;

            // Add background Image
            Image bgImage = popupObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.98f); // Koyu arka plan

            CanvasGroup cg = popupObj.AddComponent<CanvasGroup>();

            // Create Text
            GameObject textObj = new GameObject("MessageText");
            textObj.transform.SetParent(popupObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.5f);
            textRect.anchorMax = new Vector2(0.9f, 0.8f);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "YENİ SÜRÜM MEVCUT!\n\nOyuna devam etmek için lütfen güncelleyin.";
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 24;
            tmpText.fontSizeMax = 64;

            // Create Button
            GameObject btnObj = new GameObject("UpdateButton");
            btnObj.transform.SetParent(popupObj.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.2f, 0.2f);
            btnRect.anchorMax = new Vector2(0.8f, 0.35f);
            btnRect.sizeDelta = Vector2.zero;
            btnRect.anchoredPosition = Vector2.zero;

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.8f, 0.3f, 1f); // Yeşil ton

            Button btn = btnObj.AddComponent<Button>();

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "GÜNCELLE";
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.enableAutoSizing = true;
            btnText.fontSizeMin = 24;
            btnText.fontSizeMax = 60;
            btnText.fontStyle = FontStyles.Bold;

            // Add ForcedUpdatePopup script
            ForcedUpdatePopup popupScript = popupObj.AddComponent<ForcedUpdatePopup>();

            // Bind variables via SerializedObject
            SerializedObject so = new SerializedObject(popupScript);
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.FindProperty("updateButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            // Bind to BootManager
            SerializedObject bootSo = new SerializedObject(bootManager);
            bootSo.FindProperty("forcedUpdatePopup").objectReferenceValue = popupScript;
            bootSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(bootManager.gameObject.scene);

            Selection.activeGameObject = popupObj;
            
            EditorUtility.DisplayDialog("Başarılı", "Zorunlu Güncelleme UI (ForcedUpdatePopup) başarıyla oluşturuldu ve BootManager'a bağlandı!", "Harika!");
        }
    }
}
#endif
