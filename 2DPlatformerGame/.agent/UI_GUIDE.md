# UI Development Guide

## Overview

This document defines the rules and patterns for building user interfaces in Unity 6 2D games. All UI must be responsive, accessible, performant, and support multiple input methods (touch, keyboard, gamepad).

---

## 1. UI Technology Choice

### 1.1 When to Use What

| Technology | Use For |
|-----------|---------|
| **TextMeshPro (UGUI)** | In-game HUD, world-space UI, damage popups, health bars |
| **UI Toolkit (UIDocument)** | Complex menus, settings screens, inventory, shop UI |
| **Both** | Mix as needed — TMP for gameplay-adjacent UI, UI Toolkit for menu systems |

### 1.2 Canvas Configuration

```
Canvas Setup for 2D Games:

Canvas (Screen Space - Overlay):
├── Render Mode: Screen Space - Overlay
├── UI Scale Mode: Scale With Screen Size
├── Reference Resolution: 1920 x 1080
├── Screen Match Mode: Match Width Or Height
├── Match: 0.5 (balanced between width and height)
└── Sort Order: Determines canvas layering

Multiple Canvas Strategy:
├── Canvas_HUD         (Sort Order: 0)  — Gameplay HUD, always visible
├── Canvas_Menus       (Sort Order: 10) — Pause, inventory, settings
├── Canvas_Popups      (Sort Order: 20) — Notifications, tooltips
├── Canvas_Transitions (Sort Order: 30) — Fade overlays, transitions
└── Canvas_Debug       (Sort Order: 99) — Debug info (editor only)
```

### 1.3 Canvas Optimization Rules

| Rule | Details |
|------|---------|
| Separate dynamic and static elements | Dynamic elements on their own Canvas to avoid rebatch |
| Disable Raycast Target on non-interactive elements | Reduces UI raycast cost |
| Use Canvas Groups for fade/visibility | Cheaper than enabling/disabling individual elements |
| Avoid Layout Groups in hot paths | Layout recalculation is expensive |
| Pool damage popups and floating text | Don't instantiate/destroy UI elements frequently |

---

## 2. UI Architecture

### 2.1 Screen Management

```csharp
// ============================================================================
// UIScreen.cs — Base class for all UI screens
// ============================================================================
using UnityEngine;

namespace GameName.UI.Base
{
    /// <summary>
    /// Base class for all UI screens. Manages show/hide lifecycle
    /// and provides animation hooks.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        #region Private Fields

        private CanvasGroup _canvasGroup;

        #endregion

        #region Properties

        /// <summary>Gets a value indicating whether this screen is currently visible.</summary>
        public bool IsVisible { get; private set; }

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        #endregion

        #region Public Methods

        /// <summary>Shows this screen with optional animation.</summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            IsVisible = true;
            OnShow();
        }

        /// <summary>Hides this screen with optional animation.</summary>
        public virtual void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            IsVisible = false;
            OnHide();
            gameObject.SetActive(false);
        }

        #endregion

        #region Protected Methods

        /// <summary>Called after the screen is shown. Override for initialization.</summary>
        protected virtual void OnShow() { }

        /// <summary>Called before the screen is hidden. Override for cleanup.</summary>
        protected virtual void OnHide() { }

        #endregion
    }
}
```

