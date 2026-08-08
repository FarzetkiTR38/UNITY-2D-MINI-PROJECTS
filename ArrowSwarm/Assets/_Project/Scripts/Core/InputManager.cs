namespace ArrowSwarm.Core
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Utils;

    /// <summary>
    /// Handles player input globally for clicking/touching arrows
    /// using the New Input System.
    /// </summary>
    public class InputManager : Singleton<InputManager>
    {
        private Camera _mainCamera;

        private Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                    if (_mainCamera == null)
                    {
                        _mainCamera = FindFirstObjectByType<Camera>();
                    }
                }
                return _mainCamera;
            }
        }

        protected override void OnSingletonAwake()
        {
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (MainCamera == null) return;

            // Handle Mouse Click
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                ProcessClick(mouse.position.ReadValue());
                return;
            }

            // Handle Touch
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.touches.Count > 0)
            {
                var touch = touchscreen.touches[0];
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (UnityEngine.EventSystems.EventSystem.current != null && 
                        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue()))
                        return;

                    ProcessClick(touch.position.ReadValue());
                }
            }
        }

        private void ProcessClick(Vector2 screenPosition)
        {
            if (MainCamera == null) return;
            Vector2 worldPos = MainCamera.ScreenToWorldPoint(screenPosition);
            
            // Perform 2D Raycast All to hit arrow even if overlapping other colliders
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null)
                {
                    var arrow = hits[i].collider.GetComponentInParent<ArrowSwarm.Arrow.Arrow>();
                    if (arrow != null)
                    {
                        arrow.OnPlayerClick();
                        break;
                    }
                }
            }
        }
    }
}
