# Shapes arrow geometry correction

The user reports distorted thick Shapes arrows around corners and the arrowhead. Retain the configured bold Shapes presentation, silhouette design, controls and gameplay. Compare Defense's existing line/head treatment and verify through Unity MCP; do not change Defense assets or its renderer.

## Implementation

1. Reproduce the current geometry with isolated Unity renders. Preserve existing modified files and scene/config/prefab/Defense hashes.
2. Correct the filled-head overlap using the stroke radius and actual triangle geometry. Keep the arrow tip on its grid/flight position and make short bends consistent at normal and hint sizes. Avoid redundant collinear line vertices; reuse runtime buffers.
3. Verify all directions, tight bends, multiple widths, hint enlargement, sliding tail, rocket recovery and pooled reuse. Run real Shapes/Color Shift interactions and campaign playthroughs, inspect captures, repair any remaining defect and commit the evidence.

## Root cause

The shaft ends at half the nominal triangle height while retaining a rounded cap. At Shapes' .42 stroke and .72 head, the cap radius is .21 but the triangle is narrower than the shaft at that depth. The cap protrudes beyond the triangle's sides. Consecutive grid vertices and short terminal spans also expose native line-join tapering; hint enlargement makes the final span even shorter. Increasing corner tessellation alone cannot solve the head overlap.

Existing Defense uses a wider head relative to its shaft. Its rendering source and assets remain protected; puzzle fixes are limited to their existing presentation layer.
