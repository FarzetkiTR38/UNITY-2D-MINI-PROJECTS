namespace ArrowSwarm.Modes.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Small deterministic editor-only helpers; all generated styling is serialized.</summary>
    public static class ModeAuthoringUtility
    {
        public const string Art = "Assets/_Project/Art/Sprites/ModeMenu/";
        public const string Data = "Assets/_Project/ScriptableObjects/Puzzles/";
        public static TMP_FontAsset Font => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/LilitaOne_SDF.asset") ?? TMP_Settings.defaultFontAsset;

        /// <summary>Creates directories and refreshes their asset identity.</summary>
        public static void Folder(string path) { System.IO.Directory.CreateDirectory(path); AssetDatabase.Refresh(); }
        /// <summary>Assigns Inspector fields without adding runtime styling code.</summary>
        public static void Set(UnityEngine.Object target, string field, object value)
        {
            var so = new SerializedObject(target); var p=so.FindProperty(field);
            if(p==null) throw new ArgumentException(target.GetType().Name+"."+field);
            if(value is UnityEngine.Object[]) { var array=(UnityEngine.Object[])value; p.arraySize=array.Length; for(int i=0;i<array.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=array[i]; }
            else if(value is UnityEngine.Object || value==null)p.objectReferenceValue=value as UnityEngine.Object;
            else if(value is int)p.intValue=(int)value;
            else if(value is float)p.floatValue=(float)value;
            else if(value is bool)p.boolValue=(bool)value;
            else if(value is string)p.stringValue=(string)value;
            so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(target);
        }
        /// <summary>Creates a fixed authored rectangle in a top-left reference coordinate system.</summary>
        public static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(width,height);return r;
        }
        /// <summary>Creates a full-parent rectangle.</summary>
        public static RectTransform Stretch(string name,Transform parent)
        {
            var r=Rect(name,parent,0,0,0,0);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        /// <summary>Creates an authored Image.</summary>
        public static Image Image(RectTransform rect, Sprite sprite, Color color, bool raycast=false)
        {var i=rect.gameObject.AddComponent<Image>();i.sprite=sprite;i.color=color;i.raycastTarget=raycast;return i;}
        /// <summary>Creates a live TextMeshPro label.</summary>
        public static TextMeshProUGUI Text(RectTransform rect,string text,float size,Color color)
        {var t=rect.gameObject.AddComponent<TextMeshProUGUI>();t.font=Font;t.text=text;t.fontSize=size;t.color=color;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.fontStyle=FontStyles.Bold;return t;}
        /// <summary>Creates an authored button with a visible label or supplied sprite.</summary>
        public static Button Button(RectTransform r,string label,Sprite sprite=null)
        {
            var image=Image(r,sprite?sprite:AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),sprite?Color.white:new Color(.02f,.34f,.94f),true);
            if(!sprite)image.type=UnityEngine.UI.Image.Type.Sliced;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;
            if(!string.IsNullOrEmpty(label))Text(Stretch("Label",r),label,32,Color.white);
            return b;
        }
        /// <summary>Imports source art and persists a reusable rectangular sprite subasset.</summary>
        public static Sprite Slice(string source,string name,float x,float top,float width,float height)
        {
            string dest=Art+"Slices/"+name+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Sprite>(dest);if(existing)return existing;
            var importer=(TextureImporter)AssetImporter.GetAtPath(Art+source);
            importer.textureType=TextureImporterType.Sprite;importer.alphaIsTransparency=true;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+source);
            var sprite=Sprite.Create(texture,new UnityEngine.Rect(x,texture.height-top-height,width,height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            sprite.name=name;AssetDatabase.CreateAsset(sprite,dest);return sprite;
        }
        /// <summary>Loads a full source sprite.</summary>
        public static Sprite Full(string source)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Art+source);importer.textureType=TextureImporterType.Sprite;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+source);
            return Slice(source,"Full_"+System.IO.Path.GetFileNameWithoutExtension(source),0,0,texture.width,texture.height);
        }
    }
}
