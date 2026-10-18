# Shapes Revision 3

## Goal

Rework all 50 Shapes puzzles so each object is recognizable at the normal mobile zoom. Preserve four firing directions, full silhouette coverage, solvable dependencies, three lives, shared best stars, independent progress and rapid concurrent arrow input. Color Shift and Defense retain their current content and presentation.

## Phases and commits

1. **P00 — Baseline and design.** Record the current state and protect existing local edits. Define the visual and verification requirements in this document.
2. **P01 — Shapes presentation isolation.** Give Shapes its own Inspector-editable puzzle config with stronger strokes/heads and suitable color contrast. Keep scene-authored HUD, zoom, controls and shared gameplay intact. Verify through Unity MCP and commit.
3. **P02 — Fifty redesigned puzzles.** Choose each object's proportions and sampling detail independently, crop unused grid margins, preserve important color regions and replace repetitive tiny curls with longer readable paths. Refine silhouettes and distinctive features across the catalog. Validate every mask, path, dependency graph and solution before persisting assets while retaining all level/catalog GUIDs. Inspect representative maps in Unity; commit.
4. **P03 — Visual review and correction.** Capture all fifty actual gameplay scenes, inspect every overview sheet, fix unclear silhouettes or color regions and check light/dark phone/tablet views. Commit any corrections with their evidence.
5. **P04 — Runtime and delivery.** Play every redesigned level using actual pooled arrow animations and rapid firing; check retry, collision, skills, pause, completion, save isolation and protected modes. Produce `Captures/Revision3/Overview` and a clickable index with full-size captures, level dimensions, arrow counts and test results. Commit the final verification artifacts and record outcomes here.

## Acceptance

- All 50 levels keep unique titles and geometry, filled coverage, no overlap/out-of-bounds cells, a valid solver witness, inward dependencies and varied cardinal directions.
- Size follows the subject: wide animals remain wide, tall subjects remain tall, and unused rectangular borders do not dictate the composition.
- Body, face, wings, shell, beak and similar distinctive regions use coherent colors and enough sample area to survive path partitioning.
- Stronger Shapes-only strokes and heads improve the silhouette at default zoom. Arrow count is managed through longer paths rather than increasing taps just to gain detail.
- The final overview is built from actual Unity gameplay captures, not illustrations or a separate rendering of the data.
- Existing local edits are snapshotted before work; unrelated dirty files are excluded from commits. No push is requested.

## Verification record

Baseline: `4e9d6a8`. Unity 6000.3.18f1, MainMenuScene clean, Play Mode stopped, console errors zero. Ten initially modified files were backed up outside the project, including fonts, MapTestScene, TimeManager and the solution file.

P01: Unity MCP compiled the Shapes-only config authoring and verified the saved Shapes scene reference (stroke0.42, head0.72). Shared tuning remains stroke0.28/head0.58. Shapes camera coverage0.95 and inset0.015; existing HUD geometry is retained. Protected Color Shift assets and GameScene hashes remain unchanged. Runtime checks follow after catalog generation.

P02: Unity MCP generated and validated all50 final Shapes candidates before updating their existing assets. Object-specific grids range36–66 cells per axis, are tightly cropped and preserve uniform sampling pitch. Color-coherent ribbons replace per-tile curls; important face/wing/shell regions are not averaged away. All50 have four directions, inward heads and acyclic solver witnesses. First actual1080x1920 gameplay captures of penguin, rabbit and fish show the stronger Shapes strokes and retained original HUD. Authoring metrics are in Captures/Revision3/authoring-metrics.csv. All existing level/catalog GUIDs and protected mode file hashes are unchanged.
