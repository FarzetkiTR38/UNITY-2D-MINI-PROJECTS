# Board08 follow-up — Turkish title glyphs and quiet hero accents

Active request 2026-11-06. Baseline `44dcb361`. Preserve the established title family and correct missing Turkish glyphs; decorate only the two delivered Defense skill popups. Gameplay, HUD, backdrop, ads/rewards and approved compositions remain unchanged.

## Continuing checklist

- [x] Read rollout/project rules, snapshot existing dirty source and private Editor/player state.
- [x] Prove original Lilita font is missing İ, Ğ/ğ and Ş/ş.
- [x] Build a reusable renamed title-font derivative preserving original glyphs, with native-weight Turkish accents.
- [x] Import a shared TMP atlas and matching title materials; migrate title layers only, keeping Inspector-authored styling.
- [x] Check glyph provenance, unchanged Latin outlines/metrics, real localized headings and responsive bounds in Unity MCP; commit font phase.
- [x] Add subtle texture-free rays/sparkles/snowflakes around the two popup heroes, with gentle unscaled animation and no input interception.
- [x] Inspect/refine both themes at four native sizes, check all languages and real popup controls; commit effect phase.
- [ ] Restore private state/dirty files, update before/after preview and commit evidence.

## Resume

Font and effect phases verified. Next: finish private-state restoration and evidence commit, then deliver the comparison for user review. Do not begin another numbered board.

## Title implementation

`Farzetki_Title_Font_H1.ttf` derives from the existing OFL Lilita source and adds only İ, Ğ/ğ and Ş/ş. All229 original glyph bodies, metrics and Unicode mappings are preserved; native I/G/S/g/s bodies and advances remain exact. The derivative is renamed and retains upstream attribution/full OFL license. Source is28,324bytes.

One static1024x2048 Alpha8 SDF atlas contains all230supported characters at the existing90px sampling and9px padding. Six existing material appearances are cloned onto that atlas; existing genuine unsupported-script fallbacks remain. A single1024square atlas did not fit31characters, so the height was increased rather than reducing the reviewed sampling quality. Runtime atlas payload is2MiB before build compression; this is not a measured APK size.

Seven scene/prefab assets are rebound only by exact serialized font/material references. No layout, game controller or original font asset is modified. Editor authoring helpers use the new shared family/material paths so later deliberate rebuilds retain this fix. `Tools/Fonts/check_title_scope.py` independently proves source isolation.

Native proof: `Captures/UIRollout/Phase08Revisions/FontProof/turkish-title-before-after.png`.12Turkish characters resolve directly from the new font;5body metric pairs and6material atlas links pass. Existing Boot/Home/Mode/Settings navigation and three Play routes pass in `MenuFontChecks/verification.txt`.

`FontOnly/verification.txt`: both popups, both themes, all10languages and native reward success/failure/Close pass after the font migration.

## Popup accents

Only the two existing Card child lists gain decorative descendants: warm sparkles around Hint; cool sparkles and miniature snowflakes around Freeze; soft transparent-edge rays behind each existing hero. The hero artwork, title layout, buttons, HUD, backdrop and gameplay retain their existing values. Seventeen small standard UI meshes reuse the existing canvas/material pipeline; no texture, particle-system dependency or runtime spawning is added.

Two motion components gently vary the authored accents using unscaled time (3.6-second cycle, 3-unit drift and 8% scale). Heroes remain stationary. Disabling the popup restores the exact authored transform/alpha, including repeated open/close cycles. Every graphic and CanvasGroup ignores input. Both theme palettes are serialized on existing ThemedUIElement components.

Initial visual inspection caught missing CanvasRenderer components even though motion was running. These are now explicitly authored and required by the graphic class. Native checks inspect nonempty, visible rendered meshes as well as motion. Soft rays were refined to fade across both their width and length.

## Native checks

Final popup runs at 1080x1920, 1080x2400, 720x1280 and 1536x2048 each pass 1,300 assertions. Coverage includes both themes/all10languages, localized TMP bounds, visible accent meshes, animation while timeScale=0, stationary heroes, reopen restoration, real pointer press/Close/WatchAd actions, failed/successful isolated rewards and exactly one charge granted. Existing zero-charge HUD reward behavior is checked separately from the original exhausted-skill popup events.

The Editor input fixture now temporarily binds its UI actions only to its owned synthetic mouse, checks actual raycast targets and restores the original device filter. Production input code is unchanged. This removed physical-pointer contention in compact-screen tests without adding click retries.

Additional shared-font regression: Boot/Home/Mode/Settings and the three existing Play routes pass in MenuFontChecks. Profile/Leaderboard headings are checked in their current approved Editor sample presentation. An attempted older full leaderboard state verifier failed because it expects an exact Loading label while current Editor sample mode deliberately shows EDITOR / SAMPLE + LIVE; the diagnostic report is retained in Diagnostics. No leaderboard production behavior was changed to satisfy that unrelated legacy assertion.

These are native Unity Editor Game View checks with isolated provider seams, not physical-device or live-ad SDK certification. Original player preferences, provider ownership, data, language, random state and lives are restored by the scoped fixtures. The final source guard separately proves 280 of 282 original GameScene documents unchanged, only two Card child-list edits and 108 added documents confined to four local decorative roots.
