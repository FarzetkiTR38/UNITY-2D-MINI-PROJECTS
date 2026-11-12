namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;
    using X = UiRollout04Kit;

    /// <summary>Authors separate tile states, live numbers and paired earned/empty stars without runtime styling.</summary>
    internal static partial class UiRollout04LevelsAuthoring
    {
        private static void Tile(Transform grid, int number, Sources source)
        {
            var button = U.Button(grid, "Level_" + number, null, Vector2.zero, new Vector2(195, 195), source.Tile, false);
            button.GetComponent<Image>().color = Color.clear;
            var unlocked = K.Rect(button.transform, "UnlockedState", Vector2.zero, new Vector2(195, 195));
            var blue = K.Image(unlocked, "Face", source.BlueTile, Vector2.zero, new Vector2(195, 195));
            blue.preserveAspect = true; button.targetGraphic = blue;
            var openLabel = Number(unlocked, "LevelText", number, true);
            var stars = K.Rect(unlocked, "StarsContainer", new Vector2(0, -57), new Vector2(185, 62));
            var earned = new GameObject[3]; var empty = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                var gold = K.Image(stars, "EarnedStar" + i, K.Sprite("star"), new Vector2((i - 1) * 56, 0), new Vector2(56, 58));
                gold.preserveAspect = true; earned[i] = gold.gameObject;
                var pale = K.Image(stars, "EmptyStar" + i, source.EmptyStar, new Vector2((i - 1) * 56, 0), new Vector2(56, 58));
                pale.preserveAspect = true; empty[i] = pale.gameObject;
                pale.color = new Color(.23f, .68f, 1f, .6f);
                gold.gameObject.SetActive(false);
            }
            var locked = K.Rect(button.transform, "LockedState", Vector2.zero, new Vector2(195, 195));
            var gray = K.Image(locked, "Face", source.GrayTile, Vector2.zero, new Vector2(195, 195));
            gray.preserveAspect = true; gray.material = K.Material("Ui04LockedTileLight");
            gray.gameObject.AddComponent<UIThemeCoordinates>();
            K.Theme(gray, Color.white, Color.white, K.Material("Ui04LockedTileDark"));
            var lockedLabel = Number(locked, "LevelText", number, false);
            var padlock = X.Icon(locked, "LockIcon", "lock", new Vector2(0, -42), new Vector2(76, 84));
            var owner = button.gameObject.AddComponent<LevelButtonUI>();
            U.Bind(owner, "_button", button); U.Bind(owner, "_levelText", openLabel);
            U.Bind(owner, "_backgroundImage", blue); U.Bind(owner, "_lockIcon", padlock.gameObject);
            U.Bind(owner, "_starsContainer", stars.gameObject);
            U.Bind(owner, "_unlockedState", unlocked.gameObject); U.Bind(owner, "_lockedState", locked.gameObject);
            U.Bind(owner, "_lockedLevelText", lockedLabel);
            U.BindArray(owner, "_earnedStarStates", earned); U.BindArray(owner, "_emptyStarStates", empty);
            U.Bind(owner, "_earnedStarSprite", K.Sprite("star")); U.Bind(owner, "_emptyStarSprite", source.EmptyStar);
            locked.gameObject.SetActive(false);
        }

        private static TextMeshProUGUI Number(Transform parent, string name, int number, bool unlocked)
        {
            var label = U.Text(parent, name, number.ToString(), new Vector2(0, 43), new Vector2(168, 84), unlocked ? 72 : 63);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            if (unlocked)
            {
                label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Fazo_Font_Settings.asset");
                label.fontSharedMaterial = label.font.material;
                label.fontStyle = FontStyles.Bold;
                K.Theme(label, Color.white, Color.white);
            }
            return label;
        }

        private static void CreateLockedMaterials()
        {
            LockedMaterial("Light", new Color(.78f, .81f, .86f));
            LockedMaterial("Dark", new Color(.15f, .22f, .33f));
        }

        private static void CreateFrameMaterial()
        {
            string path = K.MaterialFolder + "/Ui04BoardDark.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(K.Material("Ui02PanelDark")) { name = "Ui04BoardDark" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetFloat("_RepaintInterior", 1);
            material.SetVector("_SurfaceInset", new Vector4(.05f, .03f, 0, 0));
            material.SetVector("_Corner", new Vector4(.075f, .045f, 0, 0));
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
        }

        private static void LockedMaterial(string palette, Color surface)
        {
            string path = K.MaterialFolder + "/Ui04LockedTile" + palette + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(K.Material("DarkPanel")) { name = "Ui04LockedTile" + palette };
                AssetDatabase.CreateAsset(material, path);
            }
            // The neutral raster's original large lock is repainted with the complete interior.
            // The separate true-alpha lock below the TMP number supplies the only visible padlock.
            material.SetFloat("_Artwork", 0); material.SetFloat("_SolidFill", 1);
            material.SetFloat("_SurfaceStrength", 1); material.SetFloat("_EdgeFill", 1);
            material.SetFloat("_OpaqueInterior", 0); material.SetFloat("_BevelStrength", 1);
            material.SetFloat("_LiftInk", 0); material.SetFloat("_Background", 0);
            material.SetVector("_SurfaceInset", new Vector4(.07f, .07f, 0, 0));
            material.SetVector("_Corner", new Vector4(.12f, .12f, 0, 0));
            material.SetColor("_SurfaceColor", surface); material.SetColor("_Color", Color.white);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
        }
    }
}
