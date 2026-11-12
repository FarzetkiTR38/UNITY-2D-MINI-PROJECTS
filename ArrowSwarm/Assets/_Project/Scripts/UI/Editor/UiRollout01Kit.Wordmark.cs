namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Reusable live TMP wordmark; no brand lettering is baked into sprite artwork.</summary>
    internal static partial class UiRollout01Kit
    {
        internal static void Wordmark(Transform parent, Vector2 position, float width)
        {
            float scale = width / 900f;
            var root = Rect(parent, "Wordmark", position, new Vector2(width, 305 * scale));
            var arrow = Text(root, "ArrowWordmark", "ARROW", new Vector2(0, 69 * scale),
                new Vector2(width, 230 * scale), 162 * scale, Color.white);
            arrow.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFontPath);
            arrow.fontSharedMaterial = Material("TitleBlue");
            arrow.characterSpacing = -5; arrow.textWrappingMode = TextWrappingModes.NoWrap;
            arrow.enableVertexGradient = true;
            arrow.colorGradient = new VertexGradient(Color.white, Color.white, new Color(.62f, .89f, 1), new Color(.62f, .89f, 1));
            var swarm = Text(root, "SwarmWordmark", "SWARM", new Vector2(0, -73 * scale),
                new Vector2(width, 230 * scale), 157 * scale, Color.white);
            swarm.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFontPath);
            swarm.fontSharedMaterial = Material("TitleGold");
            swarm.characterSpacing = -5; swarm.textWrappingMode = TextWrappingModes.NoWrap;
            swarm.enableVertexGradient = true;
            swarm.colorGradient = new VertexGradient(new Color(1, .98f, .28f), new Color(1, .98f, .28f),
                new Color(1, .60f, .04f), new Color(1, .60f, .04f));
        }
    }
}
