namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Parrot(float x,float y)
        {
            bool head=E(x,y,.24f,.54f,.38f,.37f);
            bool beak=E(x,y,.63f,.4f,.29f,.24f);
            bool tail=T(x,y,-.48f,-.15f,.2f,-.37f,-.64f,-.98f);
            bool perch=C(x,y,-.74f,-.58f,.6f,-.58f,.08f);
            if (!E(x,y,-.13f,-.06f,.48f,.6f)&&!head&&!beak&&!tail&&!perch) return Color.clear;
            if (perch) return H(0xB78350);
            if (beak) return H(0xF2BA45);
            if (E(x,y,.35f,.59f,.105f,.12f)) return Ink;
            if (E(x,y,.32f,.55f,.21f,.23f)) return Cream;
            if (tail||E(x,y,-.25f,-.13f,.25f,.41f)) return y<-.2f?H(0x4C82C6):H(0x71BE79);
            return H(0xE85E70);
        }
        private static Color Flamingo(float x,float y)
        {
            bool body=E(x,y,-.24f,-.03f,.55f,.39f);
            bool neck=C(x,y,.06f,.03f,.35f,.35f,.15f)||C(x,y,.35f,.35f,.25f,.65f,.15f);
            bool head=E(x,y,.31f,.7f,.23f,.21f);
            bool beak=T(x,y,.43f,.74f,.84f,.53f,.5f,.44f);
            bool legs=C(x,y,-.3f,-.28f,-.32f,-.9f,.085f)||C(x,y,-.02f,-.28f,.12f,-.62f,.085f)
                ||C(x,y,.12f,-.62f,.04f,-.88f,.085f)||C(x,y,-.45f,-.9f,-.27f,-.9f,.075f);
            if (!body&&!neck&&!head&&!beak&&!legs) return Color.clear;
            if (E(x,y,.35f,.75f,.085f,.1f)||beak&&x>.63f) return Ink;
            if (beak||legs) return H(0xDA9872);
            if (E(x,y,-.35f,-.02f,.32f,.21f)) return H(0xE85F94);
            return H(0xF4A1B5);
        }
        private static Color Toucan(float x,float y)
        {
            bool beak=E(x,y,.35f,.49f,.62f,.3f);
            bool head=E(x,y,-.23f,.43f,.36f,.42f);
            bool body=E(x,y,-.29f,-.13f,.41f,.61f);
            bool tail=T(x,y,-.54f,-.34f,-.08f,-.48f,-.61f,-.96f);
            bool perch=C(x,y,-.82f,-.7f,.64f,-.7f,.075f);
            if (!beak&&!head&&!body&&!tail&&!perch) return Color.clear;
            if (perch) return H(0xAA7F56);
            if (beak&&x>-.13f) return x>.53f?H(0xED725E):(y>.52f?H(0xEDC947):H(0x76C685));
            if (E(x,y,-.16f,.49f,.105f,.12f)) return H(0x0F203C);
            if (E(x,y,-.16f,.44f,.21f,.23f)||E(x,y,-.13f,.02f,.23f,.38f)) return Cream;
            if (E(x,y,-.43f,-.17f,.19f,.35f)) return H(0x4866A1);
            return H(0x35476D);
        }
        private static Color Peacock(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool fan=E(x,y,0,.22f,.92f,.68f)||E(a,y,.62f,-.05f,.29f,.5f);
            bool body=E(x,y,0,-.29f,.3f,.52f)||C(x,y,0,-.18f,0,.32f,.15f);
            bool head=E(x,y,0,.4f,.24f,.23f);
            bool feet=C(a,y,.1f,-.64f,.24f,-.91f,.08f);
            if (!fan&&!body&&!head&&!feet) return Color.clear;
            if (feet||T(x,y,.13f,.44f,.38f,.31f,.13f,.25f)) return H(0xD49B39);
            if (head&&Eyes(x,y,.105f,.46f,.07f)) return Ink;
            if (body||head) return H(0x4778BB);
            bool spot=E(a,y,.62f,.29f,.19f,.23f)||E(a,y,.35f,.64f,.17f,.2f)||E(x,y,0,.75f,.17f,.2f);
            bool eye=E(a,y,.62f,.29f,.095f,.12f)||E(a,y,.35f,.64f,.085f,.1f)||E(x,y,0,.75f,.085f,.1f);
            if (eye) return H(0x355D96);
            if (spot) return H(0xD9B74C);
            return H(0x4FA77B);
        }
    }
}
