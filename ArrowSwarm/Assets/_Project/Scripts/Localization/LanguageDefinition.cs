namespace ArrowSwarm.Localization
{
    using System;
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
}
