# UI redesign rollout - sequential approval

Baseline: `ce22ef28`. Current checkpoint: **01 Boot and Home implemented and verified; awaiting user approval.** Do not start board 02 until the user explicitly approves this stage.

## Rules for every board

- Back up before the first integration; preserve the working game and pre-existing local changes.
- Integrate one numbered concept board, capture actual Unity Light/Dark results, commit it, then request approval.
- Use live TMP for all wording, including the ARROW / SWARM wordmark, percentages, studio credit, status, tips and labels. Keep existing localization keys and language fallbacks.
- Separate illustrations, blank sliced plates, icons and text. Reuse shared artwork and materials across screens. Never import a full-screen mockup as a functioning UI.
- Author appearance in scenes, materials and Inspector fields. Existing controllers own startup, navigation, themes, click feedback and gameplay.
- Test through Unity MCP. Preserve player progress, preferences, providers, debug state and existing unfinished work. Keep verification isolated from live ads and leaderboard services.

## Verified backup

`C:/Users/Farzetki/Desktop/GitHub/ArrowSwarm-Backups/UI-Rollout-Before-01-20261027-214158`

- `project-source-and-local-changes.zip`: 4,169 source/art/local files, about 1,016 MiB; ZIP CRC passed. Includes actual Git LFS image content and local/ignored source. Regenerable Library, Temp, Logs and build output are excluded.
- `history.bundle`: about 116 MiB; Git bundle verification passed, history through `ce22ef28ee96fb38e34559084fa1b05ffe598270`.
- `manifest.json`: archived paths, sizes and SHA256 hashes. `RESTORE.txt`: recovery instructions into a new empty directory.
- Private `Session/`: exact original eleven dirty files, five preferences, Editor state and MCP helpers. Never stage or publish it.
- Private resume pointers: `%TEMP%/ArrowSwarm-UIRollout-path.txt` and `%TEMP%/ArrowSwarm-UIRollout-backup-path.txt`.

## Stage 01 completed

- [x] Back up and verify original state.
- [x] Inspect existing Boot, Home, localization, theme and click-feedback bindings.
- [x] Generate text-free shared crest, orbit, twelve-sprite atlas, footer and gold star.
- [x] Build shared Editor authoring helpers, compact imports and theme materials.
- [x] Apply Boot while retaining the real BootLoader / BootLoadingUI completion flow.
- [x] Apply Home while retaining MainMenuUI, GameModeSelectUI, ModeButtonVisual and existing subpanels.
- [x] Compile and verify actual startup, native pointer navigation, mode icons, all three Play routes, twenty language/theme combinations and safe interactive bounds.
- [x] Inspect four Light/Dark captures at 1080x1920 and repeat layout/capture verification at 1080x2400.
- [x] Verify all 880 existing later-panel object documents remain unchanged.
- [x] Restore the original five preferences, eleven dirty files, original Game View selection, Boot Edit scene and run-in-background setting.
- [x] Prepare the actual before/after browser preview and stage-01 commit.
- [ ] **User approval of stage 01. Board 02 is not started.**

### Implementation

Only BootScene and the Home composition in MainMenuScene are redesigned. Their visual controls are serialized in the scenes. New C# helpers and verification code live under `Scripts/UI/Editor`; the only new player-side code is a small UI background shader. Existing gameplay scripts are unchanged.

The body uses the existing Fazo_Font (Roboto Bold); the two-line TMP wordmark uses existing LilitaOne_SDF. The same crest Sprite is shared between Boot and Home; the avatar shares its texture. Navigation cards share a single plate Sprite. Mode icons, navigation icons, play triangle and tip bulb share one twelve-sprite atlas. Footer and gold star are shared by both scenes. No new font files or runtime image loaders were added.

