namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Stages popup render states without invoking victory, loss, ad or reward handlers.</summary>
    public static partial class DarkThemeCapture
    {
        private const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        private static void StageMenu(string screen,ThemeMode theme)
        {
            DataManager.Instance.SetTheme(theme);
            var menu=UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var modes=UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>(FindObjectsInactive.Include);
            modes.Show(false);
            if(screen.StartsWith("Levels"))
            {
                _scope.Player.selectedGameMode=screen=="LevelsShapes"?GameModeId.Shapes:screen=="LevelsColorShift"?GameModeId.ColorShift:GameModeId.Defense;
                ThemeVisualAudit.Menu("Levels",theme);
            }
            else if(screen=="ModeSelect") { ThemeVisualAudit.Menu("Main",theme); modes.Show(true); }
            else if(screen=="Transition")
            {
                ThemeVisualAudit.Menu("Main",theme);
                var manager=SceneTransitionManager.Instance;
                var image=(Image)typeof(SceneTransitionManager).GetField("_overlayImage",Flags).GetValue(manager);
                image.material.SetFloat("_AspectRatio",(float)Screen.width/Screen.height);
                image.material.SetVector("_Center",new Vector4(.5f,.5f,0,0));
                image.material.SetFloat("_Progress",.62f);image.gameObject.SetActive(true);image.raycastTarget=false;
            }
            else ThemeVisualAudit.Menu(screen,theme);
        }

        private static void StageGame(string screen,ThemeMode theme,bool puzzle)
        {
            DataManager.Instance.SetTheme(theme);
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="MapTestScene")
            {
                var mapHud=UnityEngine.Object.FindFirstObjectByType<GameHUD>(FindObjectsInactive.Include);
                if(mapHud!=null)mapHud.SetBarsVisible(true,true);
                var overlay=UnityEngine.Object.FindFirstObjectByType<OverlayManager>(FindObjectsInactive.Include);
                if(overlay!=null)foreach(Transform panel in overlay.transform)
                {
                    if(!panel.name.EndsWith("Panel"))continue;
                    bool show=screen=="Pause"&&panel.name=="PausePanel";
                    foreach(var behaviour in panel.GetComponents<MonoBehaviour>())behaviour.StopAllCoroutines();
                    panel.gameObject.SetActive(show);var group=panel.GetComponent<CanvasGroup>();
                    if(group!=null){group.alpha=show?1:0;group.blocksRaycasts=group.interactable=false;}
                }
                return;
            }
            if(!puzzle){ThemeVisualAudit.Game(screen,theme);return;}
            var popups=UnityEngine.Object.FindFirstObjectByType<PuzzlePopups>();
            var panels=(CanvasGroup[])typeof(PuzzlePopups).GetField("_panels",Flags).GetValue(popups);
            var dialogs=(RectTransform[])typeof(PuzzlePopups).GetField("_dialogs",Flags).GetValue(popups);
            popups.StopAllCoroutines();int chosen=screen=="Pause"?0:screen=="Win"?1:screen=="Lose"?2:screen=="Hint"?3:-1;
            for(int i=0;i<panels.Length;i++)
            {panels[i].alpha=i==chosen?1:0;panels[i].interactable=panels[i].blocksRaycasts=false;if(i==chosen)dialogs[i].localScale=Vector3.one;}
            if(chosen==1)
            {
                var stars=(GameObject[])typeof(PuzzlePopups).GetField("_winStars",Flags).GetValue(popups);
                foreach(var star in stars){star.SetActive(true);star.transform.localScale=Vector3.one;}
            }
        }

        private static void FreezeDecorations()
        {
            foreach(var feedback in UnityEngine.Object.FindObjectsByType<UIButtonFeedback>(FindObjectsSortMode.None))
                feedback.EnableIdleBreath=false;
            foreach(var particles in UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
                particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static bool DefenseReady()
        {
            if(!LevelManager.HasInstance||GameManager.Instance.CurrentState!=GameState.Playing||
                !ArrowSwarm.Arrow.ArrowSpawner.HasInstance||ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows.Count==0)return false;
            foreach(var arrow in ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows)
                if(arrow!=null&&arrow.GetComponent<ArrowSwarm.Arrow.ArrowVisuals>().IsSpawning)return false;
            return true;
        }

        /// <summary>Snapshots live Graphic values to detect irreversible theme tint or material changes.</summary>
        private sealed class GraphicState
        {
            private readonly System.Collections.Generic.List<Entry> _entries=new System.Collections.Generic.List<Entry>();
            internal GraphicState()
            {
                foreach(var g in UnityEngine.Object.FindObjectsByType<Graphic>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                    if(g.GetComponent<ThemedUIElement>()!=null)_entries.Add(new Entry(g));
            }
            internal void Verify(System.Collections.Generic.List<string> report,string label)
            {
                int checkedCount=0;
                foreach(var e in _entries)
                {
                    if(e.Graphic==null)continue;
                    if(e.Graphic.material!=e.Material||e.Graphic.color!=e.Color||
                        (e.Graphic is Image image&&image.sprite!=e.Sprite))
                        throw new InvalidOperationException(label+" Light restoration mismatch: "+e.Graphic.name);
                    checkedCount++;
                }
                report.Add("PASS: "+label+" restores "+checkedCount+" authored graphic materials/sprites/colors after Dark");
            }
            private sealed class Entry
            {
                internal readonly Graphic Graphic;internal readonly Material Material;internal readonly Color Color;internal readonly Sprite Sprite;
                internal Entry(Graphic g){Graphic=g;Material=g.material;Color=g.color;Sprite=(g as Image)?.sprite;}
            }
        }
    }
}
