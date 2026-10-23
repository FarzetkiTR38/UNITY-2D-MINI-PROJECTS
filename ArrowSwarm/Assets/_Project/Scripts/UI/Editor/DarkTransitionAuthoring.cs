namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using ArrowSwarm.Core;

    /// <summary>Authors the Dark transition palette on CoreManagers, leaving the existing Light material unchanged.</summary>
    public static class DarkTransitionAuthoring
    {
        /// <summary>Saves only the transition manager's Dark Inspector fields.</summary>
        public static void Apply()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author in Edit Mode.");
            string path="Assets/_Project/Prefabs/Core/CoreManagers.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var data=new SerializedObject(root.GetComponentInChildren<SceneTransitionManager>(true));
                string[] fields={"_darkBackgroundColor","_darkBorderColor","_darkSecondaryColor","_darkCyanColor","_darkHighlightColor","_darkPatternColor","_darkStarColor"};
                string[] hex={"0E1423","4AABFF","FFC655","46DDED","C5F5FF","5B8DC3","EBB34C"};
                for(int i=0;i<fields.Length;i++){Color color;ColorUtility.TryParseHtmlString("#"+hex[i],out color);data.FindProperty(fields[i]).colorValue=color;}
                data.FindProperty("_darkPatternOpacity").floatValue=.18f;data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
    }
}
