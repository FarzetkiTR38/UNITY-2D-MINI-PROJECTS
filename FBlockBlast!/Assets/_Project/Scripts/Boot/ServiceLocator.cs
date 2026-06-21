using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonGalaxy.Boot
{
    /// <summary>
    /// Lightweight service locator for dependency resolution.
    /// Services are registered at boot time and accessed throughout the game.
    /// Preferred over singletons for testability and explicit registration.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service instance. Overwrites any previously registered
        /// service of the same type.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"[ServiceLocator] Cannot register null service for type {typeof(T).Name}.");
                return;
            }

            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Overwriting existing service for type {type.Name}.");
            }

            _services[type] = service;
        }

        /// <summary>
        /// Retrieves a registered service. Returns null if not found.
        /// </summary>
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            Debug.LogWarning($"[ServiceLocator] Service of type {typeof(T).Name} not found.");
            return null;
        }

        /// <summary>
        /// Retrieves a registered service. Throws if not found.
        /// Use when the service is mandatory.
        /// </summary>
        public static T GetRequired<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException(
                $"[ServiceLocator] Required service of type {typeof(T).Name} not registered. " +
                "Ensure BootManager has run before accessing this service.");
        }

        /// <summary>
        /// Returns true if a service of the given type is registered.
        /// </summary>
        public static bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Unregisters a service.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        /// <summary>
        /// Clears all registered services. Called on application quit or
        /// during test teardown.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
