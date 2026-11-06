namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Uses original manager exhaustion events and native popup actions without changing the HUD reward route.</summary>
    public static partial class UiRollout08Verification
    {
        private static IEnumerator Run()
        {
            SceneManager.LoadScene("GameScene");
            yield return (Func<bool>)DefenseReady;
            Check(ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows.Count == LevelManager.Instance.TotalArrows,
                "Every generated Defense arrow has an active, enabled and completed body/head before the fixture freezes time");
            if (ArrowSwarm.Mob.MobSpawner.HasInstance) ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
            Time.timeScale = 0;
            var placements = LevelManager.Instance.CurrentLevelData.ArrowPlacements;
            var hud = UnityEngine.Object.FindFirstObjectByType<GameHUD>();
            var tip = UnityEngine.Object.FindFirstObjectByType<TipPopupUI>(FindObjectsInactive.Include);
            var freeze = UnityEngine.Object.FindFirstObjectByType<FreezePopupUI>(FindObjectsInactive.Include);
            Check(tip != null && freeze != null, "Both original exhausted-skill popup owners exist");
            using (var mouse = new NativeMouse())
            foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                DataManager.Instance.SetTheme(theme); LocalizationManager.Instance.SetLanguage("tr", false);
                yield return Settled();
                foreach (MonoBehaviour modal in new MonoBehaviour[] { tip, freeze })
                {
                    bool isTip = modal == tip;
                    Status = "Defense / " + theme + " / " + (isTip ? "hint" : "freeze");
                    EmptySkills(hud); _scope.Ads.Success = false;
                    int requests = _scope.Ads.Requests;
                    var skillButton = Field<Button>(hud, isTip ? "_skill1TipButton" : "_skill2FreezeButton");
                    yield return Click(mouse, skillButton, false);
                    Check(_scope.Ads.Requests == requests + 1 && Count(isTip) == 0 && !modal.gameObject.activeSelf,
                        "Existing zero-count HUD route remains direct rewarded ad; scoped failure grants nothing");
                    Exhaust(isTip); yield return Settled(.7f);
                    Check(modal.gameObject.activeInHierarchy && Field<CanvasGroup>(modal, "_canvasGroup").blocksRaycasts,
                        "Original manager exhaustion event opens modal and blocks gameplay input");
                    Check(GameManager.Instance.CurrentState == GameState.Playing && Time.timeScale == 0,
                        "Only fixture world clock is frozen; production game state remains Playing");
                    if (_revised) Validate(modal.transform);
                    yield return Capture((isTip ? "hint-" : "freeze-") + theme.ToString().ToLowerInvariant());
                    if (_revised)
                    {
                        foreach (var language in LocalizationManager.Instance.AvailableLanguages)
                        {
                            LocalizationManager.Instance.SetLanguage(language.code, false); yield return Settled(.04f);
                            Validate(modal.transform);
                        }
                        LocalizationManager.Instance.SetLanguage("tr", false); yield return Settled(.1f);
                    }
                    _scope.Ads.Success = false;
                    yield return Click(mouse, Field<Button>(modal, "_watchAdButton"), _revised);
                    Check(Count(isTip) == 0 && modal.gameObject.activeInHierarchy,
                        "Native Watch Ad failure preserves zero charges and leaves popup open");
                    _scope.Ads.Success = true;
                    yield return Click(mouse, Field<Button>(modal, "_watchAdButton"), false);
                    yield return Settled(.6f);
                    Check(Count(isTip) == 1 && !modal.gameObject.activeSelf,
                        "Native Watch Ad success grants exactly one charge and dismisses original popup");
                    EmptySkills(hud); Exhaust(isTip); yield return Settled(.6f);
                    yield return Click(mouse, Field<Button>(modal, "_closeButton"), _revised);
                    yield return Settled(.6f);
                    Check(!modal.gameObject.activeSelf && !Field<CanvasGroup>(modal, "_canvasGroup").blocksRaycasts,
                        "Native Close dismisses popup and releases its input blocking");
                    Check(ReferenceEquals(placements, LevelManager.Instance.CurrentLevelData.ArrowPlacements),
                        "Generated Defense board identity remains unchanged throughout popup actions");
                }
            }
            Check(_scope.Ads.Requests == 12 && _scope.FactoryCalls == 0 && !LeaderboardManager.HasInstance,
                "Exactly twelve isolated fake-ad requests; no external cloud manager or provider calls");
            ReturnToMenu(); yield return Settled(.2f);
        }

        private static void EmptySkills(GameHUD hud)
        {
            var data = DataManager.Instance;
            data.ModifyTipCount(-data.PlayerData.tipCount); data.ModifyFreezeCount(-data.PlayerData.freezeCount);
            hud.UpdateSkillBadges();
        }
        private static int Count(bool hint) => hint ? DataManager.Instance.PlayerData.tipCount : DataManager.Instance.PlayerData.freezeCount;
        private static void Exhaust(bool hint)
        { if (hint) ArrowSwarm.Tips.TipManager.Instance.UseTip(); else ArrowSwarm.Skills.FreezeManager.Instance.UseFreeze(); }

        private static bool DefenseReady()
        {
            if (!GameManager.HasInstance || GameManager.Instance.CurrentState != GameState.Playing ||
                !ArrowSwarm.Arrow.ArrowSpawner.HasInstance || ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows.Count == 0) return false;
            foreach (var arrow in ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows)
            {
                if (arrow == null || !arrow.gameObject.activeInHierarchy || arrow.IsFired) return false;
                var visuals = arrow.GetComponent<ArrowSwarm.Arrow.ArrowVisuals>();
                var body = arrow.GetComponent<LineRenderer>();
                var head = visuals != null ? Field<SpriteRenderer>(visuals, "_headRenderer") : null;
                if (visuals == null || visuals.IsSpawning || body == null || !body.enabled || body.positionCount < 2 ||
                    body.startWidth <= .001f || body.startColor.a <= .01f || head == null || !head.enabled ||
                    head.color.a <= .01f || head.transform.lossyScale.sqrMagnitude <= .001f) return false;
            }
            return UnityEngine.Object.FindFirstObjectByType<OverlayManager>(FindObjectsInactive.Include) != null;
        }

        private sealed class ModalAds : IAdService
        {
            internal bool Success;
            internal int Requests;
            public bool IsAdReady() => true;
            public bool IsInterstitialAdReady() => false;
            public void ShowRewardedAd(Action<bool> callback) { Requests++; callback(Success); }
            public void ShowInterstitialAd(Action callback) => throw new InvalidOperationException("Unexpected interstitial in modal fixture");
        }
    }
}
