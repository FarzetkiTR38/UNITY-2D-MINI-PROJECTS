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