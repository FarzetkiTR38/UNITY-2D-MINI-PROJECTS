# Puzzle expansion and presentation revision

Baseline: 5a049129323afbff6b09172d2d2157ebc479e0e6. Authorized 18 October 2026.

## Scope and protected behavior

Keep Defense gameplay, generator, scene, prefabs, economy and progress intact. Reuse the authored GameScene HUD, TMP localization, press effects, popup animations and camera gestures. Preserve the user's existing font, MapTestScene and TimeManager changes; their initial bytes are backed up outside Git. Stage only revision-owned files.

## Phases and acceptance gates

1. **P00 — Plan and baseline.** Record this implementation and existing dirty files before changes. Commit this document first.
2. **P01 — Grid-only presentation.** Remove the copied Defense outer channel/card from both puzzle scenes and their builder. Fit the camera to the grid and Color Shift border. Check both modes in Unity and commit.
3. **P02 — Dense, varied campaigns.** Replace partial/sparse silhouette generation with exact occupied-cell coverage, preserving recognizable contours. Generate 50 distinct Shapes silhouettes and 50 Color Shift boards with increasing sizes, path complexity and rotation pressure. Preserve the original five level asset GUIDs. Validate complete coverage, non-overlap, no mutual locks and a legal solver witness for all 100 levels. Extend the image authoring editor and preview range. Inspect representative levels and commit.
4. **P03 — Rescue Rocket artwork.** Generate an original transparent glossy rocket icon matching the existing skill button. Import as a Sprite, retain prompt/provenance, verify alpha and appearance in Unity, commit.
5. **P04 — Rescue Rocket gameplay.** Fill the former Freeze slot in both puzzle modes. One configurable charge per attempt. Tap to arm/cancel, select one arrow to extract without losing lives or rotations regardless of blockers/wall color. Consume only after selecting a valid arrow. Preserve pooling, win/stars, retry/pause/menu cleanup and localization. Keep Defense Freeze unchanged. Test charge, targeting, blocked extraction, final-arrow win and retry; commit.
6. **P05 — Color Shift frame and controls.** Rounded corners, fixed inner arrows, directional flowing paint between 90-degree wall states; block shots during transition and cancel safely on retry. Refine the wide rotation pill with central remaining-count and left/right chevrons. Both swiping and tapping work, one rotation per gesture, center taps do nothing. Test both directions, zero turns, intermediate colors and final wall matching; commit.
7. **P06 — Integration and regression.** Play through all 100 levels with the normal runtime rules; test failures, wrong walls, skills, retry, popups, localization, phone/tablet layouts, themes, saves and Defense regression. Fix detected issues in separate commits, capture evidence and update this document with results.

Every implementation phase is compiled and checked through Unity MCP before its commit. Visual inspection includes small and dense silhouettes, rounded frame transitions and the actual HUD. No external publication or push is required.

## Implementation details

Dense generation partitions the complete sampled silhouette into adjacent continuous arrow paths. A dependency graph proves a valid removal order and measures initial blocking/depth. Four head directions and interior dependencies prevent an all-outward trivial layout. Import sampling uses complete 2x2 blocks, reports unsupported masks explicitly, and never silently drops cells.

Color Shift uses two colors with opposite sides equal. Logical wall orientation changes by a quarter turn; only the outer frame paint animates. Rotation budgets vary by level and must satisfy the generated solution. At zero turns, remaining legal arrows can still clear; an impossible remainder loses normally.

Rescue Rocket is a deliberate single-arrow recovery tool shared by both new modes. Its allowance resets each attempt and is independent of existing saved inventories. The bottom count and targeting instruction use localized TMP text. Static artwork, colors and layout remain editable in scenes/prefabs. Effects use an existing pooled arrow view; no new arrow instantiation or Defense events.

## Verification record

To be updated per phase with Unity checks, captures, campaign metrics and commit references.

P01 verified through Unity MCP: compiled with zero errors; Shapes and Color Shift both contain only Layer3_InnerGrid, no Layer2_OuterCard. Actual portrait captures: Captures/Revision2/p01-shapes-grid-only.png and p01-color-grid-only.png. The camera now fits the grid/border without Defense channel scaling. Defense source and scene unchanged.

P02: Unity MCP built and independently verified all 100 assets. Every intended silhouette/grid cell is occupied exactly once, layouts are unique, each has four head directions and inward dependencies. Shapes grow from 24x28 to 56x60; Color Shift grows from 10x10 to 42x42 with 1–10 required turns. All 100 solver witnesses fit authored budgets. Existing level 1–5 and catalog GUIDs remain intact. MapSceneController now previews levels 1–50. Heart and level 50 were loaded/rendered in Unity (Captures/Revision2/p02-dense-heart.png, p02-shapes-50.png); full runtime playthrough follows in P06.

P03 art: built-in image_gen generated the original transparent rocket; Unity MCP confirms Sprite/Single, source alpha, Clamp, no mipmaps, max 512, uncompressed. Project asset: Assets/_Project/Art/Sprites/UI/RescueRocket.png. Original retained in the Codex generated_images folder.

Final prompt (built-in tool, no CLI): “Use case: stylized-concept. Asset type: single transparent mobile puzzle skill icon. Create ONE polished glossy toy rescue rocket, pointing diagonally up-right, centered, isolated on genuine transparent background. Chunky rounded friendly rocket with bright white pearl fuselage, deep royal-blue thick outline, cyan glass porthole, orange/golden fins and short vivid yellow-orange flame. Match a cheerful premium casual mobile game with shiny blue jelly buttons and soft beveled specular highlights. Strong readable silhouette at 64 pixels. Occupy about 80 percent of a square canvas. No button frame, no surrounding card, no text, no numbers, no logos, no watermark, no scene, no scattered decorations. Clean alpha edges and no cast background shadow. Rocket is a recovery skill that lifts one blocked arrow out of a puzzle.”

P04 verified in Unity: both modes expose the authored RescueRocketButton with a generated icon and live TMP count. Empty taps and arming/cancelling consume nothing; selecting a blocked arrow removes exactly one pooled view without losing lives/turns. Retry restores charges, opacity, geometry and FX. The focused real-event suite also passed final-arrow Rocket wins in both modes and confirmed campaign/backup/language preferences were unchanged. Full report: Captures/Revision2/interaction-verification.txt. All ten languages include the targeting instruction and tap/swipe rotation guidance. The allowance is one per attempt, editable in PuzzleConfig; Defense Freeze remains intact.
