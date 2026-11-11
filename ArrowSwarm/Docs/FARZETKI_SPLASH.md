# FarzetkiGames native launch splash

- [x] Separate the approved artwork into its tiny-sparkle background and a transparent cat/plaque/developer-credit group.
- [x] Keep the background at 841 × 1870 and export the foreground at 1024 × 1024 with true alpha.
- [x] Import both as Single Sprites with Full Rect geometry, no mipmaps and no CPU copy. Use Android ASTC 6×6 for the background and ASTC 4×4 for the foreground.
- [x] Assign the background to both native Background Image slots and the transparent group to the real Logos entry. Use a static, unblurred 2.5-second splash with Unity branding disabled.
- [x] Compare native 1080×1920 and 1080×2400 foreground dimensions, and check 720×1280 and 1536×2048.
- [x] Deliver updated native captures and commit the responsive layer integration.

Native splash appears before the saved game theme is loaded. One theme-neutral dark navy composition is shared by both game themes.

The built-in image generator extracted the two layers from the approved combined artwork. The source assets are `Assets/_Project/Art/Sprites/SharedUI/FarzetkiGamesSplash.png` for the background and `Assets/_Project/Art/Sprites/SharedUI/FarzetkiGamesSplashLogo.png` for the foreground.

Unity fills the screen with the background and independently centers the transparent native logo. A 2.5-second real logo entry replaces the previous empty interval. Native background-subtraction checks measured the visible foreground at exactly 742×753 pixels in both 1080×1920 and 1080×2400. The visible ink center moves from (542.5, 971) to (542.5, 1211), a 240-pixel vertical shift matching half the 480-pixel screen-height increase. The readback applies Unity Game View's vertical orientation on the Direct3D backend.

The Editor Boot smoke check reached MainMenuScene in Menu state. These checks cover Unity's native splash preview and the Boot scene separately. A new APK was not built. Original player preferences, the Game View size and pre-existing modified files were preserved. The existing runInBackground preference in ProjectSettings remains outside the splash commit.

The background source PNG is 1,394,144 bytes and the foreground source PNG is 1,158,334 bytes. Android texture-block estimates are 703,872 bytes for the background and 1,048,576 bytes for the alpha foreground, excluding container overhead. These are not APK download-size measurements.

Actual Unity previews are in `Captures/FarzetkiSplash/index.html`, with the focused checks in `verification.txt`. The design and integration complete this request. The numbered UI rollout gate remains in place.

## Original combined-image generation prompt

```text
Use case: logo-brand.
Create one finished portrait launch splash image for the mobile game Arrow Swarm, branding its developer FarzetkiGames. This is a production raster artwork to display through Unity's native Splash Image settings, not a phone mockup.
Use the attached FarzetkiGames logo as the primary identity reference. Faithfully preserve the distinct sharp-eared neon cat head, cyan glowing eyes, cyan FARZETKI wordmark, magenta GAMES below, angular cyan/magenta plaque. Maintain the logo spelling and recognizable geometry. Retain its dark inner areas.
Canvas and framing: exactly 841 pixels wide by 1870 pixels high. Full bleed, long portrait at the reference aspect ratio. The artwork must remain legible when the extreme top and bottom are cropped to 9:16. Center all branding and copy in the central 60% of the canvas height and central 78% of the width, with generous quiet space above and below. The cat logo plus plaque sits centered, approximately y=42% to y=58%, at about 62% of canvas width. Under the plaque, separated by tasteful breathing room, typeset the exact single line "Developed By Samet Yalçın". Use clear medium-weight rounded, premium game typography in soft icy white with a very subtle cyan glow. Preserve the Turkish ç and dotless ı in Yalçın. No other text.
Art direction: polished high-end mobile puzzle-game launch screen. Midnight navy background (#070e24) with a very restrained cobalt radial light behind the logo, fine cyan and pink twinkles, a few small glossy cyan arrow motifs and tiny warm golden four-point star sparkles confined mostly near side edges. Tie it to Arrow Swarm's glossy cyan-blue, pink and gold game UI while allowing the supplied cat brand to dominate. Keep ornamentation subtle and sparse, with generous negative space. Crisp clean edges, no noisy texture, no heavy lens flare, no orange typography, no border, no buttons, no loading bar, no credit badges, no Unity logo, no additional words.
This is one theme-neutral branded splash shared by Light and Dark players. Make the logo and author credit very readable, with tasteful saturated colors against the calm deep navy field.
```

## Full-background decoration revision, superseded

- [x] Extend the decoration into the upper and lower portions, with recognizable cyan blue, magenta pink and golden yellow stars and side arrows.
- [x] Inspect the revised native splash at all three recorded screen ratios and refresh the preview gallery.

The built-in image generator edited the current splash. The exported source remains 841×1870 with the existing Sprite reference.

