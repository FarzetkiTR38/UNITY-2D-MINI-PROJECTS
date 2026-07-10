// ============================================================================
// ServiceTemplate.cs
// Purpose: Service Locator pattern and service interface templates
// Dependencies: None (self-contained)
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameName.Core.Patterns
{
    /// <summary>
    /// Marker interface for all services registered with the ServiceLocator.
    /// </summary>
    public interface IService { }

    /// <summary>
    /// Provides a centralized registry for service instances.
    /// Preferred over Singletons for testability and flexibility.
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> Lightweight dependency access for cross-cutting services
    /// (Audio, Save, Input, Scene Loading) without Singleton anti-patterns.</para>
    /// <para><b>Registration:</b> Call in Bootstrap scene during initialization:</para>
    /// <code>
    /// ServiceLocator.Register&lt;IAudioService&gt;(audioManager);
    /// ServiceLocator.Register&lt;ISaveService&gt;(saveManager);
    /// </code>
    /// <para><b>Resolution:</b> Access from any system:</para>
    /// <code>
    /// var audio = ServiceLocator.Get&lt;IAudioService&gt;();
    /// audio.PlaySFX(clip);
    /// </code>
    /// <para><b>Testing:</b> Register mock implementations for unit tests:</para>
    /// <code>
    /// ServiceLocator.Register&lt;IAudioService&gt;(new MockAudioService());
    /// </code>
    /// <para><b>When to use:</b> For infrastructure services that many systems need.
    /// For gameplay components, prefer [SerializeField] injection or events.</para>
    /// </remarks>
    public static class ServiceLocator
    {
        #region Private Fields

        private static readonly Dictionary<Type, object> Services = new(16);

        #endregion

        #region Public Methods

        /// <summary>
        /// Registers a service instance for the given interface type.
        /// Overwrites any previously registered service of the same type.
        /// </summary>
        /// <typeparam name="T">The service interface type. Must be a reference type.</typeparam>
        /// <param name="service">The service implementation instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service),
                    $"[ServiceLocator] Cannot register null service for type {typeof(T).Name}.");
            }

            Type type = typeof(T);

            if (Services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Overwriting existing service: {type.Name}. " +
                                 $"New implementation: {service.GetType().Name}");
            }

            Services[type] = service;

            #if UNITY_EDITOR
            Debug.Log($"[ServiceLocator] Registered: {type.Name} → {service.GetType().Name}");
            #endif
        }

        /// <summary>
        /// Retrieves the registered service for the given interface type.
        /// </summary>
        /// <typeparam name="T">The service interface type.</typeparam>
        /// <returns>The registered service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no service is registered for the type.</exception>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);

            if (Services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            throw new InvalidOperationException(
                $"[ServiceLocator] Service not registered: {type.Name}. " +
                $"Ensure it is registered during Bootstrap initialization.");
        }

        /// <summary>
        /// Attempts to retrieve a registered service without throwing.
        /// </summary>
        /// <typeparam name="T">The service interface type.</typeparam>
        /// <param name="service">The service instance if found, null otherwise.</param>
        /// <returns><c>true</c> if the service was found; <c>false</c> otherwise.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);

            if (Services.TryGetValue(type, out object obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Checks if a service of the specified type is registered.
        /// </summary>
        /// <typeparam name="T">The service interface type.</typeparam>
        /// <returns><c>true</c> if a service of this type is registered.</returns>
        public static bool IsRegistered<T>() where T : class
        {
            return Services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Removes the registered service for the given type.
        /// </summary>
        /// <typeparam name="T">The service interface type to unregister.</typeparam>
        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);
            if (Services.Remove(type))
            {
                #if UNITY_EDITOR
                Debug.Log($"[ServiceLocator] Unregistered: {type.Name}");
                #endif
            }
        }

        /// <summary>
        /// Removes all registered services. Call during cleanup, scene unload, or testing teardown.
        /// </summary>
        public static void Clear()
        {
            Services.Clear();

            #if UNITY_EDITOR
            Debug.Log("[ServiceLocator] All services cleared.");
            #endif
        }

        /// <summary>
        /// Gets the count of currently registered services.
        /// </summary>
        /// <returns>Number of registered services.</returns>
        public static int GetRegisteredCount() => Services.Count;

        #endregion
    }
}

// ============================================================================
// Example Service Interfaces
// ============================================================================

namespace GameName.Core.Interfaces
{
    /// <summary>Service interface for audio playback.</summary>
    public interface IAudioService
    {
        /// <summary>Plays a sound effect once.</summary>
        void PlaySFX(AudioClip clip, float volume = 1f);

        /// <summary>Plays a UI sound effect.</summary>
        void PlayUI(AudioClip clip);

        /// <summary>Starts playing background music with optional crossfade.</summary>
        void PlayMusic(AudioClip clip, float fadeDuration = 1f);

        /// <summary>Stops the current music with optional fade out.</summary>
        void StopMusic(float fadeDuration = 1f);

        /// <summary>Sets the master volume (0-1 normalized).</summary>
        void SetMasterVolume(float normalizedVolume);

        /// <summary>Sets the music volume (0-1 normalized).</summary>
        void SetMusicVolume(float normalizedVolume);

        /// <summary>Sets the SFX volume (0-1 normalized).</summary>
        void SetSfxVolume(float normalizedVolume);
    }

    /// <summary>Service interface for save/load operations.</summary>
    public interface ISaveService
    {
        /// <summary>Saves the current game state.</summary>
        void Save();

        /// <summary>Loads the game state from disk.</summary>
        /// <returns><c>true</c> if a save was found and loaded.</returns>
        bool Load();

        /// <summary>Checks if a save file exists.</summary>
        bool HasSave { get; }

        /// <summary>Deletes the save file.</summary>
        void DeleteSave();

        /// <summary>Gets the current save data container.</summary>
        SaveData GetSaveData();
    }

    /// <summary>Service interface for scene management.</summary>
    public interface ISceneService
    {
        /// <summary>Loads a scene asynchronously.</summary>
        Awaitable LoadSceneAsync(string sceneName);

        /// <summary>Loads a scene additively.</summary>
        Awaitable LoadSceneAdditiveAsync(string sceneName);

        /// <summary>Unloads an additively loaded scene.</summary>
        Awaitable UnloadSceneAsync(string sceneName);

        /// <summary>Gets the name of the currently active scene.</summary>
        string CurrentSceneName { get; }

        /// <summary>Raised when scene loading progress updates (0-1).</summary>
        event Action<float> OnLoadingProgress;
    }

    /// <summary>Service interface for input management.</summary>
    public interface IInputService
    {
        /// <summary>Enables the specified action map.</summary>
        void EnableActionMap(string actionMapName);

        /// <summary>Disables the specified action map.</summary>
        void DisableActionMap(string actionMapName);

        /// <summary>Disables all action maps.</summary>
        void DisableAllInput();

        /// <summary>Gets the current input device type.</summary>
        InputDeviceType CurrentDevice { get; }

        /// <summary>Raised when the input device changes.</summary>
        event Action<InputDeviceType> OnDeviceChanged;
    }

    /// <summary>Defines input device types for UI adaptation.</summary>
    public enum InputDeviceType
    {
        /// <summary>Keyboard and mouse input.</summary>
        KeyboardMouse = 0,

        /// <summary>Gamepad/controller input.</summary>
        Gamepad = 1,

        /// <summary>Touchscreen input.</summary>
        Touch = 2
    }

    // Forward declaration for ISaveService
    /// <summary>Placeholder for save data — define in your Systems assembly.</summary>
    public class SaveData { }
}
