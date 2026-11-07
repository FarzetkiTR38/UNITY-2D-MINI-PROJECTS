namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.Tips;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Proves real zero-inventory HUD clicks reuse a paid hint without invoking exhausted-skill or ad paths.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator VerifyNativeLastCharge(NativeMouse pointer, GameModeId mode)
        {
            var data = DataManager.Instance;
            data.ModifyTipCount(1 - data.PlayerData.tipCount);
            Time.timeScale = 0;
            Button button;
            if (mode == GameModeId.Defense)
            {
                var hud = UnityEngine.Object.FindFirstObjectByType<GameHUD>();
                button = Field<Button>(hud, "_skill1TipButton");
            }
            else button = Field<Button>(UnityEngine.Object.FindFirstObjectByType<PuzzleHUD>(), "_hint");
            int ads = _scope.Ads.Calls;
            yield return Click(pointer, button, true);
            bool pending = mode == GameModeId.Defense ? TipManager.Instance.HasPendingSuggestion : PuzzleSession.Active.HasPendingHint;
            Check(pending && data.PlayerData.tipCount == 0, "Native HUD spends the last charge once and owns a valid hint / " + mode);
            for (int repeat = 0; repeat < 3; repeat++) yield return Click(pointer, button, true);
            pending = mode == GameModeId.Defense ? TipManager.Instance.HasPendingSuggestion : PuzzleSession.Active.HasPendingHint;
            Check(pending && data.PlayerData.tipCount == 0 && _scope.Ads.Calls == ads,
                "Three real zero-inventory HUD clicks reuse the paid hint without requesting any ad / " + mode);
            if (mode == GameModeId.Defense)
            {
                var popup = UnityEngine.Object.FindFirstObjectByType<TipPopupUI>(FindObjectsInactive.Include);
                Check(popup != null && (!popup.gameObject.activeInHierarchy || Field<CanvasGroup>(popup, "_canvasGroup").alpha < .01f),
                    "Defense native last-charge repeats do not open the exhausted Tip popup");
            }
            else
            {
                var popup = UnityEngine.Object.FindFirstObjectByType<PuzzlePopups>();
                Check(!PuzzleSession.Active.HintPromptOpen && Field<CanvasGroup[]>(popup, "_panels")[3].alpha < .01f,
                    "Puzzle native last-charge repeats do not open the exhausted Hint popup / " + mode);
            }
            File.WriteAllText(Path.Combine(_folder, mode.ToString().ToLowerInvariant() + "-native-hud.json"),
                "{\n  \"status\": \"PASS\",\n  \"mode\": \"" + mode +
                "\",\n  \"nativeClicks\": 4,\n  \"chargesSpent\": 1,\n  \"adsRequested\": 0,\n  \"exhaustedPopupOpened\": false\n}\n");
        }
    }
}
