namespace ArrowSwarm.Modes.Editor
{
    using System.Collections;
    using System.IO;
    using System.Linq;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public static partial class UIRevisionPlaytestRunner
    {
        private static Button _pressedButton;
        private static PointerEventData _pressedPointer;
        private static IEnumerator CheckButtonFeedback(Button button, string name)
        {
            Check(button != null && button.IsInteractable() && button.gameObject.activeInHierarchy, name + ": visible enabled button");
            var image = button.targetGraphic as Image;
            var feedback = button.GetComponent<UIButtonFeedback>();
            Check(image != null && image.sprite != null && image.color.a > .01f && feedback != null &&
                button.transition == Selectable.Transition.ColorTint, name + ": visible Image target, ColorTint and existing feedback");
            var target = Field<Transform>(feedback, "_targetTransform") ?? button.transform;
            Check(image.transform == target || image.transform.IsChildOf(target), name + ": scale target contains actual visible artwork");
            Vector3 baseline = target.localScale; Color initial = image.canvasRenderer.GetColor();
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerEnter = button.gameObject };
            _pressedButton = button; _pressedPointer = pointer;
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            yield return .13f;
            bool squeezed = target.localScale.sqrMagnitude < baseline.sqrMagnitude * .98f;
            Color pressed = image.canvasRenderer.GetColor();
            bool tinted = pressed.r + pressed.g + pressed.b < initial.r + initial.g + initial.b - .03f;
            ReleasePointer();
            yield return .25f;
            Check(squeezed, name + ": held pointer press visibly scales down");
            Check(tinted, name + ": held pointer press visibly darkens target graphic");
            Check(Vector3.Distance(target.localScale, baseline) < .002f, name + ": pointer release restores authored scale");
        }

        private static void ReleasePointer()
        {
            if (_pressedButton != null && _pressedPointer != null)
            {
                ExecuteEvents.Execute(_pressedButton.gameObject, _pressedPointer, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(_pressedButton.gameObject, _pressedPointer, ExecuteEvents.pointerExitHandler);
            }
            _pressedButton = null; _pressedPointer = null;
        }

        private static IEnumerator CheckLanguages(PuzzleSession session, PuzzleHUD hud, PuzzlePopups popups)
        {
            foreach (string code in new[] { "tr", "en" })
            {
                _localization.SetLanguage(code, saveToPrefs: false); yield return .05f;
                string prefix = _localization.GetText("hud_arrows") + ": ";
                Check(Field<TMP_Text>(hud, "_arrowCountText").text.StartsWith(prefix), session.Mode + ": live " + code + " arrow counter");
                Check(Field<TMP_Text>(hud, "_levelText").text == _localization.GetText("menu_level_short") + session.LevelNumber,
                    session.Mode + ": live " + code + " level label");
                Check(Field<TMP_Text>(popups, "_winLevel").text == string.Format(_localization.GetText("puzzle_level_cleared"), session.LevelNumber) &&
                    Field<TMP_Text>(popups, "_loseLevel").text == string.Format(_localization.GetText("puzzle_level_failed"), session.LevelNumber),
                    session.Mode + ": live " + code + " dynamic win/lose popup labels");
                if (session.Mode == GameModeId.ColorShift)
                    Check(Field<TMP_Text>(hud, "_turns").text == session.Turns.ToString(),
                        "ColorShift: live " + code + " rotation counter");
            }
        }

        private static void CheckLayout(PuzzleSession session, PuzzleHUD hud)
        {
            var roots = session.gameObject.scene.GetRootGameObjects();
            var transforms = roots.SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            Check(roots.SelectMany(root => root.GetComponentsInChildren<Slider>(true)).Count() == 1,
                session.Mode + ": exactly one original zoom slider");
            Check(!transforms.Any(item => item.name == "Skill2" || item.name.Contains("FreezePopup")), session.Mode + ": no freeze controls");
            string[] legacy = { "GameHUD", "OverlayManager", "PauseMenuUI", "LevelCompleteUI", "GameOverUI", "TipPopupUI", "FreezePopupUI" };
            Check(!roots.SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true)).Any(component => component != null && legacy.Contains(component.GetType().Name)),
                session.Mode + ": no legacy HUD or popup controllers");
            var shell = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Puzzles/Canvas_HUD.prefab");
            const string defenseScene = "Assets/_Project/Scenes/GameScene.unity";
            string defense = File.ReadAllText(defenseScene);
            var dependencies = AssetDatabase.GetDependencies(defenseScene, true);
            foreach (string bar in new[] { "TopBar", "BottomBar" })
            {
                var actual = hud.transform.Find(bar + "/Bg").GetComponent<Image>();
                var source = shell.transform.Find(bar + "/Bg").GetComponent<Image>();
                Check(actual.sprite != null && actual.sprite == source.sprite, session.Mode + ": " + bar + " retains original authored sprite");
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source.sprite, out string guid, out long fileId);
                bool directReference = defense.Contains("m_Sprite: {fileID: " + fileId + ", guid: " + guid);
                bool inheritedReference = dependencies.Where(path => path.EndsWith(".prefab"))
                    .Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(prefab => prefab != null)
                    .Select(prefab => prefab.transform.Find(bar + "/Bg")).Where(item => item != null)
                    .Any(item => item.GetComponent<Image>() != null && item.GetComponent<Image>().sprite == source.sprite);
                Check(directReference || inheritedReference, session.Mode + ": " + bar + " matches saved Defense GameScene or its source prefab");
            }
        }
    }
}
