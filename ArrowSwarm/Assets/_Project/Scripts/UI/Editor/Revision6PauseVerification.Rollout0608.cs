namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Scoped native before/after checks limited to Defense modal UI and its original controls.</summary>
    public static partial class Revision6PauseVerification
    {
        private static bool _rollout0608, _revised0608;

        /// <summary>Captures three modal states in both themes; all changed data and providers are restored.</summary>
        public static void Begin0608(string folder, bool revised)
        {
            if (_running) throw new InvalidOperationException("Finish the current verification first.");
            Folder = folder; _rollout0608 = true; _revised0608 = revised; Begin();
        }

        private static IEnumerator Run0608()
        {
            SceneManager.LoadScene("GameScene");
            yield return (Func<bool>)DefenseReady;
            if (ArrowSwarm.Mob.MobSpawner.HasInstance) ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
            var hud = UnityEngine.Object.FindFirstObjectByType<GameHUD>();
            var pause = UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
            var tip = UnityEngine.Object.FindFirstObjectByType<TipPopupUI>(FindObjectsInactive.Include);
            var freeze = UnityEngine.Object.FindFirstObjectByType<FreezePopupUI>(FindObjectsInactive.Include);
            var ads = AdManager.Instance; var originalAds = Field<IAdService>(ads,"_adService");
            var fake = new ModalAds(); ads.SetAdService(fake);
            var placements = LevelManager.Instance.CurrentLevelData.ArrowPlacements;
            try
            {
                using (var mouse = new NativeMouse())
                foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
                {
                    DataManager.Instance.SetTheme(theme); LocalizationManager.Instance.SetLanguage("tr",false);
                    yield return Settled();
                    yield return Click(mouse,Field<Button>(hud,"_pauseButton"),true);
                    yield return (Func<bool>)(()=>GameManager.Instance.CurrentState==GameState.Paused && Time.timeScale==0);
                    yield return Settled(.7f);
                    Check(Field<CanvasGroup>(pause,"_canvasGroup").blocksRaycasts,"Pause blocks gameplay input");
                    if (_revised0608) Validate0608(pause.transform);
                    yield return Capture("pause-"+theme.ToString().ToLowerInvariant());
                    if (_revised0608)
                    {
                        foreach (var language in LocalizationManager.Instance.AvailableLanguages)
                        {
                            LocalizationManager.Instance.SetLanguage(language.code,false);yield return Settled(.04f);Validate0608(pause.transform);
                        }
                        LocalizationManager.Instance.SetLanguage("tr",false);
                    }
                    var sound=Field<SettingsToggleUI>(pause,"_soundToggle"); bool enabled=DataManager.Instance.PlayerData.sfxEnabled;
                    yield return Click(mouse,Field<Button>(sound,"_button"),false);
                    Check(DataManager.Instance.PlayerData.sfxEnabled!=enabled,"Existing sound toggle updates scoped setting");
                    yield return Click(mouse,Field<Button>(sound,"_button"),false);
                    var vibration=Field<SettingsToggleUI>(pause,"_vibrationToggle"); enabled=DataManager.Instance.PlayerData.vibrationEnabled;
                    yield return Click(mouse,Field<Button>(vibration,"_button"),false);
                    Check(DataManager.Instance.PlayerData.vibrationEnabled!=enabled,"Existing vibration toggle updates scoped setting");
                    yield return Click(mouse,Field<Button>(vibration,"_button"),false);
                    yield return Click(mouse,Field<Button>(pause,"_continueButton"),_revised0608);
                    yield return (Func<bool>)(()=>GameManager.Instance.CurrentState==GameState.Playing && Time.timeScale>0);
                    Check(ReferenceEquals(placements,LevelManager.Instance.CurrentLevelData.ArrowPlacements),"Resume retains the exact generated board");
                    Time.timeScale=0;
                    foreach (MonoBehaviour modal in new MonoBehaviour[]{tip,freeze})
                    {
                        bool isTip=modal==tip;
                        if(isTip)tip.Show();else freeze.Show();yield return Settled(.7f);
                        if (_revised0608 && modal.transform.Find("Ui08SafeArea")!=null) Validate0608(modal.transform);
                        yield return Capture((isTip?"hint-":"freeze-")+theme.ToString().ToLowerInvariant());
                        if (_revised0608 && modal.transform.Find("Ui08SafeArea")!=null)
                        {
                            foreach(var language in LocalizationManager.Instance.AvailableLanguages)
                            {LocalizationManager.Instance.SetLanguage(language.code,false);yield return Settled(.03f);Validate0608(modal.transform);}
                            LocalizationManager.Instance.SetLanguage("tr",false);
                        }
                        int count=isTip?DataManager.Instance.PlayerData.tipCount:DataManager.Instance.PlayerData.freezeCount;
                        fake.Success=false;yield return Click(mouse,Field<Button>(modal,"_watchAdButton"),_revised0608);
                        Check(count==(isTip?DataManager.Instance.PlayerData.tipCount:DataManager.Instance.PlayerData.freezeCount),"Failed scoped ad grants no skill");
                        fake.Success=true;yield return Click(mouse,Field<Button>(modal,"_watchAdButton"),false);
                        yield return Settled(.6f);
                        Check(count+1==(isTip?DataManager.Instance.PlayerData.tipCount:DataManager.Instance.PlayerData.freezeCount),"Existing successful rewarded callback grants exactly one skill");
                        if(isTip)tip.Show();else freeze.Show();yield return Settled(.6f);
                        yield return Click(mouse,Field<Button>(modal,"_closeButton"),false);yield return Settled(.6f);
                        Check(!modal.gameObject.activeSelf,"Native Close dismisses skill popup");
                    }
                    Time.timeScale=1;
                }
                Check(fake.Requests==8,"Only eight scoped rewarded-ad requests, no live ad SDK calls");
            }
            finally { if(ads!=null)ads.SetAdService(originalAds); }
            if(_revised0608)yield return Routes0608();
            ReturnToMenu();yield return Settled(.2f);
        }

        private static void Validate0608(Transform root)
        {
            Canvas.ForceUpdateCanvases();
            foreach(var text in root.GetComponentsInChildren<TMP_Text>(false))
            {
                if(!text.isActiveAndEnabled||string.IsNullOrWhiteSpace(text.text))continue;
                text.ForceMeshUpdate();
                Check(text.font!=null&&!text.isTextTruncated,"Localized TMP fits: "+text.name+" / "+text.text);
            }
            foreach(var button in root.GetComponentsInChildren<Button>(false))
            {
                var rect=(RectTransform)button.transform;var canvas=button.GetComponentInParent<Canvas>();
                var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
                var corners=new Vector3[4];rect.GetWorldCorners(corners);
                foreach(var point in corners)
                    Check(Screen.safeArea.Contains(RectTransformUtility.WorldToScreenPoint(camera,point)),"Interactive control stays in native safe area: "+button.name);
                Check(button.GetComponent<UIButtonFeedback>()!=null,"Existing press feedback retained: "+button.name);
            }
        }

        private sealed class ModalAds:IAdService
        {
            internal bool Success; internal int Requests;
            public bool IsAdReady()=>true;
            public bool IsInterstitialAdReady()=>false;
            public void ShowRewardedAd(Action<bool> callback){Requests++;callback(Success);}
            public void ShowInterstitialAd(Action callback){throw new InvalidOperationException("Unexpected interstitial");}
        }
    }
}
