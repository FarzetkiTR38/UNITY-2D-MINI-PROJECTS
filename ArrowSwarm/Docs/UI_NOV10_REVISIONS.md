# November 10 Home logo and compact Shapes revision

Baseline is 67a1210a. This is a scoped follow-up to the November 9 revision.

- [x] Snapshot existing local files and data-service preferences. Capture the live original penguin and carry forward the existing Home screenshots from the unchanged baseline.
- [x] Import the standalone simplified Home logo and compare both themes in Unity.
- [x] Validate and author all 50 compact Shapes layouts, retaining level identities, colors, solvability and progress.
- [x] Tune the Shapes stroke and arrowhead on the compact grid, and check native framing and hit geometry.
- [x] Play all 50 in Unity and review every initial board in both themes. Deliver a per-level gallery and before/after.
- [x] Verify scene and save boundaries, restore the original level-two Play session and commit checked work.

The revision is complete. Open `Captures/UIRollout/Nov10/index.html` for the Home and penguin comparisons or `shapes.html` in the same folder for all 50 boards.

## Home logo

The dedicated `Assets/_Project/Art/Sprites/SharedUI/home_logo.png` is a 1024×1024 transparent sprite with an Android ASTC 6×6 import and no mipmaps. The built-in image generator edited the existing artwork with this brief. Preserve the smiling star, the four colored arrows, the two-line ARROW SWARM wordmark and the cobalt bevel. Remove the surrounding circular ribbons, especially the yellow loop, and retain a few small sparkles.

The Home slot is 880 instead of 940 reference pixels and sits 25 pixels higher to clear Play on short screens. The original Boot scene and logo source match the baseline. Native paired Home captures, localized layout checks and actual navigation passed.

## Shapes

Compact layouts resample the original painted silhouettes onto two-cell tiles, then reuse the existing solvable path packer with a smaller arrow budget. Grid long sides measure 30–34 cells. The 50 levels now have 40–48 arrows each and the average falls from 78.12 to 44.08. Existing level assets and GUIDs retain their titles and exact palettes. Normalized outline overlap ranges from 78.8% to 94.6%. Narrow subjects retain their proportions with a coarser edge at the smaller grid size.

The penguin changes from 50×52 and 82 arrows to 30×30 and 42 arrows. At 1080×1920 its shaft measures 14.89 px. Across the catalog, shafts measure 13.21–19.48 px and heads 22.97–33.87 px. All 50 boards use four directions. They have 12–20 dependency layers and 20–27.5% of arrows are initially legal.

## Unity checks

- The saved catalog audit and independent dense validation pass. All 50 Shapes levels complete in Preview with three lives, 2,204 accepted taps and no save writes. Captures include both themes for every level.
- The existing interaction fixture passes 321 assertions after updating its assumption that a concurrent legal shot cannot remove the blocker of the tested arrow. The retry now verifies against current occupancy.
- The production-width mesh regression passes 7,020 assertions. Native framing checks pass at 1080×1920, 720×1280, 1080×2400 and 1536×2048.
- All 15 pre-existing modified files and all five data-service preferences match their original bytes or values. Unity is back on the live Shapes level 2 with three lives and 42 arrows.

The offline gallery links and 100 full-resolution WebP images were validated on disk. The embedded browser blocks automatic opening of local file URLs.
