namespace ArrowSwarm.UI.Editor
{
    using System.Linq;
    using UnityEditor;
    using UnityEditor.U2D.Sprites;
    using UnityEngine;

    /// <summary>Imports only the missing floating cloud ornaments; skill heroes reuse their original sprites.</summary>
    internal static class UiRollout0608Art
    {
        internal const string CloudPath=UiRollout01Kit.ArtFolder+"/popup_cloud_corners.png";
        internal static void ImportClouds()=>Import(CloudPath,"cloud_left","cloud_right",512);
        private static void Import(string path,string first,string second,int maximum)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.isReadable=false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=maximum;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;
            importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=maximum,format=TextureImporterFormat.ASTC_6x6});
            importer.SaveAndReimport();
            importer.GetSourceTextureWidthAndHeight(out int width,out int height);
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var old=provider.GetSpriteRects().ToDictionary(r=>r.name);
            var rects=new SpriteRect[2];var pairs=new SpriteNameFileIdPair[2];
            for(int i=0;i<2;i++)
            {
                string name=i==0?first:second;
                rects[i]=new SpriteRect{name=name,rect=new Rect(i*width/2f,0,width/2f,height),pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,
                    spriteID=old.TryGetValue(name,out var previous)?previous.spriteID:GUID.Generate()};
                pairs[i]=new SpriteNameFileIdPair(name,rects[i].spriteID);
            }
            provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pairs);
            provider.Apply();importer.SaveAndReimport();
        }
        internal static Sprite Sprite(string name)=>AssetDatabase.LoadAllAssetsAtPath(CloudPath).OfType<Sprite>().First(s=>s.name==name);
    }
}
