namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Strawberry(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool fruit=E(x,y,0,.12f,.75f,.52f)||T(x,y,-.68f,.15f,.68f,.15f,0,-.93f);
            bool leaf=T(x,y,-.76f,.5f,.13f,.47f,-.3f,.88f)||T(x,y,-.15f,.49f,.75f,.52f,.28f,.9f)
                ||E(x,y,.12f,.72f,.16f,.2f);
            bool stem=C(x,y,0,.5f,.1f,.96f,.07f);
            if (!fruit&&!leaf&&!stem) return Color.clear;
            if (leaf||stem) return H(0x68B47C);
            bool seed=E(a,y,.43f,.2f,.075f,.1f)||E(a,y,.18f,.35f,.075f,.1f)
                ||E(a,y,.25f,-.16f,.075f,.1f)||E(x,y,0,-.48f,.075f,.1f);
            if (seed) return H(0xFFE08C);
            return x<-.2f?H(0xEF8791):H(0xD95672);
        }
        private static Color Pineapple(float x,float y)
        {
            bool fruit=E(x,y,0,-.28f,.58f,.65f);
            bool leaves=C(x,y,0,.25f,.05f,.9f,.105f)||C(x,y,-.07f,.24f,-.6f,.75f,.105f)
                ||C(x,y,.1f,.23f,.63f,.71f,.105f)||C(x,y,-.12f,.25f,-.27f,.91f,.105f)||R(x,y,0,.25f,.2f,.14f)
                ||E(x,y,-.23f,.53f,.09f,.15f);
            if (!fruit&&!leaves) return Color.clear;
            if (leaves) return x<0?H(0x5AA87A):H(0x81C280);
            if (E(Mathf.Abs(x),y,.23f,-.08f,.105f,.13f)||E(Mathf.Abs(x),y,.23f,-.4f,.105f,.13f)||E(x,y,0,-.68f,.105f,.12f)) return H(0xAC823D);
            return x<0?H(0xEDCA67):H(0xDEA84C);
        }
        private static Color Mushroom(float x,float y)
        {
            bool cap=E(x,y,0,.19f,.9f,.68f)&&y>-.12f;
            bool stem=E(x,y,0,-.4f,.35f,.51f);
            bool grass=C(x,y,-.16f,-.79f,-.51f,-.65f,.13f)||C(x,y,.15f,-.79f,.53f,-.63f,.13f);
            if (!cap&&!stem&&!grass) return Color.clear;
            if (grass) return H(0x6AAC7C);
            if (!cap) return Cream;
            if (E(x,y,-.4f,.34f,.18f,.17f)||E(x,y,.23f,.58f,.16f,.13f)||E(x,y,.51f,.09f,.16f,.14f)) return H(0xC8AA61);
            if (y<.02f) return H(0xAD5B71);
            return H(0xDE7681);
        }
        private static Color Sunflower(float x,float y)
        {
            bool stem=C(x,y,0,-.08f,0,-.94f,.085f);
            bool leaves=E(x,y,-.3f,-.56f,.35f,.16f)||E(x,y,.28f,-.72f,.32f,.15f);
            float dx=x,dy=y-.3f,r=Mathf.Sqrt(dx*dx+dy*dy),angle=Mathf.Atan2(dy,dx);
            bool flower=r<.57f+.1f*Mathf.Cos(angle*12f);
            if (!flower&&!stem&&!leaves) return Color.clear;
            if (!flower) return leaves?H(0x73B775):H(0x559F72);
            if (r<.3f) return x<-.08f?H(0xBA9152):H(0x8F6A43);
            return r>.48f?H(0xE5A84C):H(0xF4CF60);
        }
        private static Color Tulip(float x,float y)
        {
            bool flower=E(x,y,0,.32f,.57f,.48f)&&y<.59f;
            bool petals=T(x,y,-.56f,.39f,-.57f,.9f,-.1f,.48f)||T(x,y,-.27f,.45f,0,.96f,.27f,.45f)
                ||T(x,y,.1f,.48f,.57f,.9f,.56f,.39f);
            bool stem=C(x,y,0,.03f,0,-.93f,.075f);
            bool leaves=C(x,y,-.02f,-.72f,-.53f,-.43f,.13f)||C(x,y,.02f,-.62f,.51f,-.31f,.13f);
            if (!flower&&!petals&&!stem&&!leaves) return Color.clear;
            if (!flower&&!petals) return x<0?H(0x69AB78):H(0x8ABF80);
            return x<-.15f?H(0xD66696):(x>.15f?H(0xF49DB4):H(0xE881AA));
        }
    }
}
