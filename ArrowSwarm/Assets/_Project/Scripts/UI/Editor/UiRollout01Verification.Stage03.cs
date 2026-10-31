namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Captures and exercises board03 inside the existing reversible native/offline scope.</summary>
    public static partial class UiRollout01Verification
    {
        private static bool _nextStage03, _stage03, _stage03Revised, _stage03Interactions, _stage03ProfileOnly;

        /// <summary>Runs before/after board03 checks without changing real player progress or calling live providers.</summary>
        public static void Stage03At(string folder, bool revised, bool interactions)
        {
            _nextStage03 = true; _stage03Revised = revised; _stage03Interactions = interactions; _stage03ProfileOnly = false;
            try { Start(folder, false); } finally { _nextStage03 = false; }
        }

        /// <summary>Checks only the profile while the later leaderboard phase still retains its old appearance.</summary>
        public static void Stage03ProfileAt(string folder)
        {
            _nextStage03=true;_stage03Revised=true;_stage03Interactions=true;_stage03ProfileOnly=true;
            try{Start(folder,false);}finally{_nextStage03=false;}
        }

        private static IEnumerator VerifyStage03()
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
            var board = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
            foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                Install03Player(); DataManager.Instance.SetTheme(theme); LocalizationManager.Instance.SetLanguage("tr", false);
                Status = "Board03 Profile / " + theme;
                using (var mouse = new NativeMouse())
                {
                    menu.OpenProfileModal(); yield return Settled(.65f);
                    Check(profile.gameObject.activeInHierarchy && Field<CanvasGroup>(profile,"_canvasGroup").alpha>=.99f,"Original Profile controller opens fully / "+theme);
                    Field<TMP_InputField>(profile,"_nameInput").SetTextWithoutNotify("NovaRider");
                    if (_stage03Revised) VerifyModalCopy(profile.transform,Status);
                    yield return Capture("profile-"+theme.ToString().ToLowerInvariant());
                    if (_stage03Revised)
                    {
                        foreach(var language in LocalizationManager.Instance.AvailableLanguages)
                        { LocalizationManager.Instance.SetLanguage(language.code,false);yield return Settled(.04f);VerifyModalCopy(profile.transform,"Profile / "+language.code+" / "+theme); }
                        LocalizationManager.Instance.SetLanguage("tr",false);
                    }
                    if (_stage03Interactions) yield return Verify03ProfileInteractions(profile,mouse);
                    profile.Hide(); yield return Settled(.55f); menu.OpenMainPanel(); yield return Settled();
                    if(_stage03ProfileOnly)continue;
                    Install03Player(); DataManager.Instance.SetTheme(theme);
                    yield return Click(mouse,Field<Button>(menu,"_leaderboardButton"),true); yield return Settled(.6f);
                    var manager=LeaderboardManager.Instance; Install03Rows(manager,true); board.RefreshLeaderboardData();
                    Field<TextMeshProUGUI>(board,"_statusText").text="ÖRNEK / SAMPLE";
                    yield return Settled(); Status="Board03 Leaderboard / "+theme;
                    Check(Field<LeaderboardEntryUI[]>(board,"_entryRows").Length==10,"All ten original ranking slots are bound");
                    if(_stage03Revised)VerifyModalCopy(board.transform,Status);
                    yield return Capture("leaderboard-"+theme.ToString().ToLowerInvariant());
                    if(_stage03Revised)
                    {
                        foreach(var language in LocalizationManager.Instance.AvailableLanguages)
                        { LocalizationManager.Instance.SetLanguage(language.code,false);yield return Settled(.04f);VerifyModalCopy(board.transform,"Leaderboard / "+language.code+" / "+theme); }
                        LocalizationManager.Instance.SetLanguage("tr",false);
                    }
                    if(_stage03Interactions)yield return Verify03LeaderboardInteractions(board,mouse);
                    yield return Click(mouse,Field<Button>(board,"_closeButton"),true);yield return Settled(.55f);
                    menu.OpenMainPanel();yield return Settled();
                }
            }
        }

        private static void Install03Player()
        {
            var player=PlayerData.CreateDefault();player.playerName="NovaRider";player.playerCountry="TR";
            player.highestLevel=player.currentLevel=198;player.isProfileSetupCompleted=player.isTutorialCompleted=true;
            for(int i=1;i<=192;i++)player.levelStars.Add(new LevelStarData{level=i,stars=3});
            Set03(DataManager.Instance,"_playerData",player);
        }

        private static void Set03(object owner,string name,object value) => owner.GetType().GetField(name,Private).SetValue(owner,value);

        private static void Install03Rows(LeaderboardManager manager,bool onBoard)
        {
            string[] names={"SkyCaptain","ArrowFox","PixelMage","LunaQuest","StarPilot","NeonTiger","NovaRider","CloudDancer","BlueComet","SunnyWings"};
            string[] countries={"US","US","GB","TR","DE","FR","TR","JP","CA","BR"};
            int[] levels={356,310,275,248,226,203,198,176,162,140};int[] stars={1982,1743,1520,1312,1118,892,576,448,331,286};
            var rows=new List<LeaderboardEntry>();
            for(int i=0;i<10;i++)rows.Add(new LeaderboardEntry{PlayerId="board03-sample-"+i,PlayerName=names[i],CountryCode=countries[i],HighestLevel=levels[i],TotalStars=stars[i],IsPlayer=onBoard&&i==6});
            Set03(manager,"_cachedEntries",rows);Set03(manager,"_cachedPlayerRank",onBoard?7:27);
            Set03(manager,"_lastSuccessfulRefreshUtc",new DateTime(2026,10,31,12,0,0,DateTimeKind.Utc));
            typeof(LeaderboardManager).GetProperty("RefreshState").SetValue(manager,LeaderboardRefreshState.Ready,null);
        }

    }
}
