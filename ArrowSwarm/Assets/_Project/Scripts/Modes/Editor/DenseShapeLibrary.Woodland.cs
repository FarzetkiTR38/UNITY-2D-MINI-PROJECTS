namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Cat(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ears=T(a,y,.27f,.46f,.76f,.94f,.83f,.12f);
            if (!E(x,y,0,-.12f,.79f,.74f)&&!ears) return Color.clear;
            if (T(a,y,.43f,.43f,.70f,.76f,.71f,.31f)) return H(0xFF8A9A);
            if (Eyes(x,y,.3f,.03f,.105f)||E(x,y,0,-.21f,.13f,.1f)) return Ink;
            if (E(x,y,0,-.4f,.34f,.23f)) return Cream;
            if (y>.27f && (a<.085f || Mathf.Abs(a-.25f)<.065f)) return H(0xBE603A);
            return H(0xF4A746);
        }
        private static Color Puppy(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ears=E(a,y,.65f,.04f,.27f,.66f);
            if (!E(x,y,0,-.05f,.66f,.76f)&&!ears) return Color.clear;
            if (ears&&a>.51f) return H(0xB66C42);
            if (Eyes(x,y,.25f,.15f,.105f)||E(x,y,0,-.24f,.16f,.125f)) return Ink;
            if (E(x,y,0,-.56f,.12f,.18f)) return H(0xF77494);
            if (E(x,y,0,-.35f,.39f,.27f)) return Cream;
            return H(0xEDB86C);
        }
        private static Color Bear(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ears=E(a,y,.52f,.6f,.29f,.29f);
            if (!E(x,y,0,-.1f,.78f,.76f)&&!ears) return Color.clear;
            if (ears&&E(a,y,.52f,.63f,.16f,.16f)) return H(0xD78B67);
            if (Eyes(x,y,.29f,.12f,.105f)||E(x,y,0,-.2f,.16f,.12f)) return Ink;
            if (E(x,y,0,-.31f,.35f,.27f)) return Cream;
            return H(0xAE733E);
        }
        private static Color Fox(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool mask=T(x,y,-.88f,.31f,.88f,.31f,0,-.88f);
            bool ears=T(a,y,.16f,.38f,.75f,.96f,.82f,-.03f);
            if (!mask&&!ears&&!E(x,y,0,.25f,.72f,.35f)) return Color.clear;
            if (T(a,y,.37f,.42f,.68f,.77f,.68f,.19f)) return H(0x743B47);
            if (Eyes(x,y,.34f,.13f,.11f)||E(x,y,0,-.58f,.14f,.115f)) return Ink;
            if (E(a,y,.31f,-.18f,.3f,.28f)) return Cream;
            return H(0xF07B35);
        }
        private static Color Owl(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ears=T(a,y,.25f,.57f,.7f,.96f,.71f,.27f);
            bool feet=R(a,y,.31f,-.79f,.2f,.12f);
            if (!E(x,y,0,-.05f,.75f,.83f)&&!ears&&!feet) return Color.clear;
            if (feet||T(x,y,-.13f,.05f,.13f,.05f,0,-.2f)) return H(0xFFBF50);
            if (Eyes(x,y,.3f,.3f,.12f)) return Ink;
            if (E(a,y,.29f,.28f,.27f,.3f)) return Cream;
            if (a>.5f&&y<.12f) return H(0x564C90);
            if (y<-.1f&&a<.42f) return H(0xBDA6E0);
            return H(0x8371B5);
        }
        private static Color Chick(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool body=E(x,y,0,-.19f,.62f,.63f),head=E(x,y,0,.34f,.52f,.5f);
            bool wings=E(a,y,.55f,-.16f,.26f,.34f);
            bool feet=T(a,y,.14f,-.61f,.49f,-.91f,.09f,-.91f);
            bool tuft=T(x,y,-.24f,.64f,.02f,.97f,.14f,.65f)||T(x,y,0,.65f,.27f,.9f,.32f,.54f);
            if (!body&&!head&&!wings&&!feet&&!tuft) return Color.clear;
            if (Eyes(x,y,.22f,.4f,.11f)) return Ink;
            if (feet||T(x,y,-.18f,.22f,.18f,.22f,0,-.03f)) return H(0xD88832);
            if (E(a,y,.35f,.12f,.15f,.105f)) return H(0xDD9563);
            if (wings&&a>.49f) return H(0xC79235);
            if (E(x,y,0,-.29f,.37f,.37f)) return H(0xD9B659);
            return H(0xE8C347);
        }
    }
}
