# Arrow Swarm — quality pass

Baseline: `16941ad`. This pass preserves the existing design, campaigns, artwork and scene styling.

## Fixed behavior

| Area | Result | Focused Unity MCP evidence |
| --- | --- | --- |
| Save recovery | A damaged primary save can no longer replace a healthy backup during recovery or direct save. | [35 checks](save-verification.txt) |
| Leaderboard | Duplicate nicknames stay separate accounts; local profile changes use detached display rows without altering the cache. | [28 checks](leaderboard-verification.txt) |
| Fullscreen ads | One request owns its callback; overlapping taps, duplicate/stale results, failure and unrewarded close settle safely. Earned rewards wait until fullscreen close. | [22 checks](ads-verification.txt) |
| Defense skill rewards | Late rewards remain earned inventory and cannot auto-use skills in another attempt or access destroyed UI. | [12 checks](reward-owner-verification.txt) |
| Enemy gap closing | Interrupted retreats restore the original live participants; pooled reuse is protected by spawn identity. | [39 checks](gap-close-verification.txt) |
| Defense arrow rendering | Spawn and moving bodies reuse their upload array while preserving exact positions and head pose. | [201 checks](arrow-allocation-verification.txt) |
| Shapes hint clearing | Repeating an unchanged hint state skips geometry rebuilding; real transitions, flight and pool rebind remain correct. | [12 checks](hint-performance-verification.txt) |
| Menu level header | Both UI refresh paths follow the selected mode's progress and current localized prefix. | [92 checks across 10 languages](menu-header-verification.txt) |
| Native SDK callbacks | Unity object access runs on the main thread; earlier background rewards stay ahead of a later close, and cleanup exceptions cannot strand the caller. | [45 real-thread checks](ad-thread-verification.txt) |

## Integration

- [Original Defense101](Defense/defense-verification.txt): 50 checks covering original HUD, path/portal/enemies, flight, blocked bounce/life loss, Hint/Freeze, pause and retry. [Gameplay capture](Defense/defense-101.png).
- [All 100 runtime playthroughs](Playthroughs/runtime-verification.txt): 50 Shapes + 50 ColorShift, three lives, real pooled animations and no progress writes. Each level has a separate capture in that folder.
- [450 interaction checks](interaction-verification.txt): rapid Shapes input, both modes' Rocket and recovery, click/swipe rotation, rounded flowing frame, pause, timing and visual/logical parity.

## Performance evidence

The Editor's `GC.GetAllocatedBytesForCurrentThread()` returns zero even for deliberate allocations. Its initial result was rejected and the tests now use Unity's native `GC.Alloc` event recorder with positive and empty-bracket controls.

- 2,048 old `ToArray()` uploads: **2,048 allocation events**.
- 2,048 current slither calls and 2,048 shared spawn uploads: **0 allocation events** each after warm-up.
- 10,000 repeated Shapes clear calls and 10,000 repeated hint calls: **0 allocation events** each after warm-up.

These are controlled synchronous hot-path measurements, not whole-frame or whole-game zero-allocation claims. A buffer's first allocation or growth is intentionally outside the warmed measurements.

## Scope and reruns

Verification uses temporary data, deterministic ad providers and preview campaigns. No real ads are shown and no cloud scores are submitted by the fixtures. Live Android/iOS ad playback, third-party mediation ordering and physical-device performance still require device validation.

The phased implementation, commit record and resume point are in [the continuation plan](../../Docs/PROJECT_QUALITY_PASS.md). Focused entry points are the `Quality*Verification` classes under `Assets/_Project/Scripts/Modes/Editor`; run through Unity MCP. `QualityHintPerformanceVerification.Run()` uses Edit Mode. Save, leaderboard, ads and menu checks use Play Mode in the normal menu. Gap and allocation `Begin()` methods safely load Defense101; the reward-owner check requires a fresh playing Defense scene.

## Final state

[Final verification](final-state-verification.txt): all five pre-Play preferences were restored exactly, including key existence. All 364 protected scenes, ScriptableObjects, prefabs and fonts match the baseline SHA256. The Editor is back in BootScene with Play Mode off. Native Unity console counts show zero errors; two warnings deliberately exercise throwing fake ad providers.

The pass is complete in 14 focused commits, including the initial continuation plan, measured fixes and integration evidence. The four-hour follow-up is attached to this same chat; it can use the plan to distinguish completed work from a fresh project audit.
