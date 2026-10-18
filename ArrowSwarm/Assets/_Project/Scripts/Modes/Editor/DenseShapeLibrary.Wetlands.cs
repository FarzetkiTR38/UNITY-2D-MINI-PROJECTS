namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Duck(float x,float y)
        {
            bool head=E(x,y,.32f,.51f,.36f,.38f);
            bool neck=C(x,y,.24f,.31f,.24f,-.17f,.22f);
            bool beak=R(x,y,.72f,.43f,.22f,.13f)||T(x,y,.72f,.57f,.98f,.37f,.55f,.3f);
            bool tail=T(x,y,-.54f,-.14f,-.98f,.2f,-.8f,-.53f);
            if (!E(x,y,-.18f,-.3f,.7f,.46f)&&!head&&!neck&&!beak&&!tail) return Color.clear;
            if (beak) return H(0xE79B39);
            if (E(x,y,.43f,.57f,.105f,.12f)) return Ink;
            if ((head||neck)&&y>.04f&&y<.2f) return White;
            if (head||neck) return H(0x379577);
            if (E(x,y,-.28f,-.29f,.43f,.27f)) return H(0x82654E);
            return H(0xB29C61);
        }
        private static Color Swan(float x,float y)
        {
            bool head=E(x,y,.37f,.65f,.25f,.23f);
            bool neck=C(x,y,.27f,-.3f,.01f,.02f,.15f)||C(x,y,.01f,.02f,.1f,.45f,.15f)
                ||C(x,y,.1f,.45f,.35f,.65f,.15f);
            bool beak=T(x,y,.54f,.73f,.94f,.57f,.55f,.47f);
            bool tail=T(x,y,-.49f,-.3f,-.91f,.02f,-.76f,-.63f);
            bool wing=T(x,y,-.57f,-.35f,-.24f,.12f,.21f,-.37f);
            if (!E(x,y,-.19f,-.43f,.7f,.35f)&&!head&&!neck&&!beak&&!tail&&!wing) return Color.clear;
            if (beak) return H(0xDF8B4E);
            if (E(x,y,.37f,.7f,.09f,.105f)||E(x,y,.57f,.6f,.09f,.12f)) return Ink;
            if (wing||E(x,y,-.24f,-.38f,.42f,.2f)) return H(0x6AA9C9);
            return White;
        }
        private static Color Dinosaur(float x,float y)
        {
            bool torso=E(x,y,-.25f,-.23f,.5f,.48f);
            bool neck=C(x,y,.09f,-.05f,.29f,.56f,.2f);
            bool head=E(x,y,.52f,.65f,.36f,.24f);
            bool tail=T(x,y,-.48f,-.06f,-.99f,.48f,-.71f,-.51f);
            bool legs=R(x,y,-.46f,-.69f,.17f,.23f)||R(x,y,.1f,-.69f,.17f,.23f);
            if (!torso&&!neck&&!head&&!tail&&!legs) return Color.clear;
            if (E(x,y,.59f,.71f,.1f,.115f)) return Ink;
            if (E(x,y,-.16f,-.33f,.31f,.33f)||neck&&x>.28f&&y>.08f) return H(0xBCD06B);
            if (legs&&y<-.76f) return H(0x397B68);
            if (E(x,y,-.5f,-.02f,.15f,.17f)||E(x,y,-.3f,.16f,.15f,.17f)) return H(0x409B91);
            return H(0x65A46D);
        }
        private static Color Elephant(float x,float y)
        {
            bool head=E(x,y,.36f,.27f,.41f,.44f);
            bool trunk=C(x,y,.72f,.27f,.8f,-.58f,.15f)||C(x,y,.8f,-.58f,.57f,-.65f,.15f);
            bool legs=R(x,y,-.5f,-.66f,.18f,.25f)||R(x,y,.08f,-.66f,.18f,.25f);
            bool tail=C(x,y,-.68f,.15f,-.9f,-.19f,.09f);
            bool tusk=T(x,y,.48f,.1f,.76f,-.22f,.57f,.03f);
            if (!E(x,y,-.24f,.02f,.59f,.57f)&&!head&&!trunk&&!legs&&!tail&&!tusk) return Color.clear;
            if (E(x,y,.47f,.38f,.095f,.11f)) return Ink;
            if (tusk) return Cream;
            if (E(x,y,.05f,.22f,.33f,.4f)) return H(0xC383B3);
            if (legs&&y<-.74f) return H(0xB3A2CC);
            if (trunk&&x>.62f) return H(0x9C84B7);
            return H(0x817EB2);
        }
        private static Color Bee(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool wing=E(a,y,.48f,.43f,.4f,.42f);
            bool antenna=C(a,y,.2f,.58f,.38f,.9f,.085f);
            bool sting=T(x,y,-.16f,-.61f,.16f,-.61f,0,-.95f);
            if (!E(x,y,0,-.12f,.56f,.71f)&&!wing&&!antenna&&!sting) return Color.clear;
            if (antenna||sting||Eyes(x,y,.22f,.28f,.095f)) return Ink;
            if (wing&&a>.37f&&y>.03f) return H(0x78BED5);
            if (y<.04f&&y>-.19f||y<-.4f&&y>-.62f) return H(0x504B64);
            return H(0xE6BB43);
        }
        private static Color Ladybug(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool legs=C(a,y,.4f,.15f,.87f,.32f,.09f)||C(a,y,.45f,-.18f,.91f,-.25f,.09f)
                ||C(a,y,.39f,-.45f,.78f,-.71f,.09f);
            bool head=E(x,y,0,.6f,.33f,.28f);
            if (!E(x,y,0,-.1f,.7f,.76f)&&!head&&!legs) return Color.clear;
            if (Eyes(x,y,.15f,.66f,.08f)) return White;
            if (head||legs||a<.07f||E(a,y,.36f,.24f,.17f,.18f)
                ||E(a,y,.42f,-.2f,.17f,.18f)||E(a,y,.26f,-.58f,.14f,.14f)) return Ink;
            return H(0xD85767);
        }
        private static Color Dragonfly(float x,float y)
        {
            float a=Mathf.Abs(x);
            bool upper=E(a,y,.47f,.36f,.48f,.23f);
            bool lower=E(a,y,.45f,-.04f,.43f,.21f)||E(a,y,.18f,.16f,.18f,.18f);
            bool trunk=E(x,y,0,-.21f,.16f,.73f)||E(x,y,0,.52f,.25f,.24f);
            if (!upper&&!lower&&!trunk) return Color.clear;
            if (Eyes(x,y,.13f,.56f,.08f)) return Ink;
            if (trunk) return y<-.25f?H(0x347D9E):H(0x499DAC);
            return upper?H(0x75B8D7):H(0xA18CCC);
        }
    }
}
