namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Duck(float x,float y)
        {
            bool head=E(x,y,.35f,.53f,.35f,.35f);
            bool neck=C(x,y,.27f,.38f,.26f,-.08f,.22f);
            bool beak=T(x,y,.59f,.61f,.97f,.44f,.56f,.36f);
            bool tail=T(x,y,-.52f,-.12f,-.94f,.13f,-.74f,-.49f);
            if (!E(x,y,-.17f,-.29f,.69f,.48f)&&!head&&!neck&&!beak&&!tail) return Color.clear;
            if (beak) return H(0xF2A13C);
            if (E(x,y,.46f,.59f,.055f,.07f)) return Ink;
            if (head||neck) return H(0x48A784);
            if (E(x,y,-.22f,-.21f,.4f,.24f)) return H(0x846C53);
            return H(0xC6AE7A);
        }
        private static Color Swan(float x,float y)
        {
            bool head=E(x,y,.35f,.64f,.23f,.23f);
            bool neck=C(x,y,.22f,.63f,.04f,.25f,.14f)||C(x,y,.04f,.25f,.29f,-.16f,.14f);
            bool beak=T(x,y,.5f,.69f,.9f,.54f,.51f,.49f);
            bool tail=T(x,y,-.5f,-.28f,-.96f,.15f,-.76f,-.6f);
            if (!E(x,y,-.12f,-.4f,.73f,.38f)&&!head&&!neck&&!beak&&!tail) return Color.clear;
            if (beak) return H(0xED9760);
            if (E(x,y,.42f,.68f,.045f,.055f)) return Ink;
            if (E(x,y,-.2f,-.29f,.43f,.22f)) return H(0xAECDE3);
            return White;
        }
        private static Color Dinosaur(float x,float y)
        {
            bool torso=E(x,y,-.17f,-.14f,.52f,.56f);
            bool neck=C(x,y,.1f,.04f,.33f,.56f,.23f);
            bool head=E(x,y,.5f,.61f,.39f,.25f);
            bool tail=T(x,y,-.35f,.04f,-.98f,.46f,-.61f,-.51f);
            bool legs=R(x,y,-.43f,-.65f,.17f,.27f)||R(x,y,.15f,-.65f,.17f,.27f);
            if (!torso&&!neck&&!head&&!tail&&!legs) return Color.clear;
            if (E(x,y,.58f,.67f,.06f,.07f)) return Ink;
            if (E(x,y,-.06f,-.22f,.28f,.39f)) return H(0xB8DA76);
            if (legs&&y<-.75f) return H(0x3C8B79);
            if (E(x,y,-.42f,.11f,.13f,.13f)||E(x,y,-.28f,.29f,.11f,.11f)) return H(0x65B5B3);
            return H(0x64AC78);
        }
        private static Color Elephant(float x,float y)
        {
            bool head=E(x,y,.42f,.26f,.4f,.49f);
            bool trunk=C(x,y,.72f,.12f,.75f,-.61f,.16f)||C(x,y,.75f,-.61f,.5f,-.67f,.16f)
                ||E(x,y,.53f,-.33f,.25f,.38f);
            bool legs=R(x,y,-.5f,-.6f,.18f,.31f)||R(x,y,.13f,-.6f,.18f,.31f);
            bool tail=C(x,y,-.64f,.13f,-.88f,-.27f,.065f);
            if (!E(x,y,-.19f,.02f,.61f,.61f)&&!head&&!trunk&&!legs&&!tail) return Color.clear;
            if (E(x,y,.53f,.39f,.05f,.065f)) return Ink;
            if (E(x,y,.17f,.26f,.31f,.4f)) return H(0xB58FC1);
            if (legs&&y<-.73f) return H(0xCFBDDC);
            return H(0x9390C3);
        }
        private static Color Bee(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool wing=E(a,y,.48f,.43f,.4f,.42f);
            bool antenna=C(a,y,.2f,.58f,.38f,.92f,.055f);
            bool sting=T(x,y,-.14f,-.61f,.14f,-.61f,0,-.95f);
            if (!E(x,y,0,-.12f,.58f,.72f)&&!wing&&!antenna&&!sting) return Color.clear;
            if (antenna||sting||Eyes(x,y,.21f,.28f,.06f)) return Ink;
            if (wing&&a>.37f&&y>.03f) return H(0xB1E2E8);
            if (y<.09f&&Mathf.Repeat(y+1f,.38f)<.18f) return H(0x55506B);
            return H(0xF0CB51);
        }
        private static Color Ladybug(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool legs=C(a,y,.4f,.15f,.87f,.32f,.065f)||C(a,y,.45f,-.18f,.92f,-.25f,.065f)
                ||C(a,y,.39f,-.45f,.78f,-.72f,.065f);
            bool head=E(x,y,0,.6f,.32f,.28f);
            if (!E(x,y,0,-.1f,.7f,.76f)&&!head&&!legs) return Color.clear;
            if (Eyes(x,y,.13f,.66f,.045f)) return White;
            if (head||legs||a<.045f||E(a,y,.36f,.24f,.14f,.15f)
                ||E(a,y,.42f,-.2f,.14f,.15f)||E(a,y,.26f,-.58f,.12f,.12f)) return Ink;
            return H(0xE75A6D);
        }
        private static Color Dragonfly(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool upper=E(a,y,.47f,.36f,.49f,.22f);
            bool lower=E(a,y,.46f,-.03f,.42f,.2f)||E(a,y,.18f,.16f,.17f,.17f);
            bool trunk=E(x,y,0,-.21f,.14f,.73f)||E(x,y,0,.52f,.24f,.23f);
            if (!upper&&!lower&&!trunk) return Color.clear;
            if (Eyes(x,y,.12f,.56f,.055f)) return Ink;
            if (trunk) return y<0&&Mathf.Repeat(y+1f,.18f)<.045f?H(0x287EAB):H(0x51B5C6);
            return upper?H(0xA8D6F3):H(0xB5AAE7);
        }
    }
}
