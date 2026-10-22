# UI visual revision

![Before, after and themed iris](Overview.png)

The menu's visible button artwork keeps its squeeze/bounce and pressed tint. Its baked cream rectangle is clipped away with an antialiased authored mask, so the surrounding area stays clear.

The shared circular scene wipe now uses a cream arrow/star pattern, a glossy cyan/blue outer edge and a gold inner rim. Production duration remains **one second**. [Animated preview](transition-preview.gif) uses ten actual Unity screenshots from a deliberately slowed six-second QA transition, so the pattern and rim can be inspected.

## Verification
- `shader-verification.txt`: 111 real-GPU assertions passed, including five authored button materials, sampled opacity, clipping, three aspect ratios and two iris centers.
- `transition-verification.txt`: paused action and all six real scene loads passed: Main Menu → Shapes → Main Menu → Color Shift → Main Menu → Defense → Main Menu. Covered midpoints block input; completion clears the overlay, progress and coroutine. Runtime material is reused; authored material and saved progress are preserved.
- `pointer-verification.txt`: actual Input System mouse hold/exit/release/disabled checks for the five main-menu buttons. Each queued state is acknowledged by a player input update before inspecting UI feedback. Held screenshot paths are listed in the report.
- BootScene directly references the transition material through CoreManagers; Unity's dependency graph includes both material and shader. No shader compiler messages; no runtime material remains after stopping Play Mode.

## Inspector settings
- `CoreManagers` → `SceneTransitionManager`: duration and cream/blue/gold theme colors.
- `Mat_SwarmIrisWipe`: motif colors, density, opacity, spacing, size, rim widths and gloss.
- `Mat_*ButtonArtwork`: normalized silhouette bounds and corner radii. Source illustrations and TMP labels are unchanged.

## Repeat checks
Finish compilation and return fully to Edit Mode before entering Play Mode. Start the pointer/transition fixtures only after MainMenuScene is loaded and its entry transition is complete.

```csharp
ArrowSwarm.UI.Editor.MenuShaderVerification.Run("Captures/UIVisualRevision");
// Play Mode, idle MainMenuScene:
ArrowSwarm.UI.Editor.MenuPointerVerification.Begin();
ArrowSwarm.UI.Editor.MenuTransitionVerification.Begin(); // Run fixtures separately.
```

The transition fixture restores its temporary player/debug/preferences state. A separate private pre-run snapshot also preserves on-disk preferences and preexisting local font/solution edits across Play Mode shutdown.
