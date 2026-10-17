# Revised UI evidence

- `main-menu.png`, `selector-tr.png`: real localized TMP labels over textless artwork.
- `shapes.png`, `color-shift-9x16.png`: GameScene-derived gameplay HUD and puzzle maps.
- `color-shift-9x20.png`, `color-shift-3x4.png`: portrait aspect checks; measured board bounds fit between HUD panels.
- `shapes-dark.png`, `color-dark.png`: existing dark UI theme and readable puzzle palette.
- `puzzle-pause.png`, `puzzle-hint.png`, `puzzle-lose.png`, `win.png`: retained gameplay popup artwork. The first three were captured during authoring before the final swipe-track width adjustment; popup appearance is unchanged.
- `defense-reference-portrait.png`, `defense-verified.png`: original Defense reference and final working regression, with active enemies.
- `ui-verification.txt`: 183 passing focused checks, including actual button pointer feedback, retry cancellation, localization and deterministic reward callbacks.

All ten level renders and their non-saving real-animation report are in `../ThreeModes/`. Current menu/selector evidence is in this folder; older menu captures in ThreeModes are historical.
