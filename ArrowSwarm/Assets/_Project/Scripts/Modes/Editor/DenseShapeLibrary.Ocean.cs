namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Fish(float x,float y)
        {
            bool tail=T(x,y,-.42f,0,-.94f,.62f,-.94f,-.62f);
            bool fin=T(x,y,-.24f,.3f,.27f,.82f,.48f,.22f)||T(x,y,-.15f,-.2f,.13f,-.69f,.4f,-.2f);
            if (!E(x,y,.12f,0,.72f,.53f)&&!tail&&!fin) return Color.clear;
            if (E(x,y,.52f,.13f,.075f,.09f)) return Ink;
            if (tail||fin) return H(0xF4AB49);
            if (x<-.06f&&x>-.23f||x>.16f&&x<.3f) return Cream;
            return H(0xF47667);
        }
        private static Color Turtle(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool head=E(x,y,0,.67f,.25f,.3f);
            bool fins=E(a,y,.61f,.35f,.34f,.22f)||E(a,y,.52f,-.54f,.32f,.23f);
            bool tail=T(x,y,-.13f,-.6f,.13f,-.6f,0,-.93f);
            if (!E(x,y,0,-.1f,.65f,.7f)&&!head&&!fins&&!tail) return Color.clear;
            if (Eyes(x,y,.12f,.73f,.035f)) return Ink;
            if (head||fins||tail) return H(0x76C879);
            if (E(x,y,0,-.08f,.31f,.36f)) return H(0xE7CA67);
            if (Mathf.Abs(y+.1f)<.035f||Mathf.Abs(a-.35f)<.035f) return H(0x357867);
            return H(0x389B76);
        }
        private static Color Whale(float x,float y)
        {
            bool tail=T(x,y,.35f,-.16f,.96f,.58f,.91f,-.34f);
            bool fluke=T(x,y,.57f,.12f,.88f,.86f,.96f,.46f);
            bool fin=T(x,y,-.17f,-.25f,.22f,-.79f,.41f,-.31f);
            if (!E(x,y,-.19f,-.04f,.73f,.61f)&&!tail&&!fluke&&!fin) return Color.clear;
            if (E(x,y,-.55f,.13f,.07f,.085f)) return Ink;
            if (fin) return H(0x377AAD);
            if (y<-.23f&&x<.48f) return H(0xAFE5ED);
            return H(0x54ADCE);
        }
        private static Color Crab(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool arm=C(a,y,.48f,.08f,.72f,.43f,.14f);
            bool claw=E(a,y,.74f,.52f,.24f,.29f);
            bool legs=C(a,y,.36f,-.14f,.88f,-.43f,.1f)||C(a,y,.4f,-.3f,.72f,-.66f,.1f)||E(a,y,.65f,-.42f,.1f,.1f);
            bool eyes=C(a,y,.25f,.21f,.31f,.5f,.1f);
            if (!E(x,y,0,-.12f,.63f,.45f)&&!arm&&!claw&&!legs&&!eyes) return Color.clear;
            if (E(a,y,.31f,.48f,.05f,.065f)) return Ink;
            if (claw) return H(0xF27D70);
            if (E(x,y,0,-.27f,.39f,.19f)) return H(0xFFB379);
            return H(0xDA5265);
        }
        private static Color Seahorse(float x,float y)
        {
            bool head=E(x,y,.03f,.55f,.35f,.31f)||R(x,y,.4f,.55f,.26f,.12f);
            bool body=E(x,y,-.09f,-.02f,.38f,.61f);
            bool curl=E(x,y,.04f,-.55f,.44f,.3f);
            bool crest=T(x,y,-.32f,.5f,-.59f,.83f,-.17f,.77f);
            bool fin=T(x,y,-.25f,.12f,-.72f,.04f,-.38f,-.37f);
            if (!head&&!body&&!curl&&!crest&&!fin) return Color.clear;
            if (E(x,y,.14f,.65f,.065f,.075f)) return Ink;
            if (fin||crest) return H(0xC75A9F);
            if (x>.01f&&y<.37f||E(x,y,.15f,-.57f,.23f,.13f)) return H(0xFFC778);
            if (y<.2f&&Mathf.Repeat(y+1f,.19f)<.045f) return H(0xD86B95);
            return H(0xF09AA4);
        }
        private static Color Octopus(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool arms=C(a,y,.23f,-.1f,.75f,-.67f,.12f)||C(a,y,.43f,-.1f,.87f,-.25f,.12f)
                ||C(a,y,.11f,-.21f,.25f,-.85f,.12f)||E(a,y,.28f,-.4f,.15f,.24f)||E(a,y,.55f,-.27f,.09f,.1f);
            if (!E(x,y,0,.28f,.61f,.62f)&&!arms) return Color.clear;
            if (Eyes(x,y,.23f,.32f,.075f)) return Ink;
            if (E(a,y,.36f,.1f,.13f,.085f)) return H(0xF1A3D1);
            if (y<-.35f&&Mathf.Repeat(a,.24f)<.08f) return H(0xCA9AE4);
            return H(0xA86AC4);
        }
        private static Color Jellyfish(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool cap=E(x,y,0,.34f,.81f,.57f)&&y>-.13f;
            bool ribbons=C(a,y,.22f,-.08f,.35f,-.87f,.095f)||C(a,y,.59f,-.04f,.65f,-.62f,.095f);
            if (!cap&&!ribbons) return Color.clear;
            if (Eyes(x,y,.27f,.25f,.07f)) return Ink;
            if (!cap) return a>.45f?H(0x5CC2D0):H(0xB484D9);
            if (y<.05f) return H(0xAA91E7);
            if (E(x,y,-.22f,.59f,.24f,.13f)) return H(0xC4ECF4);
            return H(0x83CCD9);
        }
    }
}
