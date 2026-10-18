namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    public static partial class DenseShapeLibrary
    {
        private static Color Ink => H(0x253356);
        private static Color Cream => H(0xDFB77D);
        private static Color White => H(0xA5CFE3);
        private static Color H(uint rgb) => new Color(((rgb>>16)&255)/255f, ((rgb>>8)&255)/255f, (rgb&255)/255f, 1);
        private static bool E(float x,float y,float cx,float cy,float rx,float ry)
        {
            x=(x-cx)/rx; y=(y-cy)/ry; return x*x+y*y<=1f;
        }
        private static bool R(float x,float y,float cx,float cy,float rx,float ry)
            => Mathf.Abs(x-cx)<=rx && Mathf.Abs(y-cy)<=ry;
        private static bool T(float x,float y,float ax,float ay,float bx,float by,float cx,float cy)
        {
            float a=(x-bx)*(ay-by)-(ax-bx)*(y-by);
            float b=(x-cx)*(by-cy)-(bx-cx)*(y-cy);
            float c=(x-ax)*(cy-ay)-(cx-ax)*(y-ay);
            return (a>=0 && b>=0 && c>=0)||(a<=0 && b<=0 && c<=0)
                ||C(x,y,ax,ay,bx,by,.025f)||C(x,y,bx,by,cx,cy,.025f)||C(x,y,cx,cy,ax,ay,.025f);
        }
        private static bool C(float x,float y,float ax,float ay,float bx,float by,float radius)
        {
            float dx=bx-ax,dy=by-ay;
            float t=Mathf.Clamp01(((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy));
            float px=x-ax-t*dx,py=y-ay-t*dy;
            return px*px+py*py<=radius*radius;
        }
        private static bool Eyes(float x,float y,float spacing,float cy,float radius=.065f)
            => E(Mathf.Abs(x),y,spacing,cy,radius,radius*1.2f);
        private static bool Star(float x,float y,float cx,float cy,float radius,float inner=.48f)
        {
            x-=cx; y-=cy;
            float angle=Mathf.Atan2(x,y), r=Mathf.Sqrt(x*x+y*y);
            float sector=Mathf.Repeat(angle+Mathf.PI/5f,Mathf.PI*2f/5f)-Mathf.PI/5f;
            float edge=radius*inner/Mathf.Cos(sector);
            return r<=Mathf.Lerp(edge,radius,Mathf.Pow(1f-Mathf.Abs(sector)/(Mathf.PI/5f),2));
        }
    }
}