```csharp
// ============================================================================
// UIManager.cs — Manages screen navigation and transitions
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace GameName.UI.Base
{
    /// <summary>
    /// Manages UI screen navigation, history, and transitions.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Screens")]
        [Tooltip("All available UI screens. Register in Inspector.")]
        [SerializeField] private List<UIScreen> _screens = new();

        private readonly Stack<UIScreen> _screenHistory = new();
        private UIScreen _currentScreen;

        /// <summary>Gets the currently active screen.</summary>
        public UIScreen CurrentScreen => _currentScreen;

        /// <summary>
        /// Shows the specified screen and hides the current one.
        /// </summary>
        /// <typeparam name="T">The screen type to show.</typeparam>
        /// <param name="addToHistory">If true, the current screen is pushed to history for back navigation.</param>
        public void ShowScreen<T>(bool addToHistory = true) where T : UIScreen
        {
            for (int i = 0; i < _screens.Count; i++)
            {
                if (_screens[i] is T targetScreen)
                {
                    if (_currentScreen != null)
                    {
                        if (addToHistory)
                        {
                            _screenHistory.Push(_currentScreen);
                        }
                        _currentScreen.Hide();
                    }

                    _currentScreen = targetScreen;
                    _currentScreen.Show();
                    return;
                }
            }

            Debug.LogError($"[UIManager] Screen of type {typeof(T).Name} not found.", this);
        }

        /// <summary>Navigates back to the previous screen in history.</summary>
        public void GoBack()
        {
            if (_screenHistory.Count == 0)
            {
                Debug.LogWarning("[UIManager] No screen in history to go back to.", this);
                return;
            }

            _currentScreen?.Hide();
            _currentScreen = _screenHistory.Pop();
            _currentScreen.Show();
        }

        /// <summary>Hides all screens and clears history.</summary>
        public void HideAll()
        {
            for (int i = 0; i < _screens.Count; i++)
            {
                _screens[i].Hide();
            }
            _screenHistory.Clear();
            _currentScreen = null;
        }
    }
}
```

### 2.2 Data Binding Pattern

```csharp
// ============================================================================
// GameplayHUD.cs — HUD that binds to gameplay events
// ============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameName.UI.Screens
{
    /// <summary>
    /// Gameplay HUD displaying health, score, currency, and status.
    /// Binds to game events via ScriptableObject Event Channels.
    /// </summary>
    public class GameplayHUD : UIScreen
    {
        [Header("Health")]
        [SerializeField] private Image _healthFill;
        [SerializeField] private TMP_Text _healthText;

        [Header("Score")]
        [SerializeField] private TMP_Text _scoreText;

        [Header("Currency")]
        [SerializeField] private TMP_Text _currencyText;

        [Header("Events")]
        [SerializeField] private IntEventChannel _onHealthChanged;
        [SerializeField] private IntEventChannel _onScoreChanged;
        [SerializeField] private IntEventChannel _onCurrencyChanged;

        private int _maxHealth;

        private void OnEnable()
        {
            if (_onHealthChanged != null)
                _onHealthChanged.OnEventRaised += UpdateHealth;
            if (_onScoreChanged != null)
                _onScoreChanged.OnEventRaised += UpdateScore;
            if (_onCurrencyChanged != null)
                _onCurrencyChanged.OnEventRaised += UpdateCurrency;
        }

        private void OnDisable()
        {
            if (_onHealthChanged != null)
                _onHealthChanged.OnEventRaised -= UpdateHealth;
            if (_onScoreChanged != null)
                _onScoreChanged.OnEventRaised -= UpdateScore;
            if (_onCurrencyChanged != null)
                _onCurrencyChanged.OnEventRaised -= UpdateCurrency;
        }

        /// <summary>Initializes the HUD with max health value.</summary>
        public void Initialize(int maxHealth)
        {
            _maxHealth = maxHealth;
            UpdateHealth(maxHealth);
            UpdateScore(0);
            UpdateCurrency(0);
        }

        private void UpdateHealth(int currentHealth)
        {
            if (_maxHealth <= 0) return;

            float fillAmount = (float)currentHealth / _maxHealth;
            _healthFill.fillAmount = fillAmount;
            _healthText.SetText("{0}/{1}", currentHealth, _maxHealth);
        }

        private void UpdateScore(int score)
        {
            _scoreText.SetText("{0}", score);
        }

        private void UpdateCurrency(int currency)
        {
            _currencyText.SetText("{0}", currency);
        }
    }
}
```

---

## 3. Common UI Components

### 3.1 Health Bar (World Space)

