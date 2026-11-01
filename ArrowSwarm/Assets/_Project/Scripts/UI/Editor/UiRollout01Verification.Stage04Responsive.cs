namespace ArrowSwarm.UI.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks real safe-area geometry and the four-column campaign grid, including disabled tile numbers.</summary>
    public static partial class UiRollout01Verification
    {
        private static void Verify04Layout(LevelSelectUI levels)
        {
            Canvas.ForceUpdateCanvases();
            var card = Field<RectTransform>(levels, "_dialogBox");
            var wrapper = card.parent as RectTransform;
            var fit = wrapper != null ? wrapper.GetComponent<SafeAreaPanelFit>() : null;
            Check(fit != null && wrapper.name == "LevelsSafeArea" && card.name == "Card" &&
                Vector3.Distance(card.localScale, Vector3.one) < .005f,
                "Levels isolate the settled popup scale from the responsive wrapper");
            var canvas = wrapper.GetComponentInParent<Canvas>();
            float scale = Mathf.Max(.001f, canvas.scaleFactor);
            Vector2 extent = Field<Vector2>(fit, "_referenceExtent"), padding = Field<Vector2>(fit, "_padding");
            Vector2 available = Screen.safeArea.size / scale - padding * 2;
            float expected = Mathf.Clamp01(Mathf.Min(available.x / extent.x, available.y / extent.y));
            Vector2 centre = (Screen.safeArea.center - new Vector2(Screen.width, Screen.height) * .5f) / scale;
            Check(Vector3.Distance(wrapper.localScale, Vector3.one * expected) < .001f &&
                Vector2.Distance(wrapper.anchoredPosition, centre) < .1f,
                "Levels native " + Screen.width + "x" + Screen.height + " / safe=" + Screen.safeArea + " / fit=" + expected);
            Verify03Safe(Native03Rect(card), "Complete Levels authored extent");
            var shared = wrapper.parent;
            Check(shared.name == "Ui04SharedLevels" && shared.gameObject.activeInHierarchy &&
                !levels.transform.Find("Ui04LegacyLevels").gameObject.activeSelf,
                "Only the revised shared campaign view is active for Defense/Shapes");
            var backdrop = shared.Find("SharedBackdrop").GetComponent<Image>();
            Check(backdrop.raycastTarget && Contains03(Native03Rect(backdrop.rectTransform), new Rect(0, 0, Screen.width, Screen.height), 1),
                "Levels backdrop covers and intercepts the full native screen");
            Verify03Safe(Drawn03Rect(card.Find("HeaderCrest").GetComponent<Image>(), false), "Actual Levels header crest mesh");
            Rect footer = Drawn03Rect(card.Find("FooterCrest").GetComponent<Image>(), false);
            Verify03Safe(footer, "Actual Levels footer crest mesh");
            Rect frame = Native03Rect((RectTransform)card.Find("BoardFrame"));
            var grid = Field<Transform>(levels, "_levelGridContainer");
            var layout = grid.GetComponent<GridLayoutGroup>();
            Check(grid.IsChildOf(card) && layout != null && layout.constraint == GridLayoutGroup.Constraint.FixedColumnCount && layout.constraintCount == 4,
                "The bound campaign grid uses four authored columns inside the animated card");
            var slots = Field<List<LevelButtonUI>>(levels, "_buttonPool");
            var visible = slots.Where(tile => tile.gameObject.activeInHierarchy).ToArray();
            int first = (Field<int>(levels, "_currentPage") - 1) * 20 + 1;
            int count = GameModeSelection.Selected == GameModeId.Shapes ? Field<PuzzleCatalog>(levels, "_shapesCatalog").Count : int.MaxValue;
            int expectedSlots = Mathf.Clamp(count - first + 1, 0, 20);
            Check(slots.Count == 20 && visible.Length == expectedSlots && visible.Length > 0,
                "All twenty pooled slot identities respect the current finite/infinite campaign range");
            var bounds = new Rect[visible.Length];
            for (int i = 0; i < visible.Length; i++)
            {
                var tile = visible[i]; bounds[i] = Native03Rect((RectTransform)tile.transform);
                Verify03Safe(bounds[i], "Available or locked tile " + Field<int>(tile, "_level"));
                Check(Contains03(frame, bounds[i], 1), "Each campaign tile remains inside the authored window frame");
                if (i % 4 != 0)
                    Check(bounds[i - 1].xMax < bounds[i].xMin && Mathf.Abs(bounds[i - 1].center.y - bounds[i].center.y) < 1,
                        "Native grid columns share a row and retain a real horizontal gap / " + i);
                if (i >= 4)
                    Check(bounds[i].yMax < bounds[i - 4].yMin && Mathf.Abs(bounds[i].center.x - bounds[i - 4].center.x) < 1,
                        "Native grid rows share a column and retain a real vertical gap / " + i);
                var open = Field<GameObject>(tile, "_unlockedState"); var locked = Field<GameObject>(tile, "_lockedState");
                Check(open != null && locked != null && open.activeSelf != locked.activeSelf,
                    "A tile displays exactly one scene-authored availability state / " + i);
                var state = open.activeSelf ? open : locked;
                var number = Field<TMP_Text>(tile, open.activeSelf ? "_levelText" : "_lockedLevelText");
                number.ForceMeshUpdate();
                Check(Visible(number) && !number.isTextTruncated && GlyphsFit(number),
                    "Available and locked number glyphs fit their own visible tile / " + number.text);
                var face = state.transform.Find("Face").GetComponent<Image>();
                Check(Visible(face) && face.sprite != null && Contains03(bounds[i], Drawn03Rect(face, false), 1),
                    "The chosen authored tile artwork renders inside its own slot / " + i);
                if (!open.activeSelf)
                {
                    var lockIcon = Field<GameObject>(tile, "_lockIcon").GetComponent<Image>();
                    Check(Visible(lockIcon) && Contains03(bounds[i], Drawn03Rect(lockIcon, false), 1),
                        "The real lock drawing stays inside the locked tile / " + i);
                }
            }
            if (visible.Length == 20)
                Check(Mathf.CeilToInt(visible.Length / 4f) == 5, "The reference page visibly contains four columns by five rows");
            Rect top = bounds[0], bottom = bounds[bounds.Length - 1];
            Rect range = Native03Rect((RectTransform)card.Find("RangePlate"));
            Rect previous = Native03Rect((RectTransform)Field<Button>(levels, "_prevPageButton").transform);
            Rect next = Native03Rect((RectTransform)Field<Button>(levels, "_nextPageButton").transform);
            Rect pill = Native03Rect((RectTransform)card.Find("ModePill"));
            Check(range.yMin > top.yMax && previous.yMin > top.yMax && next.yMin > top.yMax &&
                previous.xMax < range.xMin && range.xMax < next.xMin && pill.yMin > range.yMax && footer.yMax < bottom.yMin,
                "Mode badge, pager, first/last tile rows and footer remain separate after resize");
            var title = card.Find("TitleText").GetComponent<Casual3DTitle>(); title.ApplyAll();
            Check(title.TryGetRenderedBounds(out Bounds titleBounds), "Levels dimensional title owns real visible geometry");
            Rect titleScreen = Project03(title.transform, titleBounds.min, titleBounds.max);
            Rect close = Native03Rect((RectTransform)Field<Button>(levels, "_closeButton").transform);
            Check(titleScreen.xMax < close.xMin && titleScreen.yMin > pill.yMax,
                "Rendered Levels title stays clear of Close and mode badge");
            bool shapes = GameModeSelection.Selected == GameModeId.Shapes;
            var identity = card.Find(shapes ? "ShapesIdentity" : "DefenseIdentity");
            Check(identity.gameObject.activeSelf && !card.Find(shapes ? "DefenseIdentity" : "ShapesIdentity").gameObject.activeSelf,
                "Exactly the actual selected campaign identity is active");
            var modeLabel = identity.Find("ModeLabel").GetComponent<TMP_Text>();
            Check(Visible(modeLabel) && modeLabel.text == LocalizationManager.Instance.GetText(shapes ? "mode_shapes" : "mode_defense"),
                "The visible mode label is localized live TMP for the selected campaign");
            foreach (var image in identity.GetComponentsInChildren<Image>())
                if (Visible(image)) Verify03Safe(Drawn03Rect(image, false), "Mode identity artwork " + image.name);
        }
    }
}
