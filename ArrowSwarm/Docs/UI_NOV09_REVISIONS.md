# November UI and gameplay revisions

Baseline: `1ae61974`. Scope: Home profile button, Levels/Leaderboard/Settings exits, the fatal rejected shot in all three modes, and only the Shapes board and arrows. No numbered redesign board is approved by this revision.

- [x] Read project rules and rollout notes; snapshot the existing working files and editor setup.
- [x] Capture the original Home and Shapes boards from Unity. Baseline: native 1080×1920, both themes, Shapes levels 1 and 15.
- [x] Match the three menu exits to their entry animation, retaining modal input ownership until the exit completes. Native pointer, repeat, interruption and paused-clock checks passed at 1080×1920.
- [x] Generate and integrate one compact transparent profile sprite with the original button and navigation. Unity checked both themes, real press feedback and Profile navigation.
- [x] Let the fatal rejected arrow return home before the loss popup, with inputs protected while the outcome is pending. Native runtime passed in Defense, Shapes and Color Shift.
- [x] Refine the Shapes playfield and arrows without changing the HUD, level content or shot rules. The scene opts into a rounded inlay with subtle grid dots and an enamel arrow prefab variant.
- [x] Check real menu navigation, loss timing and Shapes play in Unity; record themed before/after evidence and restore the original local files and editor state.

Resume: this revision is complete. Open `Captures/UIRollout/Nov09/index.html` to review the native before/after and mid-animation frames. Wait for the next explicit user request before changing more screens or starting another numbered board.

## Profile artwork

`Assets/_Project/Art/Sprites/SharedUI/profile_player.png` is a single 256×256 transparent sprite (63,617-byte source; Android ASTC 6×6, no mipmaps). The existing button and frame are retained.

Built-in image generation prompt: Single transparent game sprite for a glossy blue puzzle game. Draw a friendly, unmistakable head-and-shoulders avatar with a round icy blue head, tiny smiling eyes, broad cobalt-to-cyan shoulders, an enamel bevel and one small golden sparkle. Center the avatar, keep it readable at 32 pixels, and omit the ring, background, lettering and other objects.

## Verification

- Menu exit: 48 actual Unity pointer dismissals across both themes at 1080×1920, 1080×2400, 720×1280 and 1536×2048. Covers all three Levels entries, both Leaderboard dismissal controls, Settings, duplicate close calls, interrupted navigation and unscaled animation while paused.
- Home: live profile-button feedback and navigation in both themes, localized Home layout and all three Play routes. The only MainMenu scene changes are the avatar name, sprite and size.
- Fatal move: real Defense, Shapes and Color Shift arrows were sent into obstacles from one remaining life. Each visibly reverses before the real loss panel opens; Defense publishes one loss event when its arrow is exactly back at its grid position. Pause does not interrupt the last return.
- Shapes: two shaders imported without errors. The same 50 puzzles were completed in Preview with all three lives, 3,906 accepted rapid shots, overlapping flights and no save writes. Actual Light/Dark screenshots cover early, middle and late levels and four Game View sizes. The HUD, catalog, arrow geometry and collision rules were preserved. Color Shift uses the original board and arrow assets.
- All 15 pre-existing modified files and five data-service preferences matched their originals. Unity was left in Edit Mode on BootScene with the original Game View size.

See `Captures/UIRollout/Nov09/summary.json` for native reports and capture sizes. The preview also works from disk without a server.
