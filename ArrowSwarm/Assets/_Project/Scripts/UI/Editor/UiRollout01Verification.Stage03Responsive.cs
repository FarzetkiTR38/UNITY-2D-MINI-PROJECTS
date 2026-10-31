namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks native safe-area fitting, popup scale isolation and visible board03 geometry after resize.</summary>
    public static partial class UiRollout01Verification
    {
        private static void Verify03Layout(ProfileSetupModalUI profile)
        {
            Canvas.ForceUpdateCanvases();
            var card = Field<RectTransform>(profile, "_modalRect");
            Verify03Fit(profile.transform, card, "ProfileSafeArea");
            Check(Vector3.Distance(Field<Vector3>(profile, "_targetScale"), Vector3.one) < .001f,
                "Profile animation target remains one independently of responsive fitting");
            var input = Field<TMP_InputField>(profile, "_nameInput");
            Check(input.interactable && input.targetGraphic.raycastTarget && input.textViewport != null,
                "Responsive Profile retains the real editable nickname field");
            var inputRect = (RectTransform)input.transform;
            Verify03Safe(Native03Rect(inputRect), "Profile name input");
            var country = (RectTransform)Field<Button>(profile, "_countryButton").transform;
            var previous = (RectTransform)Field<Button>(profile, "_prevCountryButton").transform;
            var next = (RectTransform)Field<Button>(profile, "_nextCountryButton").transform;
            var label = Field<TextMeshProUGUI>(profile, "_countryText").rectTransform;
            Check(Native03Rect(previous).xMax <= Native03Rect(label).xMin + 1 &&
                Native03Rect(label).xMax <= Native03Rect(next).xMin + 1,
                "Country label stays between its two image-only arrows after resize");
            var save = (RectTransform)Field<Button>(profile, "_saveButton").transform;
            var skip = (RectTransform)Field<Button>(profile, "_skipButton").transform;
            Check(Native03Rect(inputRect).yMin > Native03Rect(country).yMax &&
                Native03Rect(country).yMin > Native03Rect(save).yMax &&
                Native03Rect(save).yMin > Native03Rect(skip).yMax,
                "Profile nickname, country, Save and Skip retain their visual separation");
            Verify03Safe(Drawn03Rect(card.Find("ProfileCrest").GetComponent<Image>(), false),
                "Profile crest actual preserve-aspect mesh");
        }

        private static void Verify03Layout(LeaderboardUI board)
        {
            Canvas.ForceUpdateCanvases();
            var frame = Field<RectTransform>(board, "_dialogBox");
            Verify03Fit(board.transform, frame, "SafeAreaFit");
            var rows = Field<LeaderboardEntryUI[]>(board, "_entryRows");
            Rect above = Native03Rect((RectTransform)frame.Find("ConnectionStatusBadge"));
            int visible = 0;
            foreach (var row in rows)
            {
                if (!row.gameObject.activeInHierarchy) continue;
                var bounds = Native03Rect((RectTransform)row.transform);
                Verify03Safe(bounds, "Ranking row " + row.name);
                Check(above.yMin > bounds.yMax, "Ranking row remains separate from the row or status above: " + row.name);
                var flag = Field<CountryFlagGraphic>(row, "_countryFlag");
                Check(flag != null && flag.GetComponent<CanvasRenderer>() != null && Visible(flag),
                    "Country flag owns an active CanvasRenderer: " + row.name);
                var flagBounds = Drawn03Rect(flag, true);
                Check(Contains03(bounds, flagBounds, 1), "Rendered flag remains inside its own ranking row: " + row.name);
                Verify03Safe(flagBounds, "Country flag " + row.name);
                var code = Native03Rect(Field<TextMeshProUGUI>(row, "_countryCodeText").rectTransform);
                Check(flagBounds.xMax <= code.xMin + 1, "Flag and country code remain separate: " + row.name);
                above = bounds; visible++;
            }
            Check(visible > 0, "Responsive ranking verification exercises visible data rows");
            var summary = Native03Rect((RectTransform)frame.Find("PlayerBar"));
            var footer = Native03Rect((RectTransform)frame.Find("FooterArea"));
            Verify03Safe(summary, "Player summary"); Verify03Safe(footer, "Leaderboard footer");
            Check(above.yMin > summary.yMax && summary.yMin > footer.yMax,
                "Last ranking row, pinned player summary and footer remain separate");
            var title = Field<TextMeshProUGUI>(board, "_titleText").GetComponentInParent<Casual3DTitle>(true);
            title.ApplyAll();
            Check(title.TryGetRenderedBounds(out Bounds titleBounds), "Leaderboard title has visible dimensional geometry");
            var titleScreen = Project03(title.transform, titleBounds.min, titleBounds.max);
            var back = Native03Rect((RectTransform)Field<Button>(board, "_backButton").transform);
            var close = Native03Rect((RectTransform)Field<Button>(board, "_closeButton").transform);
            var status = Native03Rect((RectTransform)frame.Find("ConnectionStatusBadge"));
            Check(titleScreen.xMin > back.xMax && titleScreen.xMax < close.xMin && titleScreen.yMin > status.yMax,
                "Rendered leaderboard title stays clear of Back, Close and status badge");
        }

        private static void Verify03Fit(Transform root, RectTransform card, string wrapperName)
        {
            var wrapper = (RectTransform)root.Find(wrapperName);
            var fit = wrapper.GetComponent<SafeAreaPanelFit>();
            Check(fit != null && card.parent == wrapper && Vector3.Distance(card.localScale, Vector3.one) < .001f,
                root.name + ": responsive wrapper stays outside the settled popup animation");
            var canvas = wrapper.GetComponentInParent<Canvas>();
            float scale = Mathf.Max(.001f, canvas.scaleFactor);
            Vector2 extent = Field<Vector2>(fit, "_referenceExtent"), padding = Field<Vector2>(fit, "_padding");
            Vector2 available = Screen.safeArea.size / scale - padding * 2;
            float expected = Mathf.Clamp01(Mathf.Min(available.x / Mathf.Max(1, extent.x), available.y / Mathf.Max(1, extent.y)));
            Vector2 center = (Screen.safeArea.center - new Vector2(Screen.width, Screen.height) * .5f) / scale;
            Check(Vector3.Distance(wrapper.localScale, Vector3.one * expected) < .001f &&
                Vector2.Distance(wrapper.anchoredPosition, center) < .1f,
                root.name + ": native " + Screen.width + "x" + Screen.height + " / safe=" + Screen.safeArea + " / fit=" + expected);
            Verify03Safe(Native03Rect(card), root.name + " complete authored extent");
            var backdrop = root.Find("SharedBackdrop").GetComponent<Image>();
            Check(Contains03(Native03Rect(backdrop.rectTransform),
                new Rect(0, 0, Screen.width, Screen.height), 1), root.name + ": modal backdrop covers the full native screen");
        }

        private static Rect Drawn03Rect(Graphic graphic, bool colored)
        {
            var renderer = graphic.GetComponent<CanvasRenderer>();
            Check(renderer != null, graphic.name + ": graphic has a CanvasRenderer");
            var mesh = renderer.GetMesh();
            // CanvasRenderer owns the returned mesh; inspect it without destroying its live geometry.
            {
                Check(mesh != null && mesh.vertexCount > 0 && mesh.triangles.Length >= 3, graphic.name + ": native renderer contains triangles");
                bool opaque = false, pigment = false;
                foreach (var color in mesh.colors32)
                { opaque |= color.a > 0; pigment |= color.a > 0 && (color.r < 245 || color.g < 245 || color.b < 245); }
                Check(opaque && (!colored || pigment), graphic.name + ": rendered mesh contains visible artwork colors");
                var vertices = mesh.vertices;
                Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
                var camera = Camera03(graphic.transform);
                foreach (var point in vertices)
                {
                    Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, graphic.transform.TransformPoint(point));
                    min = Vector2.Min(min, screen); max = Vector2.Max(max, screen);
                }
                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }

        private static UnityEngine.Camera Camera03(Transform target)
        {
            var canvas = target.GetComponentInParent<Canvas>();
            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        private static Rect Native03Rect(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
            foreach (var corner in corners)
            { var p = RectTransformUtility.WorldToScreenPoint(Camera03(rect), corner); min = Vector2.Min(min, p); max = Vector2.Max(max, p); }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static Rect Project03(Transform target, Vector3 min, Vector3 max)
        {
            Vector2 a = RectTransformUtility.WorldToScreenPoint(Camera03(target), target.TransformPoint(min));
            Vector2 b = RectTransformUtility.WorldToScreenPoint(Camera03(target), target.TransformPoint(max));
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        private static bool Contains03(Rect outer, Rect inner, float tolerance) =>
            inner.xMin >= outer.xMin - tolerance && inner.xMax <= outer.xMax + tolerance &&
            inner.yMin >= outer.yMin - tolerance && inner.yMax <= outer.yMax + tolerance;

        private static IEnumerator Verify03FlagVariants(LeaderboardEntryUI row)
        {
            var flag = Field<CountryFlagGraphic>(row,"_countryFlag");
            foreach (string code in new[] { "TR","US","GB","DE","FR","ES","IT","BR","JP","KR","RU","CA","AU","NL","GL","ZZ" })
            {
                flag.SetCountryCode(code); yield return Settled(.02f); Canvas.ForceUpdateCanvases();
                var bounds = Drawn03Rect(flag,true);
                Check(flag.CountryCode == code && Contains03(Native03Rect(flag.rectTransform),bounds,1),
                    "Country variant renders inside its real flag slot / " + code);
            }
        }

        private static void Verify03Safe(Rect bounds, string context) =>
            Check(Contains03(Screen.safeArea, bounds, 2), context + ": real geometry fits native safe area / " + bounds);
    }
}
