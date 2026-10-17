# GameScene UI revision — 2026-10-17

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

Final verification results are appended after the last authored scene revision.
