namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    /// <summary>Native geometric authoring references with silhouette-aware region colors.</summary>
    public static class ShapeSilhouettes
    {
        public static readonly string[] Names = { "Ruby Heart", "Polar Penguin", "Moon Rabbit", "Prism Butterfly", "Starlight Rocket" };

        /// <summary>Samples a named silhouette in normalized coordinates.</summary>
        public static Color Sample(int shape, float x, float y)
        {
            switch (shape)
            {
                case 0:
                    float hy = y * 1.15f + .15f;
                    float f = x * x + hy * hy - .72f;
                    if (f * f * f - x * x * hy * hy * hy > 0) return Color.clear;
                    return Color.Lerp(new Color(.95f,.12f,.32f), new Color(1f,.5f,.62f), (x + y + 2f) / 4f);
                case 1:
                    bool body = Ellipse(x, y + .12f, .64f, .76f), head = Ellipse(x, y - .5f, .48f, .42f);
                    bool wing = Ellipse(Mathf.Abs(x) - .62f, y + .12f, .27f, .46f);
                    bool foot = Ellipse(Mathf.Abs(x) - .34f, y + .85f, .29f, .12f);
                    if (!body && !head && !wing && !foot) return Color.clear;
                    if (foot || (Mathf.Abs(x) < .2f && y > .31f && y < .44f)) return new Color(1f,.62f,.08f);
                    if (Ellipse(Mathf.Abs(x)-.19f,y-.58f,.06f,.08f)) return new Color(.08f,.12f,.25f);
                    if (Ellipse(x, y + .18f, .43f, .57f) || Ellipse(Mathf.Abs(x)-.19f,y-.55f,.18f,.22f)) return new Color(.77f,.9f,.97f);
                    return new Color(.12f,.32f,.57f);
                case 2:
                    bool ear = Ellipse(Mathf.Abs(x) - .31f + .12f * y, y - .5f, .2f, .48f);
                    bool face = Ellipse(x, y + .3f, .73f, .62f);
                    if (!ear && !face) return Color.clear;
                    if (ear && Ellipse(Mathf.Abs(x) - .31f + .12f*y, y - .52f, .105f, .34f)) return new Color(.98f,.43f,.66f);
                    if (Ellipse(Mathf.Abs(x)-.24f,y+.22f,.065f,.09f)) return new Color(.27f,.2f,.48f);
                    if (Ellipse(x,y+.43f,.11f,.07f) || Ellipse(Mathf.Abs(x)-.45f,y+.42f,.15f,.1f)) return new Color(.96f,.51f,.7f);
                    return new Color(.55f,.52f,.85f);
                case 3:
                    float ax = Mathf.Abs(x);
                    bool upper = Ellipse(ax - .48f, y - .34f, .46f, .5f);
                    bool lower = Ellipse(ax - .42f, y + .45f, .37f, .39f);
                    bool trunk = Ellipse(x,y,.09f,.72f);
                    if (!upper && !lower && !trunk) return Color.clear;
                    if (trunk) return new Color(.3f,.22f,.55f);
                    float band = Mathf.Sin((ax * 3f + y) * 7f);
                    return y > 0 ? Color.Lerp(new Color(.13f,.64f,.88f),new Color(.5f,.3f,.84f),band*.5f+.5f)
                        : Color.Lerp(new Color(1f,.49f,.32f),new Color(.95f,.27f,.62f),band*.5f+.5f);
                default:
                    bool hull = Mathf.Abs(x) < .36f && y > -.45f && y < .55f;
                    bool nose = y >= .55f && y < .96f && Mathf.Abs(x) < (.96f-y)*.87f;
                    bool fins = y < -.13f && y > -.61f && Mathf.Abs(x) < .68f + y*.35f;
                    bool flame = y < -.48f && y > -.98f && Mathf.Abs(x) < (.98f+y)*.55f;
                    if (!hull && !nose && !fins && !flame) return Color.clear;
                    if (flame) return new Color(1f,.58f,.08f);
                    if (Ellipse(x,y-.18f,.23f,.24f)) return new Color(.05f,.64f,.88f);
                    if (nose || (!hull && fins)) return new Color(.95f,.22f,.39f);
                    return new Color(.41f,.49f,.77f);
            }
        }
        private static bool Ellipse(float x, float y, float rx, float ry) => x*x/(rx*rx)+y*y/(ry*ry) <= 1f;
    }
}
