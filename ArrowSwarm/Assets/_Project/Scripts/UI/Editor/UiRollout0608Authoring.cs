namespace ArrowSwarm.UI.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;
    using K=UiRollout01Kit;
    using U=UiRollout02Kit;

    /// <summary>Authors only Defense modal appearance while retaining original components and action bindings.</summary>
    public static partial class UiRollout0608Authoring
    {
        /// <summary>Applies the approved board06 pause composition to GameScene only.</summary>
        public static void Pause()
        {
            RequireEdit();UiRollout0608Art.ImportClouds();
            var owner=UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
            if(owner.transform.Find("Ui06SafeArea")!=null)throw new InvalidOperationException("Pause already authored; edit existing composition.");
            var fields=new SerializedObject(owner);
            var sound=(SettingsToggleUI)fields.FindProperty("_soundToggle").objectReferenceValue;
            var vibration=(SettingsToggleUI)fields.FindProperty("_vibrationToggle").objectReferenceValue;
            var card=Card(owner,"Ui06SafeArea",new Vector2(1000,2050));
            var frame=Frame(card,new Vector2(1000,1500),new Vector2(0,-60));
            U.PopupCrest(card,new Vector2(0,790),new Vector2(730,330));
            var title=U.Title(card,"TitleText","PAUSED",new Vector2(0,510),new Vector2(940,240),126,"pause_title");
            K.Image(card,"HeaderStar",K.Sprite("star"),new Vector2(0,365),new Vector2(72,72)).preserveAspect=true;
            U.Bind(owner,"_soundToggle",ToggleRow(card,sound,265,"settings_sfx","SOUND","SoundIcon"));
            U.Bind(owner,"_vibrationToggle",ToggleRow(card,vibration,100,"settings_vibration","VIBRATION","VibrationIcon"));
            string[] bindings={"_continueButton","_retryButton","_levelsButton","_mainMenuButton"};
            string[] keys={"pause_resume","pause_retry","pause_levels","pause_main_menu"};
            string[] fallbacks={"RESUME","RETRY","LEVELS","MAIN MENU"};
            for(int i=0;i<4;i++)
            {
                var original=(Button)fields.FindProperty(bindings[i]).objectReferenceValue;
                var button=UnityEngine.Object.Instantiate(original.gameObject,card).GetComponent<Button>();
                button.name=original.name;U.Bind(owner,bindings[i],button);
                Sprite icon=null;foreach(var image in button.GetComponentsInChildren<Image>(true))
                    if(image.transform!=button.transform && image.sprite!=null){icon=image.sprite;break;}
                ReskinButton(button,card,new Vector2(0,-105-i*180),new Vector2(850,155),keys[i],fallbacks[i],icon,false);
            }
            foreach(int side in new[]{-1,1}) K.Image(card,side<0?"LeftCloud":"RightCloud",UiRollout0608Art.Sprite(side<0?"cloud_left":"cloud_right"),
                new Vector2(side*355,-790),new Vector2(380,380)).preserveAspect=true;
            U.Bind(owner,"_dialogBox",card);U.Bind(owner,"_titleText",title);U.Bind(owner,"_boardFrameImage",frame);
            Save();
        }

        private static SettingsToggleUI ToggleRow(Transform card,SettingsToggleUI toggle,float y,string key,string fallback,string iconName)
        {
            var oldRow=toggle.transform.parent;var icon=oldRow.Find(iconName).GetComponent<Image>().sprite;
            var row=K.Image(card,iconName+"Row",K.Sprite("light_plate"),new Vector2(0,y),new Vector2(850,145));
            K.RoundedPlate(row,true);
            K.Image(row.transform,iconName,icon,new Vector2(-330,0),new Vector2(120,120)).preserveAspect=true;
            U.Text(row.transform,"Label",fallback,new Vector2(-45,0),new Vector2(380,100),46,key);
            var original=toggle; toggle=UnityEngine.Object.Instantiate(original.gameObject,row.transform).GetComponent<SettingsToggleUI>();toggle.name=original.name;
            Normalize((RectTransform)toggle.transform,new Vector2(310,0),new Vector2(205,92));
            if(toggle.GetComponent<UIButtonFeedback>()==null)toggle.gameObject.AddComponent<UIButtonFeedback>();
            var state=new SerializedObject(toggle);
            var pause=UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
            var source=(Button)new SerializedObject(pause).FindProperty("_continueButton").objectReferenceValue;
            state.FindProperty("_onSprite").objectReferenceValue=source.GetComponent<Image>().sprite;
            state.FindProperty("_offSprite").objectReferenceValue=source.GetComponent<Image>().sprite;
            state.FindProperty("_useColorTint").boolValue=true;
            state.FindProperty("_onColor").colorValue=Color.white;
            state.FindProperty("_offColor").colorValue=new Color(.38f,.5f,.75f);
            var knob=K.Image(toggle.transform,"StateKnob",AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),new Vector2(55,0),new Vector2(60,60));
            var status=K.Text(toggle.transform,"StateText","ON",new Vector2(-35,0),new Vector2(105,65),35,Color.white);
            status.enableAutoSizing=true;status.fontSizeMin=18;status.fontSizeMax=35;
            state.FindProperty("_knobRect").objectReferenceValue=knob.rectTransform;
            state.FindProperty("_statusText").objectReferenceValue=status;
            state.FindProperty("_onKnobX").floatValue=60;state.FindProperty("_offKnobX").floatValue=-60;
            state.FindProperty("_onTextX").floatValue=-35;state.FindProperty("_offTextX").floatValue=35;
            state.FindProperty("_onTextKey").stringValue="settings_on";
            state.FindProperty("_offTextKey").stringValue="settings_off";
            state.ApplyModifiedPropertiesWithoutUndo();
            return toggle;
        }

        private static RectTransform Card(MonoBehaviour owner,string name,Vector2 extent)
        {
            var previous=owner.transform.Find("DialogBox");if(previous!=null)previous.gameObject.SetActive(false);
            var safe=U.SafeWrapper(owner.transform,name,extent);
            return K.Rect(safe,"Card",Vector2.zero,extent);
        }
        private static Image Frame(Transform card,Vector2 size,Vector2 position)
        {
            var frame=K.Image(card,"BoardFrame",K.Sprite("light_plate"),position,size);
            frame.type=Image.Type.Sliced;frame.pixelsPerUnitMultiplier=.75f;
            K.Theme(frame,Color.white,Color.white,K.Material("Ui04BoardDark"));return frame;
        }
        private static void ReskinButton(Button button,Transform parent,Vector2 position,Vector2 size,string key,string fallback,Sprite icon,bool gold)
        {
            var face=button.GetComponent<Image>();var sprite=face.sprite;
            foreach(Transform child in button.transform)child.gameObject.SetActive(false);
            button.transform.SetParent(parent,false);Normalize((RectTransform)button.transform,position,size);
            foreach(var theme in button.GetComponents<ThemedUIElement>())UnityEngine.Object.DestroyImmediate(theme);
            face.material=null;face.color=Color.white;face.sprite=gold?K.Sprite("primary_plate"):sprite;
            face.type=face.sprite.border.sqrMagnitude>0?Image.Type.Sliced:Image.Type.Simple;face.preserveAspect=false;
            if(icon!=null)K.Image(button.transform,"ActionIcon",icon,new Vector2(-245,0),new Vector2(106,106)).preserveAspect=true;
            var label=K.Text(button.transform,"ActionText",fallback,new Vector2(icon!=null?50:0,0),new Vector2(icon!=null?540:680,105),52,Color.white,key);
            label.enableAutoSizing=true;label.fontSizeMin=27;label.fontSizeMax=52;label.textWrappingMode=TextWrappingModes.NoWrap;
            var feedback=button.GetComponent<UIButtonFeedback>()??button.gameObject.AddComponent<UIButtonFeedback>();
            var config=new SerializedObject(feedback);config.FindProperty("_targetTransform").objectReferenceValue=button.transform;
            config.FindProperty("_enableIdleBreath").boolValue=false;config.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Normalize(RectTransform rect,Vector2 position,Vector2 size)
        {
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.localScale=Vector3.one;
            rect.anchoredPosition=position;rect.sizeDelta=size;
        }
        private static void RequireEdit()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="GameScene")
                throw new InvalidOperationException("Use idle Edit GameScene.");
        }
        private static void Save()=>EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
