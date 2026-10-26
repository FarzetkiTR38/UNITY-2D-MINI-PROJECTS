# Project quality pass 4 — whole-game improvement roadmap

Baseline: `5f057df`. User authorizes a broad, autonomous quality pass: enter every game/UI flow through Unity MCP, verify Light/Dark, fix proven failures, improve visual quality and measured performance, and list valuable future features. Every coherent implementation phase gets a separate commit. Read this file first when asked to continue.

## Preserve
- Existing Defense rules, three separate campaigns, shared stars and current working progress remain protected.
- Previous quality passes 1–3 are complete; do not repeat their fixes. Read their records before selecting a finding.
- Current dirty solution/font/ad-prefab files and five player preferences are privately snapshotted. Never stage preexisting changes or publish player JSON. Restore test-owned state in finally.
- No live ads, synthetic leaderboard submissions or cloud writes in fixtures. Use scoped service adapters. Leaderboard concepts from the previous task remain preview-only until a design is chosen.
- Root owns Unity MCP and commits; agents audit/implement disjoint source areas. No script edits while runtime fixtures are active; compile and inspect console before accepting changes.
- Static visuals remain scene/material/Inspector authored. TMP/localization and existing input effects are retained. New scripts stay under 200 lines.

## Phases / resume checklist
- [x] 0. Read previous records, snapshot starting state, commit this roadmap and establish current Unity baseline (`2e0d20d`).
- [x] 1. Sweep all six scenes and visible popup families in both themes. Thirty paired screen states captured through the actual Boot-configured editor, zero missing scripts across six scenes. All ten language files parse with the same 100 keys; all 46 nonempty scene-bound keys resolve. Full navigation/input regression continues in later phases.
- [x] 2. Audit data/profile/leaderboard asynchronous behavior and implement only reproducible reliability/UX fixes, with offline before/after evidence. Local-only sync optimization passed; original startup/pending/version/reconnect suites also passed with fake providers.
- [ ] 3. Audit Defense, Shapes and Color Shift lifecycle/input/skills/pooling. Terminal outcomes and delayed Hint fixes passed; orphan celebration prefab repair/pool verification remains in progress. No speculative solver rewrite or device FPS claim.
- [x] 4. Improve confirmed visual/readability/localization issues found in the full sweep. Country native input, all ten actual profile Saves and 80 pause-title combinations passed. Final paired screen regression remains in phase 5.
- [ ] 5. Exercise real Defense, tutorial/pause/win/lose/retry/menu and all 100 puzzle levels plus representative real-input/skill/rotation/camera flows. Distinguish graphical rendering evidence from timing/performance evidence.
- [x] 6. Publish a prioritized feature list with benefit, scope and dependencies; implement small evidence-backed improvements where appropriate, leaving speculative/new mechanics clearly separate. Seven source-grounded ideas are in `PROJECT_QUALITY_PASS_4_IDEAS.md` and explicitly remain unimplemented.
- [ ] 7. Final regressions, original preference/file preservation, screenshots and overview. Record finished commits and next resume point explicitly.

## Entry points to verify
Boot/startup; main Play for each mode; mode selector; separate level pages/locked levels; settings/theme/language/audio/haptics; profile name/country/Save/Skip/back; leaderboard/cache/offline/reopen; Defense tutorial/HUD/hint/freeze/pause/retry/win/lose; Shapes rapid shots/bounce/lives/hint/Rocket/progress; Color Shift legal/blocked arrows, wrong wall, clicks/swipes/rotation limit/extra turns/dead ends; pinch/pan/cancel; map-preview tooling. Check source bindings, actual rendered states and real callbacks/input where meaningful.

## Evidence and acceptance
New evidence: `Captures/QualityPass4/`. Each phase records reproduction, final behavior, meaningful MCP checks, restoration and commit. Do not label an untested path as passed. Visual preference changes must fit the game's established glossy blue/gold style and both themes. Avoid redesign solely for novelty.

## Current resume point
Commits: `2e0d20d` roadmap, `61ca3d5` local-only score sync isolation, `32b201a` complete paired baseline/reference audit, `5ab0138` terminal gameplay/earned Hint fixes, `9365162` native country controls + honest Save copy, `e925819` localized curved title fitting. All these changes passed their targeted MCP checks. Original startup/pending/version/reconnect and ten-language actual profile Save regressions passed again.

Next: finish the newly discovered orphan `ParticleSystemDestroyer` on the runtime-instantiated ConfettiParticle prefab (native missingScripts=1 before evidence). Then run fresh Boot Play: confetti pool test, existing Defense smoke, 313 localization checks, all 100 puzzle levels, 450 interaction checks, and final paired graphical sweep. Finish exact initial dirty-file/preferences restoration and final overview. Root owns MCP; no script edits during running fixtures. Preserve old regression report bytes; copies of this run belong under QualityPass4.

## Findings / commits / ideas
### Local preferences no longer generate score uploads
- Before: the native offline-provider fixture recorded 29 failed isolation assertions. Every local setting dirtied pending score state; after an initialized manager, each repeated pair of setting changes sent two unnecessary score submissions (28 uploads for the 14-case batch).
- Change: local audio/theme/language/haptic/mode/inventory/tutorial/setup saves still persist and notify, but do not mark or submit leaderboard state. Genuine level/star/profile changes and public zero-argument `NotifyAndSave()` retain their synchronization contract.
- After: `settings-sync-verification.txt` passes with zero failures, zero uploads for the full local-only batch, and genuine progress/reconnect/profile/auto-save controls intact. Original data reference, preferences, provider factory and lazy manager ownership restore exactly. No real cloud operations were performed.
- Evidence: `Captures/QualityPass4/settings-sync-before.txt`, `settings-sync-verification.txt`.

### Terminal gameplay and earned rewards
- Defense BEFORE: a real generated board launched all pooled arrows; a real mob consumed the last life while the final arrow was still moving. Later completions published Win, opened success above Game Over and unlocked progress after losing (three failed checks).
- Defense AFTER: spawner and central completion entry both require Playing. The same real sequence stays Lose with zero wins and no new progress. A separate normal board still wins once with three stars; repeated completion in Lose, Win and Menu cannot publish another win.
- Puzzle BEFORE: six failed checks showed earned Hint inventory being discarded after actual Retry, Menu transition or scene destruction in both Shapes and Color Shift.
- Puzzle AFTER: successful earned inventory is credited before the stale-owner/attempt check. Old sessions cannot show messages or change the new board. Current-owner, failure and duplicate-provider controls also pass; existing AdManager exactly-once ownership is retained.
- Evidence: `defense-terminal-before.txt`, `defense-terminal-verification.txt`, `puzzle-reward-before.txt`, `puzzle-reward-verification.txt` in `Captures/QualityPass4/`. Every scope restored data/preferences/providers/debug/config. Real ads and cloud submissions were not used.

### Visual/profile repairs under final review
- Native country pointer repair now passes left/right/label input checks with exactly one owned callback per click. Before report recorded three failed checks. Repair is limited to authored hit graphics/target graphics and existing feedback.
- Optional geometry fitting on the four gameplay pause headings passes 80 actual scene/language/theme combinations, including repeated refresh stability and viewport bounds. Fonts/colors/arch are retained. A final graphical sweep will recheck the persisted scene overrides.
- Profile action now says Save in all ten languages, matching the real save-and-close action. Actual profile Save/language regressions are running before commit.

Previously observed cloud Leaderboards connectivity warning remains evidence of unavailable connectivity; do not hide it or treat a fake-provider success as live service certification.
