namespace ArrowSwarm.UI.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Removes only tiny rank ornaments and replaces the two pointed chrome shapes with rounded pills.</summary>
    public static partial class UiNovemberMenuRevisions
    {
        /// <summary>Removes small top-three stars permanently while keeping medal colors and centering existing rank labels.</summary>
        public static string Podium()
        {
            var owner = Owner<LeaderboardUI>();
            foreach (var row in owner.GetComponentsInChildren<LeaderboardEntryUI>(true))
            {
                var fields = new SerializedObject(row);
                fields.FindProperty("_crownIcon").objectReferenceValue = null;
                fields.ApplyModifiedPropertiesWithoutUndo();
                var ornament = row.transform.Find("CrownIcon");
                if (ornament != null) UnityEngine.Object.DestroyImmediate(ornament.gameObject);
                foreach (string path in new[] { "NormalValues/RankText","HighlightedValues/RankText" })
                {
                    var rank = row.transform.Find(path) as RectTransform;
                    if (rank == null) continue;
                    rank.anchoredPosition = new Vector2(rank.anchoredPosition.x,0); EditorUtility.SetDirty(rank);
                }
                EditorUtility.SetDirty(row);
            }
            return Finish("leaderboard rank ornaments");
        }

        /// <summary>Rounds only the existing status and Refresh chrome; preserves all text, icons, listeners and positions.</summary>
        public static string LeaderboardPills()
        {
            var owner = Owner<LeaderboardUI>();
            var board = Bound<RectTransform>(owner,"_dialogBox");
            var status = Child(board,"ConnectionStatusBadge").GetComponent<Image>();
            var refresh = Bound<Button>(owner,"_retryButton");
            var statusSize = status.rectTransform.sizeDelta;
            var refreshSize = refresh.GetComponent<RectTransform>().sizeDelta;
            Surface(status,K.Material("Ui03LeaderboardRimBlue"));
            Face(status.transform,"RoundedStatusFace",statusSize-new Vector2(6,6),
                K.Material("Ui03LeaderboardFaceLight"),K.Material("Ui03LeaderboardFaceDark"));
            Surface(refresh.image,K.Material("Ui03LeaderboardRimGold"));
            Face(refresh.transform,"RoundedRefreshFace",refreshSize-new Vector2(10,10),K.Material("Ui03LeaderboardPlayer"));
            refresh.targetGraphic = refresh.transform.Find("RoundedRefreshFace").GetComponent<Image>();
            EditorUtility.SetDirty(refresh);
            return Finish("leaderboard status and refresh pills");
        }

        private static void Face(Transform parent, string name, Vector2 size, Material light, Material dark = null)
        {
            var existing = parent.Find(name);
            var image = existing != null ? existing.GetComponent<Image>() : K.Image(parent,name,null,Vector2.zero,size);
            Center(image.rectTransform,Vector2.zero,size); image.raycastTarget = false;
            Surface(image,light,dark); image.transform.SetAsFirstSibling();
        }
    }
}
