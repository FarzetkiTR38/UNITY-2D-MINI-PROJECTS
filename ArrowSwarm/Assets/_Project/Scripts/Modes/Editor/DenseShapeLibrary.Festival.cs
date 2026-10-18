namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Sailboat(float x,float y)
        {
            bool hull=T(x,y,-.92f,-.32f,.94f,-.32f,.48f,-.82f)||T(x,y,-.92f,-.32f,.48f,-.82f,-.56f,-.82f);
            bool mast=R(x,y,-.06f,.22f,.09f,.69f);
            bool sail=T(x,y,-.13f,.81f,-.13f,-.21f,-.87f,-.21f)||T(x,y,.02f,.65f,.83f,-.21f,.02f,-.21f);
            bool flag=T(x,y,-.02f,.91f,.4f,.77f,-.02f,.61f);
            if (!hull&&!mast&&!sail&&!flag) return Color.clear;
            if (flag) return H(0xE48092);
            if (mast) return H(0xAC8A67);
            if (hull) return y<-.56f?H(0x6285AD):H(0x81BBCE);
            return x<-.13f?H(0xEFCB76):H(0xDDA0B5);
        }
        private static Color IceCream(float x,float y)
        {
            bool cone=T(x,y,-.51f,-.05f,.51f,-.05f,0,-.98f);
            bool left=E(x,y,-.35f,.22f,.41f,.4f),right=E(x,y,.35f,.22f,.41f,.4f);
            bool top=E(x,y,0,.62f,.42f,.35f);
            bool bridge=R(x,y,0,-.06f,.35f,.2f);
            if (!cone&&!left&&!right&&!top&&!bridge) return Color.clear;
            if (top) return H(0xB5A0D4);
            if (left) return H(0xEBA0B1);
            if (right) return H(0x9BCBA7);
            if (bridge&&!cone) return Cream;
            if (Mathf.Repeat((x+y)*5f,1f)<.13f||Mathf.Repeat((x-y)*5f,1f)<.13f) return H(0xBA8B59);
            return H(0xD5AE78);
        }
        private static Color Teapot(float x,float y)
        {
            bool body=E(x,y,-.12f,-.16f,.62f,.55f);
            bool spout=T(x,y,.22f,-.14f,.94f,.43f,.7f,-.34f);
            bool handle=E(x,y,-.64f,.03f,.32f,.37f);
            bool lid=E(x,y,-.12f,.37f,.48f,.14f)||E(x,y,-.12f,.57f,.13f,.15f);
            bool baseBand=R(x,y,-.12f,-.67f,.44f,.09f);
            if (!body&&!spout&&!handle&&!lid&&!baseBand) return Color.clear;
            if (lid||baseBand) return H(0x7995C1);
            if (handle&&x<-.62f) return H(0xB5ACD2);
            if (E(x,y,-.12f,-.13f,.1f,.1f)) return H(0xE9C86F);
            if (E(x,y,-.12f,-.13f,.27f,.26f)) return H(0xDA9BB8);
            if (Mathf.Abs(y+.39f)<.045f) return H(0x9AB7D2);
            return White;
        }
        private static Color Windmill(float x,float y)
        {
            bool house=T(x,y,-.47f,-.88f,.47f,-.88f,.22f,.52f)||T(x,y,-.47f,-.88f,.22f,.52f,-.22f,.52f);
            bool roof=T(x,y,-.38f,.4f,.38f,.4f,0,.83f);
            bool blades=C(x,y,-.7f,-.22f,.7f,.8f,.12f)||C(x,y,-.7f,.8f,.7f,-.22f,.12f);
            bool hub=E(x,y,0,.29f,.2f,.2f);
            if (!house&&!roof&&!blades&&!hub) return Color.clear;
            if (hub) return H(0xD6AE64);
            if (blades) return Mathf.Repeat((x+y)*5f,1f)<.15f?H(0xB7A591):H(0xE1CEAA);
            if (roof) return H(0xBF788B);
            if (R(x,y,0,-.67f,.12f,.22f)||R(x,y,0,-.16f,.09f,.12f)) return H(0x768CAF);
            return H(0xDEC2A6);
        }
    }
}
