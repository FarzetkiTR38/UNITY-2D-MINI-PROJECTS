namespace ArrowSwarm.Modes.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>Transient gizmo preview has no runtime objects and never writes progress.</summary>
    [InitializeOnLoad]
    public static class PuzzleEditorPreview
    {
        private static PuzzleLevel _level;
        static PuzzleEditorPreview(){SceneView.duringSceneGui+=Draw;}
        /// <summary>Displays a validated or work-in-progress puzzle in Scene View.</summary>
        public static void Show(PuzzleLevel level)
        {
            _level=level;
            var view=SceneView.lastActiveSceneView;
            if(view && level){view.in2DMode=true;view.Frame(new Bounds(Vector3.zero,new Vector3(level.Width+3,level.Height+3,1)),false);}
            SceneView.RepaintAll();
        }
        /// <summary>Removes the transient preview.</summary>
        public static void Clear(){_level=null;SceneView.RepaintAll();}
        private static void Draw(SceneView view)
        {
            if(!_level || Application.isPlaying)return;
            Vector3 origin=new Vector3(-(_level.Width-1)*.5f,-(_level.Height-1)*.5f);
            Handles.Label(origin+new Vector3(0,_level.Height+1),_level.Title+" · EDITOR PREVIEW (no save changes)");
            foreach(var a in _level.Arrows)
            {
                if(a.Points.Count<2)continue;
                Handles.color=a.Color;var points=new Vector3[a.Points.Count];for(int i=0;i<points.Length;i++)points[i]=origin+(Vector3)(Vector2)a.Points[i];
                Handles.DrawAAPolyLine(3,points);Vector3 tip=points[0],d=(Vector3)(Vector2)a.Direction,p=new Vector3(-d.y,d.x);
                Handles.DrawAAPolyLine(3,tip-d*.4f+p*.25f,tip,tip-d*.4f-p*.25f);
            }
            if(_level.Mode==GameModeId.ColorShift)
            {
                float x=_level.Width*.5f+.4f,y=_level.Height*.5f+.4f;
                Vector3[] c={new Vector3(-x,y),new Vector3(x,y),new Vector3(x,-y),new Vector3(-x,-y)};
                for(int i=0;i<4;i++){Handles.color=_level.WallColors[i&1];Handles.DrawAAPolyLine(8,c[i],c[(i+1)%4]);}
            }
        }
    }
}
