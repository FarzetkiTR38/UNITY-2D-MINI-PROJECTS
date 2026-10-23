namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Brings missing map-preview bindings into parity with the existing Defense UI without changing Light styling.</summary>
    public static class MapPreviewThemeAuthoring
    {
        /// <summary>Copies only missing bindings with an exact matching hierarchy path from GameScene into MapTestScene.</summary>
        public static void Apply()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author in Edit Mode.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            var bindings=new Dictionary<string,string>();
            try
            {
                var source=EditorSceneManager.OpenScene("Assets/_Project/Scenes/GameScene.unity");
                foreach(var root in source.GetRootGameObjects())foreach(var binding in root.GetComponentsInChildren<ThemedUIElement>(true))
                    bindings.Add(Path(binding.transform),JsonUtility.ToJson(binding));
                var target=EditorSceneManager.OpenScene("Assets/_Project/Scenes/MapTestScene.unity");int count=0;
                foreach(var root in target.GetRootGameObjects())foreach(var graphic in root.GetComponentsInChildren<Graphic>(true))
                {
                    string json;if(graphic.GetComponent<ThemedUIElement>()!=null||!bindings.TryGetValue(Path(graphic.transform),out json))continue;
                    var binding=graphic.gameObject.AddComponent<ThemedUIElement>();JsonUtility.FromJsonOverwrite(json,binding);
                    var data=new SerializedObject(binding);data.FindProperty("_customLightColor").colorValue=graphic.color;
                    data.ApplyModifiedPropertiesWithoutUndo();
                    if(graphic is Image&&graphic.GetComponent<UIThemeCoordinates>()==null)graphic.gameObject.AddComponent<UIThemeCoordinates>();
                    count++;
                }
                EditorSceneManager.SaveScene(target);
                Debug.Log("[ArrowSwarm] Added "+count+" missing map-preview theme bindings.");
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        private static string Path(Transform t){return t.parent==null?t.name:Path(t.parent)+"/"+t.name;}
    }
}
