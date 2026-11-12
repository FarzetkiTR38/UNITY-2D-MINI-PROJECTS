namespace ArrowSwarm.UI
{
    using UnityEngine;

    /// <summary>National-colour geometry for the existing country list, rebuilt only on identity/layout changes.</summary>
    public partial class CountryFlagGraphic
    {
        private void UnitedStates()
        {
            Box(0,0,1,1,Color.white);
            for(int i=0;i<13;i+=2)Box(0,i/13f,1,1f/13,Red);
            Box(0,6f/13,.45f,7f/13,Navy);
            for(int y=0;y<9;y++)for(int x=0;x<(y%2==0?6:5);x++)Star(.04f+x*.071f+(y%2)*.035f,.94f-y*.051f,.023f,Color.white);
        }
        private void UnionJack(float x,float y,float w,float h)
        {
            Box(x,y,w,h,Navy);
            Poly(Color.white,new Vector2(x,y),new Vector2(x+.12f*w,y),new Vector2(x+w,y+.88f*h),new Vector2(x+w,y+h),new Vector2(x+.88f*w,y+h),new Vector2(x,y+.12f*h));
            Poly(Color.white,new Vector2(x,y+h),new Vector2(x,y+.88f*h),new Vector2(x+.88f*w,y),new Vector2(x+w,y),new Vector2(x+w,y+.12f*h),new Vector2(x+.12f*w,y+h));
            Poly(Red,new Vector2(x,y),new Vector2(x+.045f*w,y),new Vector2(x+w,y+.955f*h),new Vector2(x+w,y+h),new Vector2(x+.955f*w,y+h),new Vector2(x,y+.045f*h));
            Poly(Red,new Vector2(x,y+h),new Vector2(x,y+.955f*h),new Vector2(x+.955f*w,y),new Vector2(x+w,y),new Vector2(x+w,y+.045f*h),new Vector2(x+.045f*w,y+h));
            Box(x+.39f*w,y,.22f*w,h,Color.white);Box(x,y+.34f*h,w,.32f*h,Color.white);
            Box(x+.44f*w,y,.12f*w,h,Red);Box(x,y+.41f*h,w,.18f*h,Red);
        }
        private void Canada()
        {
            Box(0,0,1,1,Red);Box(.25f,0,.5f,1,Color.white);
            // Concave leaf is represented by a radial fan around its centre.
            var outline=new[]{new Vector2(.5f,.5f),new Vector2(.49f,.16f),new Vector2(.48f,.34f),new Vector2(.31f,.34f),new Vector2(.35f,.43f),new Vector2(.30f,.57f),new Vector2(.39f,.55f),new Vector2(.38f,.68f),new Vector2(.45f,.64f),new Vector2(.5f,.84f),new Vector2(.55f,.64f),new Vector2(.62f,.68f),new Vector2(.61f,.55f),new Vector2(.70f,.57f),new Vector2(.65f,.43f),new Vector2(.69f,.34f),new Vector2(.52f,.34f),new Vector2(.51f,.16f),new Vector2(.49f,.16f)};
            Poly(Red,outline);
        }
        private void Brazil()
        {
            Box(0,0,1,1,Green);Poly(Gold,new Vector2(.08f,.5f),new Vector2(.5f,.88f),new Vector2(.92f,.5f),new Vector2(.5f,.12f));
            Disc(.5f,.5f,.265f,Blue);Poly(Color.white,new Vector2(.36f,.55f),new Vector2(.36f,.50f),new Vector2(.64f,.40f),new Vector2(.64f,.45f));
        }
        private void Korea()
        {
            Box(0,0,1,1,Color.white);Disc(.5f,.5f,.255f,Red);
            float aspect=rectTransform.rect.height/Mathf.Max(1,rectTransform.rect.width);
            var p=new Vector2[18];p[0]=new Vector2(.5f,.5f);
            for(int i=0;i<=16;i++){float a=Mathf.PI+i*Mathf.PI/16;p[i+1]=new Vector2(.5f+Mathf.Cos(a)*.255f*aspect,.5f+Mathf.Sin(a)*.255f);}Poly(Blue,p);
            Disc(.5f-.1275f*aspect,.5f,.1275f,Red);Disc(.5f+.1275f*aspect,.5f,.1275f,Blue);
            // Geon, Gam, Ri and Gon retain their distinct solid/broken bars.
            float dx=.38f*aspect, angle=Mathf.Atan(1.5f)*Mathf.Rad2Deg;
            KoreaTrigram(.5f-dx,.7533f,angle,0);KoreaTrigram(.5f+dx,.7533f,-angle,5);
            KoreaTrigram(.5f-dx,.2467f,-angle,2);KoreaTrigram(.5f+dx,.2467f,angle,7);
        }
        private void KoreaTrigram(float x,float y,float angle,int brokenMask)
        {
            float aspect=rectTransform.rect.height/Mathf.Max(1,rectTransform.rect.width),a=angle*Mathf.Deg2Rad;
            var along=new Vector2(Mathf.Cos(a)*aspect,Mathf.Sin(a));
            var across=new Vector2(-Mathf.Sin(a)*aspect,Mathf.Cos(a));
            for(int i=0;i<3;i++)
            {
                Vector2 centre=new Vector2(x,y)+across*((i-1)*.0625f);
                if((brokenMask&(1<<i))==0)KoreaBar(centre,along,across,.25f);
                else{KoreaBar(centre-along*.0729167f,along,across,.1041667f);KoreaBar(centre+along*.0729167f,along,across,.1041667f);}
            }
        }
        private void KoreaBar(Vector2 centre,Vector2 along,Vector2 across,float length)
        {
            Vector2 half=along*(length*.5f),thickness=across*(1f/48);
            Poly(Color.black,centre-half-thickness,centre+half-thickness,centre+half+thickness,centre-half+thickness);
        }
        private void Australia()
        {
            Box(0,0,1,1,Navy);UnionJack(0,.5f,.5f,.5f);Star(.26f,.25f,.13f,Color.white);
            Star(.77f,.80f,.075f,Color.white);Star(.64f,.52f,.075f,Color.white);Star(.90f,.56f,.075f,Color.white);Star(.77f,.20f,.075f,Color.white);Star(.84f,.40f,.04f,Color.white);
        }
        private void Globe()
        {
            Box(0,0,1,1,new Color(.06f,.45f,.85f));Disc(.5f,.5f,.40f,Color.white);Disc(.5f,.5f,.35f,new Color(.06f,.45f,.85f));
            Box(.24f,.48f,.52f,.04f,Color.white);Box(.48f,.15f,.04f,.7f,Color.white);
        }
    }
}
