# Localized menu artwork

The four sibling PNGs below were edited with the built-in `image_gen.imagegen` tool. Original supplied/reference artwork remains available. No raster editing scripts were used.

- `Assets/_Project/Art/Sprites/ModeMenu/ModeModalTextless.png`: original illustrated cards, empty text areas.
- `Assets/_Project/Art/Sprites/ModeMenu/ModeModalFrame.png`: empty modal frame behind independently animated cards and close button.
- `Assets/_Project/Art/Sprites/ModeMenu/MainMenuTextless.png`: original menu controls with empty text areas for localized TMP labels.
- `Assets/_Project/Art/Sprites/ModeMenu/MainMenuBackdrop.png`: background plate without controls, avoiding a static duplicate behind an animated button.

`ModeMenuLocalizationUpgrade.UpgradeLoadedScene()` authors `Image` sprites from these sources and places localized `TextMeshProUGUI` / `LocalizedText` elements above them. Each visible control owns its existing `UIButtonFeedback` target and Unity `ColorTint` transition. The upgrade operates on the loaded menu and leaves scene saving to its caller. Shared typography remains editable in the scene; `LocalizedMenuOutline.mat` is an editor-authored material asset.

## Generation prompts

### ModeModalTextless

Use case: text-localization. Edit target: supplied transparent game mode selection modal UI. Remove only the baked word labels GAME MODES, DEFENSE, SHAPES, and COLOR SHIFT from their current locations. Fill each removed text area seamlessly using its existing local background (white panel heading, pale mint landscape in first card upper-left, pale pink cloud second card upper-left, pale mint third card upper-left). This is production Unity UI art that will receive real localized text. Keep absolutely everything else unchanged: exact pixel dimensions 941x1672, exact geometry, panel/card/button positions and dimensions, glossy blue/chrome outlines, cute arrow and shield illustrations, hearts, colorful arrow heart, rotation icon, gradients, highlights and shadows. Keep existing blank blue select pill buttons blank. Preserve close icon (white X) and its blue square. Preserve full alpha transparency outside panel. No words, letters, numbers, logos or watermark anywhere. Do not redesign, recompose, crop or move the UI. No background outside panel; true transparency.

### MainMenuTextless

Use case: text-localization. Edit target: existing Arrow Swarm main menu UI art. Remove ONLY these labels baked into five button faces: PLAY, GAME MODE, LEVELS, LEADERBOARD, SETTINGS. Seamlessly restore each underlying blue or white/light-blue gradient button surface. Leave the play triangle icon, game mode illustration and dropdown chevron/divider, levels cards icon, trophy icon and settings gear untouched. Preserve EVERYTHING else exactly: same 941x1672 portrait layout, positions and sizes, cream background with decorative arrows/dots, top two blank stat badges, central ARROW SWARM character logo and its words, all glossy blue/gold button outlines, highlights, shadows, paths, stars and cute arrows. Output a production UI reference with blank button text areas for live TextMeshPro labels. Do not add words, move objects or redesign. Logo text ARROW SWARM must stay unchanged.

### MainMenuBackdrop

Use case: precise-object-edit. Edit target: existing portrait Arrow Swarm main menu UI image. Produce the background plate ONLY by removing the five large buttons and every icon/word within them in lower half from y=805 to y=1602: the PLAY gold-edged blue button, the GAME MODE light-blue button, and LEVELS, LEADERBOARD, SETTINGS blue buttons. Seamlessly reconstruct the existing soft warm cream background and any small pale blue decorative arrows/dots behind those five controls. Leave generous blank cream spaces where these controls were because they will become separate animated Unity UI images. Keep every other part unchanged, especially the two top blank blue stat badges with trophy/star, full central ARROW SWARM logo and characters, exact top positions, dimensions, color palette, all small blue arrow/dot decorations around outside. No replacement controls, no words in lower half, no new art, no redesign. Same 941x1672 portrait framing; the logo words ARROW SWARM stay unchanged.

### ModeModalFrame

Use case: precise-object-edit. Edit target: existing blank-text game mode modal. Produce its EMPTY MODAL FRAME background plate: remove all THREE large landscape rectangular illustrated game mode cards and their blue pill buttons inside (from x60..883 and y456..1365), and remove the small blue X close button in the top-right. Seamlessly replace their areas with the same plain white/light-gray panel interior as the blank header. Preserve only the giant outer glossy blue/chrome rounded rectangular frame and its white interior at the exact same position/size on the transparent 941x1672 portrait canvas. Keep all transparency outside the outer modal. Keep original panel white interior subtle gradient, blue outlines, edge highlights and drop shadow. No cards, no illustrations, no buttons, no letters, no words, no numbers. This empty frame will sit behind separately rendered interactive card sprites in Unity; do not move or redesign frame.

## Localization keys

The existing ten language dictionaries all contain the same additional keys: `modes_title`, `mode_defense`, `mode_shapes`, `mode_color_shift`, `modes_select`, `modes_selected`, `menu_game_mode`, `menu_level_short`, plus twelve puzzle HUD/help keys. Existing `menu_play`, `menu_levels`, `menu_leaderboard` and `menu_settings` keys are reused.

The brand logo remains artwork. Selector headers, mode names, selection states and all menu button labels are real text. Dynamic changes update labels and active state only; visual styles are not overwritten at runtime.
