# November UI and gameplay revisions

Baseline: `1ae61974`. Scope: Home profile button, Levels/Leaderboard/Settings exits, the fatal rejected shot in all three modes, and only the Shapes board and arrows. No numbered redesign board is approved by this revision.

- [x] Read project rules and rollout notes; snapshot the existing working files and editor setup.
- [x] Capture the original Home and Shapes boards from Unity. Baseline: native 1080×1920, both themes, Shapes levels 1 and 15.
- [ ] Match the three menu exits to their entry animation, retaining modal input ownership until the exit completes.
- [ ] Generate and integrate one compact transparent profile sprite with the original button and navigation.
- [ ] Let the fatal rejected arrow return home before the loss popup, with inputs protected while the outcome is pending.
- [ ] Refine the Shapes playfield and arrows without changing the HUD, level content or shot rules.
- [ ] Check real menu navigation, loss timing and Shapes play in Unity; record themed before/after evidence and restore the original local files and editor state.

Resume: original captures are in Captures/UIRollout/Nov09/Before. Verify each isolated revision and commit the verified files.
