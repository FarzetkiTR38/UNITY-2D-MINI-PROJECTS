# Arrow Swarm — three-mode implementation

Approved scope: user instructions dated 17 October 2026. This is the single implementation and handoff document. Commit each completed phase; keep verification evidence and remaining work here. Never describe an unverified phase as complete.

## Product contract

- Modes: **Defense**, **Shapes**, **Color Shift**. Existing Defense gameplay, generator, damage, mobs, tutorial and saved progression remain compatible.
- Shapes / Color Shift: clear all arrows, three lives freshly initialized per attempt, blocked shots advance to impact then reverse and cost one life. No enemies, timer or freeze power.
- Independent selected/current/highest levels and per-level best stars for each mode. Shared star total and leaderboard. Winning awards remaining lives (1–3); replay replaces only a lower personal best, preventing repeated-star farming and preserving existing Defense scoring.
- Two new five-level MVP campaigns. Show a campaign-complete message after level five; never accidentally launch Defense or index beyond the catalog.
- Shapes: detailed recognizable silhouettes, harmonious image-aware colors, varied bent paths and dependencies, no unsolvable facing cycles. Suggested subjects: heart, penguin, rabbit head, butterfly, rocket. Include harder dense examples.
- Shapes editor: clean-background image import, threshold/alpha silhouette extraction, color sampling, deterministic candidate generation, solvability validation, manual path/head/color editing, undo and save. User can import personally or provide a reference for an agent to author. This is an offline Unity authoring tool, not a runtime AI service or arbitrary-photo recognition promise.
- Color Shift: exactly two colors per MVP level, opposite walls same color; stationary arrows, only frame rotates in 90-degree steps. Wide horizontal swipe bar (left/right); one accepted gesture consumes one turn, including reverse turns. Turns set per level. Wall mismatch uses the same impact/reverse/life penalty as an arrow collision. At zero turns continue legal shots; lose only when no remaining valid clearing sequence exists. Generated levels must have a proven solution within the budget.
- Reuse existing zoom/pan where compatible. Puzzle input must respect UI blocking, zoom gestures, pause and animation locks.
- Retain hint, pause, retry, victory/defeat, star presentation, rewarded continuation and ad pacing where relevant; freeze is Defense-only. Provide concise first-level guidance for both new modes.
- Main menu follows supplied cream/glossy-blue reference: level and shared stars at top, large mascot logo, Play, mode selector showing active mode, Levels, Leaderboard, Settings. Values must be live text, never a screenshot of sample numbers.
- Mode modal follows supplied three-card reference: dimmed background, title, close, Defense / Shapes / Color Shift cards, Select / Selected states. Selecting changes active campaign without starting it; Play starts the selected mode, Levels opens its progression.
- Visuals are scene/prefab/Inspector authored, not runtime restyling. Preserve existing subpanels/profile/settings functionality and safe-area support.
- Add mode/level preview to MapSceneController workflow; preview never changes real player progression or awards stars.

## Architecture and protection boundaries

- New code under Assets/_Project/Scripts/Modes (data/rules/session), Puzzle (view/input/movement), UI and Editor. Use dedicated namespaces, events for cross-system notifications, pooled arrows and ScriptableObject tuning/catalog assets.
- Keep Defense's LevelGenerator and its solver unchanged. Avoid changing Arrow/Mob combat behavior to support puzzle mode. Use independent puzzle rules and views, sharing only safe utilities, camera facilities and services.
- Use separate ShapesScene and ColorShiftScene with a dedicated puzzle session. Legacy persistent managers must not generate, award, listen or spawn enemies in puzzle scenes. Scene names must not match legacy Map* scene routing.
- Route mode-aware navigation at small explicit entry points. Legacy StartGame/Restart/Next semantics remain Defense-compatible. Return from a puzzle must restore a valid menu/Defense state and clear animations/events/pools.
- Preserve old PlayerData fields and save key. Add nullable-safe mode progress defaults; old JSON and backup loads work. Defense stars remain in their existing list; new modes add separate lists. Shared total sums best records across all modes. Do not delete/reset actual PlayerPrefs during verification.
- Puzzle level assets are intentionally authored, per explicit user request; previous procedural-only project guidance continues to apply to Defense. Shapes generation is offline and saves validated assets.
- Data definitions: mode id, campaign level id/title, grid bounds, ordered orthogonal arrow paths/head direction/color, frame palette/orientation/budget, optional image/source metadata. Stable identifiers prevent collisions with Defense levels.
- Pure rules: occupancy on points AND edges, self-path constraints, first obstacle/exit calculation, legal removal, dependency validation; Color Shift solver includes remaining arrows, orientation parity and remaining turns. Return a solution witness and minimum required turns, not just a boolean.
- Configuration owns speeds, stroke widths, palettes, interaction thresholds, animation timings, lives and other new tunables.

