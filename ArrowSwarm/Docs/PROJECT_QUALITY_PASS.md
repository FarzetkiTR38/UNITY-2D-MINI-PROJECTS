# Project quality pass — continuation plan

Baseline: `16941ad`. The user authorizes proactive inspection and improvements, with a commit and Unity MCP verification for each coherent change. Continue from the first unchecked phase when asked to resume. Preserve the existing game design, levels, artwork and scene styling; work from reproducible defects or measured hot paths.

## Phases

- [x] 0. Inspect project rules, establish a clean baseline and audit persistence, async UI and runtime code. Protect authored assets by hashes. Commit this implementation/continuation record.
- [x] 1. Preserve the last valid backup when recovering a corrupted primary save. Validate normal backup rotation and repeated recovery with temporary data, restoring real preferences afterwards.
- [x] 2. Distinguish leaderboard accounts by identity rather than display name; avoid modifying cached rows when applying the local player snapshot. Verify duplicate names, rename and cache immutability offline.
- [x] 3. Complete rewarded-ad requests once, including skips/failures; reject overlapping requests without overwriting callbacks. Test delayed/faulty service callbacks and production close/reward ordering without showing real ads.
- [x] 4. Prevent delayed Defense reward callbacks from using a skill in a later attempt or touching a destroyed popup/HUD. Preserve legitimately earned inventory. Exercise retry/menu/new-level transitions with delayed fake rewards.
- [x] 5. Restore all gap-close participants when another enemy death interrupts the retreat. Verify real pooled mobs, overlapping deaths, freeze, cancellation and reuse.
- [x] 6. Remove per-frame temporary position arrays from Defense arrow movement/spawn, preserving exact rendered points. Measure warmed allocation and validate actual flight/bounce.
- [x] 7. Avoid rebuilding every stationary Shapes arrow when clearing an already absent hint. Verify unchanged geometry and real rapid input; measure the no-op path.
- [x] 8. Make the menu level header consistently follow the selected mode and localization. Verify actual shared TMP label after mode changes, player-data notifications and language changes.
- [ ] 9. Marshal SDK callbacks onto the Unity main thread before lifecycle, game and UI access. Preserve reward/close order and release requests even if cleanup/preload throws; verify background thread dispatch without live ads.
- [ ] 10. Run integration checks appropriate to the changed paths through Unity MCP. Restore player data/settings and original editor scene, verify protected assets, publish reports and record commits/completion below.

## Evidence and working rules

Focused verification and screenshots belong to `Captures/QualityPass/`, with separate reports for each phase. Never use live ads or cloud writes for tests. Snapshot/restore every touched preference and in-memory player field; do not publish private player JSON in reports. Use explicit task paths when staging commits. Compile and run each focused test before its commit, then run integration once at the end unless a new failure justifies repeating it.

## Findings selected for work

- `DataManager.Load` calls `Save` after backup recovery; unconditional rotation can replace the good backup with the corrupted primary.
- `LeaderboardManager.GetTopPlayers` treats equal display names as identity and mutates cached row objects while applying local data.
- Rewarded close without reward never settles its callback; concurrent requests share one callback slot. Defense HUD/popup reward lambdas outlive their original owner/attempt.
- `MobSpawner` stops a gap-close coroutine without restoring all previously reversed participants; a later death changes list indices and can leave a surviving mob with negative speed.
- `ArrowVisuals` converts moving point lists to new arrays per frame. `PuzzleSession.Fire` clears highlights on all stationary arrows; `Highlight(false)` currently rebuilds even when unchanged.
- MainMenu and GameModeSelect write the same level TMP label using different mode/prefix logic.

## Deferred audit notes

`ObjectPool.Release` accepts duplicate releases, but the audit did not find a definite production trigger. Keep this as a follow-up rather than changing pool ownership semantics speculatively. General popup localization gaps can be assessed after the selected-mode header fix.

## Progress / resume notes

Implementation has started. Each completed phase must be checked above and described here with its MCP evidence and commit. If work is interrupted, leave the current phase and next concrete command/test documented here before stopping.

Phase 1 complete: Unity MCP Play Mode passed normal rotation, repeated damaged-primary recovery, missing/blank/both-broken slots, direct save and legacy three-mode migration. Original data reference, auto-save and five preferences restored. Evidence: Captures/QualityPass/save-verification.txt.

