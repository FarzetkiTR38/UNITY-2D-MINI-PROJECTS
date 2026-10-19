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