## Phase sequence and acceptance gates

### P00 — baseline and this plan
- Record clean Git state, HEAD, active Unity scene/editor state and existing console errors.
- Record Defense code hashes and scene/prefab references. Commit this document first.
- Gate: no gameplay or saved-data changes.

### P01 — mode identity and compatible progression
- Add explicit stable mode enum, selection service and campaign records. Defensive initialization for old saves.
- Add mode-specific best-star/unlock/current accessors and aggregate total without changing Defense callers.
- Shared total feeds existing leaderboard sync. Verify independent level 1 records, replay best-only behavior, old-save defaults, backup preservation.
- Commit after compile and focused checks.

### P02 — pure puzzle data and solver
- Add serialized level/catalog/config assets and point/edge occupancy rules.
- Add dependency solver and Color Shift state solver with solution witness and budget validation.
- Validate missing/duplicate/nonadjacent/off-board/overlapping paths, head direction, invalid colors, deadlocks and insufficient budgets.
- Gate: tests/check harness covers blocked shot, removal, both frame orientations, no-turn win/loss and no duplicate reward.

### P03 — supplied UI assets and main-menu layout
- Import reference images non-destructively; slice reusable artwork into project sprites, preserving alpha. Create matching clean assets only where needed for live controls.
- Author menu in scene/Inspector with separate buttons/hit regions/dynamic labels; retain existing settings, levels, leaderboard/profile panel wiring.
- Inspect portrait render against reference, including top chips, logo scale, spacing, buttons and cream background.
- Commit asset sources, generated subassets, importer settings and saved scene.

### P04 — mode modal and mode-aware navigation
- Author overlay and three cards, selectable state, dismissal, input blocking and selected-mode label/icon.
- Play launches selected scene. Levels displays chosen campaign's unlocks/stars; Defense list retains original behavior.
- Persist selected mode and return consistently; avoid double listeners or game start from selection alone.
- Gate: all three selections, close/back, settings/leaderboard and menu return visually/runtime checked. Commit.

### P05 — isolated puzzle runtime and Shapes gameplay
- Pool puzzle arrows, draw bent colored strokes and arrowheads, implement nearest valid touch and UI/gesture blocking.
- Snake movement follows existing path and head ray; occupied cells release at accepted launch. Impact/reverse leaves original occupancy, costs exactly one life and serializes mutations while moving.
- Add lifecycle: load, spawn, playing, pause, impact, exit, win/loss, retry/menu. No mob/path combat subscriptions.
- Fit board and use existing zoom support. Gate: win/lose/retry/pause and scene switching without Defense regressions. Commit.

### P06 — Color Shift frame and gestures
- Author two-color opposite walls, animate frame-only rotation and accept deliberate horizontal swipes over the bar.
- Match wall at actual head exit, reject blocked/mismatched shots with impact, lock input during rotation/impact and decrement once per gesture.
- Re-evaluate solvability at zero turns, allow all remaining legal clearing moves and fail only when stuck. Handle final-arrow and exhausted-budget order correctly.
- Gate: left/right/parity, both mismatches, zero-turn clearing/deadlock, pause during animation. Commit.

