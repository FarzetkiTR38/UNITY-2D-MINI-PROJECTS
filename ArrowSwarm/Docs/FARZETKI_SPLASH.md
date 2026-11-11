# FarzetkiGames native launch splash

- [x] Generate one branded image using the supplied neon cat logo, a dark navy field and subtle Arrow Swarm arrows and stars.
- [x] Preserve the reference's 841 × 1870 output resolution with proportional scaling and a centered crop. The logo and developer credit remain inside the frame.
- [x] Import the full image as one Sprite with no mipmaps, CPU copy or alpha channel, and Android ASTC 6×6 compression.
- [x] Assign the Sprite in Player Settings → Splash Image with Unity branding disabled and a static, unblurred presentation.
- [x] Inspect the native Unity preview at phone aspect ratios, check Boot handoff and commit the integration.

Native splash appears before the saved game theme is loaded. One theme-neutral dark navy composition is shared by both game themes.

The built-in image generator produced the artwork from the user's logo. The final source asset is `Assets/_Project/Art/Sprites/SharedUI/FarzetkiGamesSplash.png`.

Unity's native splash uses the same Sprite for its background and alternate portrait image, with blur and overlay opacity disabled. One 2.5-second empty logo interval displays the complete background artwork. Preview captures at 1080×2400, 720×1280 and 1536×2048 confirm that the logo and credit remain inside the actual native crop. The readback applies Unity Game View's vertical orientation on the Direct3D backend.

The Editor Boot smoke check reached MainMenuScene in Menu state. These checks cover Unity's native splash preview and the Boot scene separately. A new APK was not built. Original player preferences, the Game View size and pre-existing modified files were preserved. The existing runInBackground preference in ProjectSettings remains outside the splash commit.

The source PNG is 1,727,305 bytes. Android ASTC 6×6 at 841×1870 uses an estimated 703,872 bytes of texture blocks, excluding container overhead. This is not an APK download-size measurement.

Actual Unity previews are in `Captures/FarzetkiSplash/index.html`, with the focused checks in `verification.txt`. The design and integration complete this request. The numbered UI rollout gate remains in place.

## Generation prompt

```text
Use case: logo-brand.
Create one finished portrait launch splash image for the mobile game Arrow Swarm, branding its developer FarzetkiGames. This is a production raster artwork to display through Unity's native Splash Image settings, not a phone mockup.
Use the attached FarzetkiGames logo as the primary identity reference. Faithfully preserve the distinct sharp-eared neon cat head, cyan glowing eyes, cyan FARZETKI wordmark, magenta GAMES below, angular cyan/magenta plaque. Maintain the logo spelling and recognizable geometry. Retain its dark inner areas.
Canvas and framing: exactly 841 pixels wide by 1870 pixels high. Full bleed, long portrait at the reference aspect ratio. The artwork must remain legible when the extreme top and bottom are cropped to 9:16. Center all branding and copy in the central 60% of the canvas height and central 78% of the width, with generous quiet space above and below. The cat logo plus plaque sits centered, approximately y=42% to y=58%, at about 62% of canvas width. Under the plaque, separated by tasteful breathing room, typeset the exact single line "Developed By Samet Yalçın". Use clear medium-weight rounded, premium game typography in soft icy white with a very subtle cyan glow. Preserve the Turkish ç and dotless ı in Yalçın. No other text.
Art direction: polished high-end mobile puzzle-game launch screen. Midnight navy background (#070e24) with a very restrained cobalt radial light behind the logo, fine cyan and pink twinkles, a few small glossy cyan arrow motifs and tiny warm golden four-point star sparkles confined mostly near side edges. Tie it to Arrow Swarm's glossy cyan-blue, pink and gold game UI while allowing the supplied cat brand to dominate. Keep ornamentation subtle and sparse, with generous negative space. Crisp clean edges, no noisy texture, no heavy lens flare, no orange typography, no border, no buttons, no loading bar, no credit badges, no Unity logo, no additional words.
This is one theme-neutral branded splash shared by Light and Dark players. Make the logo and author credit very readable, with tasteful saturated colors against the calm deep navy field.
```
