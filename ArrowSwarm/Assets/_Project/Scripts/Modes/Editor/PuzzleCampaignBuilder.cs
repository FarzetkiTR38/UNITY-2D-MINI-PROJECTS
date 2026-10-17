namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using U = ModeAuthoringUtility;

    /// <summary>Authors two five-level campaigns and proves their solutions before saving.</summary>
    public static class PuzzleCampaignBuilder
    {
        /// <summary>Creates initial catalogs, leaving existing authored levels intact on subsequent runs.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Create MVP Campaigns")]
        public static void Build()
        {
            U.Folder(U.Data);
            if(!AssetDatabase.LoadAssetAtPath<PuzzleConfig>(U.Data+"PuzzleConfig.asset"))AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PuzzleConfig>(),U.Data+"PuzzleConfig.asset");
            var shapes=new PuzzleLevel[5];var shifts=new PuzzleLevel[5];
            for(int i=0;i<5;i++)
            {
                int shape=i;
                int w=28+i*2,h=34+i*2;
                shapes[i]=Create("Shapes_"+(i+1),()=>
                {
                    System.Func<int,int,Color> sample=(x,y)=>ShapeSilhouettes.Sample(shape,(x/(float)(w-1)*2f-1f)*1.06f,(y/(float)(h-1)*2f-1f)*1.06f);
                    var arrows=ShapePathGenerator.Generate(w,h,(x,y)=>sample(x,y).a>.5f,sample,31415+shape*119,4,14+shape*2);
                    var level=ScriptableObject.CreateInstance<PuzzleLevel>();
                    level.Author(GameModeId.Shapes,ShapeSilhouettes.Names[shape],w,h,arrows);return level;
                });
                int size=10+i*2;
                shifts[i]=Create("ColorShift_"+(i+1),()=>
                {
                    var palette=Palettes(shape);
                    var basePaths=ShapePathGenerator.Generate(size,size,(x,y)=>true,(x,y)=>Color.white,720+shape*71,4,9+shape*2);
                    var arrows=new List<PuzzleArrowData>();
                    int groups=shape+2;
                    for(int j=0;j<basePaths.Count;j++)
                    {
                        var a=basePaths[j];int phase=(basePaths.Count-1-j)*groups/basePaths.Count;
                        int key=PuzzleRules.WallId(a.Direction,phase&1);
                        arrows.Add(new PuzzleArrowData(a.Points,a.Direction,palette[key],key));
                    }
                    var level=ScriptableObject.CreateInstance<PuzzleLevel>();
                    level.Author(GameModeId.ColorShift,new[]{"First Shift","Coral Current","Golden Circuit","Violet Switch","Prism Lock"}[shape],size,size,arrows,palette,groups);
                    return level;
                });
            }
            Catalog("ShapesCatalog",shapes);Catalog("ColorShiftCatalog",shifts);AssetDatabase.SaveAssets();
            PuzzleVerification.ValidateAll();
        }
        private static PuzzleLevel Create(string name,System.Func<PuzzleLevel> factory)
        {
            string path=U.Data+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<PuzzleLevel>(path);if(existing)return existing;
            var level=factory();level.name=name;
            var solution=PuzzleSolver.Solve(level);if(!solution.Solvable)throw new System.InvalidOperationException(name+": "+solution.Error);
            AssetDatabase.CreateAsset(level,path);return level;
        }
        private static void Catalog(string name,PuzzleLevel[] levels)
        {
            string path=U.Data+name+".asset";var catalog=AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(path);
            if(!catalog){catalog=ScriptableObject.CreateInstance<PuzzleCatalog>();AssetDatabase.CreateAsset(catalog,path);}
            catalog.Author(levels);EditorUtility.SetDirty(catalog);
        }
        private static Color[] Palettes(int index)
        {
            string[] a={"31B9C1","F27885","E2AC31","8873CF","39A8E0"},b={"C5A668","469EC4","618BD8","DB9C50","DE72A2"};
            Color x,y;ColorUtility.TryParseHtmlString("#"+a[index],out x);ColorUtility.TryParseHtmlString("#"+b[index],out y);return new[]{x,y};
        }
    }
}
