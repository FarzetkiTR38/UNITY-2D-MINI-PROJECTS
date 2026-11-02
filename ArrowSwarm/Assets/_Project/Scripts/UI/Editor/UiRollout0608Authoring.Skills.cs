namespace ArrowSwarm.UI.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K=UiRollout01Kit;
    using U=UiRollout02Kit;

    /// <summary>Shared hint/freeze card authoring with localized TMP, two reused actions and existing hero sprites.</summary>
    public static partial class UiRollout0608Authoring
    {
        /// <summary>Applies board08 to only the existing Defense hint/freeze owners in GameScene.</summary>
        public static void Skills()
        {
            RequireEdit();
            Skill(UnityEngine.Object.FindFirstObjectByType<TipPopupUI>(FindObjectsInactive.Include),true);
            Skill(UnityEngine.Object.FindFirstObjectByType<FreezePopupUI>(FindObjectsInactive.Include),false);Save();
        }
        private static void Skill(MonoBehaviour owner,bool hint)
        {
            if(owner.transform.Find("Ui08SafeArea")!=null)throw new System.InvalidOperationException("Skills already authored; edit existing composition.");
            Sprite hero=null;
            foreach(var image in owner.transform.Find("DialogBox").GetComponentsInChildren<Image>(true))
                if(image.name=="SkillIcon")hero=image.sprite;
            if(hero==null)throw new System.InvalidOperationException("Reuse the existing skill hero sprite.");
            var fields=new SerializedObject(owner);
            var watch=(Button)fields.FindProperty("_watchAdButton").objectReferenceValue;
            var close=(Button)fields.FindProperty("_closeButton").objectReferenceValue;
            watch=UnityEngine.Object.Instantiate(watch.gameObject,owner.transform).GetComponent<Button>();watch.name="WatchAdButton";
            close=UnityEngine.Object.Instantiate(close.gameObject,owner.transform).GetComponent<Button>();close.name="CloseButton";
            U.Bind(owner,"_watchAdButton",watch);U.Bind(owner,"_closeButton",close);
            Sprite icon=null;foreach(var image in watch.GetComponentsInChildren<Image>(true))
                if(image.transform!=watch.transform&&image.sprite!=null){icon=image.sprite;break;}
            var card=Card(owner,"Ui08SafeArea",new Vector2(1000,1750));
            Frame(card,new Vector2(800,1300),Vector2.zero);
            string prefix=hint?"tip_popup_":"freeze_popup_";
            ReuseTitle(owner,card,true);
            K.Image(card,"SkillHero",hero,new Vector2(0,-10),new Vector2(520,520)).preserveAspect=true;
            var message=U.Text(card,"Message",hint?"Watch an ad to get 1 hint":"Watch an ad to get 1 freeze",new Vector2(0,-300),new Vector2(710,105),46,prefix+"subtitle");
            ReskinButton(watch,card,new Vector2(0,-420),new Vector2(700,155),"tip_popup_watch_ad","WATCH AD",icon,true);
            ReskinButton(close,card,new Vector2(0,-570),new Vector2(640,125),"tip_popup_close","CLOSE",null,false);
            var face=close.GetComponent<Image>();
            var pause=UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
            var resume=(Button)new SerializedObject(pause).FindProperty("_continueButton").objectReferenceValue;
            U.Variants(face,K.Sprite("light_plate"),resume.GetComponent<Image>().sprite);face.type=Image.Type.Sliced;
            var label=close.transform.Find("ActionText").GetComponent<TMPro.TextMeshProUGUI>();
            K.Theme(label,new Color(.025f,.16f,.57f),Color.white);
            U.Bind(owner,"_dialogBox",card);U.Bind(owner,"_messageText",message);
        }
    }
}
