namespace ArrowSwarm.UI.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Repairs clipped source artwork and oversized tutorial cards in the scene and prefab authoring pass.</summary>
    internal static class ThemeLayoutRepairs
    {
        internal static void Apply(GameObject root)
        {
            foreach (var tutorial in root.GetComponentsInChildren<ArrowSwarm.Tutorial.TutorialOverlayUI>(true))
            {
                var card = tutorial.transform.Find("CompleteCard");
                if (card != null) card.localScale=Vector3.one*0.73f;
                if (tutorial.transform.Find("CompletionBackdrop") == null)
                {
                    var rect=Rect("CompletionBackdrop",tutorial.transform,Vector2.zero,Vector2.one);
                    rect.SetSiblingIndex(0);
                    var image=rect.gameObject.AddComponent<Image>();
                    image.color=new Color(0.025f,0.04f,0.07f,0.72f);
                    image.raycastTarget=true;
                    var group=rect.gameObject.AddComponent<CanvasGroup>();
                    group.alpha=0;
                    group.blocksRaycasts=false;
                    var data=new SerializedObject(tutorial);
                    data.FindProperty("_completionBackdrop").objectReferenceValue=group;
                    data.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            foreach (var profile in root.GetComponentsInChildren<ProfileSetupModalUI>(true))
            {
                var data = new SerializedObject(profile);
                var button = data.FindProperty("_diceButton").objectReferenceValue as Button;
                if (button == null) continue;
                var frame=button.transform.Find("DiceButtonFrame") as RectTransform;
                if (frame == null)
                {
                    frame=Rect("DiceButtonFrame",button.transform,new Vector2(0.14f,0.14f),new Vector2(0.86f,0.86f));
                    var display=frame.gameObject.AddComponent<Image>();
                    var assets=AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/_FarzetkiArts/LevelPanel/ArrowSwarm_LevelMenu_Sprites (3)-Photoroom.png");
                    foreach (var asset in assets)
                        if (asset is Sprite sprite && sprite.name.EndsWith("_0")) { display.sprite=sprite; break; }
                    display.preserveAspect=true;
                    display.raycastTarget=false;
                    button.targetGraphic=display;
                    button.GetComponent<Image>().color=new Color(1,1,1,0);
                }
                var existing=button.transform.Find("DiceArtwork") ?? frame.Find("DiceArtwork");
                if (existing != null)
                {
                    var rect=existing as RectTransform;
                    rect.SetParent(frame,false);
                    rect.anchorMin=new Vector2(0.24f,0.24f);
                    rect.anchorMax=new Vector2(0.76f,0.76f);
                    rect.offsetMin=Vector2.zero;
                    rect.offsetMax=Vector2.zero;
                    continue;
                }
                var skin = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                var die = Rect("DiceArtwork",frame,new Vector2(0.24f,0.24f),new Vector2(0.76f,0.76f));
                var body = die.gameObject.AddComponent<Image>();
                body.sprite=skin;
                body.type=Image.Type.Sliced;
                body.color=Color.white;
                body.raycastTarget=false;
                foreach (var point in new[]{new Vector2(0.25f,0.25f),new Vector2(0.75f,0.25f),new Vector2(0.5f,0.5f),new Vector2(0.25f,0.75f),new Vector2(0.75f,0.75f)})
                {
                    var dot=Rect("Pip",die,point-Vector2.one*0.065f,point+Vector2.one*0.065f);
                    var image=dot.gameObject.AddComponent<Image>();
                    image.sprite=skin;
                    image.color=new Color(0.06f,0.18f,0.37f,1);
                    image.raycastTarget=false;
                }
            }
        }

        private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {
            var rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchorMin=min;
            rect.anchorMax=max;
            rect.offsetMin=Vector2.zero;
            rect.offsetMax=Vector2.zero;
            return rect;
        }
    }
}
