namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Fish(float x,float y)
        {
            bool tail=T(x,y,-.42f,0,-.96f,.59f,-.96f,-.59f);
            bool fins=T(x,y,-.25f,.27f,.22f,.78f,.48f,.22f)||T(x,y,-.13f,-.2f,.15f,-.68f,.42f,-.2f);
            if (!E(x,y,.13f,0,.73f,.51f)&&!tail&&!fins) return Color.clear;
            if (E(x,y,.53f,.13f,.115f,.13f)) return Ink;
            if (tail||fins) return H(0xE5A03D);
            if (x>-.25f&&x<-.04f||x>.18f&&x<.37f) return H(0xA8D5DA);
            if (E(x,y,.53f,.05f,.27f,.33f)) return H(0xEDAF5A);
            return H(0xE86D68);
        }
        private static Color Turtle(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool shell=E(x,y,0,-.12f,.62f,.68f);
            bool head=E(x,y,0,.66f,.3f,.3f);
            bool front=C(a,y,.45f,.22f,.76f,.44f,.17f);
            bool rear=C(a,y,.4f,-.46f,.7f,-.67f,.16f);
            bool tail=C(x,y,0,-.66f,0,-.89f,.09f);
            if (!shell&&!head&&!front&&!rear&&!tail) return Color.clear;
            if (head&&Eyes(x,y,.145f,.74f,.09f)) return Ink;
            if (head||front||rear||tail) return H(0x88C66D);
            if (!E(x,y,0,-.12f,.51f,.57f)) return H(0x286D63);
            if (a/.31f+Mathf.Abs(y+.12f)/.39f<1.35f) return H(0xCCBC57);
            return a>.28f?H(0x408D68):H(0x6CB16C);
        }
        private static Color Whale(float x,float y)
        {
            bool body=E(x,y,-.22f,-.04f,.71f,.46f);
            bool peduncle=C(x,y,.31f,-.04f,.64f,.16f,.18f);
            bool upper=T(x,y,.49f,.12f,.69f,.85f,.91f,.49f);
            bool lower=T(x,y,.56f,.13f,.98f,-.18f,.96f,.4f);
            bool fin=C(x,y,-.02f,-.25f,.24f,-.64f,.15f);
            if (!body&&!peduncle&&!upper&&!lower&&!fin) return Color.clear;
            if (E(x,y,-.57f,.13f,.11f,.125f)) return Ink;
            if (fin) return H(0x346D9C);
            if (upper||lower) return H(0x3789B8);
            if (E(x,y,-.28f,-.32f,.57f,.21f)) return H(0x8ECBD7);
            return H(0x4C9FCA);
        }
        private static Color Crab(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool claw=E(a,y,.74f,.55f,.23f,.3f);
            bool arm=C(a,y,.46f,.06f,.72f,.4f,.15f)||E(a,y,.43f,.25f,.17f,.16f);
            bool legs=C(a,y,.43f,-.04f,.91f,-.18f,.105f)||C(a,y,.41f,-.2f,.86f,-.49f,.11f)
                ||C(a,y,.35f,-.34f,.67f,-.72f,.11f)||E(a,y,.61f,-.18f,.14f,.14f);
            bool eyes=C(a,y,.25f,.2f,.3f,.5f,.12f);
            if (!E(x,y,0,-.16f,.61f,.43f)&&!claw&&!arm&&!legs&&!eyes) return Color.clear;
            if (claw&&y>.68f&&a>.67f&&a<.81f) return Color.clear;
            if (E(a,y,.3f,.5f,.085f,.1f)) return Ink;
            if (claw) return H(0xF28B58);
            if (E(x,y,0,-.29f,.4f,.2f)) return H(0xEAA76A);
            return H(0xCF5062);
        }
        private static Color Seahorse(float x,float y)
        {
            bool head=E(x,y,-.03f,.57f,.33f,.3f);
            bool snout=R(x,y,.4f,.55f,.35f,.15f);
            bool neck=C(x,y,-.12f,.4f,-.27f,.05f,.19f);
            bool belly=E(x,y,-.1f,-.1f,.36f,.45f);
            bool curl=E(x,y,.04f,-.62f,.46f,.3f);
            bool crest=T(x,y,-.29f,.54f,-.58f,.9f,-.12f,.77f);
            bool fin=T(x,y,-.26f,.16f,-.73f,-.02f,-.34f,-.36f);
            if (!head&&!snout&&!neck&&!belly&&!curl&&!crest&&!fin) return Color.clear;
            if (E(x,y,.04f,.65f,.105f,.12f)) return Ink;
            if (fin||crest) return H(0xA64F8F);
            if (E(x,y,.18f,-.64f,.22f,.13f)) return H(0xAD5687);
            if (curl||x>-.1f&&y<.31f) return H(0xEAB25C);
            if (y<.24f&&Mathf.Repeat(y+1f,.34f)<.1f) return H(0xC46691);
            return H(0xE48B9A);
        }
        private static Color Octopus(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool arms=C(a,y,.23f,-.1f,.75f,-.67f,.14f)||C(a,y,.43f,-.1f,.86f,-.25f,.13f)
                ||C(a,y,.11f,-.21f,.25f,-.84f,.13f)||E(a,y,.28f,-.4f,.16f,.24f)||E(a,y,.55f,-.27f,.1f,.12f);
            if (!E(x,y,0,.28f,.61f,.62f)&&!arms) return Color.clear;
            if (Eyes(x,y,.24f,.32f,.105f)) return Ink;
            if (E(a,y,.39f,.06f,.16f,.11f)) return H(0xDB91BF);
            if (y<-.35f&&a>.37f) return H(0xBD8AD5);
            return H(0x945AB4);
        }
        private static Color Jellyfish(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool cap=E(x,y,0,.35f,.8f,.56f)&&y>-.12f;
            bool ribbons=C(a,y,.2f,-.04f,.22f,-.85f,.13f)||C(a,y,.58f,-.04f,.68f,-.65f,.12f);
            if (!cap&&!ribbons) return Color.clear;
            if (Eyes(x,y,.27f,.25f,.1f)) return Ink;
            if (!cap) return a>.45f?H(0x51A8C1):H(0xA071C4);
            if (y<.07f) return H(0x9876CB);
            if (E(x,y,-.24f,.61f,.27f,.15f)) return H(0xA1CFDD);
            return H(0x66B5C9);
        }
    }
}
