# Board 03 — Profile and Leaderboard

Reference: `Captures/Revision6/Concepts/03-profile-leaderboard.png`, the AFTER column of Revision6. Implement the real game, using existing behavior and live localized TMP. Standing preferences are recorded in root AGENTS.md for future numbered boards.

- [x] P0: Read reference/architecture/controllers; checkpoint MainMenu/Boot, ProjectSettings, 14 protected local files, five preferences and initial Edit Boot/Game View state. Create an outside-workspace Git bundle. Pointer: `%TEMP%/ArrowSwarm-UI03-path.txt`. Baseline `c74362a9`.
- [x] P1: Capture the current Profile/Leaderboard natively in both themes without live service writes. Inventory/reuse assets and generate only one shared atlas for missing glossy symbols. Commit the reviewed shared assets/checkpoint.
- [x] P2: Implement the Profile composition: character crest, two-line dimensional TMP title, subtitle, form/name/dice/country controls, Save and Skip. Preserve keyboard, profile validation and persistence semantics. Native MCP check, correct and commit.
- [x] P3: Implement the Leaderboard composition: header, source/status badge, 10 compact rows with flags/country/name/level/stars, medal palettes, current-player highlight, personal summary, saved-result/time and Refresh. Keep live/offline/cache/sample provenance truthful. Native MCP check, correct and commit.
- [x] P4: Verify real controls and data boundaries, all 10 languages/2 themes, phone/tablet/landscape sizes, empty/offline/loading states, long/missing identity data and selected-player states. Regress approved Home/Mode/Settings. Fix any discovered errors and commit.
- [x] P5: Publish actual before/after screenshots and portable preview, asset reuse/cost and source-preservation reports. Restore exact local files/preferences and initial Editor state. Final commit and request approval before board04.

No gameplay changes or production fake ranks. Sample rows are explicitly labeled, Editor-only, scoped, and never persisted or submitted. Existing data/controllers own their callbacks; static UI styles are scene-authored. Native tests use cloned data and an offline provider, restore input/device filters, and avoid external authentication/profile/score/ad operations.

Current next action: **Await user approval of board03. Do not begin board04.** The delivered preview is `Captures/UIRollout/Phase03/index.html`; light/dark paired sheets and the four-pane overview use complete genuine native frames. `verification-summary.json` records six final PASS reports and 32,775 successful assertions, with the actual five PNG sizes checked from their headers.

## Completed phases and evidence

- P0 `43e39642`: durable preferences/checklist and private checkpoint.
- P1 `12c1bcf4`: native Before plus shared six-icon atlas and reversible fixture.
- P2 `a4ed021d`: reference Profile, localized dimensional TMP and native behavior checks.
- P3 `7887e9e2`: reference Leaderboard, country identity/highlight/materials and native state checks.
- P4 `87699b6e`: five sizes, all flags/fallback, Back/Close ownership, Home/Mode/Settings and real Play routing to all three scenes.
- P5 `b960ba75`: actual before/after preview, source/cleanup proofs and final delivery commit.

All 930 unrelated MainMenu scene documents, including the previously approved Mode/Settings and untouched Levels hierarchy, remain byte-identical to the fresh private baseline. BootScene is byte-identical too. One ExecuteAlways mode wrapper preview-scale serialization was restored only after confirming the complete document otherwise matched. `preserved-scene.json` records the final proof.

`cleanup-report.json` confirms the exact five original preferences and initial Edit Boot / Game View18 / background=false. `protected-files-restored.json` confirms all fourteen original local files plus ProjectSettings are byte-identical after the final Unity asset refresh. Private preference values are never published. The original eleven dirty files and Recovery files remain outside these commits.

`console-counts.json` records zero native Console errors. During dynamic Korean glyph generation TMP emitted one importer cache warning; the original SDF asset and the other protected font caches were restored exactly. Tests ran in Unity Game View, not on physical devices, and they do not claim campaign completion or live cloud/ad certification. Native fake rows remain fixture-only. The existing interactive sample tool remains available at **ArrowSwarm > Preview > Leaderboard Samples** during Play.

Regenerate the portable preview from the project root:

```powershell
python Captures/UIRollout/Phase03/Tools/build_preview.py --after After --checks After/verification.txt After/Phone_720x1280/verification.txt After/Phone_1080x1920/verification.txt After/Tablet_1536x2048/verification.txt After/Landscape_1920x1080/verification.txt Regression/verification.txt
```
