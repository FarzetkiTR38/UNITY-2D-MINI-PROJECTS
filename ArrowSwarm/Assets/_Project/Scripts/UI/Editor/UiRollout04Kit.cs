namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.U2D.Sprites;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Imports one reusable symbol atlas and authors its shared Image instances.</summary>
    internal static class UiRollout04Kit
    {
        private const string Path = "Assets/_Project/Art/Sprites/SharedUI/phase04_symbols.png";

        internal static Image Icon(Transform parent,string name,string icon,Vector2 position,Vector2 size)
        {
            var sprite=AssetDatabase.LoadAllAssetsAtPath(Path).OfType<Sprite>().FirstOrDefault(s=>s.name==icon);
            if(sprite==null)throw new InvalidOperationException("Import the shared board04 symbols first: "+icon);
            var image=UiRollout01Kit.Image(parent,name,sprite,position,size);
            image.preserveAspect=true;return image;
        }

        internal static void Import()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Import in Edit Mode.");
            AssetDatabase.ImportAsset(Path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(Path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=100;importer.alphaIsTransparency=true;importer.sRGBTexture=true;
            importer.mipmapEnabled=false;importer.isReadable=false;importer.filterMode=FilterMode.Bilinear;
            importer.wrapMode=TextureWrapMode.Clamp;importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.CompressedHQ;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;
            importer.SetTextureSettings(settings);
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=1024,format=TextureImporterFormat.ASTC_6x6,compressionQuality=80});
            importer.SaveAndReimport();
            var factories=new SpriteDataProviderFactories();factories.Init();
            var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var previous=provider.GetSpriteRects().ToDictionary(x=>x.name);
            string[] names={"circle","triangle","square","lock","shield","geometry_trio"};
            var rects=new List<SpriteRect>();var pairs=new List<SpriteNameFileIdPair>();
            for(int i=0;i<names.Length;i++)
            {
                SpriteRect old;
                var rect=new SpriteRect{name=names[i],rect=new Rect((i%3)*512,1024-(i/3+1)*512,512,512),pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,
                    spriteID=previous.TryGetValue(names[i],out old)?old.spriteID:GUID.Generate()};
                if(i==5)rect.rect=new Rect(1053,184,436,160);
                rects.Add(rect);pairs.Add(new SpriteNameFileIdPair(rect.name,rect.spriteID));
            }
            provider.SetSpriteRects(rects.ToArray());provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pairs);
            provider.Apply();importer.SaveAndReimport();
        }
    }
}
