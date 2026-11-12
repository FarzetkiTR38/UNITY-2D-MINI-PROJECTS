namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Authors ten real data rows with reusable geometry, country flags and inspectable local-player states.</summary>
    internal static partial class UiRollout03LeaderboardAuthoring
    {
        private sealed class Values
        {
            internal RectTransform Root;
            internal TextMeshProUGUI Rank, Country, Name, Level, Stars;
        }

        private static LeaderboardEntryUI Row(Transform parent, int rank, float y)
        {
            string palette = rank == 1 ? "RimGold" : rank == 2 ? "RimSilver" : rank == 3 ? "RimBronze" : "RimBlue";
            var outer = Surface(parent, "Entry" + rank, new Vector2(0, y), new Vector2(960, 112), palette);
            var inner = Surface(outer.transform, "ContentPill", Vector2.zero, new Vector2(950, 102),
                rank == 1 || rank == 3 ? "FaceWarm" : "FaceLight", "FaceDark");
            var badge = Surface(outer.transform, "LeftBadge", new Vector2(-415, 0), new Vector2(118, 104),
                rank <= 3 ? palette : "BadgeBlue", rank <= 3 ? null : "BadgeDark");
            var highlight = Surface(outer.transform, "LocalPlayerHighlight", Vector2.zero, new Vector2(960, 112), "Player");
            highlight.gameObject.SetActive(false);
            var crown = K.Image(outer.transform, "CrownIcon", K.Sprite("star"), new Vector2(-415, 49), new Vector2(27, 27));
            crown.preserveAspect = true; crown.gameObject.SetActive(rank <= 3);
            var flag = K.Rect(outer.transform, "CountryFlag", new Vector2(-306, 0), new Vector2(62, 46)).gameObject.AddComponent<CountryFlagGraphic>();
            flag.raycastTarget = false; flag.SetCountryCode("TR");
            var separator = Divider(outer.transform, "NameDivider", 88);
            Divider(outer.transform, "StarsDivider", 252);
            var star = K.Image(outer.transform, "StarIcon", K.Sprite("star"), new Vector2(292, 0), new Vector2(58, 62));
            star.preserveAspect = true;
            var normal = RowValues(outer.transform, "NormalValues", rank, false);
            var local = RowValues(outer.transform, "HighlightedValues", rank, true);
            local.Root.gameObject.SetActive(false);
            var entry = outer.gameObject.AddComponent<LeaderboardEntryUI>();
            U.Bind(entry, "_cardBackground", outer); U.Bind(entry, "_rankBadge", badge); U.Bind(entry, "_crownIcon", crown);
            U.Bind(entry, "_contentPill", inner); U.Bind(entry, "_divider", separator); U.Bind(entry, "_starIcon", star);
            U.Bind(entry, "_rankText", normal.Rank); U.Bind(entry, "_playerNameText", normal.Name);
            U.Bind(entry, "_levelText", normal.Level); U.Bind(entry, "_starsText", normal.Stars);
            U.Bind(entry, "_countryFlag", flag); U.Bind(entry, "_countryCodeText", normal.Country);
            U.Bind(entry, "_playerHighlight", highlight.gameObject);
            U.Bind(entry, "_normalValuesRoot", normal.Root.gameObject); U.Bind(entry, "_highlightedValuesRoot", local.Root.gameObject);
            U.Bind(entry, "_highlightRankText", local.Rank); U.Bind(entry, "_highlightNameText", local.Name);
            U.Bind(entry, "_highlightLevelText", local.Level); U.Bind(entry, "_highlightStarsText", local.Stars);
            U.Bind(entry, "_highlightCountryText", local.Country);
            return entry;
        }

        private static Values RowValues(Transform parent, string name, int rank, bool local)
        {
            var values = new Values { Root = K.Rect(parent, name, Vector2.zero, new Vector2(960, 112)) };
            values.Rank = RowLabel(values.Root, "RankText", rank.ToString(), -415, 112, rank <= 3 ? 70 : 54, local);
            // Metallic badges stay bright in both themes, so their numerals use the same deep ink.
            if (rank <= 3 && !local) K.Theme(values.Rank, Ink, Ink);
            values.Country = RowLabel(values.Root, "CountryCodeText", "TR", -231, 66, 30, local);
            values.Name = RowLabel(values.Root, "PlayerNameText", "Player", -58, 245, 34, local);
            values.Name.alignment = TextAlignmentOptions.MidlineLeft;
            values.Level = RowLabel(values.Root, "LevelText", "Lv.1", 172, 145, 32, local);
            values.Stars = RowLabel(values.Root, "StarsText", "0", 386, 124, 34, local);
            return values;
        }

        private static TextMeshProUGUI RowLabel(Transform parent, string name, string text, float x, float width, float size, bool local)
        {
            var label = U.Text(parent, name, text, new Vector2(x, 0), new Vector2(width, 85), size);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            if (local) K.Theme(label, Color.white, Color.white);
            else K.Theme(label, Ink, Ice);
            return label;
        }

        private static Image Divider(Transform parent, string name, float x)
        {
            var line = K.Image(parent, name, null, new Vector2(x, 0), new Vector2(2, 66));
            K.Theme(line, new Color(.12f, .61f, .92f, .70f), new Color(.17f, .59f, .91f, .70f));
            return line;
        }
    }
}
