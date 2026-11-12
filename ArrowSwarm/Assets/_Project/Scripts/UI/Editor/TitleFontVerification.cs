namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TextCore.LowLevel;
    using UnityEngine.UI;

    /// <summary>Checks native Turkish glyph identity and captures temporary native before/after title proof.</summary>
    public static class TitleFontVerification
    {
        private const string Root = "Assets/_Project/Fonts/";
        private const string FontPath = Root + "Farzetki_Title_Font_H1_SDF.asset";
        private static readonly List<TMP_FontAsset> OwnedFonts = new List<TMP_FontAsset>();
        private static readonly List<Material> OwnedMaterials = new List<Material>();
        private static GameObject _canvas;
        private static UiRollout01NativeFrameRunner _runner;
        private static TMP_FontAsset _old, _revised;
        private static string _folder, _capture;
        private static int _frame;
        private static float _timeScale;
        private static double _deadline;

        /// <summary>Current temporary native proof stage; all owned objects are removed after capture.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Validates direct Turkish coverage, unchanged native widths and six matching material atlases.</summary>
        public static string Verify()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null) throw new InvalidOperationException("Build the renamed title TMP asset first.");
            font.ReadFontAssetDefinition();
            foreach (char letter in "İıĞğŞşÇçÖöÜü")
                if (!font.characterLookupTable.TryGetValue(letter, out TMP_Character ch) || ch.textAsset != font)
                    throw new InvalidOperationException("Turkish glyph needs a fallback: " + letter);
            string[] pairs = { "Iİ", "GĞ", "SŞ", "gğ", "sş" };
            foreach (string pair in pairs)
            {
                var a = font.characterLookupTable[pair[0]].glyph.metrics;
                var b = font.characterLookupTable[pair[1]].glyph.metrics;
                if (Mathf.Abs(a.horizontalAdvance - b.horizontalAdvance) > .001f ||
                    Mathf.Abs(a.width - b.width) > .001f || Mathf.Abs(a.horizontalBearingX - b.horizontalBearingX) > .001f)
                    throw new InvalidOperationException("Native body metrics differ: " + pair);
            }
            foreach (string name in new[] { "Front", "Rim", "Extrusion", "Shadow", "TitleBlue", "TitleGold" })
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "Farzetki_Title_H1/" + name + ".mat");
                if (material == null || material.mainTexture != font.atlasTexture)
                    throw new InvalidOperationException("Title material atlas mismatch: " + name);
            }
            return "{\"status\":\"PASS\",\"directTurkishCharacters\":12,\"nativeBodyMetricPairs\":5,\"matchingMaterialAtlases\":6}";
        }

        /// <summary>Creates a temporary full-screen comparison in Play; returns immediately while native frames capture it.</summary>
        public static string CaptureProof(string folder)
        {
            if (!Application.isPlaying || _canvas != null) throw new InvalidOperationException("Use idle Play Mode.");
            string report = Verify(); _folder = folder; Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "title-font-verification.json"), report);
            _timeScale = Time.timeScale; Time.timeScale = 0; _frame = Time.frameCount;
            _deadline = EditorApplication.timeSinceStartup + 30;
            try
            {
                _revised = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
                var original = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "LilitaOne_SDF.asset");
                _old = TemporaryFont(original);
                _old.fallbackFontAssetTable = new List<TMP_FontAsset>();
                foreach (var fallback in original.fallbackFontAssetTable) _old.fallbackFontAssetTable.Add(TemporaryFont(fallback));
                _old.TryAddCharacters("I G S i ı g s İ Ğ Ş ğ ş İPUCU BİTTİ DONDURMA ŞEKİLLER OYUN MODLARI ÖĞÜT ÇİZGİSİ");
                BuildProof(); _capture = null; Status = "Waiting for native comparison frame";
                _runner = UiRollout01NativeFrameRunner.Begin(Tick);
                EditorApplication.update += Watchdog; AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
                EditorApplication.playModeStateChanged += ModeChanged;
                EditorApplication.QueuePlayerLoopUpdate(); return Status;
            }
            catch { Cleanup(); throw; }
        }

        private static TMP_FontAsset TemporaryFont(TMP_FontAsset source)
        {
            var font = TMP_FontAsset.CreateFontAsset(source.sourceFontFile, 90, 9, GlyphRenderMode.SDFAA,
                1024, 1024, AtlasPopulationMode.Dynamic, false);
            font.hideFlags = HideFlags.HideAndDontSave; OwnedFonts.Add(font); return font;
        }

        private static void BuildProof()
        {
            _canvas = new GameObject("Temporary Turkish title proof", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas.hideFlags = HideFlags.HideAndDontSave;
            var canvas = _canvas.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 32000;
            var scaler = _canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = .5f;
            var background = UiRollout01Kit.Rect(_canvas.transform, "Proof background", Vector2.zero, new Vector2(2200, 4000));
            background.gameObject.AddComponent<Image>().color = new Color(.94f, .96f, .99f);
            Caption("TÜRKÇE BAŞLIK HARFLERİ", new Vector2(0, 870), 42, _revised);
            Caption("ÖNCE", new Vector2(-265, 770), 36, _revised); Caption("SONRA", new Vector2(265, 770), 36, _revised);
            foreach (bool revised in new[] { false, true })
            {
                float x = revised ? 265 : -265; var font = revised ? _revised : _old;
                Caption("I İ  G Ğ  S Ş", new Vector2(x, 650), 76, font);
                Caption("i ı  g ğ  s ş", new Vector2(x, 540), 76, font);
                string[] titles = { "İPUCU BİTTİ", "DONDURMA BİTTİ", "ŞEKİLLER", "OYUN MODLARI", "ÖĞÜT ÇİZGİSİ" };
                for (int i = 0; i < titles.Length; i++) Title(titles[i], new Vector2(x, 360 - i * 225), revised);
            }
            Caption("Aynı harf gövdeleri · Aynı kalınlık · Doğru Türkçe", new Vector2(0, -850), 29, _revised);
            Canvas.ForceUpdateCanvases();
        }

        private static void Caption(string value, Vector2 position, float size, TMP_FontAsset font)
        {
            var rect = UiRollout01Kit.Rect(_canvas.transform, value, position, new Vector2(size < 45 ? 1020 : 500, 120));
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font = font; text.fontSharedMaterial = font.material;
            text.text = value; text.fontSize = size; text.color = new Color(.04f, .14f, .34f);
            text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.NoWrap; text.raycastTarget = false;
        }

        private static void Title(string value, Vector2 position, bool revised)
        {
            var template = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Casual3DTitle.prefab");
            var copy = UnityEngine.Object.Instantiate(template, _canvas.transform);
            var rect = (RectTransform)copy.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.localScale = Vector3.one; rect.anchoredPosition = position; rect.sizeDelta = new Vector2(490, 170);
            foreach (var text in copy.GetComponentsInChildren<TMP_Text>(true))
            {
                string name = text.name.Replace("Text_", ""); text.font = revised ? _revised : _old;
                var material = AssetDatabase.LoadAssetAtPath<Material>(revised ? Root + "Farzetki_Title_H1/" + name + ".mat" : Root + "LilitaOne_" + name + ".mat");
                if (!revised) { material = new Material(material); material.mainTexture = _old.atlasTexture; OwnedMaterials.Add(material); }
                text.fontSharedMaterial = material;
            }
            var title = copy.GetComponent<Casual3DTitle>(); var settings = new SerializedObject(title);
            settings.FindProperty("_localizationKey").stringValue = ""; settings.FindProperty("_fontStyle").intValue = (int)FontStyles.Normal;
            settings.FindProperty("_fontSize").floatValue = 74; settings.FindProperty("_fitToRect").boolValue = true;
            settings.ApplyModifiedPropertiesWithoutUndo(); title.Text = value; title.ApplyAll();
        }

        private static void Tick()
        {
            try { TickNative(); }
            catch (Exception error) { Status = "FAIL: " + error.Message; Cleanup(); }
        }

        private static void TickNative()
        {
            if (Time.frameCount <= _frame + 3) return;
            if (_capture == null)
            {
                _capture = Path.Combine(_folder, "turkish-title-before-after.png");
                if (File.Exists(_capture)) File.Delete(_capture);
                Canvas.ForceUpdateCanvases(); ScreenCapture.CaptureScreenshot(_capture); _frame = Time.frameCount; return;
            }
            if (Time.frameCount > _frame + 3 && File.Exists(_capture) && new FileInfo(_capture).Length > 0)
            { Status = "PASS: native title proof captured"; Cleanup(); }
        }

        private static void Watchdog()
        {
            if (!Application.isPlaying || EditorApplication.timeSinceStartup > _deadline)
            { Status = "ABORT: title proof stopped or timed out"; Cleanup(); }
            else EditorApplication.QueuePlayerLoopUpdate();
        }
        private static void ModeChanged(PlayModeStateChange state) { if (state == PlayModeStateChange.ExitingPlayMode) Cleanup(); }
        private static void Cleanup()
        {
            EditorApplication.update -= Watchdog; AssemblyReloadEvents.beforeAssemblyReload -= Cleanup;
            EditorApplication.playModeStateChanged -= ModeChanged;
            if (_runner != null) _runner.End(true); _runner = null;
            if (_canvas != null) UnityEngine.Object.DestroyImmediate(_canvas); _canvas = null;
            foreach (var material in OwnedMaterials) UnityEngine.Object.DestroyImmediate(material); OwnedMaterials.Clear();
            foreach (var font in OwnedFonts)
            { foreach (var atlas in font.atlasTextures) if (atlas != null) UnityEngine.Object.DestroyImmediate(atlas);
                UnityEngine.Object.DestroyImmediate(font.material); UnityEngine.Object.DestroyImmediate(font); }
            OwnedFonts.Clear(); Time.timeScale = _timeScale;
        }
    }
}
