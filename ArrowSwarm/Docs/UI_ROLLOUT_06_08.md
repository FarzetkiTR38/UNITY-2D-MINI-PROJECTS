# Boards06 and08 — Defense popup UI only

User authorization: 2026-11-02; implement board06 Pause only, skip07 entirely, then implement08 Tip/Freeze modals. Gameplay/HUD/paths/enemies/arrows/rules remain unchanged. No approval stop between06 and08 (explicitly authorized).

- [x] Read project instructions and architecture; inspect Revision6 reference06/08.
- [x] Snapshot initial dirty files, GameScene, fonts and ProjectSettings privately.
- [x] Native before captures and original action bindings recorded.
- [x] Pause: reference frame/crest/clouds, localized dimensional title, existing toggles and four original actions; native Light/Dark checks; commit.
- [x] Tip/Freeze: shared compact hero atlas, existing localized text/actions, reference composition; native Light/Dark checks; commit.
- [x] Native controls, localization, two aspect ratios, source/gameplay preservation checks; restore private state; before/after preview; delivery commit.

Reference: Captures/Revision6/Concepts/06-defense-game-pause.png and08-defense-skills.png. Original scripts are retained; appearance is authored in GameScene. Use existing shared sprites/materials/fonts; generate only the missing bulb/snowflake hero atlas. Next numbered board waits for review.

Phase06: real native Pause/Resume, both setting toggles, 10-language typography and both themes pass in PauseReview/verification.txt. Original overlay prefab remains linked and untouched; scene-specific copies of its existing controls are bound to the original PauseMenuUI handlers. Native inspection corrected inherited-prefab reparenting and toggle label/knob overlap. Studio/HUD/map/enemy/arrow behavior is unchanged.

Phase08: hint/freeze cards integrated with text-free shared hero atlas, existing film/play icon and live localized TMP. Original reward handlers are untouched. After/verification.txt and Responsive/verification.txt pass; successful and failed rewards, Close, original Retry/Levels/Home routes and actual press shrink are verified. Final layout uses a narrower skill card, separated icon/text slots and a bright Dark close button.

## Final delivery — 2026-11-03

- Preview: `Captures/UIRollout/Phase06_08/index.html`; actual comparison sheets `comparison-light.png` and `comparison-dark.png`.
- Final native suites: `After/verification.txt` at1080x1920 and `Responsive/verification.txt` at1080x2400; both PASS, zero failed checks. Both themes, all10 supported languages, real pause/resume, sound/vibration controls, Retry/Levels/Home routes, real press shrink, failed/successful scoped rewards and Close are covered. Pixel sizes can be checked directly from PNG headers. These are Editor Game View tests, not physical-device or live SDK certification.
- Runtime production scripts, shared overlay prefab, HUD/map/arrow/enemy behavior and07 outcomes remain unchanged. `source-preservation.json` verifies42 unrelated original overlay overrides and323 owned source objects; incidental serialized camera defaults and formatting noise were restored. Shared prefab matches the starting commit. `protected-files-restored.json` verifies exact initial dirty/font/ProjectSettings bytes; `editor-restoration.txt` verifies original Game View and Edit MainMenu.
- During visual iteration inherited prefab reparenting failed; corrected by cloning existing controls into scene-only cards and binding the original controller handlers. A route fixture race after Retry was fixed by keeping one native pointer across routes and waiting for spawning to finish; no product input/gameplay code was changed.
- Real reward callbacks use a temporary test ad provider. Failed ads grant nothing; success grants exactly one charge to cloned progress; original data object, five preferences, provider factory, random state, debug/config and language restore on completion. No level was completed by the fixture.
- Art: two reusable text-free atlases only; all functional copy and toggle state labels remain localized TMP. No new font or runtime renderer/controller was added. Native importer settings and texture estimates are in `ASSET_REUSE.md` and `native-imports.txt`.

## Resume / next action

Completed authorized scope:06 pause only,07 skipped,08 hint/freeze UI only. Await user review; do not start another numbered board without authorization. For deliberate reproduction, start fresh idle MainMenu Play with no leaderboard manager, call `Revision6PauseVerification.Begin0608(folder, true)` through Unity MCP. For source authoring use idle Edit GameScene and `UiRollout0608Authoring.Pause()` / `.Skills()` on the original source checkpoint; these helpers deliberately reject already-authored cards. Edit current scene/Inspector for follow-ups rather than re-running them blindly. The continuing master checklist links this file.
