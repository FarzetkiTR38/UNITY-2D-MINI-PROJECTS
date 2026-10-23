namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Path;
    using ArrowSwarm.Skills;
    using ArrowSwarm.Tips;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Arrow = ArrowSwarm.Arrow.Arrow;
    using ArrowSpawner = ArrowSwarm.Arrow.ArrowSpawner;

    public static partial class DefenseRevisionVerification
    {
        private static void CheckOriginalLayout(GameHUD hud)
        {
            Check(hud != null && hud.TopPanelRect.gameObject.activeInHierarchy && hud.BottomPanelRect.gameObject.activeInHierarchy,
                "Original GameHUD and both authored bars remain visible at101");
            var hint = Field<Button>(hud, "_skill1TipButton"); var freeze = Field<Button>(hud, "_skill2FreezeButton");
            Check(Usable(hint) && Usable(freeze) && hint.name == "Skill1" && freeze.name == "Skill2",
                "Original enabled Hint and Freeze skill buttons remain present");
            Check(!hud.GetComponentsInChildren<Transform>(true).Any(item => item.name.Contains("Rocket")) &&
                UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.PuzzleHUD>() == null, "Defense retains Freeze and has no puzzle HUD or rescue rocket");
            Check(hud.GetComponentsInChildren<Slider>(true).Length == 1 && Field<Slider>(hud, "_zoomSlider") != null,
                "Defense retains one original zoom slider");
            foreach (string bar in new[] { "TopBar", "BottomBar" })
            {
                var image = hud.transform.Find(bar + "/Bg").GetComponent<Image>();
                Check(image.sprite != null && SavedBarSprite(bar, image.sprite), bar + " uses the saved original GameScene sprite");
            }
            var tutorial = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.Tutorial.TutorialManager>(FindObjectsInactive.Include);
            Check(tutorial == null || !tutorial.IsTutorialActive, "Level101 keeps the normal HUD and disables tutorial guidance");
            Check(GameManager.Instance.CurrentLives == 3 && LevelManager.Instance.ArrowsFired == 0,
                "Defense starts with three lives and zero fired arrows");
        }

        private static bool SavedBarSprite(string bar, Sprite sprite)
        {
            const string scene = "Assets/_Project/Scenes/GameScene.unity";
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long fileId);
            if (File.ReadAllText(scene).Contains("m_Sprite: {fileID: " + fileId + ", guid: " + guid)) return true;
            return AssetDatabase.GetDependencies(scene, true).Where(path => path.EndsWith(".prefab"))
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(prefab => prefab != null)
                .Select(prefab => prefab.transform.Find(bar + "/Bg")).Where(item => item != null)
                .Any(item => item.GetComponent<Image>() != null && item.GetComponent<Image>().sprite == sprite);
        }

        private static void CheckOriginalBoard()
        {
            Check(LevelManager.Instance.CurrentLevelData.IsValid && LevelManager.Instance.CurrentLevelData.Level == 101 &&
                ArrowSpawner.Instance.ActiveArrows.Count == LevelManager.Instance.TotalArrows &&
                LevelManager.Instance.TotalArrows > 1, "Original LevelManager generated a valid populated101 map");
            Check(GridManager.HasInstance && PathManager.HasInstance && PathManager.Instance.Waypoints.Count > 1 &&
                PathManager.Instance.TotalPathLength > 0, "Original grid and perimeter path are initialized");
            Check(UnityEngine.Object.FindObjectsByType<ArrowSwarm.Effects.PortalVisualEffect>(FindObjectsSortMode.None).Length > 0 &&
                MobSpawner.Instance.ActiveMobs.Any(mob => mob != null && mob.gameObject.activeInHierarchy && mob.IsAlive),
                "Original portal effects and living Defense enemies remain present");
        }

        private static IEnumerator CheckSkillsAndPause(GameHUD hud)
        {
            Status = "Defense101 actual hint, freeze, pause and resume";
            Check(_data.PlayerData.tipCount == 2 && _data.PlayerData.freezeCount == 2, "Temporary inventory avoids all rewarded ad paths");
            Field<Button>(hud, "_skill1TipButton").onClick.Invoke();
            var highlighter = TipManager.Instance.GetComponent<TipHighlighter>();
            var highlighted = Private<Arrow>(highlighter, "_highlightedArrow");
            Check(_data.PlayerData.tipCount == 1 && Private<bool>(highlighter, "_isHighlighting") && highlighted != null &&
                !highlighted.IsFired && GridManager.Instance.IsPathClear(highlighted.HeadPoint, highlighted.HeadDirection),
                "Actual Hint button spends one temporary token and highlights a legal original arrow");
            var mob = MobSpawner.Instance.ActiveMobs.First(item => item != null && item.IsAlive && item.gameObject.activeInHierarchy);
            var follower = mob.GetComponent<PathFollower>(); float distance = follower.CurrentDistance;
            int spawned = MobSpawner.Instance.SpawnedCount;
            Field<Button>(hud, "_skill2FreezeButton").onClick.Invoke();
            var freeze = FreezeManager.Instance; var timer = Field<GameObject>(hud, "_skill2ActiveTimerRoot");
            Check(_data.PlayerData.freezeCount == 1 && freeze.IsFrozen && freeze.RemainingTime > 0 &&
                timer != null && timer.activeInHierarchy && Field<TMP_Text>(hud, "_skill2ActiveTimerText").text.Length > 0,
                "Actual Freeze button spends one temporary charge and shows its live timer");
            yield return .2f;
            Check(follower.IsPaused && Mathf.Abs(follower.CurrentDistance - distance) < .001f &&
                MobSpawner.Instance.SpawnedCount == spawned, "Freeze stops real mob movement and spawning");
            Field<Button>(hud, "_pauseButton").onClick.Invoke();
            float paused = freeze.RemainingTime; int pauseFrame = Time.frameCount; float pauseTime = Time.unscaledTime;
            var pause = UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
            var pauseGroup = Field<CanvasGroup>(pause, "_canvasGroup");
            PauseDiagnostic("immediate pause", freeze, paused, pauseGroup);
            Check(GameManager.Instance.CurrentState == GameState.Paused && Time.timeScale == 0f,
                "Actual Pause immediately enters Paused state and zero time scale");
            yield return (Func<bool>)(() => pauseGroup.interactable && pauseGroup.alpha > .999f &&
                Time.frameCount > pauseFrame + 1 && Time.unscaledTime > pauseTime + .15f);
            PauseDiagnostic("settled pause", freeze, paused, pauseGroup);
            Check(GameManager.Instance.CurrentState == GameState.Paused && Time.timeScale == 0f &&
                freeze.IsFrozen && Mathf.Abs(freeze.RemainingTime - paused) < .001f &&
                pauseGroup.interactable, "Actual Pause freezes gameplay and the active Freeze timer");
            var resume = Field<Button>(pause, "_continueButton"); Check(Usable(resume), "Original Continue button is visible and enabled");
            resume.onClick.Invoke(); int resumeFrame = Time.frameCount;
            PauseDiagnostic("immediate resume", freeze, paused, pauseGroup);
            Check(GameManager.Instance.CurrentState == GameState.Playing && Time.timeScale == 1f,
                "Actual Continue immediately enters Playing state and restores time scale");
            yield return (Func<bool>)(() => Time.frameCount > resumeFrame + 1 && pauseGroup.alpha < .001f &&
                !pauseGroup.interactable && GameManager.Instance.CurrentState == GameState.Playing);
            PauseDiagnostic("settled resume", freeze, paused, pauseGroup);
            Check(GameManager.Instance.CurrentState == GameState.Playing && Time.timeScale == 1f &&
                freeze.RemainingTime < paused, "Actual Continue resumes gameplay and Freeze countdown");
            yield return (Func<bool>)(() => !freeze.IsFrozen || freeze.RemainingTime <= .5f);
            Check(freeze.IsFrozen && follower.IsPaused && Mathf.Abs(follower.CurrentDistance - distance) < .001f &&
                MobSpawner.Instance.SpawnedCount == spawned, "Enemies and spawns remain frozen for the active duration");
            yield return (Func<bool>)(() => !freeze.IsFrozen);
            yield return .1f;
            Check(freeze.RemainingTime == 0f && !timer.activeSelf && !follower.IsPaused, "Freeze expires and clears the timer and movement pause");
        }

        private static IEnumerator CheckArrowsAndRetry(GameHUD hud)
        {
            Status = "Defense101 actual legal shot, blocked bounce and Retry";
            var legal = ArrowSpawner.Instance.ActiveArrows.FirstOrDefault(item => item != null && !item.IsFired &&
                GridManager.Instance.IsPathClear(item.HeadPoint, item.HeadDirection));
            Check(legal != null, "Original101 contains an unblocked legal arrow");
            int fired = LevelManager.Instance.ArrowsFired, lives = GameManager.Instance.CurrentLives;
            legal.OnPlayerClick();
            Check(legal.IsFired && LevelManager.Instance.ArrowsFired == fired + 1 && GameManager.Instance.CurrentLives == lives &&
                Field<TMP_Text>(hud, "_arrowCountText").text.EndsWith((fired + 1) + "/" + LevelManager.Instance.TotalArrows),
                "Actual legal Arrow.OnPlayerClick fires, updates the HUD counter and preserves lives");
            var blocked = ArrowSpawner.Instance.ActiveArrows.FirstOrDefault(item => item != null && !item.IsFired &&
                !item.IsMarkedBlocked && !GridManager.Instance.IsPathClear(item.HeadPoint, item.HeadDirection));
            Check(blocked != null, "Original101 contains a blocked-arrow fixture");
            blocked.OnPlayerClick();
            Check(blocked.IsBlockedAnimating && !blocked.IsFired && GameManager.Instance.CurrentLives == lives - 1,
                "Actual blocked Arrow.OnPlayerClick starts the original bounce and costs exactly one life");
            yield return (Func<bool>)(() => !blocked.IsBlockedAnimating); yield return .5f;
            Check(!blocked.IsFired && blocked.IsMarkedBlocked && GameManager.Instance.CurrentLives == 2 &&
                LevelManager.Instance.ArrowsFired == fired + 1 && Hearts(hud).Count(image => image.gameObject.activeSelf) == 2,
                "Blocked bounce returns without firing and original HUD shows two remaining lives");
            Check(_data.PlayerData.freezeCount == 1, "One temporary Freeze charge remains before retry cancellation check");
            Field<Button>(hud, "_skill2FreezeButton").onClick.Invoke();
            Check(FreezeManager.Instance.IsFrozen && _data.PlayerData.freezeCount == 0, "Second temporary Freeze is active before retry");
            Field<Button>(hud, "_pauseButton").onClick.Invoke(); int pauseFrame = Time.frameCount;
            var pause = UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
            var pauseGroup = Field<CanvasGroup>(pause, "_canvasGroup");
            Check(GameManager.Instance.CurrentState == GameState.Paused && Time.timeScale == 0f,
                "Pause before Retry immediately freezes the original game state");
            yield return (Func<bool>)(() => pauseGroup.interactable && pauseGroup.alpha > .999f && Time.frameCount > pauseFrame + 1);
            PauseDiagnostic("settled pause before Retry", FreezeManager.Instance, FreezeManager.Instance.RemainingTime, pauseGroup);
            var retry = Field<Button>(pause, "_retryButton"); Check(Usable(retry), "Original Retry button is visible and enabled");
            int scene = SceneManager.GetActiveScene().handle; retry.onClick.Invoke();
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().handle != scene && DefenseReady());
            yield return .5f;
            var fresh = UnityEngine.Object.FindFirstObjectByType<GameHUD>();
            Check(fresh != hud && GameManager.Instance.CurrentLives == 3 && LevelManager.Instance.ArrowsFired == 0 &&
                ArrowSpawner.Instance.RemainingArrows == LevelManager.Instance.TotalArrows && !FreezeManager.Instance.IsFrozen &&
                Hearts(fresh).Length == 3 && Hearts(fresh).All(image => image.gameObject.activeSelf && image.color.a > .99f) &&
                Field<TMP_Text>(fresh, "_arrowCountText").text.EndsWith("0/" + LevelManager.Instance.TotalArrows),
                "Actual Retry reloads101, restores three visible lives/zero fired arrows and cancels Freeze");
        }

        private static bool Usable(Button button) => button != null && button.gameObject.activeInHierarchy && button.IsInteractable();
        private static void PauseDiagnostic(string stage, FreezeManager freeze, float paused, CanvasGroup group)
        {
            Results.Add("PAUSE TIMING: " + stage + ", frame=" + Time.frameCount + ", state=" + GameManager.Instance.CurrentState +
                ", timeScale=" + Time.timeScale + ", frozen=" + freeze.IsFrozen + ", remaining=" + freeze.RemainingTime +
                ", remainingDiff=" + (freeze.RemainingTime - paused) + ", alpha=" + group.alpha + ", interactable=" + group.interactable);
        }
        private static T Field<T>(UnityEngine.Object source, string name) where T : UnityEngine.Object
        { return new SerializedObject(source).FindProperty(name).objectReferenceValue as T; }
        private static T Private<T>(object source, string name) => (T)source.GetType().GetField(name,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(source);
        private static Image[] Hearts(GameHUD hud)
        {
            var array = new SerializedObject(hud).FindProperty("_heartIcons");
            return Enumerable.Range(0, array.arraySize).Select(index => array.GetArrayElementAtIndex(index).objectReferenceValue as Image).ToArray();
        }
    }
}
