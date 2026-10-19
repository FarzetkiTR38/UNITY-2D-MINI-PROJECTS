namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Crown(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool baseBand=R(x,y,0,-.52f,.73f,.22f);
            bool center=T(x,y,-.48f,-.43f,.48f,-.43f,0,.93f);
            bool side=T(a,y,.24f,-.45f,.89f,.66f,.74f,-.45f);
            bool gems=E(a,y,.83f,.63f,.12f,.12f)||E(x,y,0,.87f,.13f,.13f);
            if (!baseBand&&!center&&!side&&!gems) return Color.clear;
            if (E(x,y,0,-.05f,.12f,.18f)||E(a,y,.53f,-.14f,.09f,.13f)) return x<0?H(0x6BBACB):H(0xD67FA9);
            if (y<-.52f) return H(0xD59B46);
            if (gems) return H(0xF8DD79);
            return H(0xEDBD5D);
        }
        private static Color Castle(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool keep=R(x,y,0,-.35f,.45f,.5f);
            bool towers=R(a,y,.65f,-.15f,.2f,.7f);
            bool roof=T(a,y,.36f,.48f,.94f,.48f,.65f,.91f)||T(x,y,-.48f,.1f,.48f,.1f,0,.63f);
            bool flag=C(x,y,0,.5f,0,.96f,.065f)||T(x,y,.01f,.95f,.37f,.84f,.01f,.71f);
            if (!keep&&!towers&&!roof&&!flag) return Color.clear;
            if (flag) return H(0xE89C52);
            if (roof) return H(0xA377C5);
            if (E(x,y,0,-.6f,.19f,.31f)&&y<-.36f) return H(0x956EAB);
            if (R(a,y,.65f,.17f,.095f,.17f)||R(x,y,0,-.07f,.1f,.18f)) return H(0x648DAF);
            if (y<-.5f) return H(0xB2A0C4);
            return H(0xD4C7E1);
        }
        private static Color Anchor(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ring=E(x,y,0,.66f,.27f,.28f);
            bool shaft=R(x,y,0,-.12f,.11f,.7f);
            bool bar=C(x,y,-.52f,.25f,.52f,.25f,.09f);
            bool arms=C(a,y,.03f,-.71f,.58f,-.52f,.12f)||C(a,y,.58f,-.52f,.76f,-.08f,.12f);
            bool tips=T(a,y,.49f,-.2f,.91f,-.17f,.76f,.13f);
            if (!ring&&!shaft&&!bar&&!arms&&!tips) return Color.clear;
            if (E(x,y,0,.67f,.12f,.13f)) return H(0xE5BE6A);
            if (tips) return H(0x72C4CD);
            if (x<-.03f) return H(0x7CBBD5);
            return H(0x4D92BB);
        }
        private static Color Guitar(float x,float y)
        {
            bool lower=E(x,y,-.12f,-.49f,.54f,.43f);
            bool upper=E(x,y,-.12f,-.04f,.4f,.34f);
            bool neck=R(x,y,-.01f,.45f,.135f,.5f);
            bool head=R(x,y,-.01f,.83f,.19f,.15f);
            bool pegs=R(x,y,-.01f,.84f,.29f,.085f);
            if (!lower&&!upper&&!neck&&!head&&!pegs) return Color.clear;
            if (pegs&&!head) return H(0x9C9FB8);
            if (neck||head) return H(0x966B58);
            if (E(x,y,-.12f,-.22f,.21f,.22f)) return H(0x79536A);
            if (R(x,y,-.12f,-.65f,.22f,.075f)) return H(0x815B57);
            return x<-.18f?H(0xDE9659):H(0xF0BC73);
        }
        private static Color Balloon(float x,float y)
        {
            bool canopy=E(x,y,0,.28f,.75f,.65f)||T(x,y,-.59f,-.11f,.59f,-.11f,0,-.6f);
            bool ropes=T(x,y,-.4f,-.24f,.4f,-.24f,0,-.8f)||R(x,y,0,-.57f,.24f,.17f);
            bool basket=R(x,y,0,-.76f,.29f,.16f);
            if (!canopy&&!ropes&&!basket) return Color.clear;
            if (basket) return y<-.79f?H(0x997347):H(0xBA8B56);
            if (ropes&&!canopy) return H(0xA18672);
            float band=x/(Mathf.Max(.14f,Mathf.Sqrt(Mathf.Max(0,1f-(y-.28f)*(y-.28f)/.4225f))));
            if (Mathf.Abs(band)<.21f) return H(0xEBC85F);
            if (Mathf.Abs(band)<.49f) return H(0x76B8D0);
            return H(0xD889AF);
        }
    }
}