Five source PNGs total 6,426,922 bytes. Android imports use ASTC 6x6, no mipmaps, Read/Write disabled. Imported texture dimensions produce an estimated 1,704,016-byte ASTC block payload across the five textures. This is a texture estimate, not a measured APK/download size; no device build was performed. Native Editor texture-memory measurements are recorded separately and include Editor/emulation overhead.

### Native evidence and boundaries

`Captures/UIRollout/Phase01/Checks/verification.txt`: PASS, 1,678 successful assertions, zero failed assertions. Real pointer input verifies press feedback, mode selection, Levels/Leaderboard/Settings/Profile opening and dismissal, and Play entering Defense GameScene, ShapesScene and ColorShiftScene. Defense uses scoped debug level101 to verify routing; this is not a claim of completing its campaign. No stage completes a level or awards a reward.

`Responsive1080x2400/verification.txt`: PASS, 1,220 successful assertions, zero failed assertions. This second run repeats actual Boot/Home captures, typography, bindings, twenty language/theme combinations and safe bounds; navigation routes are covered by the primary run. These are Editor Game View checks, not physical notch/device certification.

Both runs restore cloned player data, five preferences, original cloud factory/ad provider, language, random state, lives, launch flags and native input devices/settings. External service operation counters remain zero. Original pre-task preferences and all eleven initial dirty-file byte hashes were checked again after leaving Play.

`preserved-panels.json`: all880 original source documents match baseline by fileID, ignoring only document order and whitespace: Mode Select88, Settings208, Profile184, Levels109, Leaderboard291. These panels retain their old design until their own board is approved.

`asset-reuse-report.json` and `native-import-measurements.json` record actual importer state and reuse. `Before/` uses documented earlier native captures whose source scene YAML remained unchanged before integration; provenance is recorded in `before-provenance.json`.

### Preview and rebuilding

Open `Captures/UIRollout/Phase01/index.html` directly, or serve the `Captures` directory and visit `/UIRollout/Phase01/index.html`. Use the self-contained local HTML directly. The previous localhost8170 server stopped answering; automatic approval review blocked restarting it during the revision pass. Actual native PNGs remain available, and `Revisions/progress.html` adds eight Light/Dark fill checkpoints. It offers Home/Boot Light/Dark, before/after, click-to-enlarge, full-size/fit and Escape close.

To rebuild deliberately in Edit Mode: `UiRollout01Kit.ImportAssets()` first; then open BootScene and invoke `UiRollout01BootAuthoring.Apply()`, save it; open MainMenuScene and invoke `UiRollout01HomeAuthoring.Apply()`, save it. Helpers are Editor-only and do not rebuild the player UI at startup. Avoid reauthoring an already reviewed scene without a reason.

To recheck: enter Play from Boot, wait for idle MainMenu before opening the real leaderboard, then invoke `UiRollout01Verification.Begin()`. `CaptureAt(folder)` repeats capture/layout checks at the current Game View size. Do not edit scripts during a running fixture. The verifier uses scoped preview services and restores them on success, error, Stop or reload.

## Board order and resume gate

01 Boot/Home -> 02 mode selection/settings -> 03 profile/leaderboard -> 04 Defense/Shapes levels -> 05 Color Shift levels/transition -> 06 Defense game/pause -> 07 Defense outcomes -> 08 Defense skills -> 09 tutorial -> 10 Shapes game/pause -> 11 Shapes outcomes -> 12 Shapes hint/Color Shift game -> 13 Color Shift pause/win -> 14 Color Shift lose/hint -> 15 map preview/pause.

If asked to resume without stage-01 approval, show this completed preview and request the approval already required by the user. After explicit approval, begin board02 using these same shared resources where suitable. Each later board gets its own implementation notes, native evidence, commit and approval checkpoint.

Board01 requested refinements are complete; see UI_ROLLOUT_01_REVISIONS.md for three revision phases, current validation and the offline preview note. The single-line FARZETKI GAMES credit, full visible cloud footer and rounded inset loading fill supersede the first board01 preview. Await approval before board02.
