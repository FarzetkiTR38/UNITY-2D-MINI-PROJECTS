namespace ArrowSwarm.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Draws small country identities without emoji fonts or per-country texture dependencies.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public partial class CountryFlagGraphic : MaskableGraphic
    {
        [SerializeField] private string _countryCode = "TR";
        private VertexHelper _mesh;
        private static readonly Color Red = new Color32(224, 33, 49, 255);
        private static readonly Color Blue = new Color32(0, 56, 146, 255);
        private static readonly Color Navy = new Color32(0, 36, 99, 255);
        private static readonly Color Gold = new Color32(255, 204, 0, 255);
        private static readonly Color Green = new Color32(0, 150, 72, 255);

        /// <summary>The normalized identity shown by this graphic; styling does not depend on UI theme.</summary>
        public string CountryCode => _countryCode;

        /// <summary>Changes only the displayed country data and rebuilds geometry when necessary.</summary>
        public void SetCountryCode(string code)
        {
            string next = string.IsNullOrWhiteSpace(code) ? "GL" : code.Trim().ToUpperInvariant();
            if (next == _countryCode) return;
            _countryCode = next; SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); _mesh = mesh;
            switch (_countryCode)
            {
                case "TR": Box(0, 0, 1, 1, Red); Disc(.43f, .5f, .25f, Color.white); Disc(.49f, .53f, .21f, Red); Star(.67f,.5f,.12f,Color.white); break;
                case "US": UnitedStates(); break;
                case "GB": UnionJack(0,0,1,1); break;
                case "FR": Vertical(Blue, Color.white, Red); break;
                case "IT": Vertical(Green, Color.white, Red); break;
                case "DE": Horizontal(Gold, Red, Color.black); break;
                case "RU": Horizontal(Red, Blue, Color.white); break;
                case "NL": Horizontal(Blue, Color.white, Red); break;
                case "ES": Box(0,0,1,1,Red); Box(0,.25f,1,.5f,Gold); break;
                case "JP": Box(0,0,1,1,Color.white); Disc(.5f,.5f,.25f,Red); break;
                case "CA": Canada(); break;
                case "BR": Brazil(); break;
                case "KR": Korea(); break;
                case "AU": Australia(); break;
                default: Globe(); break;
            }
            _mesh = null;
        }

        private void Horizontal(Color bottom, Color middle, Color top)
        { Box(0,0,1,1,top); Box(0,0,1,1f/3,bottom); Box(0,1f/3,1,1f/3,middle); }
        private void Vertical(Color left, Color middle, Color right)
        { Box(0,0,1,1,right); Box(0,0,1f/3,1,left); Box(1f/3,0,1f/3,1,middle); }
        private void Box(float x,float y,float w,float h,Color c)
        { Poly(c,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h)); }
        private void Poly(Color c,params Vector2[] p)
        {
            int start=_mesh.currentVertCount; Rect r=rectTransform.rect;
            foreach(var v in p) _mesh.AddVert(new Vector3(r.xMin+v.x*r.width,r.yMin+v.y*r.height),c*color,Vector2.zero);
            for(int i=1;i<p.Length-1;i++)_mesh.AddTriangle(start,start+i,start+i+1);
        }
        private void Disc(float x,float y,float radius,Color c)
        {
            var p=new Vector2[34]; p[0]=new Vector2(x,y);
            float aspect=rectTransform.rect.height/Mathf.Max(1,rectTransform.rect.width);
            for(int i=0;i<=32;i++){float a=i*Mathf.PI/16;p[i+1]=new Vector2(x+Mathf.Cos(a)*radius*aspect,y+Mathf.Sin(a)*radius);}
            Poly(c,p);
        }
        private void Star(float x,float y,float radius,Color c)
        {
            var p=new Vector2[12]; p[0]=new Vector2(x,y);
            float aspect=rectTransform.rect.height/Mathf.Max(1,rectTransform.rect.width);
            for(int i=0;i<=10;i++){float a=Mathf.PI*.5f+i*Mathf.PI/5;float rad=i%2==0?radius:radius*.42f;p[i+1]=new Vector2(x+Mathf.Cos(a)*rad*aspect,y+Mathf.Sin(a)*rad);}
            Poly(c,p);
        }
    }
}
