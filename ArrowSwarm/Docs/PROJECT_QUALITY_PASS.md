# Project quality pass — continuation plan

Baseline: `16941ad`. The user authorizes proactive inspection and improvements, with a commit and Unity MCP verification for each coherent change. Continue from the first unchecked phase when asked to resume. Preserve the existing game design, levels, artwork and scene styling; work from reproducible defects or measured hot paths.

## Phases

- [x] 0. Inspect project rules, establish a clean baseline and audit persistence, async UI and runtime code. Protect authored assets by hashes. Commit this implementation/continuation record.
- [x] 1. Preserve the last valid backup when recovering a corrupted primary save. Validate normal backup rotation and repeated recovery with temporary data, restoring real preferences afterwards.
- [ ] 2. Distinguish leaderboard accounts by identity rather than display name; avoid modifying cached rows when applying the local player snapshot. Verify duplicate names, rename and cache immutability offline.
- [x] 3. Complete rewarded-ad requests once, including skips/failures; reject overlapping requests without overwriting callbacks. Test delayed/faulty service callbacks and production close/reward ordering without showing real ads.
- [ ] 4. Prevent delayed Defense reward callbacks from using a skill in a later attempt or touching a destroyed popup/HUD. Preserve legitimately earned inventory. Exercise retry/menu/new-level transitions with delayed fake rewards.
- [x] 5. Restore all gap-close participants when another enemy death interrupts the retreat. Verify real pooled mobs, overlapping deaths, freeze, cancellation and reuse.
- [ ] 6. Remove per-frame temporary position arrays from Defense arrow movement/spawn, preserving exact rendered points. Measure warmed allocation and validate actual flight/bounce.
- [ ] 7. Avoid rebuilding every stationary Shapes arrow when clearing an already absent hint. Verify unchanged geometry and real rapid input; measure the no-op path.
- [ ] 8. Make the menu level header consistently follow the selected mode and localization. Verify actual shared TMP label after mode changes, player-data notifications and language changes.
- [ ] 9. Run integration checks appropriate to the changed paths through Unity MCP. Restore player data/settings and original editor scene, verify protected assets, publish reports and record commits/completion below.

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