```text
Use case: precise-object-edit. Edit the supplied existing 841x1870 FarzetkiGames launch splash.
The requested change is to decorate the ENTIRE background from the very top to the very bottom, rather than only the middle. The top quarter and bottom quarter currently look empty. Fill both of these regions, as well as the side margins, with a harmonious airy distribution of glossy CYAN BLUE stars, MAGENTA PINK stars and GOLDEN YELLOW stars, plus smaller four-point sparkles and a few graceful cyan or pink arrow motifs with short curved trails along both sides. Use recognizably shaped little five-point game stars in all three colors, not only tiny colored dots. Place some larger stars near the top left, upper right, lower left and lower right, with smaller stars between. Continue the decorations across all vertical zones, including within the uppermost 10% and lowermost 10%. Balance the pattern with varied size and spacing, approximately 20 colorful stars total and several smaller twinkles. Keep the dark midnight navy backdrop, luminous cyan/magenta brand palette and polished playful mobile puzzle-game feel. Richer and more festive, but clean and readable.
Preserve the center logo exactly: the neon cat face and ears, cyan FARZETKI, magenta GAMES, angular plaque, its proportions, size and position. Preserve the exact credit line "Developed By Samet Yalçın", its spelling including ç and dotless ı, its size, white color and location below the logo. Leave clear space immediately around the lettering. Do not overlap any new decoration with the logo or the credit. No other text, no border, no buttons, no extra logos.
Keep the existing portrait composition and aspect ratio. The source/final design resolution is 841x1870. Make one finished production splash artwork, not a mockup or a sheet of alternatives.
```

## Approved decoration interpretation

The user clarified that decoration means the original pinhead-sized cyan, pink and gold specks and thin gold four-point glints. The large glossy five-point stars in the previous revision were rejected.

- [x] Edit the first artwork directly, extending its tiny lights into the upper and lower background.
- [x] Export at 841×1870 using the existing Sprite reference.
- [x] Inspect the updated native splash at the three recorded screen ratios and refresh the preview gallery.

The built-in image generator used the following correction prompt.

```text
Use case: precise-object-edit. The attached image is the ORIGINAL APPROVED FarzetkiGames splash and is the exact edit target.
Make one restrained change: extend its existing tiny dot-like sky sparkles upward into the empty top and downward into the empty bottom, so these same tiny decorations are spread throughout the background. Match the original image's subtle luminous cyan blue and magenta pink specks, just 2–6 pixels across at this source width, with a few tiny warm gold specks and delicate thin four-point twinkles 10–20 pixels across including glow. Keep this as a finely sprinkled starry night background with sparse airy spacing. Top and bottom should have the same light point density as the side background surrounding the cat. Place the points in every vertical region, including near the top and bottom margins, without forming bands. Most dots should be tiny and dim, with occasional brighter pinpoints.
Preserve EXACTLY the existing neon cat logo and FARZETKI / GAMES plaque, their geometry, lettering, proportions, size, colors and position. Preserve the existing exact white credit line "Developed By Samet Yalçın" and its typography, scale and position. Preserve the original three cyan arrow motifs and their subtle trails. Keep the dark navy background and all existing glow characteristics. Keep clear reading space around the logo and the credit.
Do not create any large star objects. No five-point stars, no faceted gems, no glossy star icons, no confetti, no additional arrow types, no swirls, no new text or decorative objects. Only the original pinhead-sized light dots and a few thin little four-point glints are to be extended into the empty top and bottom.
Maintain the existing portrait aspect and composition, destined for an 841 × 1870 pixel launch splash. One finished flat artwork, not a mockup.
```

## Responsive layer extraction prompts

Background generation uses the built-in image generator.

```text
Use case: precise-object-edit.
Edit the supplied approved FarzetkiGames splash to produce ONLY its separate background layer.
Remove the entire central neon cat logo, FARZETKI/GAMES plaque, and the entire "Developed By Samet Yalçın" credit line. Inpaint those regions with a smooth continuation of the original deep midnight navy backdrop and subtle cobalt radial light. Keep the original distribution and tiny size of the cyan/pink/gold point-like sparkles across the entire height, and the three existing cyan arrow motifs on the side edges with their narrow trails. Where the logo was, leave a quiet subtly glowing navy area suitable to overlay that logo again.
The ornaments must be only tiny light dots 2–6 pixels wide with a few very thin four-point gold glints, exactly as the source. No five-point star icons, no large stars, no additional objects, no text and no remaining pieces of the cat or wordmark.
Maintain full-bleed portrait composition and original aspect ratio, target final export 841×1870 pixels. Opaque background. One production image.
```

Foreground extraction uses the built-in image generator with transparent output.

```text
Use case: background-extraction.
Extract the EXACT central FarzetkiGames branding group from the supplied approved splash as a standalone transparent PNG. Keep the neon cat head with its cyan eyes and pink/cyan ear and fur outlines, the angular FARZETKI / GAMES plaque directly underneath, and the exact white developer credit "Developed By Samet Yalçın" below that. Faithfully preserve the original art, proportions, letter shapes, spacing, colors and Turkish ç and dotless ı. Preserve the cat's dark navy face/interior and the dark navy interior behind the plaque letters; those belong to the logo.
Remove all surrounding midnight sky, stars, light dots, four-point sparkles and all three cyan arrows/trails. Outside the cat/plaque/credit outlines and their restrained neon glow, the alpha must be completely transparent. No dark rectangle, no backdrop, no checkerboard drawn into the pixels.
Use a square 1024×1024 transparent canvas. Center the entire group, fitted proportionally, with about 6–8% transparent margin on each side. Arrange the same cat over the same plaque, with the same developer credit centered below, exactly as the source. Keep the group as one coordinated image that will be Unity's native splash logo. No additional text or changes.
```
