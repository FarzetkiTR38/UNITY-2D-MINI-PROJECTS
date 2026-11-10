namespace ArrowSwarm.Localization
{
    using System;

    /// <summary>
    /// Provides canonical bidirectional mapping between the ten supported ISO language codes
    /// and their corresponding Profile/Leaderboard country codes and display labels.
    /// </summary>
    public partial class LocalizationManager
    {
        /// <summary>
        /// Normalizes any language code, country code, or language name into one of the ten supported ISO language codes.
        /// </summary>
        /// <param name="langOrCountry">Language code (e.g. 'de'), country code (e.g. 'DE'), or language name (e.g. 'GERMAN').</param>
        /// <returns>Normalized two-letter ISO language code (defaults to 'en').</returns>
        public static string NormalizeLanguageCode(string langOrCountry)
        {
            if (string.IsNullOrWhiteSpace(langOrCountry)) return DefaultLanguage;

            string token = langOrCountry.Trim().ToUpperInvariant();
            switch (token)
            {
                case "TR":
                case "TURKISH":
                case "TÜRKÇE":
                case "TURKCE":
                    return "tr";

                case "ES":
                case "SPANISH":
                case "ESPAÑOL":
                case "ESPANOL":
                    return "es";

                case "DE":
                case "GERMAN":
                case "DEUTSCH":
                    return "de";

                case "FR":
                case "FRENCH":
                case "FRANÇAIS":
                case "FRANCAIS":
                    return "fr";

                case "PT":
                case "BR":
                case "PORTUGUESE":
                case "PORTUGUÊS":
                case "PORTUGUES":
                    return "pt";

                case "IT":
                case "ITALIAN":
                case "ITALIANO":
                    return "it";

                case "RU":
                case "RUSSIAN":
                case "РУССКИЙ":
                    return "ru";

                case "JA":
                case "JP":
                case "JAPANESE":
                case "日本語":
                    return "ja";

                case "KO":
                case "KR":
                case "KOREAN":
                case "한국어":
                    return "ko";

                case "EN":
                case "US":
                case "GB":
                case "CA":
                case "AU":
                case "NL":
                case "GL":
                case "ENGLISH":
                case "ENGLİSH":
                default:
                    return "en";
            }
        }

        /// <summary>
        /// Maps a language code, country code, or language name to its canonical two-letter country code.
        /// </summary>
        /// <param name="langOrCountry">Language or country identifier.</param>
        /// <returns>Canonical two-letter country code (e.g. 'US', 'TR', 'DE').</returns>
        public static string GetCountryCodeForLanguage(string langOrCountry)
        {
            switch (NormalizeLanguageCode(langOrCountry))
            {
                case "tr": return "TR";
                case "es": return "ES";
                case "de": return "DE";
                case "fr": return "FR";
                case "pt": return "BR";
                case "it": return "IT";
                case "ru": return "RU";
                case "ja": return "JP";
                case "ko": return "KR";
                default: return "US";
            }
        }

        /// <summary>
        /// Maps a two-letter country code to its corresponding supported ISO language code.
        /// </summary>
        /// <param name="countryCode">Country code (e.g. 'TR', 'DE', 'US').</param>
        /// <returns>Supported two-letter ISO language code (e.g. 'tr', 'de', 'en').</returns>
        public static string GetLanguageCodeForCountry(string countryCode)
        {
            return NormalizeLanguageCode(countryCode);
        }

        /// <summary>
        /// Returns the Profile country selector display label for a given language or country code.
        /// </summary>
        /// <param name="langOrCountry">Language or country identifier.</param>
        /// <returns>Formatted country label (e.g. '[DE] Germany', '[TR] Türkiye').</returns>
        public static string GetCountryDisplayName(string langOrCountry)
        {
            switch (NormalizeLanguageCode(langOrCountry))
            {
                case "tr": return "[TR] Türkiye";
                case "es": return "[ES] Spain";
                case "de": return "[DE] Germany";
                case "fr": return "[FR] France";
                case "pt": return "[BR] Brazil";
                case "it": return "[IT] Italy";
                case "ru": return "[RU] Russia";
                case "ja": return "[JP] Japan";
                case "ko": return "[KR] South Korea";
                default: return "[US] United States";
            }
        }
    }
}
