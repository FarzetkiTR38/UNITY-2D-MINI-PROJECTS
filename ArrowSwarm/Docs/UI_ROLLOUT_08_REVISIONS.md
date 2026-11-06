# Board08 follow-up — Turkish title glyphs and quiet hero accents

Active request 2026-11-06. Baseline `44dcb361`. Preserve the established title family and correct missing Turkish glyphs; decorate only the two delivered Defense skill popups. Gameplay, HUD, backdrop, ads/rewards and approved compositions remain unchanged.

## Continuing checklist

- [x] Read rollout/project rules, snapshot existing dirty source and private Editor/player state.
- [x] Prove original Lilita font is missing İ, Ğ/ğ and Ş/ş.
- [x] Build a reusable renamed title-font derivative preserving original glyphs, with native-weight Turkish accents.
- [x] Import a shared TMP atlas and matching title materials; migrate title layers only, keeping Inspector-authored styling.
- [x] Check glyph provenance, unchanged Latin outlines/metrics, real localized headings and responsive bounds in Unity MCP; commit font phase.
- [ ] Add subtle reused-art rays/sparkles/snowflakes around the two popup heroes, with gentle unscaled animation and no input interception.
- [ ] Inspect/refine both themes and two portrait ratios, check all languages and real popup controls; commit effect phase.
- [ ] Restore private state/dirty files, update before/after preview and commit evidence.

## Resume

Font phase verified. Next: author the two local popup accent groups, inspect both themes and repeat native controls/localization/resolution checks. Do not begin another numbered board.

## Title implementation

`Farzetki_Title_Font_H1.ttf` derives from the existing OFL Lilita source and adds only İ, Ğ/ğ and Ş/ş. All229 original glyph bodies, metrics and Unicode mappings are preserved; native I/G/S/g/s bodies and advances remain exact. The derivative is renamed and retains upstream attribution/full OFL license. Source is28,324bytes.

One static1024x2048 Alpha8 SDF atlas contains all230supported characters at the existing90px sampling and9px padding. Six existing material appearances are cloned onto that atlas; existing genuine unsupported-script fallbacks remain. A single1024square atlas did not fit31characters, so the height was increased rather than reducing the reviewed sampling quality. Runtime atlas payload is2MiB before build compression; this is not a measured APK size.

Seven scene/prefab assets are rebound only by exact serialized font/material references. No layout, game controller or original font asset is modified. Editor authoring helpers use the new shared family/material paths so later deliberate rebuilds retain this fix. `Tools/Fonts/check_title_scope.py` independently proves source isolation.

Native proof: `Captures/UIRollout/Phase08Revisions/FontProof/turkish-title-before-after.png`.12Turkish characters resolve directly from the new font;5body metric pairs and6material atlas links pass. Existing Boot/Home/Mode/Settings navigation and three Play routes pass in `MenuFontChecks/verification.txt`.

`FontOnly/verification.txt`: both popups, both themes, all10languages and native reward success/failure/Close pass after the font migration.
