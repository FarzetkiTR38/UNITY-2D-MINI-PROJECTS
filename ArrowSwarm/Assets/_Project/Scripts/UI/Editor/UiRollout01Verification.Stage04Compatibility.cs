namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Proves that optional visual bindings keep every unmodified legacy view operational.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator Verify04LegacyCompatibility(LevelSelectUI levels, NativeMouse mouse)
        {
            int page = Field<int>(levels, "_currentPage");
            var previous = Field<Button>(levels,"_prevPageButton");
            var next = Field<Button>(levels,"_nextPageButton");
            var pool = Field<List<LevelButtonUI>>(levels,"_buttonPool");
            Check(Field<GameObject>(pool[0],"_unlockedState")==null,"Unmodified legacy tile keeps its original style path");
            if(previous.interactable)
            {
                yield return Click(mouse,previous,true);
                Check(Field<int>(levels,"_currentPage")==page-1,"Legacy Previous remains wired exactly once");
                yield return Click(mouse,next,true);
                Check(Field<int>(levels,"_currentPage")==page,"Legacy Next remains wired exactly once");
            }
            else yield return Press04Disabled(mouse,previous,"Legacy first-page Previous");
            yield return Press04Disabled(mouse,next,"Legacy highest-page Next");
            var locked = Field<Button>(pool[page==6?2:1],"_button");
            yield return Press04Disabled(mouse,locked,"Legacy locked level");
        }
    }
}
