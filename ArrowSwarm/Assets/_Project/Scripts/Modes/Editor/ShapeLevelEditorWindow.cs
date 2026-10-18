namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Offline clean-image authoring, generation and manual path/color editing.</summary>
    public sealed partial class ShapeLevelEditorWindow : EditorWindow
    {
        private Texture2D _source;
        private PuzzleLevel _level;
        private int _width=32,_height=38,_seed=42,_selected=-1;
        private int _minLength=4,_maxLength=18;
        private float _backgroundTolerance=.15f,_alphaThreshold=.2f;
        private Color _background=Color.white;
        private Vector2 _scroll;
        private string _status="Choose a clean image or an existing Shapes level.";

        [MenuItem("Arrow Swarm/Puzzles/Shapes Image and Path Editor")]
        public static void Open()
        {
            var window=GetWindow<ShapeLevelEditorWindow>("Shapes Editor");
            if(Selection.activeObject is PuzzleLevel){window._level=(PuzzleLevel)Selection.activeObject;window._selected=-1;window.Validate();}
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("SHAPES · IMAGE → VALIDATED PUZZLE",EditorStyles.boldLabel);
            _level=(PuzzleLevel)EditorGUILayout.ObjectField("Level asset",_level,typeof(PuzzleLevel),false);
            _source=(Texture2D)EditorGUILayout.ObjectField("Source image",_source,typeof(Texture2D),false);
            if(GUILayout.Button("Import PNG / JPG from disk"))ImportImage();
            EditorGUILayout.BeginHorizontal();_width=EditorGUILayout.IntSlider("Columns",_width,8,64);_height=EditorGUILayout.IntSlider("Rows",_height,8,64);EditorGUILayout.EndHorizontal();
            _seed=EditorGUILayout.IntField("Seed",_seed);
            _background=EditorGUILayout.ColorField("Background to ignore",_background);
            _backgroundTolerance=EditorGUILayout.Slider("Background tolerance",_backgroundTolerance,0,.6f);
            _alphaThreshold=EditorGUILayout.Slider("Alpha threshold",_alphaThreshold,0,1);
            _minLength=EditorGUILayout.IntSlider("Min path points",_minLength,2,16);
            _maxLength=EditorGUILayout.IntSlider("Max path points",_maxLength,_minLength,40);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Generate from image"))Generate();
            if(GUILayout.Button("Validate / solve") && _level)Validate();
            if(GUILayout.Button("Save validated asset") && _level)Save();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(_status,MessageType.Info);
            _scroll=EditorGUILayout.BeginScrollView(_scroll);
            if(_level){DrawPreview();DrawSelected();}
            EditorGUILayout.EndScrollView();
        }
        private void ImportImage()
        {
            string path=EditorUtility.OpenFilePanel("Import clean shape image","","png,jpg,jpeg");if(string.IsNullOrEmpty(path))return;
            string directory=ModeAuthoringUtility.Art+"ShapeSources";ModeAuthoringUtility.Folder(directory);
            string dest=AssetDatabase.GenerateUniqueAssetPath(directory+"/"+System.IO.Path.GetFileName(path));
            System.IO.File.Copy(path,dest);AssetDatabase.ImportAsset(dest);_source=AssetDatabase.LoadAssetAtPath<Texture2D>(dest);
        }
        private void Generate()
        {
            if(!_source){_status="Choose a source image first.";return;}
            var importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_source)) as TextureImporter;
            if(importer!=null && !importer.isReadable){importer.isReadable=true;importer.SaveAndReimport();}
            // One image sample per atomic tile keeps every selected cell coverable by an arrow.
            _width += _width & 1; _height += _height & 1;
            System.Func<int,int,Color> sample=(x,y)=>_source.GetPixelBilinear((x/2*2+1f)/_width,(y/2*2+1f)/_height);
            System.Func<int,int,bool> mask=(x,y)=>
            {
                var c=sample(x,y);var d=new Vector3(c.r-_background.r,c.g-_background.g,c.b-_background.b);
                return c.a>_alphaThreshold && d.magnitude>_backgroundTolerance;
            };
            List<PuzzleArrowData> arrows;
            try { arrows=ShapePathGenerator.Generate(_width,_height,mask,sample,_seed,_minLength,_maxLength); }
            catch (System.Exception error) { _status=error.Message; return; }
            if(_level && !AssetDatabase.Contains(_level))DestroyImmediate(_level);
            _level=CreateInstance<PuzzleLevel>();_level.Author(GameModeId.Shapes,_source.name,_width,_height,arrows,null,0,_source);
            _selected=-1;Validate();
        }
        private void Validate()
        {
            var result=PuzzleSolver.Solve(_level);
            _status=result.Solvable?$"VALID · {_level.Arrows.Count} arrows · {result.Steps.Count} solution steps. Click an arrow to edit.":"INVALID · "+result.Error;
        }
        private void Save()
        {
            if(!PuzzleSolver.Solve(_level).Solvable){_status="Fix validation errors before saving.";return;}
            if(AssetDatabase.Contains(_level)){EditorUtility.SetDirty(_level);AssetDatabase.SaveAssets();_status="Validated asset saved.";return;}
            string path=EditorUtility.SaveFilePanelInProject("Save Shapes Level",_level.Title,"asset","Save a validated level",ModeAuthoringUtility.Data);
            if(string.IsNullOrEmpty(path))return;
            AssetDatabase.CreateAsset(_level,path);AssetDatabase.SaveAssets();_status="Level saved. Add it to ShapesCatalog to publish in the campaign.";
        }
        private void DrawSelected()
        {
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Add arrow"))
            {Undo.RecordObject(_level,"Add puzzle arrow");_level.Arrows.Add(new PuzzleArrowData(new List<Vector2Int>{new Vector2Int(1,1),new Vector2Int(1,0)},Vector2Int.up,Color.cyan));_selected=_level.Arrows.Count-1;EditorUtility.SetDirty(_level);}
            if(GUILayout.Button("Preview in Scene View"))PuzzleEditorPreview.Show(_level);
            EditorGUILayout.EndHorizontal();
            if(_selected<0 || _selected>=_level.Arrows.Count)return;
            var arrow=_level.Arrows[_selected];EditorGUILayout.LabelField("Arrow "+(_selected+1),EditorStyles.boldLabel);
            Color color=EditorGUILayout.ColorField("Path color",arrow.Color);
            if(color!=arrow.Color){Undo.RecordObject(_level,"Change arrow color");_level.Arrows[_selected]=new PuzzleArrowData(arrow.Points,arrow.Direction,color,arrow.ColorId);EditorUtility.SetDirty(_level);}
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Reverse head / tail"))
            {Undo.RecordObject(_level,"Reverse arrow");var points=new List<Vector2Int>(arrow.Points);points.Reverse();_level.Arrows[_selected]=new PuzzleArrowData(points,points[0]-points[1],color,arrow.ColorId);EditorUtility.SetDirty(_level);}
            if(GUILayout.Button("Delete arrow")){Undo.RecordObject(_level,"Delete arrow");_level.Arrows.RemoveAt(_selected);_selected=-1;EditorUtility.SetDirty(_level);return;}
            EditorGUILayout.EndHorizontal();
            for(int i=0;i<arrow.Points.Count;i++)
            {
                var point=EditorGUILayout.Vector2IntField(i==0?"Head":"Point "+i,arrow.Points[i]);
                if(point!=arrow.Points[i]){Undo.RecordObject(_level,"Edit arrow point");arrow.Points[i]=point;EditorUtility.SetDirty(_level);}
            }
            if(GUILayout.Button("Append tail point")){Undo.RecordObject(_level,"Extend arrow");arrow.Points.Add(arrow.Points[arrow.Points.Count-1]+Vector2Int.down);EditorUtility.SetDirty(_level);}
            if(GUILayout.Button("Remove last point") && arrow.Points.Count>2){Undo.RecordObject(_level,"Trim arrow");arrow.Points.RemoveAt(arrow.Points.Count-1);EditorUtility.SetDirty(_level);}
        }
        private void OnEnable(){Undo.undoRedoPerformed+=Repaint;}
        private void OnDisable(){Undo.undoRedoPerformed-=Repaint;}
    }
}
