// ============================================================================
// EventChannelTemplate.cs
// Purpose: ScriptableObject event channels for decoupled communication
// Dependencies: None (self-contained)
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using UnityEngine;

namespace GameName.Core.Events
{
    /// <summary>
    /// ScriptableObject-based event channel with no parameters.
    /// Used for fire-and-forget notifications between decoupled systems.
    /// </summary>
    /// <remarks>
    /// <para><b>Creation:</b> Assets → Create → GameName → Events → Void Event Channel</para>
    /// <para><b>Raiser example:</b> <c>_onPlayerDied.RaiseEvent();</c></para>
    /// <para><b>Listener example:</b></para>
    /// <code>
    /// private void OnEnable() => _onPlayerDied.OnEventRaised += HandlePlayerDied;
    /// private void OnDisable() => _onPlayerDied.OnEventRaised -= HandlePlayerDied;
    /// </code>
    /// </remarks>
    [CreateAssetMenu(fileName = "New_VoidEventChannel", menuName = "GameName/Events/Void Event Channel", order = 0)]
    public class VoidEventChannel : ScriptableObject
    {
        /// <summary>Raised when this event channel is invoked.</summary>
        public event Action OnEventRaised;

        #if UNITY_EDITOR
        [Tooltip("Description of what this event represents. Editor only.")]
        [SerializeField, TextArea(2, 4)]
        private string _description = "";
        #endif

        /// <summary>Raises the event, notifying all subscribers.</summary>
        public void RaiseEvent()
        {
            if (OnEventRaised == null)
            {
                Debug.LogWarning($"[EventChannel:{name}] Event raised with no listeners.", this);
                return;
            }

            OnEventRaised.Invoke();
        }
    }

    /// <summary>
    /// Base class for typed ScriptableObject event channels.
    /// Derive from this to create channels that carry data.
    /// </summary>
    /// <typeparam name="T">The type of data this event carries.</typeparam>
    public abstract class EventChannel<T> : ScriptableObject
    {
        /// <summary>Raised when this event channel is invoked with data.</summary>
        public event Action<T> OnEventRaised;

        #if UNITY_EDITOR
        [Tooltip("Description of what this event represents. Editor only.")]
        [SerializeField, TextArea(2, 4)]
        private string _description = "";
        #endif

        /// <summary>Raises the event with the specified data, notifying all subscribers.</summary>
        /// <param name="value">The data to pass to listeners.</param>
        public void RaiseEvent(T value)
        {
            if (OnEventRaised == null)
            {
                Debug.LogWarning($"[EventChannel:{name}] Event raised with no listeners. Value: {value}", this);
                return;
            }

            OnEventRaised.Invoke(value);
        }
    }

    /// <summary>
    /// Event channel carrying an integer value.
    /// Use for: score changes, health changes, currency changes, count updates.
    /// </summary>
    [CreateAssetMenu(fileName = "New_IntEventChannel", menuName = "GameName/Events/Int Event Channel", order = 1)]
    public class IntEventChannel : EventChannel<int> { }

    /// <summary>
    /// Event channel carrying a float value.
    /// Use for: normalized progress, damage amounts, timer values.
    /// </summary>
    [CreateAssetMenu(fileName = "New_FloatEventChannel", menuName = "GameName/Events/Float Event Channel", order = 2)]
    public class FloatEventChannel : EventChannel<float> { }

    /// <summary>
    /// Event channel carrying a string value.
    /// Use for: notifications, dialogue lines, scene names.
    /// </summary>
    [CreateAssetMenu(fileName = "New_StringEventChannel", menuName = "GameName/Events/String Event Channel", order = 3)]
    public class StringEventChannel : EventChannel<string> { }

    /// <summary>
    /// Event channel carrying a Transform reference.
    /// Use for: target tracking, spawn notifications, position events.
    /// </summary>
    [CreateAssetMenu(fileName = "New_TransformEventChannel", menuName = "GameName/Events/Transform Event Channel", order = 4)]
    public class TransformEventChannel : EventChannel<Transform> { }

    /// <summary>
    /// Event channel carrying a Vector2 value.
    /// Use for: position events, direction events, input events.
    /// </summary>
    [CreateAssetMenu(fileName = "New_Vector2EventChannel", menuName = "GameName/Events/Vector2 Event Channel", order = 5)]
    public class Vector2EventChannel : EventChannel<Vector2> { }

    /// <summary>
    /// Event channel carrying a boolean value.
    /// Use for: toggle events, state changes, enable/disable notifications.
    /// </summary>
    [CreateAssetMenu(fileName = "New_BoolEventChannel", menuName = "GameName/Events/Bool Event Channel", order = 6)]
    public class BoolEventChannel : EventChannel<bool> { }

    /// <summary>
    /// Event channel carrying a GameObject reference.
    /// Use for: entity spawn/death events, collision notifications.
    /// </summary>
    [CreateAssetMenu(fileName = "New_GameObjectEventChannel", menuName = "GameName/Events/GameObject Event Channel", order = 7)]
    public class GameObjectEventChannel : EventChannel<GameObject> { }
}
