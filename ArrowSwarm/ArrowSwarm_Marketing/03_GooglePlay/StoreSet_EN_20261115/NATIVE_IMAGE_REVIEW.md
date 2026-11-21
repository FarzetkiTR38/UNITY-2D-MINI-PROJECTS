# Independent native source review

Eight fresh native Unity captures were inspected at full image size. These are actual game UI, not generated gameplay mockups. English is set through the existing localization system. The store footer is added separately, below the native image.

## Evidence and scope

- [x] Inspect the actual eight PNGs, including top/bottom HUD, controls, board boundaries and readable text.
- [x] Read the native manifest and the store capture helpers, including the local demonstration leaderboard provider.
- [x] Confirm the actual leaderboard uses the existing renderer and real refresh path. The ten displayed players, scores and rank are **fictional demonstration data**, not verified live/global standings.
- [x] Confirm the provider implementation returns local completed tasks and contains no external request/authentication code. The manifest reports zero external service calls; this is source/counter evidence, not packet capture.
- [x] Read the capture's state restoration evidence: original data, auto-save, five preferences, language/theme, providers, timing, launch context, random state and background setting report PASS.
- [x] Independently inspect the corrected Defense recaptures: full portal/HP values clear the HUD and screen edge; Freeze uses a calm single legal shot with no damage-label clutter.
- [x] Rerun the structural source checker against the complete final manifest: all eight native English sources pass.

The checker inspects PNG integrity/dimensions/nonempty alpha and Unity's visible TextMeshPro inventory. It is not OCR. Layout and artwork are covered by the independent visual inspection below, not inferred from metadata labels.

Seven native PNGs are fully opaque. Native leaderboard artwork preserves partial alpha in 20,347 UI-effect pixels (alpha 155–254; no fully transparent pixels). The final composer deliberately reads the rendered RGB values, so source RGB pixels are preserved without an accidental second transparency blend. This is recorded in `native_source_checks.json`; the native source itself is unedited.

## Actual visual inspection

| Source | Actual content | Review |
|---|---|---|
| 01 Home | Original English Play, mode selector, three navigation buttons; original logo and profile control | Clean composition. Top stats, full logo, buttons and bottom clouds visible. English functional copy. |
| 02 Defense | Native level 10, 15 arrows, natural enemies halfway around the path | **Corrected and reinspected:** 12% wider capture-only native camera fits the complete portal and HP values below the top HUD. Full board, original HUD and complete bottom controls retained. No source scene or gameplay-stat edits. |
| 03 Shapes | Level 12 Emerald Turtle, 30×30 grid, 45 arrows | Distinct turtle silhouette, readable native arrows and complete board. Top/bottom HUD and both skills fit. |
| 04 Color Shift | Level 3 purple/gold board, 12 arrows, two rotations | Complete frame, original HUD, rotation bar and English instruction visible. No crop or obscured controls. |
| 05 Defense Freeze | Native level 15, 24 initial arrows, actual Freeze activation and one legal departing arrow | **Corrected and reinspected:** 12% wider native camera fits the portal and whole board. Frozen-blue enemies and the single pink arrow are clear, with no overlapping damage values. `Arrows: 1/24` and the native 4.6-second Freeze timer confirm the active skill; the native culture-formatted timer reads `4,6s` (numeric formatting, not untranslated Turkish UI text). |
| 06 Shapes Advanced | Level 21 Violet Octopus, 32×30 grid, 44 arrows | Octopus face/tentacles readable. Full board and all HUD/skill controls visible; no Turkish functional words. |
| 07 Color Shift Rotate | Level 15 green/purple board, 31 arrows, actual 90° rotation, one turn consumed | Full frame, distinct second palette and complete rotation/HUD controls. English instruction readable. |
| 08 Leaderboard | Original English native leaderboard; ten fictional entries and highlighted Nova at #6 | Title, back/close, all 10 rows, result summary and Refresh visible. Rank numbers legible, no Editor/Sample debug banner and no private original player name. |

## Reproduce the structural verification

Run from the repository root, using the existing cached marketing-media Python environment:

```powershell
py -3.13 ArrowSwarm_Marketing/Tools/check_store_sources_en.py
```

The checker writes `native_source_checks.json` alongside this review only after all assertions pass. It never edits the native PNGs, scenes, preferences or player data. Recompose the final screenshots after any native recapture; output hashes must correspond to the final native inputs. The completed structural verification and independent native visual review both pass.
