namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Queues real pointer states onto disabled controls and proves that callbacks/data stay untouched.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator Press04Disabled(NativeMouse mouse, Button button, string context)
        {
            Check(button != null && button.gameObject.activeInHierarchy && !button.interactable && button.targetGraphic != null,
                "Disabled control is genuinely visible and unavailable / " + context);
            yield return Settled(.1f); Canvas.ForceUpdateCanvases();
            var rect = button.GetComponent<RectTransform>(); var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 centre = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = centre }, hits);
            Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button,
                "Native disabled-control pointer hits its actual authored button / " + context);
            int calls = 0; UnityAction spy = () => calls++;
            string player = JsonUtility.ToJson(DataManager.Instance.PlayerData), scene = SceneManager.GetActiveScene().name;
            button.onClick.AddListener(spy);
            try
            {
                mouse.Send(new Vector2(2, 2), false); yield return mouse.Wait(); yield return Settled(.06f);
                mouse.Send(centre, true); yield return mouse.Wait(); yield return Settled(.12f);
                mouse.Send(centre, false); yield return mouse.Wait(); yield return Settled(.25f);
                Check(calls == 0 && SceneManager.GetActiveScene().name == scene && !SceneTransitionManager.Instance.IsTransitioning &&
                    JsonUtility.ToJson(DataManager.Instance.PlayerData) == player,
                    "Actual disabled press invokes no callback, scene load or data change / " + context);
            }
            finally { button.onClick.RemoveListener(spy); }
        }
    }
}
