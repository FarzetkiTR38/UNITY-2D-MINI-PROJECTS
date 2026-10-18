# Shapes Revision 4 — readable puzzles

## Objective

Keep the recognizable Revision3 silhouettes, object proportions, semantic color regions and bold Shapes-only strokes. Replace repetitive long parallel U-ribbons with varied medium-length bent paths and genuine removal dependencies. Keep rapid shots, full coverage, four cardinal directions, three lives and existing progress. Color Shift and Defense remain unchanged.

## Phases

1. **Baseline and criteria.** Protect existing edits and measure all50 current levels: initial legal arrows, available choices through removal, inward heads, bends, straight spans and dependencies. Depth alone is not an adequate difficulty measure.
2. **Balanced path generation.** Author Shapes-only geometry with a mix of readable turns and interlocking arrow groups; preserve exact masks, grid dimensions and painted regions. Evaluate multiple deterministic candidates for both visible path variety and dependency structure. Validate the whole candidate catalog before writing assets and retain all level GUIDs. Compile and check through Unity MCP; commit.
3. **Gameplay and visual review.** Play all50 with the real solver witness and pooled concurrent shots, test blocked hits/retry/pause/skills and compare representative layouts visually. Produce a new Revision4 overview from actual Unity captures. Fix any weak examples, then commit the final tests and review artifacts.

## Acceptance

- Exact Revision3 silhouette coverage and existing visual tuning; no smaller, thinner or less distinct objects.
- More varied L/S/U/turning routes, fewer whole-object parallel stripe patterns, meaningful arrow tracing and a clear removal order.
- A measurable improvement in initial/ongoing choices and inward dependencies across the catalog, rather than one deep chain surrounded by free outward arrows.
- All50 unique, fully covered, non-overlapping, independently solvable puzzles. A difficulty score never excuses a deadlock.
- Report measurements as difficulty proxies, not proof of a subjective difficulty level; include real captures for the user's review.

Baseline: `1bdf74e`. Ten initially modified files were snapshotted outside the project. Shared and Shapes visual configs, original scenes, catalog identity and all Color Shift level assets are protected by hashes.

## Phase 2 results

Unity MCP compilation and the independent persisted catalog audit passed for all50 levels. Every occupied cell retains its Revision3 Color32, coordinates and dimensions; existing catalog references and level GUIDs remain intact. Generation now joins medium color-coherent packets into varied turning paths and chooses safe heads against actual opening and midgame choice pressure. Opening targets gradually decrease from27% to20%, with higher inward dependency targets later in the campaign.

| Mean across50 | Revision3 | Revision4 |
| --- | ---: | ---: |
| Initial legal arrow ratio | 41.5% | 23.4% |
| Greedy middle-game legal choice ratio | 42.3% | 19.6% |
| Inward heads | 13.2% | 37.1% |
| Cell-weighted bends | 3.17 | 6.51 |
| Longest straight span | 36.08 | 13.68 |

For context, Revision2 had14.69 cell-weighted bends and2.94 longest straight span: the new geometry sits between those tiny repeated curls and Revision3's long stripes. Choice ratios are structural proxies, dependent on removal order; they do not establish subjective human difficulty.

Validation initially rejected one early candidate for excess opening choices before any level asset was written. Adjusting the general opening curve resolved it without a level-specific exception or weaker acceptance criteria. Rebuilding levels1,4,50 from the persisted colored masks reproduced their geometry exactly. Fresh Unity console errors:0. Runtime playthroughs and visual review follow in phase3.
