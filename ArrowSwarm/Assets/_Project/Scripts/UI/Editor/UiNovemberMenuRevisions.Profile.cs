namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Refines only the existing username avatar, dice and country controls; keeps input and action bindings.</summary>
    public static partial class UiNovemberMenuRevisions
    {
        /// <summary>Centers the circular person badge in the input end cap and masks the atlas's square backing.</summary>
        public static string Avatar()
        {
            var owner = Owner<ProfileSetupModalUI>();
            var input = Bound<TMP_InputField>(owner,"_nameInput");
            var legacy = input.transform.Find("Image");
            if (legacy != null && legacy.GetComponent<Image>() != input.targetGraphic)
                UnityEngine.Object.DestroyImmediate(legacy.gameObject);
            var icon = Child(input.transform,"PersonIcon").GetComponent<Image>();
            var bounds = new Vector4(.127f,.055f,.97f,.884f);
            icon.material = ArtworkMask("ProfileAvatar",bounds); icon.type = Image.Type.Simple;
            icon.preserveAspect = true; icon.raycastTarget = false; Coordinates(icon);
            var size = new Vector2(112,112);
            var center = new Vector2((bounds.x+bounds.z)*.5f,(bounds.y+bounds.w)*.5f);
            Center(icon.rectTransform,new Vector2(-275,0)-(center-Vector2.one*.5f)*size,size);
            var viewport = input.textViewport;
            if (viewport == null) throw new System.InvalidOperationException("Keep the real username text viewport bound.");
            viewport.offsetMin = new Vector2(145,viewport.offsetMin.y);
            EditorUtility.SetDirty(icon); EditorUtility.SetDirty(viewport);
            return Finish("profile avatar");
        }

        /// <summary>Reuses current dice/arrow art within clean round blue controls, leaving callbacks and navigation intact.</summary>
        public static string ProfileControls()
        {
            var owner = Owner<ProfileSetupModalUI>();
            var dice = Bound<Button>(owner,"_diceButton");
            var previous = Bound<Button>(owner,"_prevCountryButton");
            var next = Bound<Button>(owner,"_nextCountryButton");
            GlossyControl(dice.image);
            Center(dice.GetComponent<RectTransform>(),new Vector2(370,145),new Vector2(132,132));
            var art = Child(dice.transform,"DiceArtwork").GetComponent<Image>();
            art.material = ArtworkMask("ProfileDice",new Vector4(.02f,.02f,.98f,.98f));
            art.type = Image.Type.Simple; art.preserveAspect = true; art.raycastTarget = false; Coordinates(art);
            Center(art.rectTransform,new Vector2(0,4),new Vector2(116,116));
            Arrow(previous,true); Arrow(next,false);
            return Finish("profile dice and country arrows");
        }

        private static void Arrow(Button button, bool previous)
        {
            GlossyControl(button.image);
            var root = button.GetComponent<RectTransform>();
            Center(root,root.anchoredPosition,new Vector2(112,112));
            var arrow = Child(button.transform,"Arrow").GetComponent<Image>();
            arrow.sprite = K.Sprite("icon_play"); arrow.type = Image.Type.Simple; arrow.preserveAspect = true;
            arrow.raycastTarget = false;
            Center(arrow.rectTransform,Vector2.zero,new Vector2(52,64));
            arrow.rectTransform.localEulerAngles = new Vector3(0,0,previous ? 180 : 0);
            EditorUtility.SetDirty(button); EditorUtility.SetDirty(arrow);
        }

        private static void GlossyControl(Image image)
        {
            image.sprite = K.Sprite("profile_tile"); image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.material = ArtworkMask("ProfileDice",new Vector4(.02f,.02f,.98f,.98f));
            K.Theme(image,Color.white,Color.white,image.material);
            Coordinates(image); EditorUtility.SetDirty(image);
        }
    }
}
