namespace ArrowSwarm.Modes.Editor
{
    using UnityEditor;
    using UnityEngine;
    using U = ModeAuthoringUtility;

    /// <summary>Authors dense fifty-level campaigns while preserving existing level and catalog GUIDs.</summary>
    public static class PuzzleCampaignBuilder
    {
        /// <summary>Validates both complete campaigns in memory before updating existing level assets.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Build Dense 50-Level Campaigns")]
        public static void Build()
        {
            U.Folder(U.Data);
            if (!AssetDatabase.LoadAssetAtPath<PuzzleConfig>(U.Data + "PuzzleConfig.asset"))
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PuzzleConfig>(), U.Data + "PuzzleConfig.asset");
            var shapes = new PuzzleLevel[DenseShapeLibrary.Count]; var shifts = new PuzzleLevel[DenseShapeLibrary.Count];
            try
            {
                for (int i = 0; i < shapes.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Dense puzzle campaigns", "Validating level " + (i + 1) + "/50", i / 50f);
                    shapes[i] = DenseCampaignGeneration.CreateShape(i); shifts[i] = DenseCampaignGeneration.CreateColorShift(i);
                }
                for (int i = 0; i < shapes.Length; i++)
                {
                    shapes[i] = Persist("Shapes_" + (i + 1), shapes[i]);
                    shifts[i] = Persist("ColorShift_" + (i + 1), shifts[i]);
                }
                Catalog("ShapesCatalog", shapes); Catalog("ColorShiftCatalog", shifts); AssetDatabase.SaveAssets();
                DenseCampaignVerification.ValidateCatalogs();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                foreach (var level in shapes) if (level != null && !AssetDatabase.Contains(level)) UnityEngine.Object.DestroyImmediate(level);
                foreach (var level in shifts) if (level != null && !AssetDatabase.Contains(level)) UnityEngine.Object.DestroyImmediate(level);
            }
        }

        private static PuzzleLevel Persist(string name, PuzzleLevel generated)
        {
            string path = U.Data + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<PuzzleLevel>(path);
            if (existing == null) { generated.name = name; AssetDatabase.CreateAsset(generated, path); return generated; }
            existing.Author(generated.Mode, generated.Title, generated.Width, generated.Height, generated.Arrows,
                generated.WallColors, generated.TurnBudget, generated.SourceImage);
            EditorUtility.SetDirty(existing); UnityEngine.Object.DestroyImmediate(generated); return existing;
        }

        private static void Catalog(string name, PuzzleLevel[] levels)
        {
            string path = U.Data + name + ".asset"; var catalog = AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(path);
            if (!catalog) { catalog = ScriptableObject.CreateInstance<PuzzleCatalog>(); AssetDatabase.CreateAsset(catalog, path); }
            catalog.Author(levels); EditorUtility.SetDirty(catalog);
        }
    }
}
