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
            if (E(x,y,.24f,.04f,.065f,.085f)||E(x,y,.62f,-.41f,.04f,.055f)) return Ink;
            if (E(x,y,.33f,-.24f,.14f,.095f)) return H(0xEEA1BB);
            if (ear&&y>.48f) return H(0xD9A2D4);
            return H(0xDCC5E7);
        }
        private static Color Dragon(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool body=E(x,y,0,-.22f,.35f,.58f)||E(x,y,0,.42f,.44f,.37f)||E(a,y,.26f,.24f,.14f,.25f);
            bool wings=T(a,y,.19f,.15f,.91f,.63f,.84f,-.22f)||T(a,y,.19f,.15f,.84f,-.22f,.42f,-.4f);
            bool horns=T(a,y,.17f,.61f,.45f,.98f,.43f,.46f);
            bool feet=E(a,y,.3f,-.73f,.26f,.16f);
            bool tail=C(x,y,.12f,-.6f,.64f,-.76f,.12f);
            if (!body&&!wings&&!horns&&!feet&&!tail) return Color.clear;
            if (horns) return H(0xF2CB72);
            if (Eyes(x,y,.16f,.47f,.055f)) return Ink;
            if (wings&&a>.31f) return H(0x73C4BB);
            if (E(x,y,0,-.24f,.2f,.41f)) return H(0xD4D887);
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
            if (E(x,y,.47f,.4f,.055f,.07f)||E(x,y,.67f,.24f,.055f,.05f)) return Ink;
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
            bool spikes=x<.28f&&y>-.45f&&radius<.75f+.13f*Mathf.Cos(angle*15f);
            if (!body&&!face&&!feet&&!spikes) return Color.clear;
            if (E(x,y,.48f,-.12f,.055f,.065f)||E(x,y,.86f,-.3f,.07f,.06f)) return Ink;
            if (face||feet) return H(0xD9B78A);
            return Mathf.Repeat((x+y)*6f,1f)<.18f?H(0xE1B480):H(0x9E755D);
        }
        private static Color Frog(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool eyes=E(a,y,.4f,.53f,.28f,.31f);
            bool legs=E(a,y,.62f,-.38f,.32f,.35f)||E(a,y,.6f,-.73f,.34f,.15f)||E(a,y,.27f,-.67f,.16f,.15f);
            bool arms=C(a,y,.37f,-.03f,.7f,-.48f,.17f);
            if (!E(x,y,0,-.1f,.61f,.65f)&&!eyes&&!legs&&!arms) return Color.clear;
            if (Eyes(x,y,.4f,.59f,.075f)) return Ink;
            if (E(a,y,.4f,.56f,.18f,.2f)) return Cream;
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
            if (Eyes(x,y,.3f,.14f,.055f)) return H(0x16283F);
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
            if (Eyes(x,y,.32f,.07f,.06f)||E(x,y,0,-.38f,.11f,.08f)) return H(0x24334E);
            if (E(a,y,.32f,.03f,.32f,.22f)) return H(0x536079);
            if (E(x,y,0,-.4f,.3f,.26f)||y>.43f&&a<.16f) return White;
            return H(0x9CA9BD);
        }
    }
}
