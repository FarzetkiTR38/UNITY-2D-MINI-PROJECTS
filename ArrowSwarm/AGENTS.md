# Arrow Swarm: ongoing UI rollout

Read `.agents/AGENTS.md`, the architecture skill and `Docs/UI_REDESIGN_ROLLOUT.md` before project changes.

## User's standing instructions

- `Captures/Revision6/index.html` is the visual source of truth. Integrate the active board's **SONRA · AI TASARIM ÖNERİSİ · OYUNDA DEĞİL** images into the actual game; do not merely create another proposal.
- Match the supplied composition and artwork through repeated native Unity MCP inspection and corrections. Test Light/Dark, localization, real controls, safe areas and different resolutions.
- Reuse the existing sprites, fonts, materials and components. Generate only missing raster artwork through the built-in image generator, preferably as reusable atlases. Keep texture/import/build costs modest.
- Prefer localized TextMeshPro for titles and all functional text. The explicitly approved raster Boot logo is an exception. Do not bake UI words, names, ranks or numbers into new images.
- Author static appearance in scenes/prefabs/Inspector. Preserve existing gameplay, saves, cloud behavior and the already approved UI.
- Keep one continuing implementation checklist, check completed work, and commit each completed/tested phase. Record the exact next action for resumption.
- Deliver actual before/after Unity captures and a reviewable preview. Wait for approval before starting the next numbered board.
- The user grants autonomy within the active board and does not want routine clarification/permission questions.
- Preserve pre-existing dirty files and private preferences/player state; never blanket-stage or reset unrelated work.

## Current approved scope

The user explicitly authorized **06 · Defense pause popup only**, skipped07, then **08 · Defense hint/freeze UI only**. Both are implemented and native-verified. Gameplay, HUD, map, enemies and arrows are preserved; shared overlay prefab is unchanged. Current delivery: `Captures/UIRollout/Phase06_08/index.html`; checked implementation and exact next action: `Docs/UI_ROLLOUT_06_08.md`. Await user review before starting another numbered board. Prior board04 refinements remain in `Docs/UI_ROLLOUT_04_REVISIONS.md`. Continue all standing instructions for later boards.
