# Shapes Revision 4 — readable puzzles

## Objective

Keep the recognizable Revision3 silhouettes, object proportions, semantic color regions and bold Shapes-only strokes. Replace repetitive long parallel U-ribbons with varied medium-length bent paths and genuine removal dependencies. Keep rapid shots, full coverage, four cardinal directions, three lives and existing progress. Color Shift and Defense remain unchanged.

## Phases

1. **Baseline and criteria.** Protect existing edits and measure all 50 current levels: initial legal arrows, available choices through removal, inward heads, bends, straight spans and dependencies. Depth alone is not an adequate difficulty measure.
2. **Balanced path generation.** Author Shapes-only geometry with a mix of readable turns and interlocking arrow groups; preserve exact masks, grid dimensions and painted regions. Evaluate multiple deterministic candidates for both visible path variety and dependency structure. Validate the whole candidate catalog before writing assets and retain all level GUIDs. Compile and check through Unity MCP; commit.
3. **Gameplay and visual review.** Play all 50 with the real solver witness and pooled concurrent shots, test blocked hits/retry/pause/skills and compare representative layouts visually. Produce a new Revision4 overview from actual Unity captures. Fix any weak examples, then commit the final tests and review artifacts.

## Acceptance

- Exact Revision3 silhouette coverage and existing visual tuning; no smaller, thinner or less distinct objects.
- More varied L/S/U/turning routes, fewer whole-object parallel stripe patterns, meaningful arrow tracing and a clear removal order.
- A measurable improvement in initial/ongoing choices and inward dependencies across the catalog, rather than one deep chain surrounded by free outward arrows.
- All50 unique, fully covered, non-overlapping, independently solvable puzzles. A difficulty score never excuses a deadlock.
- Report measurements as difficulty proxies, not proof of a subjective difficulty level; include real captures for the user's review.

Baseline: `1bdf74e`. Ten initially modified files were snapshotted outside the project. Shared and Shapes visual configs, original scenes, catalog identity and all Color Shift level assets are protected by hashes.

## Phase 2 results

Unity MCP compilation and the independent persisted catalog audit passed for all 50 levels. Every occupied cell retains its Revision3 Color32, coordinates and dimensions; existing catalog references and level GUIDs remain intact. Generation now joins medium color-coherent packets into varied turning paths and chooses safe heads against actual opening and midgame choice pressure. Opening targets gradually decrease from 27% to 20%, with higher inward dependency targets later in the campaign.

| Mean across 50 | Revision3 | Revision4 |
| --- | ---: | ---: |
| Initial legal arrow ratio | 41.5% | 23.4% |
| Greedy middle-game legal choice ratio | 42.3% | 19.6% |
| Inward heads | 13.2% | 37.1% |
| Cell-weighted bends | 3.17 | 6.51 |
| Longest straight span | 36.08 | 13.68 |

For context, Revision2 had 14.69 cell-weighted bends and 2.94 longest straight span: the new geometry sits between those tiny repeated curls and Revision3's long stripes. Choice ratios are structural proxies, dependent on removal order; they do not establish subjective human difficulty.

Validation initially rejected one early candidate for excess opening choices before any level asset was written. Adjusting the general opening curve resolved it without a level-specific exception or weaker acceptance criteria. Rebuilding levels 1, 4, 50 from the persisted colored masks reproduced their geometry exactly. Fresh Unity console errors: 0. Runtime playthroughs and visual review follow in phase 3.

## Phase 3 results

- Unity MCP played all 50 revised levels through actual `PuzzleSession.Tap` calls and pooled flights, with concurrent batches, no Rocket use, three lives and no campaign/inventory writes. These full-campaign tests use a temporary faster config clone; authored production tuning is restored. The 55–139 arrows per level were accepted and settled correctly.
- Existing interaction regression passed 450 checks in Shapes and Color Shift, including real pointer rapid clicks, blocked hits, overlapping damage, pause, retry, hint/Rocket exclusion, production animation timing and Color Shift frame gestures. The HUD/popup/localization regression passed 183 checks. Final Unity console errors: 0.
- An independent Python audit reconstructed head-ray dependencies directly from the previous/current asset data and completed 1,200 removal sequences across 12 policies. Mean middle-game legal-choice ratios decreased under every policy; even the most-unlocking policy fell from 58.8% to 30.4%. Early-to-late pressure increases across all four deterministic policies and the random-policy average. Individual exceptions are listed transparently in the report.
- Visual review covered all 50 actual initial boards, 7 dark captures and 5 matched before/after comparisons. No new clipping, holes or loss of color regions/major features was found. Some straight runs remain in narrow limbs, leaves and feathers. The 139-arrow peacock remains the densest board; existing zoom is useful there.
- All 57 fresh 1080×1920 PNGs and 7 overview JPEGs decoded successfully. Every HTML gallery link resolves. The existing scene/config/catalog hashes and all Color Shift levels are unchanged; the ten pre-existing modified files remain byte-for-byte identical to the initial snapshots.

Review: [Revision4 gallery](../Captures/Revision4/index.html), [overview](../Captures/Revision4/README.md), [balance](../Captures/Revision4/balance-comparison.txt), [independent policies](../Captures/Revision4/policy-comparison.txt), [visual review](../Captures/Revision4/visual-review.txt), [full playthroughs](../Captures/Revision4/Playthroughs/runtime-verification.txt), [interactions](../Captures/Revision4/interaction-verification.txt), [HUD/localization](../Captures/Revision4/ui-verification.txt).
