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
            if (Eyes(x,y,.3f,.03f,.075f)||E(x,y,0,-.21f,.09f,.065f)) return Ink;
            if (E(x,y,0,-.4f,.34f,.23f)) return Cream;
            if (y>.27f && (a<.085f || Mathf.Abs(a-.25f)<.065f)) return H(0xBE603A);
            if (C(a,y,.43f,-.31f,.69f,-.24f,.024f)||C(a,y,.44f,-.43f,.69f,-.46f,.024f)) return H(0xBE603A);
            return H(0xF4A746);
        }
        private static Color Puppy(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ears=E(a,y,.65f,.04f,.27f,.66f);
            if (!E(x,y,0,-.05f,.66f,.76f)&&!ears) return Color.clear;
            if (ears&&a>.51f) return H(0xB66C42);
            if (Eyes(x,y,.25f,.15f,.075f)||E(x,y,0,-.24f,.14f,.1f)) return Ink;
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
            if (Eyes(x,y,.29f,.12f)||E(x,y,0,-.2f,.13f,.09f)) return Ink;
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
            if (Eyes(x,y,.34f,.13f,.09f)||E(x,y,0,-.58f,.12f,.09f)) return Ink;
            if (T(a,y,.1f,-.61f,.78f,.12f,.3f,-.06f)) return Cream;
            return H(0xF07B35);
        }
        private static Color Owl(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool ears=T(a,y,.25f,.57f,.7f,.96f,.71f,.27f);
            bool feet=R(a,y,.31f,-.79f,.2f,.12f);
            if (!E(x,y,0,-.05f,.75f,.83f)&&!ears&&!feet) return Color.clear;
            if (feet||T(x,y,-.13f,.05f,.13f,.05f,0,-.2f)) return H(0xFFBF50);
            if (Eyes(x,y,.3f,.3f,.09f)) return Ink;
            if (E(a,y,.29f,.28f,.27f,.3f)) return Cream;
            if (a>.5f&&y<.12f) return H(0x564C90);
            if (y<-.1f&&a<.42f) return H(0xBDA6E0);
            return H(0x8371B5);
        }
        private static Color Chick(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool feet=T(a,y,.15f,-.61f,.49f,-.91f,.04f,-.91f);
            bool tuft=T(x,y,-.18f,.6f,.1f,.96f,.22f,.56f);
            if (!E(x,y,0,-.05f,.7f,.75f)&&!E(a,y,.63f,-.09f,.27f,.33f)&&!feet&&!tuft) return Color.clear;
            if (feet||T(x,y,-.15f,.1f,.15f,.1f,0,-.12f)) return H(0xEE8D30);
            if (Eyes(x,y,.24f,.22f)) return Ink;
            if (E(a,y,.4f,-.07f,.14f,.09f)) return H(0xFFA16F);
            if (a>.55f&&y<.1f) return H(0xE7B13E);
            return H(0xF9D85D);
        }
    }
}