```csharp
// ============================================================================
// WorldHealthBar.cs — World-space health bar above entities
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

namespace GameName.UI.Components
{
    /// <summary>
    /// World-space health bar displayed above damageable entities.
    /// Automatically hides when health is full.
    /// </summary>
    public class WorldHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Canvas _canvas;

        [Header("Settings")]
        [Tooltip("Duration to show after taking damage.")]
        [SerializeField, Min(0f)] private float _showDuration = 3f;

        [Tooltip("Color at full health.")]
        [SerializeField] private Color _fullColor = Color.green;

        [Tooltip("Color at zero health.")]
        [SerializeField] private Color _emptyColor = Color.red;

        [Tooltip("Hide when health is full.")]
        [SerializeField] private bool _hideWhenFull = true;

        private float _showTimer;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
            if (_hideWhenFull)
            {
                _canvas.enabled = false;
            }
        }

        private void LateUpdate()
        {
            // Billboard: face the camera
            if (_mainCamera != null)
            {
                transform.rotation = _mainCamera.transform.rotation;
            }

            // Auto-hide timer
            if (_showTimer > 0f)
            {
                _showTimer -= Time.deltaTime;
                if (_showTimer <= 0f && _hideWhenFull)
                {
                    _canvas.enabled = false;
                }
            }
        }

        /// <summary>Updates the health bar fill and color.</summary>
        /// <param name="currentHealth">Current health value.</param>
        /// <param name="maxHealth">Maximum health value.</param>
        public void UpdateHealthBar(int currentHealth, int maxHealth)
        {
            if (maxHealth <= 0) return;

            float ratio = (float)currentHealth / maxHealth;
            _fillImage.fillAmount = ratio;
            _fillImage.color = Color.Lerp(_emptyColor, _fullColor, ratio);

            if (ratio < 1f || !_hideWhenFull)
            {
                _canvas.enabled = true;
                _showTimer = _showDuration;
            }
        }
    }
}
```

### 3.2 Damage Popup

```csharp
// ============================================================================
// DamagePopup.cs — Floating damage number that animates and pools
// ============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace GameName.UI.Components
{
    /// <summary>
    /// Animated floating damage number. Uses object pooling for performance.
    /// </summary>
    public class DamagePopup : MonoBehaviour, IPoolable
    {
        [Header("References")]
        [SerializeField] private TMP_Text _text;

        [Header("Animation")]
        [Tooltip("How long the popup is visible.")]
        [SerializeField, Min(0.1f)] private float _lifetime = 0.8f;

        [Tooltip("Vertical float speed.")]
        [SerializeField] private float _floatSpeed = 2f;

        [Tooltip("Horizontal spread range.")]
        [SerializeField] private float _spreadRange = 0.5f;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _criticalColor = Color.yellow;
        [SerializeField] private Color _healColor = Color.green;

        private float _elapsedTime;
        private Vector3 _moveDirection;
        private Color _startColor;
        private IObjectPool<DamagePopup> _pool;

        /// <summary>Sets the pool reference for automatic return.</summary>
        public void SetPool(IObjectPool<DamagePopup> pool)
        {
            _pool = pool;
        }

        /// <summary>Configures and starts the popup animation.</summary>
        /// <param name="amount">The damage/heal amount to display.</param>
        /// <param name="isCritical">Whether this is a critical hit.</param>
        /// <param name="isHeal">Whether this is healing.</param>
        public void Setup(int amount, bool isCritical = false, bool isHeal = false)
        {
            _text.SetText("{0}", Mathf.Abs(amount));

            if (isHeal)
            {
                _startColor = _healColor;
                _text.SetText("+{0}", amount);
            }
            else if (isCritical)
            {
                _startColor = _criticalColor;
                _text.SetText("{0}!", amount);
                transform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                _startColor = _normalColor;
                transform.localScale = Vector3.one;
            }

            _text.color = _startColor;
            _elapsedTime = 0f;
            _moveDirection = new Vector3(
                Random.Range(-_spreadRange, _spreadRange),
                _floatSpeed,
                0f
            );
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;
            float progress = _elapsedTime / _lifetime;

            // Move upward with spread
            transform.position += _moveDirection * Time.deltaTime;

            // Fade out
            Color color = _startColor;
            color.a = 1f - progress;
            _text.color = color;

            // Scale down slightly
            float scale = 1f - (progress * 0.3f);
            transform.localScale = Vector3.one * scale;

            if (_elapsedTime >= _lifetime)
            {
                ReturnToPool();
            }
        }

        public void OnGetFromPool()
        {
            _elapsedTime = 0f;
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
        }

        private void ReturnToPool()
        {
            _pool?.Release(this);
        }
    }
}
```

