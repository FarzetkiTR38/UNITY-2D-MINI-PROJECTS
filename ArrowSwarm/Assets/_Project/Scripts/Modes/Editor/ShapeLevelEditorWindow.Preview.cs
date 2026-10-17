namespace ArrowSwarm.Modes.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>Clickable color/path visualization for the offline Shapes editor.</summary>
    public sealed partial class ShapeLevelEditorWindow
    {
        private void DrawPreview()
        {
            float width=Mathf.Min(position.width-40,700),height=width*_level.Height/_level.Width;
            var area=GUILayoutUtility.GetRect(width,height);
            EditorGUI.DrawRect(area,new Color(.98f,.96f,.91f));
            float s=Mathf.Min(area.width/(_level.Width+2),area.height/(_level.Height+2));
            System.Func<Vector2Int,Vector3> project=p=>new Vector3(area.x+s*(p.x+1),area.yMax-s*(p.y+1));
            Handles.BeginGUI();
            for(int i=0;i<_level.Arrows.Count;i++)
            {
                var a=_level.Arrows[i];if(a.Points.Count<2)continue;
                var points=new Vector3[a.Points.Count];for(int j=0;j<points.Length;j++)points[j]=project(a.Points[j]);
                Handles.color=a.Color;Handles.DrawAAPolyLine(i==_selected?5:2.5f,points);
                Vector3 tip=points[0],d=new Vector3(a.Direction.x,-a.Direction.y),perp=new Vector3(-d.y,d.x);
                Handles.DrawAAPolyLine(2.5f,tip-d*s*.4f+perp*s*.25f,tip,tip-d*s*.4f-perp*s*.25f);
                if(Event.current.type==EventType.MouseDown && area.Contains(Event.current.mousePosition))
                    for(int j=1;j<points.Length;j++)if(HandleUtility.DistancePointToLineSegment(Event.current.mousePosition,points[j-1],points[j])<6){_selected=i;Repaint();break;}
            }
            Handles.EndGUI();
        }
    }
}
