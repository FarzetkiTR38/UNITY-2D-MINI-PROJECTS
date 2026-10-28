# Board01 revisions

Baseline `febf1768`. These three requested fixes belong to board01; board02 remains behind the user's approval gate. Preserve the existing local changes and player preferences; private revision snapshot is referenced by `%TEMP%/ArrowSwarm-UIRollout-revisions-path.txt`.

- [x] R1: Restore an explicit, legible FARZETKI GAMES TMP studio credit inside Boot's safe area. Native Light/Dark check, then commit.
- [x] R2: Keep the entire shared cloud footer inside Boot and Home screen bounds, preserving its aspect ratio and texture reuse. Native paired-theme check, then commit.
- [x] R3: Replace the distorted nested button-shaped progress fill with a rounded, correctly inset fill. Check 0/low/mid/full values and the real loading animation, then commit.
- [x] Final: Repeat native phone layout checks, update actual previews, restore all original preferences/local bytes and Editor state. Return revised board01 for approval; do not begin board02.

Native evidence will be saved under `Captures/UIRollout/Phase01/Revisions/`.

R1 native check passed in Light/Dark at1080x1920: exact full studio wording and all four credit corners inside safe area; screenshots inspected. Other typography/theme binding checks also passed. R1 changes only Boot credit and its authoring/verification.

R2 native check passed in Boot/Home Light/Dark at1080x1920. The entire cloud artwork is now above screen bottom, with aspect preserved by a width-driven fitter. Both scenes reference the same Sprite; all corner/bottom assertions passed and actual captures were inspected.

R3 complete: a small shared rounded UI shader renders the track/fill without new raster artwork. The Slider now drives a zero-offset fill inside a 7-unit inset area; a rounded stencil slot also clips the very first percentages. All text remains TMP. No gameplay or runtime C# controller was changed.

Final native verification: 1,678 assertions passed at1080x1920 (including actual pointer navigation and all three Play routes); 1,220 passed at1080x2400. Both runs include eight progress values per theme, real decoded screenshot pixels, the5% rounded-cap corner, a real timed loading sequence with monotonic geometry, full studio credit safe bounds, and entire shared cloud bounds. Screenshots were inspected. No levels/rewards or live cloud/ad operations occurred.

All880 old later-panel source documents still match ce22ef28. Saving MainMenu had normalized10 TMP serialization fields in seven old modal documents; those ten fields were restored exactly, without changing the new Home/clouds, and the preservation proof now passes.

Original five preferences and all eleven pre-existing dirty files were restored exactly. Game View18 (1080x1920), Boot Edit scene and run-in-background=false were restored. Native console and loading shader had zero errors at final check.

Preview: `Captures/UIRollout/Phase01/index.html`, with native captures under `Checks/`; `Revisions/progress.html` shows the16 native cropped progress images. Both are self-contained local HTML pages with relative image links. The previous localhost8170 preview service stopped answering; automatic approval review blocked restarting/replacing the local HTTP service with only a policy-block reason. No server/process workaround was used; deliver the offline HTML and real PNGs instead. Do not describe a broken browser screenshot as a successful preview.

Board02 remains unstarted, awaiting user approval of revised board01.
## R4 - illustrated Boot logo (user-approved TMP exception)

Baseline9f0f02e5. The user explicitly requests Boot's four characters and ARROW SWARM lettering as one polished image matching concept01. All other functional wording staysTMP; Home is outside this requested visual change.

Reuse the existing transparent combined logo at `Assets/_Project/Art/_FarzetkiArts/Logos/ChatGPT Image 9 Eyl 2026 18_42_19 (3)-Photoroom.png` (already used by existing UI). Do not add a duplicate PNG or alter its importer. Replace only Boot's previous BrandCrest plus TMP Wordmark hierarchy, retain live TMP credit/status/tip/percentage, and fit the lower orbit illustration below the logo. Original loading, progress mask, cloud footer and startup callbacks remain intact.

- [x] Snapshot initial local files, preferences, active player and Editor state privately (`%TEMP%/ArrowSwarm-BootLogo-path.txt`).
- [x] Reuse the original sprite in Boot's existing `_titleLogoImage` slot; keep Home source unchanged.
- [x] Inspect actual paired-theme logo composition and safe bounds at both phone sizes; verify real loading and TMP copy.
- [x] Restore initial player/preferences/local bytes, update native previews and commit R4.

