# November UI and gameplay revisions

Baseline: `1ae61974`. Scope: Home profile button, Levels/Leaderboard/Settings exits, the fatal rejected shot in all three modes, and only the Shapes board and arrows. No numbered redesign board is approved by this revision.

- [x] Read project rules and rollout notes; snapshot the existing working files and editor setup.
- [x] Capture the original Home and Shapes boards from Unity. Baseline: native 1080×1920, both themes, Shapes levels 1 and 15.
- [x] Match the three menu exits to their entry animation, retaining modal input ownership until the exit completes. Native pointer, repeat, interruption and paused-clock checks passed at 1080×1920.
- [x] Generate and integrate one compact transparent profile sprite with the original button and navigation. Unity checked both themes, real press feedback and Profile navigation.
- [ ] Let the fatal rejected arrow return home before the loss popup, with inputs protected while the outcome is pending.
- [ ] Refine the Shapes playfield and arrows without changing the HUD, level content or shot rules.
- [ ] Check real menu navigation, loss timing and Shapes play in Unity; record themed before/after evidence and restore the original local files and editor state.

Resume: original captures are in Captures/UIRollout/Nov09/Before. Verify each isolated revision and commit the verified files.

## Profile artwork

`Assets/_Project/Art/Sprites/SharedUI/profile_player.png` is a single 256×256 transparent sprite (63,617-byte source; Android ASTC 6×6, no mipmaps). The existing button and frame are retained.

Built-in image generation prompt: Single transparent game sprite for a glossy blue puzzle game. Draw a friendly, unmistakable head-and-shoulders avatar with a round icy blue head, tiny smiling eyes, broad cobalt-to-cyan shoulders, an enamel bevel and one small golden sparkle. Center the avatar, keep it readable at 32 pixels, and omit the ring, background, lettering and other objects.
