namespace ArrowSwarm.Localization
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Supported language metadata definition.
    /// </summary>
    [Serializable]
    public struct LanguageDefinition
    {
        public string code;
        public string displayName;
        public string nativeName;
        public TextAsset jsonAsset;

        public LanguageDefinition(string c, string d, string n, TextAsset a = null)
        {
            code = c;
            displayName = d;
            nativeName = n;
            jsonAsset = a;
        }
    }

    /// <summary>
    /// Central manager for game-wide multi-language localization.
    /// Loads key-value JSON dictionaries, notifies listeners, and persists selected language.
    /// </summary>
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        private const string PrefsLanguageKey = "ArrowSwarm_SelectedLanguage";
        private const string DefaultLanguage = "en";

        [Header("Configured Languages")]
        [SerializeField] private LanguageDefinition[] _languages;

        private string _currentLanguage = DefaultLanguage;
        private readonly Dictionary<string, string> _localizedStrings = new Dictionary<string, string>();

        /// <summary>Fired whenever the active language changes.</summary>
        public static event Action OnLanguageChanged;

        /// <summary>Gets the current ISO language code (e.g. 'en', 'tr').</summary>
        public string CurrentLanguage => _currentLanguage;

        /// <summary>Gets the array of all available language definitions.</summary>
        public LanguageDefinition[] AvailableLanguages => _languages;

        protected override void OnSingletonAwake()
        {
            InitializeLanguagesList();
            AutoLinkJsonFiles();

            string saved = PlayerPrefs.GetString(PrefsLanguageKey, string.Empty);
            if (string.IsNullOrEmpty(saved))
            {
                saved = DetectSystemLanguage();
            }

            SetLanguage(saved);
        }

        /// <summary>Changes the current language and reloads strings.</summary>
        public void SetLanguage(string langCode)
        {
            if (string.IsNullOrEmpty(langCode)) langCode = DefaultLanguage;

            TextAsset asset = FindAssetForCode(langCode);
            if (asset == null && langCode != DefaultLanguage)
            {
                langCode = DefaultLanguage;
                asset = FindAssetForCode(langCode);
            }

            _currentLanguage = langCode;
            PlayerPrefs.SetString(PrefsLanguageKey, _currentLanguage);
            PlayerPrefs.Save();

            ParseJsonStrings(asset != null ? asset.text : string.Empty);
            OnLanguageChanged?.Invoke();
            Debug.Log($"[ArrowSwarm] Language set to: {_currentLanguage} ({_localizedStrings.Count} keys loaded)");
        }

        /// <summary>Cycles to the next available language.</summary>
        public void NextLanguage()
        {
            if (_languages == null || _languages.Length == 0) return;
            int idx = GetCurrentLanguageIndex();
            idx = (idx + 1) % _languages.Length;
            SetLanguage(_languages[idx].code);
        }

        /// <summary>Cycles to the previous available language.</summary>
        public void PrevLanguage()
        {
            if (_languages == null || _languages.Length == 0) return;
            int idx = GetCurrentLanguageIndex();
            idx = (idx - 1 + _languages.Length) % _languages.Length;
            SetLanguage(_languages[idx].code);
        }

        /// <summary>Gets the current language index in the available languages array.</summary>
        public int GetCurrentLanguageIndex()
        {
            if (_languages == null) return 0;
            for (int i = 0; i < _languages.Length; i++)
            {
                if (_languages[i].code.Equals(_currentLanguage, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return 0;
        }

        /// <summary>Translates a key into the active language.</summary>
        public string GetText(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(key)) return fallback ?? string.Empty;
            if (_localizedStrings.TryGetValue(key, out string val)) return val;
            return fallback ?? key;
        }

        private TextAsset FindAssetForCode(string code)
        {
            if (_languages == null) return null;
            for (int i = 0; i < _languages.Length; i++)
            {
                if (_languages[i].code.Equals(code, StringComparison.OrdinalIgnoreCase))
                    return _languages[i].jsonAsset;
            }
            return null;
        }

        private void ParseJsonStrings(string json)
        {
            _localizedStrings.Clear();
            if (string.IsNullOrEmpty(json)) return;

            // Clean fast JSON parsing for flat "key": "value" dictionary
            string cleaned = json.Trim().TrimStart('{').TrimEnd('}');
            string[] pairs = cleaned.Split('\n');

            for (int i = 0; i < pairs.Length; i++)
            {
                string line = pairs[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

                int colonIdx = line.IndexOf(':');
                if (colonIdx <= 0) continue;

                string key = line.Substring(0, colonIdx).Trim().Trim('"');
                string val = line.Substring(colonIdx + 1).Trim().TrimEnd(',').Trim().Trim('"');

                val = val.Replace("\\n", "\n").Replace("\\\"", "\"");
                _localizedStrings[key] = val;
            }
        }

        private string DetectSystemLanguage()
        {
            return Application.systemLanguage switch
            {
                SystemLanguage.Turkish => "tr",
                SystemLanguage.Spanish => "es",
                SystemLanguage.German => "de",
                SystemLanguage.French => "fr",
                SystemLanguage.Portuguese => "pt",
                SystemLanguage.Italian => "it",
                SystemLanguage.Russian => "ru",
                SystemLanguage.Japanese => "ja",
                SystemLanguage.Korean => "ko",
                _ => DefaultLanguage
            };
        }

        private void InitializeLanguagesList()
        {
            if (_languages != null && _languages.Length >= 10) return;

            _languages = new LanguageDefinition[]
            {
                new LanguageDefinition("en", "English", "English"),
                new LanguageDefinition("tr", "Turkish", "Türkçe"),
                new LanguageDefinition("es", "Spanish", "Español"),
                new LanguageDefinition("de", "German", "Deutsch"),
                new LanguageDefinition("fr", "French", "Français"),
                new LanguageDefinition("pt", "Portuguese", "Português"),
                new LanguageDefinition("it", "Italian", "Italiano"),
                new LanguageDefinition("ru", "Russian", "Русский"),
                new LanguageDefinition("ja", "Japanese", "日本語"),
                new LanguageDefinition("ko", "Korean", "한국어")
            };
        }

        private void AutoLinkJsonFiles()
        {
#if UNITY_EDITOR
            string basePath = "Assets/_Project/Data/Localization";
            for (int i = 0; i < _languages.Length; i++)
            {
                if (_languages[i].jsonAsset != null) continue;
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:TextAsset", new[] { basePath });
                foreach (var guid in guids)
                {
                    string p = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    if (p.EndsWith($"_{_languages[i].code}.json"))
                    {
                        _languages[i].jsonAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                        break;
                    }
                }
            }
#endif
        }
    }
}