Phase 1 commit: 6ee6175. Phase 3 complete: Unity MCP passed 22 offline callback assertions for overlapping reward/interstitial requests, stale/duplicate events, provider exceptions, cancellation, and reward-before-close ownership. No live SDK playback exercised. Evidence: Captures/QualityPass/ads-verification.txt.

Phase 3 commit: 266f4b4. Phase 5 complete: Unity MCP real pooled Defense101 passed overlapping D/B deaths, retreat expiry, pause/freeze independence, StopSpawning and same-object/new-spawn reuse. Original configuration, progress and preferences restored. Evidence: Captures/QualityPass/gap-close-verification.txt.

Phase 5 commit: 29ba498. Phase 7 focused check complete: Unity MCP edit-mode real PuzzleArrow prefab passed repeated clear/hint geometry, actual transitions, in-flight window and Rocket pool rebind. The initial Mono byte measurement was later rejected by a positive-control check; see native profiler verification below. Rapid gameplay integration remains in phase 10. Evidence: Captures/QualityPass/hint-performance-verification.txt.

Phase 7 commit: 6e1c7b3. Phase 2 complete: Unity MCP passed duplicate nicknames, authenticated identity, profile rename, aggregate stars, cloud-ahead scores, detached rows/cache immutability, null/empty cache and ranking. The lazy manager fixture is inactive to avoid UGS initialization. Evidence: Captures/QualityPass/leaderboard-verification.txt.

Phase 2 commit: ef08fc4. Phase 4 complete: Unity MCP real Defense HUD passed 12 assertions for duplicate taps, live Hint/Freeze, failure, same-level retry, disabled HUD and destroyed HUD/Hint/Freeze popups. Earned inventory retained once; original inventory/provider/preferences restored. Evidence: Captures/QualityPass/reward-owner-verification.txt. Measurement correction: Mono byte-counter results in the initial phase 7 report are invalid; native Unity GC.Alloc positive-control verification is now being run before accepting the performance claim.

Phase 4 commit: 225dc1c. Phase 7 measurement correction passed via Unity MCP: native GC.Alloc recorder detects a deliberate managed allocation, empty bracket=0 events, 10,000 repeated clear=0 events and repeated hint=0 events. Geometry/pool/config checks also passed again. The corrected report replaces the unsupported byte claim. Shared native allocation helper is now used for phase 6.

Measurement correction commit: 3854e5f. Phase 8 complete: Unity MCP actual shared TMP menu header passed 92 assertions across Defense/Shapes/ColorShift, separate progress, both UI refresh writers, data notifications and all ten localized prefixes. Existing scene styling untouched. Evidence: Captures/QualityPass/menu-header-verification.txt.

Phase 8 commit: b8f4fae. Phase 6 complete: Unity MCP real Defense renderer preserved every point across growing/shrinking body buffers and all head poses. Native GC.Alloc: empty bracket=0, old ToArray=2,048 events, current slither=0 and shared spawn uploader=0 across 2,048 calls. Gap-close isolation and original renderer restoration passed too. Evidence: Captures/QualityPass/arrow-allocation-verification.txt.

Phase 6 commit: 93ba59f. Integration so far: original Defense101 full regression passed, then all 50 Shapes plus 50 ColorShift levels passed real pooled runtime playthroughs. Additional final-review finding: installed GoogleMobileAds 11.5 changelog and official Unity global-settings docs require main-thread dispatch for callbacks that touch Unity objects. Added phase 9; fake service tests cannot prove SDK thread affinity, so this gets a real background-thread/Unity-update check before final acceptance. Source: https://developers.google.com/admob/unity/global-settings#raise_ad_events_on_the_unity_main_thread .

Phase 10 integration evidence complete so far: Defense101 passed 51 assertions; all 100 puzzle levels completed with three lives through actual pooled animations; both modes passed 451 real-input/skill/recovery/rapid-shot assertions. 100 untouched gameplay captures saved under Captures/QualityPass/Playthroughs. Console had zero errors after this run. Editor returned to original BootScene; all 364 protected authored files match baseline SHA256. Final acceptance still waits for phase 9 native SDK callback threading test and final preference restoration. Phase 9 is the current resume point.
