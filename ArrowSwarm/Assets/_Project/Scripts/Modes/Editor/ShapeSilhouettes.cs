namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    /// <summary>Bold native authoring references using large, discrete color regions that survive arrow tiling.</summary>
    public static class ShapeSilhouettes
    {
        public static readonly string[] Names = { "Ruby Heart", "Polar Penguin", "Moon Rabbit", "Prism Butterfly", "Starlight Rocket" };

        /// <summary>Samples a named silhouette in normalized coordinates without continuous palette gradients.</summary>
        public static Color Sample(int shape, float x, float y)
        {
            switch (shape)
            {
                case 0: return Heart(x,y);
                case 1: return Penguin(x,y);
                case 2: return Rabbit(x,y);
                case 3: return Butterfly(x,y);
                default: return Rocket(x,y);
            }
        }

        private static Color Heart(float x,float y)
        {
            float hy=y*1.15f+.15f, f=x*x+hy*hy-.72f;
            if (f*f*f-x*x*hy*hy*hy>0) return Color.clear;
            if (Ellipse(x+.38f,y-.32f,.33f,.36f)) return H(0xF57F9C);
            if (x>.23f) return H(0xC84478);
            return y<-.25f?H(0xD7496F):H(0xE95B80);
        }
        private static Color Penguin(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool body=Ellipse(x,y+.13f,.6f,.71f),head=Ellipse(x,y-.47f,.49f,.43f);
            bool wings=Ellipse(a-.61f,y+.14f,.28f,.43f),feet=Ellipse(a-.32f,y+.82f,.29f,.15f);
            if (!body&&!head&&!wings&&!feet) return Color.clear;
            if (feet||Ellipse(x,y-.28f,.2f,.13f)) return H(0xE8A139);
            if (Ellipse(a-.22f,y-.57f,.105f,.125f)) return H(0x253356);
            if (Ellipse(x,y+.18f,.42f,.52f)||Ellipse(a-.21f,y-.48f,.19f,.25f)) return H(0xA5CFE3);
            return wings?H(0x234F7C):H(0x326A95);
        }
        private static Color Rabbit(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ear=Ellipse(a-.31f+.1f*y,y-.5f,.22f,.48f),face=Ellipse(x,y+.3f,.73f,.62f);
            if (!ear&&!face) return Color.clear;
            if (ear&&Ellipse(a-.31f+.1f*y,y-.53f,.125f,.33f)) return H(0xDC6E9C);
            if (Ellipse(a-.25f,y+.21f,.105f,.12f)) return H(0x47356C);
            if (Ellipse(x,y+.43f,.13f,.1f)) return H(0xAE4F83);
            if (Ellipse(a-.44f,y+.38f,.17f,.13f)) return H(0xD98DB3);
            if (Ellipse(x,y+.44f,.32f,.23f)) return H(0xC5A5D5);
            return H(0x9680C6);
        }
        private static Color Butterfly(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool upper=Ellipse(a-.46f,y-.31f,.47f,.51f),lower=Ellipse(a-.4f,y+.43f,.39f,.39f);
            bool bridge=Ellipse(x,y+.08f,.22f,.55f),body=Ellipse(x,y,.11f,.75f);
            if (!upper&&!lower&&!bridge&&!body) return Color.clear;
            if (body) return H(0x51416C);
            if (y>.02f) return Ellipse(a-.49f,y-.36f,.27f,.29f)?H(0x3C9DB9):H(0x7768B9);
            return Ellipse(a-.41f,y+.43f,.23f,.24f)?H(0xE4AB55):H(0xCF658F);
        }
        private static Color Rocket(float x,float y)
        {
            bool hull=Mathf.Abs(x)<.35f&&y>-.45f&&y<.55f;
            bool nose=y>=.55f&&y<.96f&&Mathf.Abs(x)<(.96f-y)*.87f;
            bool fins=y<-.13f&&y>-.61f&&Mathf.Abs(x)<.68f+y*.35f;
            bool flame=y<-.43f&&y>-.98f&&Mathf.Abs(x)<(.98f+y)*.55f;
            if (!hull&&!nose&&!fins&&!flame) return Color.clear;
            if (flame) return H(0xE8A03E);
            if (Ellipse(x,y-.17f,.24f,.26f)) return Ellipse(x,y-.17f,.15f,.16f)?H(0x79BDCF):H(0x285D91);
            if (nose||!hull&&fins) return H(0xD95D7F);
            return y<-.17f?H(0x7665AA):H(0xA695C7);
        }
        private static Color H(uint rgb) => new Color(((rgb>>16)&255)/255f,((rgb>>8)&255)/255f,(rgb&255)/255f,1);
        private static bool Ellipse(float x,float y,float rx,float ry) => x*x/(rx*rx)+y*y/(ry*ry)<=1f;
    }
}
