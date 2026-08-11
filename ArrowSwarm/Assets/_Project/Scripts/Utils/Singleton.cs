namespace ArrowSwarm.Utils
{
    using UnityEngine;

    /// <summary>
    /// Generic singleton base class for MonoBehaviour managers.
    /// Ensures only one instance exists and persists across scene loads.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting;

        /// <summary>
        /// True if a valid instance currently exists.
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Global access point to the singleton instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting && !Application.isPlaying)
                {
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null && Application.isPlaying)
                        {
                            var singletonObject = new GameObject($"[{typeof(T).Name}]");
                            _instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            _applicationIsQuitting = false;

            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
            OnSingletonAwake();
        }

        /// <summary>
        /// Called once when the singleton is first initialized. Override instead of Awake.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
