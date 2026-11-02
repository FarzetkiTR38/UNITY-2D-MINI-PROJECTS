# Boards06 and08 — Defense popup UI only

User authorization: 2026-11-02; implement board06 Pause only, skip07 entirely, then implement08 Tip/Freeze modals. Gameplay/HUD/paths/enemies/arrows/rules remain unchanged. No approval stop between06 and08 (explicitly authorized).

- [x] Read project instructions and architecture; inspect Revision6 reference06/08.
- [x] Snapshot initial dirty files, GameScene, fonts and ProjectSettings privately.
- [x] Native before captures and original action bindings recorded.
- [x] Pause: reference frame/crest/clouds, localized dimensional title, existing toggles and four original actions; native Light/Dark checks; commit.
- [x] Tip/Freeze: shared compact hero atlas, existing localized text/actions, reference composition; native Light/Dark checks; commit.
- [ ] Native controls, localization, two aspect ratios, source/gameplay preservation checks; restore private state; before/after preview; delivery commit.

Reference: Captures/Revision6/Concepts/06-defense-game-pause.png and08-defense-skills.png. Original scripts are retained; appearance is authored in GameScene. Use existing shared sprites/materials/fonts; generate only the missing bulb/snowflake hero atlas. Next numbered board waits for review.

Phase06: real native Pause/Resume, both setting toggles, 10-language typography and both themes pass in PauseReview/verification.txt. Original overlay prefab remains linked and untouched; scene-specific copies of its existing controls are bound to the original PauseMenuUI handlers. Native inspection corrected inherited-prefab reparenting and toggle label/knob overlap. Studio/HUD/map/enemy/arrow behavior is unchanged.

Phase08: hint/freeze cards integrated with text-free shared hero atlas, existing film/play icon and live localized TMP. Original reward handlers are untouched. After/verification.txt and Responsive/verification.txt pass; successful and failed rewards, Close, original Retry/Levels/Home routes and actual press shrink are verified. Final layout uses a narrower skill card, separated icon/text slots and a bright Dark close button.
