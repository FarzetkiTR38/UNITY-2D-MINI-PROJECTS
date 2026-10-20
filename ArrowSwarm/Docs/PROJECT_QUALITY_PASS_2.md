# Project quality pass 2 — scheduled continuation

Baseline: `10d4919`, clean working tree. The previous pass is complete; its reports remain in `Captures/QualityPass/`. This run was started by the user-authorized, single four-hour follow-up. Do not create another automation unless requested.

Preserve working gameplay, all campaigns, artwork and authored styling. Changes require a concrete repro or measured hot path. Root controls Unity MCP; agents own separate source areas and never commit or manipulate the Editor. New scripts stay within 200 lines and project architecture rules.

## Phases

- [x] 0. Review previous work and project rules, establish a clean baseline, audit new areas and protect authored assets. Commit this continuation plan.
- [x] 1. Correct Defense touch cancellation and multi-touch tap ownership. A canceled OS touch must never fire an arrow or lose a life; fresh single-finger taps still work.
- [ ] 2. Make the camera follow actual active touch controls and rebase when fingers change. Reset drag/pinch state after pan-to-pinch-to-release/cancel. Verify real Input System events.
- [ ] 3. Remove temporary HUD corner arrays from camera pan/smooth-zoom calculations. Confirm exact bounds and native GC.Alloc counts with positive controls.
- [ ] 4. Restore the existing localization keys in Defense HUD, profile Save/Skip reopening and level-range labels. Language changes refresh text without resetting modal drafts, awards or page selection. Preserve Inspector styling and dictionaries.
- [x] 5. Start the leaderboard service on the first real panel opening so cached/global rows can appear. Verify laziness before opening, cached immediate rendering and safe panel lifecycle without live cloud traffic.
- [ ] 6. Preserve pending score synchronization when progress changes before the lazy leaderboard manager exists. Verify initialization consumes the latest score through a deterministic fake service, restoring all real data/preferences.
- [ ] 7. Run appropriate integration and visual checks, restore original player preferences and BootScene/Edit Mode, compare protected hashes, publish reports and commit the final overview.

## Verification and safety of existing work

Evidence belongs to `Captures/QualityPass2/`. Do not overwrite earlier reports. Snapshot data object/settings and preference existence before tests; restore them in finally. No synthetic scores, test names or accounts may be sent to a live backend. Use deterministic service adapters for asynchronous cloud tests. Do not run actual ad SDK loads or shows.

Touch tests must use Input System devices/events, not only private state assertions. Camera tests must cover sparse slots, finger-ID replacement and gesture transitions. Native allocation measurement must include an allocation-producing positive control, since the Editor's Mono byte counter was proven unreliable in pass 1.

Localize through existing `menu_level_short`, `hud_arrows`, `profile_save_play`, `profile_skip`, and `menu_levels` keys; do not rewrite authored colors/fonts/sprites or invent a new UI layout. Current GameOver/LevelComplete English subtitle code has no bound visible TMP target, so it is not a confirmed visible defect and is outside this pass.

## Progress / resume point

Audit complete. Implementation starts with the first unchecked phase. Each phase must record its actual MCP evidence and commit below before completion. If interrupted, resume at the first unchecked phase and inspect the current working tree before editing.

## Deferred notes

Duplicate ObjectPool release has no demonstrated production trigger; keep deferred. Do not alter gameplay or pool ownership speculatively. Cloud concurrency concerns require a controlled reproducer before expanding the two confirmed leaderboard fixes.

Phase 5 complete: Unity MCP passed 10 actual first-open panel/cache/lifecycle checks using a delayed fake provider. Opening creates one lazy manager, cached rows appear immediately, late refresh does not reveal a closed panel, and reopening reuses it. Exact data/preference/provider restoration passed. Evidence: Captures/QualityPass2/leaderboard-startup-verification.txt.

Phase 5 commit: 896fbef. Phase 1 complete: real queued Input System touches in Defense101 proved canceled touch causes no shot/life loss, two-finger release cannot become a tap, canceled gestures clear ownership, and a fresh single-finger release fires exactly one legal arrow. Original devices, settings, camera, player and preferences restored. Evidence: Captures/QualityPass2/touch-input-verification.txt. Controlled old score method also reproduced pending-loss race; before report retained for phase 6.
