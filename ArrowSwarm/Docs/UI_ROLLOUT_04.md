# Board04 — Defense and Shapes level selection

Reference: `Captures/Revision6/Concepts/04-levels-defense-shapes.png`, Revision6 AFTER. Board03 explicitly approved2026-11-01. Standing rules remain in root AGENTS.md.

- [x] P0: Inspect rules/reference/current controllers; checkpoint fresh MainMenu/Boot, ProjectSettings,14 protected files,5 preferences, initial Edit Boot/Game View18/background=false; verify outside-workspace Git bundle. Private pointer `%TEMP%/ArrowSwarm-UI04-path.txt`; baseline `519f9042`.
- [x] P1: Capture actual Before Defense/Shapes/ColorShift in both themes using scoped cloned progress/offline services. Inventory/reuse approved art; generate only missing text-free reusable symbols/tile assets if needed. Commit shared assets and baseline evidence.
- [x] P2: Add optional authored mode-view bindings and optional scene-authored level-card states; preserve all legacy fallbacks and ColorShift behavior. Handle listener rebinding/pool reuse correctly. Keep numbers visible on revised locked cards and earned/empty star states faithful to real progress. Compile/native compatibility checks and commit.
- [x] P3: Author reference composition for Defense/Shapes: shared backdrop/crest/frame, dimensional localized TMP title, mode identity badge, range/pagers,4x5 cards, locks/earned and empty stars, compact footer. Bind real controllers. Repeat native Light/Dark inspection and corrections; verify ColorShift retains its original appearance and commit.
- [x] P4: Native behavior and layout matrix:10 languages,2 themes,5 screen sizes, locked/unlocked/cleared states,0–3 stars, highest-page selection, pager bounds, finite Shapes50 last-page range, mode progression isolation, no duplicate callbacks, safe controls and real level-entry routing. Regress approved screens. Correct discovered defects and commit.
- [x] P5: Actual before/after page and sheets, source-preservation/reuse/cost reports, exact protected-file/preferences/Editor restoration, final commit. Deliver and await user approval before board05.

Preserve gameplay/generator/catalogues/cloud/ad contracts. Sample progress is temporary Editor-only cloned data; never persist or submit it. Static appearance belongs to scene/Inspector/prefabs. New view bindings only choose already-authored objects and data; ColorShift keeps its legacy view until approved board05. Existing sprites/materials/title prefab are reused; no baked functional text or numbers. Verify native PNG dimensions from file headers, never editor callback Screen guesses. Do not edit scripts during a running native fixture. Restore every original dirty file instead of blanket-staging/resetting.

Current next action: **Await user approval of board04. Do not start board05.** Final actual before/after preview: `Captures/UIRollout/Phase04/index.html`. The four-pane overview and Light/Dark paired sheets resize complete original frames without retouching. ColorShift source/pixel comparison evidence is linked separately.

## Completed commits

- P0 `9a195024`: board03 approval, fresh verified checkpoint and standing plan.
- P1 `951d9ca6`: true-alpha shared symbols, compact import and six native Before frames.
- P2 `a0267f7c`: optional campaign views/card states; native legacy compatibility.
- P3 `61a9f645`: scene composition, modal/card states and initial complete behavior proof.
- P4 `0171d949`: final title clearance, five sizes, all supported languages/themes and approved-screen/routing regressions.
- P5: final portable preview, exact cleanup proofs and delivery commit (see Git log).

## Verification and preservation

Seven final reports contain **56,261 passed assertions and no failures**. Actual PNG headers confirm720x1280,1080x1920,1080x2400,1536x2048,1920x1080. The full pass exercises real Previous/Next/disabled/locked pointer input, all0–3 star states, exact pool reuse, independent mode progress, final Shapes41–50, and actual unlocked Defense102/Shapes2 entry. Regressions include Boot/Home/Mode/Settings, Profile/Leaderboard and all three Play scene routes. The title's12 authored-unit clearance is converted to screen scale; a landscape test-unit mismatch was corrected without changing the design.

`preserved-scene.json`: all2215 unrelated MainMenu documents and whole Boot source match the fresh baseline exactly. Original ColorShift hierarchy changes only its validated container parent and explicit view bindings. Five neutral optional template fields and seven sub0.001 title-offset serialization differences were individually verified, privately backed up and restored to baseline; guarded helper reports preserve that proof. The shared Ui02 material source remains unchanged. The shader's new repaint option defaults0 and only Ui04BoardDark enables it.

`cleanup-report.json` confirms the exact five initial preferences and initial Edit Boot/Game View18/background=false. `protected-files-restored.json` confirms all14 protected files plus ProjectSettings match original bytes after final Unity asset refresh. Existing11dirtyfiles and Recovery/.codex files were preserved outside these commits; private values are not published.

Native Console errors0. Five warnings came from dynamic Japanese/Korean TMP cache imports and existing UnityCloudService/UGS DNS retry failures. Original font source bytes were restored. Preview-provider counters for auth/profile/score/fetch/ads stayed0; this does not assert that unrelated pre-existing SDK startup tasks made no network requests. Physical devices, live SDK connectivity and campaign completion are not certified. No gameplay, generator, catalogue, progression or cloud-schema change was made.

## Reproduce

From fresh idle MainMenu Play reached through Boot (before opening the real leaderboard), `UiRollout01Verification.Stage04At(folder,true,true)` runs full scoped behavior/capture checks; `Stage04At(folder,true,false)` runs the native layout/language capture matrix at the selected Game View size. Never edit scripts during a running fixture. Restore the private checkpoint after testing.

```powershell
python Captures/UIRollout/Phase04/Tools/summarize_verification.py --root Captures/UIRollout/Phase04
python Captures/UIRollout/Phase04/Tools/build_preview.py --root Captures/UIRollout/Phase04 --after After --checks After/verification.txt After/Phone_720x1280/verification.txt After/Phone_1080x1920/verification.txt After/Tablet_1536x2048/verification.txt After/Landscape_1920x1080/verification.txt Regression03/verification.txt Regression02/verification.txt
```