---

## 4. UI Input Handling

### 4.1 Multi-Input Support

```csharp
// Support keyboard, gamepad, and touch navigation

// 1. Use EventSystem with InputSystemUIInputModule
// 2. Set first selected element for gamepad/keyboard navigation
// 3. Use Navigation: Explicit for precise control, Automatic for simple cases
// 4. Ensure all interactive elements have proper Navigation settings
// 5. Use Selectable.Select() to set initial focus when opening menus

// Example: Setting initial focus
protected override void OnShow()
{
    base.OnShow();

    // Set first button as selected for gamepad/keyboard
    if (_firstButton != null)
    {
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_firstButton.gameObject);
    }
}
```

### 4.2 Pause Menu Pattern

```csharp
// ============================================================================
// PauseScreen.cs — Pause menu with resume, settings, and quit
// ============================================================================
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameName.UI.Screens
{
    /// <summary>
    /// Pause menu screen. Handles time scale and input context switching.
    /// </summary>
    public class PauseScreen : UIScreen
    {
        [Header("Input")]
        [SerializeField] private InputActionReference _pauseAction;

        [Header("Events")]
        [SerializeField] private VoidEventChannel _onGamePaused;
        [SerializeField] private VoidEventChannel _onGameResumed;

        private bool _isPaused;

        private void OnEnable()
        {
            if (_pauseAction != null)
            {
                _pauseAction.action.Enable();
                _pauseAction.action.performed += OnPausePerformed;
            }
        }

        private void OnDisable()
        {
            if (_pauseAction != null)
            {
                _pauseAction.action.performed -= OnPausePerformed;
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            if (_isPaused) Resume();
            else Pause();
        }

        /// <summary>Pauses the game and shows the pause menu.</summary>
        public void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            Show();
            _onGamePaused?.RaiseEvent();
        }

        /// <summary>Resumes the game and hides the pause menu.</summary>
        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            Hide();
            _onGameResumed?.RaiseEvent();
        }

        /// <summary>Handles the quit button.</summary>
        public void OnQuitPressed()
        {
            Time.timeScale = 1f;
            // Load main menu via SceneLoader
        }

        protected override void OnHide()
        {
            base.OnHide();
            // Ensure time scale is restored if hidden externally
            if (_isPaused)
            {
                Time.timeScale = 1f;
                _isPaused = false;
            }
        }
    }
}
```

---

## 5. UI Best Practices Summary

| Practice | Details |
|----------|---------|
| Use `TMP_Text.SetText()` with format params | Avoids string allocation |
| Disable Raycast Target on labels/images | Reduces EventSystem overhead |
| Use Canvas Groups for visibility | More efficient than toggling individual elements |
| Pool floating text and popups | Never Instantiate/Destroy in gameplay |
| Separate static and dynamic canvases | Prevents full-canvas rebatch |
| Always set first selected for gamepad | Ensures controller/keyboard navigation works |
| Use Localized Strings for all text | No hardcoded strings |
| Use anchors for responsive layout | UI adapts to screen size |
| Test on multiple aspect ratios | 16:9, 18:9, 20:9, 4:3 |
| Keep UI hierarchy shallow | Deep hierarchies impact rebuild performance |
