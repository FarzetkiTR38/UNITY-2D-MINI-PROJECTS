# Shapes natural neck / Defense rules 7 and 14

Baseline: `6b0c281`. User supplied a twenty-rule Defense reference and requested the rule requiring the arrowhead to continue at least one weight of straight body behind it. Source was checked against the actual Defense generator, not treated as instructions to port unrelated systems.

## Rule review

- Rules 7 and 14 are the applicable neck and head-alignment invariants: `secondPos = headPos - headStep`, then `HeadDirection = path[0] - path[1]` after geometry changes.
- Rules 8, 11 and 15–17 describe existing orthogonal-path, self-obstruction and solvability protections; puzzle validation/solver already covers these concerns.
- Rules 1–6 concern Defense tutorials, mob difficulty, map rotation and growth entry points. Rules 9–10, 12–13 and 18–20 describe its generation, absorption, weight/dependency and palette choices. They are not wholesale replacements for the published Shapes silhouettes and balance.

All 3,906 arrows in the fifty-level Shapes catalog already satisfy the one-unit logical head/neck invariant. However, 1,663 arrows turn after that first unit. Shapes placed the triangle's tip on the logical head point, so its .72-cell height consumed almost all of the visible neck (.07 cell after the rounded bend). Defense centers the triangle on that point instead.

## Implementation phases

1. Record the rule audit and protect existing scenes, catalogs, levels, prefabs and Defense sources.
2. Reuse an explicit natural-neck predicate in validation and pooled view binding. Add Inspector-owned centered-head tuning only to Shapes, matching Defense's anchoring. Account for that anchor in head overlap and cached rounded geometry. Preserve every level cell, color and dependency.
3. Verify rule rejection fixtures, all 3,906 catalog necks, visible straight necks in all four directions at normal/hint sizes, motion and pool recovery. Capture actual Unity before/after renders and run both puzzle campaigns/interactions through MCP. Commit each coherent phase.

The requirement is one aligned **logical** unit behind the head, with a clearly visible straight shaft, matching Defense. Requiring an additional complete visible cell after subtracting artwork would be a different two-cell minimum and would force changes to 42.6% of the published arrow layouts, including isolated 2×2 packets.

## Result

`PuzzleRules.HasNaturalNeck` explicitly expresses the Defense invariant and is shared by level validation and pooled view binding. Invalid, sideways, reversed, zero, diagonal or missing necks fail before rendering. The new `PuzzleConfig.CenterHeadOnGridPoint` switch is enabled only in the dedicated Shapes config and preserved by Shapes presentation authoring.

The sprite center now occupies its logical head point. Its forward extent is accounted for in cap overlap and blocked-shot travel, preserving the previous visible contact gap. Rounded corners reserve space behind the actual triangle base even when highlighted. The centered head's whole triangle participates in touch selection, keeping the enlarged tip and broad base selectable. Color Shift keeps its existing anchor and hit behavior.

Unity MCP passed 90,098 rule/catalog/visible-neck/head-selection assertions for all 3,906 arrows in normal and hint states. Actual exposed straight shaft minima are .430000/.175999 cells respectively; the one-unit logical neck remains unchanged. Geometry checks passed 7,020 centered-anchor and 6,764 legacy-anchor assertions. All 100 puzzle campaign levels and 450 existing interaction assertions passed with no save writes. An additional real session Hint → Tap at the enlarged forward tip fired exactly its intended arrow, despite being outside the old path-only hit radius. Final console: zero errors.

All 352 protected scene, level/catalog, prefab and Defense-source files match the initial bytes. Shapes config changes only the new centered-head flag; every authored level's cells, colors, coverage and dependencies are unchanged. Verification restores transient player/settings state and exits Play Mode back to the original BootScene.

The [review overview](../Captures/ShapesNeck/README.md) contains native Unity before/after renders, actual gameplay screenshots and all reports. Previous review artifacts remain untouched.
