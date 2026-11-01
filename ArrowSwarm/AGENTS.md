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

Boards01,02 and03 are complete and approved. The user approved board03 and authorized **04 · Savunma ve Şekiller Bölümleri** on2026-11-01. Follow `Docs/UI_ROLLOUT_04.md`. Integrate only Revision6 board04 now; preserve ColorShift level selection/transition for board05. Await approval of board04 before starting board05. Continue all standing instructions for later boards.
