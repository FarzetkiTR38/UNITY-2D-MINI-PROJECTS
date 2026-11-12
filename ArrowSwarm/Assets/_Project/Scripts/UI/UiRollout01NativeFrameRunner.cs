#if UNITY_EDITOR
namespace ArrowSwarm.UI
{
    using System;
    using UnityEngine;

    /// <summary>Temporary Editor verification driver; geometry is inspected in the actual player frame.</summary>
    public sealed class UiRollout01NativeFrameRunner : MonoBehaviour
    {
        private Action _tick;

        public static UiRollout01NativeFrameRunner Begin(Action tick)
        {
            var host = new GameObject("UI rollout native verification frame") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(host);
            try
            {
                var runner = host.AddComponent<UiRollout01NativeFrameRunner>();
                runner._tick = tick;
                return runner;
            }
            catch { DestroyImmediate(host); throw; }
        }

        private void LateUpdate() => _tick?.Invoke();

        private void OnDestroy() => _tick = null;

        public void End(bool immediate = false)
        {
            _tick = null; enabled = false;
            if (immediate || !Application.isPlaying) DestroyImmediate(gameObject); else Destroy(gameObject);
        }
    }
}

#endif
