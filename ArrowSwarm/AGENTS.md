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

Boards 01 and 02 are complete and approved. Board **03 · Profil ve Sıralama** is implemented and verified. Delivery preview: `Captures/UIRollout/Phase03/index.html`; checked implementation: `Docs/UI_ROLLOUT_03.md`. Await user approval of board03 before starting board04. Continue applying all standing instructions when the user authorizes later boards.
