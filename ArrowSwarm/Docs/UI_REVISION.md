# GameScene UI revision — 2026-10-17 / 18

Baseline: `b9b28bd`. User approved autonomous implementation and local phase commits.

## R00 — scope and protection

- Use the saved, working GameScene as the visual source for every gameplay mode.
- Preserve Defense scene, shared HUD/overlay prefabs, generator, combat objects, tuning and saved progress.
- Shapes and Color Shift receive isolated copies of its actual canvases, including scene overrides, fonts, themed graphics, zoom slider and popup artwork.
- Freeze remains specific to Defense. Color Shift adds a wide swipe track using the existing glossy panel artwork.
- All selector/menu words become localized TMP text. Retain decorative illustrations and the Arrow Swarm brand logo as artwork.
- Actual visible button graphics receive the project's UIButtonFeedback squeeze/bounce and Unity ColorTint. Popup transitions use UIPopupAnimator.

## R01 — localized menu and selector

Textless illustration plates and separately authored visible controls; TMP labels use the game's Fazo font and existing Noto fallbacks. Selection updates exactly one localized selected state. Add keys in all ten dictionaries, maintain no-wrap sizing, retain existing menu navigation. Commit after visual and interaction checks.

## R02 — gameplay shell and puzzle adapters

Capture GameScene canvas copies in dedicated puzzle shell prefabs. Strip Defense-only controllers before binding puzzle events; preserve visual components, layout and zoom slider callbacks. Route Pause/Resume/Retry/Levels/Menu, win/loss stars, hint prompt and rewarded continuation to PuzzleSession. Preserve authored scales during button, heart and popup animations; cancel transient effects on retry or scene exit.

Use the same rounded map card and light/dark theme, fit each puzzle between actual HUD boundaries, and use the existing filled triangle/stroke proportions. Logical level colors stay intact; dark palette presentation must transform matching arrows and walls identically. Authoring commands must reproduce this design rather than the old template HUD. Commit the verified runtime and scenes.

## R03 — verification and evidence

- Compile and repeat pure rule/save verification and all ten real pooled puzzle solutions in non-saving preview.
- Exercise visible pointer feedback, pause/resume, retry during shot/rotation, immediate retry during heart break, hint prompt and result actions.
- Compare top/bottom sprites to saved Defense; check one zoom slider, no freeze/Defense controllers or missing references.
- Inspect actual Unity renders for menu, selector, new gameplay, popups, light/dark themes and portrait aspect ratios. Change language without writing the saved preference.
- Re-enter Defense and verify generated board and enemy flow. Compare protected files to baseline; remove incidental font-cache/editor files from the change.
- Keep reports/screenshots under Captures/UIRevision and Captures/ThreeModes. Record actual results and remaining device-only limits, then commit evidence.

## Results

- R00 committed as `450bfbd`; R01 as `6a9ad2e`; R02 as `5f023d5`. R03 records the verification tools and evidence.
- Unity compilation succeeded. Final fresh Defense session logged zero console errors. One earlier capture attempt produced a screenshot-write error and a Unity PlayerLoop capture-tool error; subsequent captures and the fresh regression succeeded. No application exception remained.
- `UIRevisionPlaytestRunner.Begin()` passed 183 checks across both modes: original panel sprites, exactly one zoom slider, controller isolation, actual held-pointer scale/tint/release, Pause/Resume, retry during moving arrows/rotating frame, heart-break cancellation, localized counters/results, three-shot loss, hint modal and deterministic failed/successful rewards/continuation. Original ad service, inventory, preview state, data, saved JSON, saved language and auto-save policy were restored. Reward tests use an injected editor-only service; no real ad request is made by the suite.
- All ten catalogs passed real pooled movement/frame playthroughs with three lives and no preview save writes. All ten authored levels also passed the pure solver, along with save compatibility and obstruction/rotation/deadlock checks. Report: `Captures/ThreeModes/runtime-verification.txt`.
- Main menu/selector inspected in Unity. All active selector words and main-menu control labels are TMP; Turkish labels fit on one line with the same game font. Every one of the three choices produced exactly one selected label, with data restored afterward. All ten localization dictionaries have the same 98 keys and valid format placeholders. Japanese fallback rendering was also checked.
- Shapes, Color Shift, pause, win/loss and empty-hint popups were visually inspected. Light/dark rendering uses the original UI theme; readable dark board colors preserve logical palette IDs and transform arrows/walls identically.
- Actual 1080×1920, 1080×2400 and 1536×2048 views were checked. Measured complete board bounds stayed between top HUD and the Color Shift rotation track, within horizontal screen bounds. Captures show the final swipe track and gameplay framing.
- MainMenuScene, ShapesScene and ColorShiftScene have zero missing scripts; required puzzle references are assigned. Static review found zero dangling local scene references, no duplicate project GUIDs and one zoom slider in each puzzle scene.
- Returned through the actual puzzle Pause → Main Menu button and Main Menu Play route. Fresh Defense regression: original GameScene, Playing state, valid generated level, 56 arrows, two spawned/active enemies, no PuzzleSession. The screenshot was frozen briefly for capture; the runtime clock was restored before stopping Play Mode.
- GameScene, MapTestScene, original Canvas_HUD/Canvas_Overlay prefabs, Defense Arrow/Mob scripts, LevelGenerator and GameConfig compare identically to baseline `b9b28bd`. The shared HUDFeedbackEffects change preserves authored baselines and adds safe reset cancellation; the revised UI suite covers it.
- No Android device build, physical touch test, production ad delivery or live leaderboard submission was performed.

## Reproducing the revision

- **Arrow Swarm → Puzzles → Apply GameScene UI To Puzzle Scenes** captures the saved Defense canvas appearance and regenerates isolated puzzle UI. Save the open scene first; this deliberately replaces the generated puzzle canvases.
- Open MainMenuScene, then **Upgrade Loaded Menu Localization And Feedback**; save the scene afterward.
- Launch a non-saving puzzle preview from MapSceneController, then **Verify Revised UI (Play Mode Preview)**. The report is `Captures/UIRevision/ui-verification.txt`.
- **Playtest All Levels (Play Mode)** runs the ten real solutions. Exit Play Mode after testing. The editor remains on MainMenuScene after this task.
