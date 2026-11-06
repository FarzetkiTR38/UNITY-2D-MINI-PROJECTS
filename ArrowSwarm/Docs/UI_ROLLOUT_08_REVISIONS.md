# Board08 follow-up — Turkish title glyphs and quiet hero accents

Active request 2026-11-06. Baseline `44dcb361`. Preserve the established title family and correct missing Turkish glyphs; decorate only the two delivered Defense skill popups. Gameplay, HUD, backdrop, ads/rewards and approved compositions remain unchanged.

## Continuing checklist

- [x] Read rollout/project rules, snapshot existing dirty source and private Editor/player state.
- [x] Prove original Lilita font is missing İ, Ğ/ğ and Ş/ş.
- [ ] Build a reusable renamed title-font derivative preserving original glyphs, with native-weight Turkish accents.
- [ ] Import a shared TMP atlas and matching title materials; migrate title layers only, keeping Inspector-authored styling.
- [ ] Check glyph provenance, unchanged Latin outlines/metrics, real localized headings and responsive bounds in Unity MCP; commit font phase.
- [ ] Add subtle reused-art rays/sparkles/snowflakes around the two popup heroes, with gentle unscaled animation and no input interception.
- [ ] Inspect/refine both themes and two portrait ratios, check all languages and real popup controls; commit effect phase.
- [ ] Restore private state/dirty files, update before/after preview and commit evidence.

## Resume

Font derivative/import and native before captures are next. Do not begin another numbered board.
