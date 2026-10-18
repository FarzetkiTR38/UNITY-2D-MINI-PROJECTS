namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Unicorn(float x,float y)
        {
            bool face=E(x,y,.04f,-.12f,.49f,.69f)||E(x,y,.39f,-.44f,.39f,.27f);
            bool ear=T(x,y,-.31f,.31f,-.28f,.85f,.07f,.43f);
            bool horn=T(x,y,.1f,.41f,.31f,.96f,.39f,.35f);
            bool mane=E(x,y,-.4f,.17f,.35f,.51f)||E(x,y,-.52f,-.39f,.32f,.48f);
            if (!face&&!ear&&!horn&&!mane) return Color.clear;
            if (horn) return H(0xF7CE62);
            if (mane&&x<-.3f) return y>.2f?H(0xB085D3):(y>-.25f?H(0xED84B8):H(0x78C6D4));
            if (E(x,y,.24f,.04f,.105f,.12f)||E(x,y,.62f,-.41f,.07f,.085f)) return Ink;
            if (E(x,y,.34f,-.26f,.2f,.14f)) return H(0xEEA1BB);
            if (ear&&y>.48f) return H(0xD9A2D4);
            return H(0xDCC5E7);
        }
        private static Color Dragon(float x,float y)
        {
            bool body=E(x,y,-.12f,-.25f,.42f,.48f);
            bool neck=C(x,y,.11f,-.04f,.28f,.36f,.19f);
            bool head=E(x,y,.4f,.4f,.28f,.25f)||E(x,y,.65f,.29f,.3f,.18f);
            bool wing=T(x,y,-.23f,.02f,-.52f,.85f,-.91f,.18f)
                ||T(x,y,-.23f,.02f,-.91f,.18f,-.66f,-.14f);
            bool horns=T(x,y,.24f,.55f,.14f,.91f,.42f,.61f)
                ||T(x,y,.48f,.55f,.59f,.88f,.63f,.48f);
            bool feet=E(x,y,-.3f,-.71f,.26f,.16f)||E(x,y,.25f,-.71f,.27f,.16f);
            bool tail=C(x,y,-.37f,-.43f,-.69f,-.61f,.12f)
                ||C(x,y,-.69f,-.61f,-.85f,-.47f,.115f)||T(x,y,-.8f,-.51f,-.96f,-.27f,-.72f,-.35f);
            if (!body&&!neck&&!head&&!wing&&!horns&&!feet&&!tail) return Color.clear;
            if (horns) return H(0xF2CB72);
            if (E(x,y,.43f,.43f,.105f,.12f)||E(x,y,.84f,.3f,.065f,.065f)) return Ink;
            if (wing&&x<-.3f) return C(x,y,-.28f,.04f,-.52f,.82f,.07f)?H(0x438C78):H(0x73C4BB);
            if (tail) return H(0x438C78);
            if (E(x,y,.04f,-.31f,.2f,.34f)||head&&y<.24f) return H(0xD4D887);
            if (feet) return H(0x438C78);
            return H(0x57AA8A);
        }
        private static Color Squirrel(float x,float y)
        {
            bool tail=E(x,y,-.49f,.03f,.44f,.76f)||E(x,y,-.4f,.56f,.38f,.35f);
            bool body=E(x,y,.24f,-.3f,.4f,.52f);
            bool head=E(x,y,.36f,.35f,.33f,.33f);
            bool ears=T(x,y,.1f,.48f,.1f,.91f,.35f,.59f)||T(x,y,.44f,.54f,.64f,.88f,.63f,.32f);
            bool foot=E(x,y,.38f,-.76f,.39f,.15f);
            if (!tail&&!body&&!head&&!ears&&!foot) return Color.clear;
            if (E(x,y,.47f,.4f,.095f,.11f)||E(x,y,.67f,.24f,.08f,.075f)) return Ink;
            if (E(x,y,.39f,-.32f,.2f,.35f)) return Cream;
            if (tail&&x<-.13f) return E(x,y,-.47f,.09f,.21f,.47f)?H(0xDB9D68):H(0xB9754C);
            if (E(x,y,.55f,-.12f,.19f,.22f)) return H(0xAC7A42);
            return H(0xCC8957);
        }
        private static Color Hedgehog(float x,float y)
        {
            bool body=E(x,y,-.15f,-.09f,.73f,.66f);
            bool face=T(x,y,.14f,.31f,.96f,-.32f,.14f,-.65f);
            bool feet=E(x,y,-.47f,-.68f,.22f,.14f)||E(x,y,.4f,-.68f,.22f,.14f);
            float angle=Mathf.Atan2(y+.12f,x+.2f);
            float radius=Mathf.Sqrt((x+.2f)*(x+.2f)+(y+.12f)*(y+.12f));
            bool spikes=x<.28f&&y>-.45f&&radius<.72f+.17f*Mathf.Cos(angle*11f);
            if (!body&&!face&&!feet&&!spikes) return Color.clear;
            if (E(x,y,.48f,-.12f,.1f,.115f)||E(x,y,.86f,-.3f,.095f,.085f)) return Ink;
            if (face||feet) return H(0xD9B78A);
            return y>.27f||x<-.53f?H(0xAE805A):H(0x906B53);
        }
        private static Color Frog(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool eyes=E(a,y,.4f,.53f,.28f,.31f);
            bool legs=E(a,y,.62f,-.38f,.32f,.35f)||E(a,y,.6f,-.73f,.34f,.15f)||E(a,y,.27f,-.67f,.16f,.15f);
            bool arms=C(a,y,.37f,-.03f,.7f,-.48f,.17f);
            if (!E(x,y,0,-.1f,.61f,.65f)&&!eyes&&!legs&&!arms) return Color.clear;
            if (Eyes(x,y,.4f,.59f,.11f)) return Ink;
            if (E(a,y,.4f,.56f,.22f,.23f)) return Cream;
            if (E(x,y,0,-.3f,.37f,.35f)) return H(0xC4DA88);
            if (E(a,y,.39f,.03f,.12f,.08f)) return H(0xE4A678);
            return H(0x72B977);
        }
        private static Color Panda(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ears=E(a,y,.53f,.63f,.29f,.29f);
            bool face=E(x,y,0,-.08f,.79f,.78f);
            if (!ears&&!face) return Color.clear;
            if (ears&&y>.48f) return H(0x465370);
            if (Eyes(x,y,.3f,.14f,.09f)) return H(0x16283F);
            if (E(a,y,.31f,.11f,.21f,.27f)||E(x,y,0,-.3f,.12f,.09f)) return H(0x52627E);
            if (E(a,y,.46f,-.3f,.13f,.09f)) return H(0xDDA5B5);
            return White;
        }
        private static Color Raccoon(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ears=T(a,y,.26f,.42f,.67f,.94f,.82f,.13f);
            if (!E(x,y,0,-.12f,.8f,.72f)&&!ears) return Color.clear;
            if (T(a,y,.43f,.42f,.63f,.73f,.68f,.28f)) return H(0x9C86A9);
            if (Eyes(x,y,.32f,.07f,.095f)||E(x,y,0,-.38f,.14f,.105f)) return H(0x24334E);
            if (E(a,y,.32f,.03f,.32f,.22f)) return H(0x536079);
            if (E(x,y,0,-.4f,.3f,.26f)||y>.43f&&a<.16f) return White;
            return H(0x9CA9BD);
        }
    }
}
