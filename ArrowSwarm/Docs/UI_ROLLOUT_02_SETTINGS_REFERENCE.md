# Settings reference correction

User request: match only the theme-choice area and bottom gear ornament to the supplied old screenshot. All other approved UI must remain unchanged. Board03 is not authorized yet.

- [x] Privately checkpoint current player/preferences, Play Settings state, MainMenu/Boot, ProjectSettings and 14 protected local files. Pointer: %TEMP%/ArrowSwarm-SettingsReferenceFix-path.txt.
- [x] Inspect original 5890e6e8 scene: one joined 350x150 illustrated selector, two independently tappable halves; original masked DarkFooterSettingsGear.
- [x] Restore joined art with the original atlas sprites and isolated caption-cleaning materials and live localized TMP. Restore compact 337x166 ornament flush to current approved board; retain all other rows/title/frame/layout.
- [x] Native Unity MCP tests: 1080x2400, 720x1280, 1536x2048 and 1920x1080. Both themes, all 10 languages, real pointer callbacks, feedback and TMP/safe bounds pass. Screens inspected.
- [x] Prove all other scene documents byte-identical (including restoring only Canvas editor auto-size noise). Restore all private files/player/preferences and original Play Settings state. Commit changes and show final evidence.

No new bitmap, font, runtime loader or gameplay changes. Original texture bytes/importers are unchanged. The existing SettingsUI owns selection, saved theme and callbacks. Existing UIButtonFeedback targets the whole joined control; tests measure that actual authored target.

The old board contained its own empty ornament socket. A 24-vertex Sprite reuses that original texture and remaps only four UVs inside the bottom-center 12.5% band to the adjacent straight border; every other position/UV is unchanged. This removes the doubled arch behind the compact ornament. Source-preservation proof: 1,394 other scene documents remain byte-identical; only the requested selector/footer/frame binding and their owner references change. Original DarkUI shader and shared theme materials are unchanged. No new texture/font assets.

Completed: all four native sizes pass 7,472 assertions each, zero failures. Final images and before/after crops are in Captures/UIRollout/Phase02SettingsReference/index.html. All 14 protected local files, ProjectSettings and Boot source byte-match their fresh checkpoint; original runtime player, five preferences, language, selected mode and open Settings restored after startup settled. Initial 11 dirty files and unrelated untracked files remain intact. Earlier exploratory captures/assets are archived privately; only passing final evidence is published. Board03 remains gated on user approval.
