# Board 03 — Profile and Leaderboard

Reference: `Captures/Revision6/Concepts/03-profile-leaderboard.png`, the AFTER column of Revision6. Implement the real game, using existing behavior and live localized TMP. Standing preferences are recorded in root AGENTS.md for future numbered boards.

- [x] P0: Read reference/architecture/controllers; checkpoint MainMenu/Boot, ProjectSettings, 14 protected local files, five preferences and initial Edit Boot/Game View state. Create an outside-workspace Git bundle. Pointer: `%TEMP%/ArrowSwarm-UI03-path.txt`. Baseline `c74362a9`.
- [x] P1: Capture the current Profile/Leaderboard natively in both themes without live service writes. Inventory/reuse assets and generate only one shared atlas for missing glossy symbols. Commit the reviewed shared assets/checkpoint.
- [x] P2: Implement the Profile composition: character crest, two-line dimensional TMP title, subtitle, form/name/dice/country controls, Save and Skip. Preserve keyboard, profile validation and persistence semantics. Native MCP check, correct and commit.
- [x] P3: Implement the Leaderboard composition: header, source/status badge, 10 compact rows with flags/country/name/level/stars, medal palettes, current-player highlight, personal summary, saved-result/time and Refresh. Keep live/offline/cache/sample provenance truthful. Native MCP check, correct and commit.
- [ ] P4: Verify real controls and data boundaries, all 10 languages/2 themes, phone/tablet/landscape sizes, empty/offline/loading states, long/missing identity data and selected-player states. Regress approved Home/Mode/Settings. Fix any discovered errors and commit.
- [ ] P5: Publish actual before/after screenshots and portable preview, asset reuse/cost and source-preservation reports. Restore exact local files/preferences and initial Editor state. Final commit and request approval before board04.

No gameplay changes or production fake ranks. Sample rows are explicitly labeled, Editor-only, scoped, and never persisted or submitted. Existing data/controllers own their callbacks; static UI styles are scene-authored. Native tests use cloned data and an offline provider, restore input/device filters, and avoid external authentication/profile/score/ad operations.

Current next action: P4 native resolution matrix and approved Home/Mode/Settings regression. P3 PASS: real Refresh, loading/empty/offline/failed-cache states, outside-top-ten rank, long/missing identity, two themes and ten languages; colored flags now render with required CanvasRenderer, subdued dark gloss reviewed in actual frame. Native provider/ad counters remain zero. Mode wrapper auto-fit preview scale was restored only after verifying every other source property matched baseline.