### P07 — puzzle HUD, results, hints and relevant services
- Three lives, arrows remaining, level/title/mode, rotation counter/bar, pause/retry/menu, victory stars and campaign completion.
- Record results once, award best stars, unlock only this mode, refresh shared total/leaderboard through existing service.
- Hint chooses a safe action from a valid solution (may suggest rotation); use shared hint inventory. No freeze in new modes.
- Relevant ad continuation/pacing and first-level instructions; cancel stale ad/animation callbacks on scene changes.
- Gate: continue/retry cannot duplicate stars, no forced ad on editor preview, no progression writes in preview. Commit.

### P08 — Shapes image authoring and manual editor
- EditorWindow with source texture, threshold/background/alpha controls, resolution, seed and palette settings; visualize sampled mask.
- Generate connected multi-segment paths, varied directions and removable dependency layers; accept only validated candidates with readable silhouette.
- Select/edit paths, head direction and color, add/delete arrows, undo/redo, display validation and solution, save/load level asset, preview in dedicated scene.
- Gate: import a clean sample, generate, edit, save/reload and prove solution. Commit.

### P09 — ten polished MVP levels
- Five Shapes with recognizable subjects, image-aware palettes and increasing complexity; five Color Shift levels with distinct two-color palettes, varied bent arrows/dependencies and configured turn budgets.
- Include advanced Shapes (rabbit/butterfly/penguin details) rather than only simple outlines. Inspect actual gameplay renders at phone scale.
- Every level has a validated clearing sequence; Color Shift witness fits budget. Catalogs ordered and no missing assets.
- Gate: render all levels, play through representative easy/hard and validate every solution. Commit.

### P10 — MapSceneController preview integration
- Add explicit mode selector and puzzle level index/catalog preview. Preserve existing Defense map selector/range/tutorial controls.
- Editor preview shows actual arrows and frame; play preview uses isolated puzzle runtime without modifying chosen live campaign or save.
- Previous/next/restart/fit operate on selected mode, restored on return.
- Gate: all three modes preview, chosen levels and Defense map controls still work. Commit.

### P11 — regression and delivery
- Unity compile, focused rules/save checks, missing-component/reference scan, build scene registration.
- Exercise Boot → menu → all modes → pause/retry/results → menu → Defense, and independent stars/progress with aggregate leaderboard total.
- Check portrait layout/safe areas and capture main menu/modal/each campaign screenshots via Unity MCP.
- Compare protected Defense files to baseline. Review Git diff for unintended edits, commit final verified fixes and document exact checks/limitations.

## Status and evidence