Verification must require the raster branding only in Boot. Home must still contain its live TMP ARROW/SWARM wordmark; never relax the Home localization checks. Board02 remains behind explicit user approval.
R4 completed: native full check1686PASS/0FAIL at1080x1920 and responsive1228PASS/0FAIL at1080x2400. Both themed captures inspected. Boot uses the original glossy raster logo at840x840, centered at(0,500); the lower orbit is930x700 at(0,-145). Transparent swirls visually connect the illustrations; the ARROW/SWARM lettering remains separate from the lower arrow body. The studio credit, eight-point loading checks, live timed callback, cloud bounds and Home TMP rules all pass.

No new game raster asset, importer or runtime gameplay/controller changes. The entire MainMenuScene file still has SHA25633803aacf3fdcad0fac09163289c1dfb28c221ddda029172b19a606d0273449e, exactly matching the R4 baseline. Native current PNGs are under Checks/ and R4-BootLogo/; the initial-layout pair is retained only as an iteration record.

Initial Play/MainMenu state, Turkish language, ColorShift selection, exact runtime player JSON and all five preferences were restored. All eleven pre-existing local files match their initial byte hashes; locked font-cache files were restored atomically with generated caches preserved privately. Native console0errors/0warnings. Original logo PNG/meta remain unchanged. Board02 is still unstarted pending approval.
## R5 - reference lower Boot and backdrop polish

Baseline `0d002e6f`. Scope is Boot only; Home and later boards remain unchanged. Private snapshot pointer: `%TEMP%/ArrowSwarm-BootLower-path.txt`.

- [x] R5a: Beveled blue loading frame, animated chevrons, right-aligned TMP percentage, compact tip and two-line FARZETKI / GAMES TMP credit with reused gold stars. Native verification passed; committed as `851c52fc`.
- [x] R5b: Dedicated warm cream Light and saturated navy/cyan Dark atmosphere, real arrow motifs, full cloud footer and clean dark tip material. Existing Home shaders/materials and MainMenuScene source are unchanged.
- [x] Final: Publish current native captures and preview; restore the original player, preferences, protected files and Editor presentation. Return board01 for approval before board02.

R5 adds no raster artwork or runtime C# controller. Functional wording remains TMP; the already-approved combined Boot logo remains the sole raster branding exception. The studio stars and background arrow reuse existing sprites.

Native verification:

- R5a: 1,270 PASS / 0 FAIL at 1080x1920, including paired themes, eight progress values, actual flowing chevrons at fixed 65%, fixed right percentage, both credit lines and the original timed Boot callback.
- Final primary: **1,914 PASS / 0 FAIL at 1080x1920**, including real pointer navigation and all three campaign Play routes.
- Final responsive: **1,456 PASS / 0 FAIL at 1080x2400**.
- Both final runs cover every Boot tip/status in every supported language, complete visible TMP lines, actual moving fill decoration, eight fill values, both credit lines, cloud bounds and twenty Home language/theme combinations. Paired screenshots were inspected at both sizes.

The navigation fixture now waits for the newly enabled panel's player updates before measuring buttons. A preview held an old PNG open, so final native checks used fresh R5b evidence folders. The passing results were published atomically to Checks/ and Responsive1080x2400/. The existing Unity MCP service was reconnected through its HTTP session after the tool connection changed; no additional server was started.

Evidence: `Revisions/R5b-BootAtmosphere/`, `Revisions/R5b-Responsive1080x2400/` and `Revisions/boot-r5-light-dark.png`. The portable index opens on the revised Boot Light capture. All 880 older panel documents remain preserved because the complete MainMenuScene SHA256 is still `33803aacf3fdcad0fac09163289c1dfb28c221ddda029172b19a606d0273449e`.

Final state: the original runtime player JSON and all five preferences match; all fourteen protected source files match their private snapshots. Play/MainMenu, ColorShift, Turkish, Game View 18 and background=true are restored. Both revised UI shaders and the Unity console have zero errors. Two Editor-startup helper warnings remain: Aura's .gitignore and an existing Device Simulator .device file were memory-mapped when their helpers attempted to rewrite them. Neither affected the passing Game View tests; these warnings are recorded in verification-summary.json rather than claiming a warning-free final Editor.

**Board02 remains unstarted pending explicit approval.**
