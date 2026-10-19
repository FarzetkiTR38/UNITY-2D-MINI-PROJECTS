# Shapes arrow geometry correction

The user reports distorted thick Shapes arrows around corners and the arrowhead. Retain the configured bold Shapes presentation, silhouette design, controls and gameplay. Compare Defense's existing line/head treatment and verify through Unity MCP; do not change Defense assets or its renderer.

## Implementation

1. Reproduce the current geometry with isolated Unity renders. Preserve existing modified files and scene/config/prefab/Defense hashes.
2. Correct the filled-head overlap using the stroke radius and actual triangle geometry. Keep the arrow tip on its grid/flight position and make short bends consistent at normal and hint sizes. Avoid redundant collinear line vertices; reuse runtime buffers.
3. Verify all directions, tight bends, multiple widths, hint enlargement, sliding tail, rocket recovery and pooled reuse. Run real Shapes/Color Shift interactions and campaign playthroughs, inspect captures, repair any remaining defect and commit the evidence.

## Root cause

The shaft ends at half the nominal triangle height while retaining a rounded cap. At Shapes' .42 stroke and .72 head, the cap radius is .21 but the triangle is narrower than the shaft at that depth. The cap protrudes beyond the triangle's sides. Consecutive grid vertices and short terminal spans also expose native line-join tapering; hint enlargement makes the final span even shorter. Increasing corner tessellation alone cannot solve the head overlap.

Existing Defense uses a wider head relative to its shaft. Its rendering source and assets remain protected; puzzle fixes are limited to their existing presentation layer.

## Result

The puzzle renderer retains its LineRenderer and configured widths. The filled triangle's cached alpha bounds and slope now determine the shaft cap overlap from the actual stroke radius. Collinear grid vertices are removed from the visual centerline and corners use cached quarter-circle samples instead of unstable native corner fans. Short terminal fillets retain tangents under enlarged heads. Identical tangent points shared by adjacent half-cell fillets are stored once, including the thicker future-tuning case found during validation.

The visual cache is prepared at binding or hint changes; travel rendering reuses its buffers without managed allocations. Arrow tips remain at their logical grid/flight positions. Hit selection uses the existing path; theme colors, rocket collapse, flight timing and puzzle rules are unchanged.

## Verification

Unity MCP passed 6,764 geometry assertions across three width/head pairs, four directions, three path layouts, normal/highlight presentation and sixteen travel samples. The checks use the native baked mesh, terminal cap containment, finite/bounded vertices, smooth nonduplicate segments, head direction and rocket/pool reset. Actual Unity camera captures reproduce the old artifact and show the repaired normal/hint arrows.

All 50 Shapes and 50 Color Shift levels passed real runtime solution playthroughs with three lives and no save writes. Another 450 existing interaction assertions passed for rapid pointer taps, concurrent flights, pause, bounce/damage, retry, hint, rocket, rotation and gestures. The final console had zero errors.

All 318 protected scene/config/prefab/Defense source hashes match the baseline; all ten pre-existing modified files retain their initial contents. No scene, authored level, config, prefab or Defense source change was required. The Editor returned to MainMenuScene outside Play Mode.

The [review overview](../Captures/ShapesArrowGeometry/README.md) contains before/after captures, all 100 initial board captures and the verification reports. Editor runners accept separate output paths so earlier reviews remain unchanged.