- P00 complete: clean baseline HEAD `786aeea`; Unity 6000.3.18f1, MapTestScene, edit mode, scene not dirty, zero console errors. Plan committed as `4c0090f`.
- P01 complete (`10ee57b`): additive old-save-compatible records; in-memory old JSON / independent unlock / best-only stars / aggregate 7 / JSON round-trip checks passed without writing PlayerPrefs.
- P02 complete (`47dc59f`): pure solver/data and repeatable verification command. Checks passed for zero-turn matches/mismatches, obstacle release, facing deadlocks and invalid overlaps.
- P03 artwork imported (`b5135d5`); main menu authored together with P04 (`1da0cf0`). Original settings/profile/leaderboard/level panels retained. Actual Unity main-menu screenshot inspected. Live chips showed Lv.102 and 26 stars from existing save. Modal refinement replaces rectangular source cuts with an alpha-transparent panel and live selection labels.
- P05 core runtime (`17a0e47`), P06 swipe control (`06a3657`), P07 scenes/HUD/services (`e5aa2ab`) implemented. Initial runtime checks exposed and fixed an empty prefab reference; subsequent scene startup had zero errors. Camera zoom reuses existing implementation through an additive bounds initializer. New scenes use an authored cream backdrop to avoid dark-theme contrast failures.
- P08 editor (`d89a563`) implemented: image import, sampling, deterministic generation, validation, manual point/color editing, reverse/add/delete, Undo, save and Scene View preview.
- P09 catalogs (`6745045`) implemented and all ten independently solved. Shapes arrow counts: 58, 60, 48, 58, 37. Color Shift arrow counts: 19, 20, 25, 28, 30; minimum turns 1, 2, 3, 4, 3 within configured budgets 2, 3, 4, 5, 6. Initial catalogs were generated before scene assembly to provide valid serialized scene references; their dedicated content commit remains separate.
- P10 (`2618d62`) implemented: mode selection and scene/play previews in MapSceneController; default Defense path remains unchanged.
- P11 verified: both five-level campaigns completed through actual pooled movement and frame coroutines, using a cloned accelerated test configuration. All ten won with three lives; PlayerData JSON remained byte-identical across every preview win. Final report is `Captures/ThreeModes/runtime-verification.txt`.
- All ten level screenshots inspected at 1080×1920. Main menu and final modal inspected after fixing Unity's automatic transparent sprite cropping. Three new/edited scenes have zero missing scripts and all required puzzle references assigned.
- Runtime negative checks passed: wrong wall advances/reverses and costs exactly one life while retaining all 19 arrows; third wrong attempt loses; Retry restores three lives and the whole board; a duplicate rotation during animation is ignored; reversing consumes a turn; legal shots at zero turns continue, then an immovable board loses with remaining lives intact.
- Menu checks passed for all three mode cards and exactly one Selected label; Defense showed 20 paginated level buttons, each new campaign exactly five. These checks temporarily disabled auto-save and restored the original mode; they did not write test scores to cloud.
- MapTestScene routing check passed for Shapes level 4 with Preview=true, 58 arrows and zero Defense mobs. MapTestScene's saved asset remains unchanged. New cameras render only PuzzleBoard layer 8, preventing persistent Defense board visuals from leaking into puzzles.
- Defense re-entry passed in the original GameScene: generated data valid, 57 arrows, active enemy spawning and Playing state; no PuzzleSession remained and no console errors were logged. Defense generator, combat Arrow/Mob files, GameConfig, GameScene and MapTestScene compare identically to baseline `786aeea`.
- Clean-image authoring check passed on an in-memory red silhouette with a white background: image sampling produced 29 arrows and an independently valid clearing solution, without saving a test asset or changing player data.
- Final checks: Unity compilation and repeatable rule/save verification passed with zero errors; existing unrelated compiler warnings are unchanged. No Android device build, physical touch test, production ad delivery or live UGS ranking submission was performed. Existing UGS level-first/star-tie-break ranking formula is preserved; its star input now includes all three campaigns. No server migration or new leaderboard is required.

## Editor usage

- Open `MapTestScene`, select `MapSceneController`, choose Defense / Shapes / Color Shift under **Game Mode Preview**. Defense retains its existing map controls. Puzzle modes expose level 1–5, Scene View rendering and non-saving Play preview. Stop Play Mode to return to the map scene.
- **Arrow Swarm → Puzzles → Shapes Image and Path Editor** opens image authoring. Import a clean PNG/JPG, choose grid/seed/background tolerance, generate, click an arrow to select it, edit its points/color or reverse head/tail, validate, save. New saved levels must be added to ShapesCatalog explicitly; invalid puzzles cannot be saved through this editor.
- **Verify Rules and Save Compatibility** runs pure focused regression checks. **Validate All Levels** proves all authored assets. **Playtest All Levels (Play Mode)** drives real pooled movement in preview mode and writes its report/screenshots under `Captures/ThreeModes`; it must leave PlayerData unchanged.
- Runtime styling is serialized in MainMenuScene, ShapesScene, ColorShiftScene and PuzzleArrow.prefab. PuzzleConfig controls speed, stroke/head sizes, pool size, gesture thresholds and three-life defaults. Each Color Shift level owns its palette and turn budget.

## Continuation protocol

The gameplay UI and menu localization described in the earlier P07/P11 history were revised on October 17–18. See `Docs/UI_REVISION.md` for the current GameScene-derived canvases, localized TMP selector, feedback, verification results and regeneration commands. The former generic puzzle HUD and cream backdrop are superseded.

Read this document and Git log first. Inspect working tree before changes. Continue the first incomplete phase, preserve existing work, record evidence and commit finished phases individually. Never reset user data, overwrite dirty scenes or claim visual equivalence without viewing Unity renders. User authorized implementation and per-phase local commits; no further questions/approval required. Do not push or publish externally unless separately requested.
