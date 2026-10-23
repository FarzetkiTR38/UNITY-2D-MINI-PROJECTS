# Arrow Swarm — quality pass 3

Baseline: `3cc995f`. This lifecycle follow-up preserves the campaigns, artwork and authored styling from the completed [previous pass](../QualityPass2/README.md).

## Confirmed fixes

| Area | Behavior after the fix | Unity MCP evidence | Commit |
| --- | --- | --- | --- |
| Color Shift rotation gestures | OS cancellation and held releases across Pause/Retry or Pause/Resume keep the turn budget at **2 → 2**. After each case, a fresh side tap and horizontal swipe still spend exactly one turn. | [66 recorded assertions](frame-input-verification.txt) using actual Dynamic Input System UI events | `97fee66` |
| Leaderboard reconnect | Reopening an offline-created panel after reconnection submits the latest pending level, shared campaign stars and profile. A failed upload stays pending; each later refresh retries once. Explicit synchronization and overlapping callers share an active upload without recursive retries. | [26 checks](leaderboard-reconnect-verification.txt), deterministic provider and real panel lifecycle | `33bd6f4` |
| Profile language | Saving a name or country preserves the explicitly selected language. The startup-default English case also stays unchanged and does not create a language preference. | [67 checks across all ten languages](profile-language-verification.txt), actual Show/name/Save actions | `15e1df4` |

Counts exclude each suite headline. The frame report includes repeated preference-restoration assertions at separate lifecycle boundaries; these are not 66 distinct gesture scenarios.

## Before-fix evidence

These reports deliberately contain failures and remain as controlled reproduction evidence:

- [Frame input before](frame-input-before.txt): cancellation, held Pause/Retry release and held Pause/Resume release each incorrectly consumed a turn, **2 → 1**. Fresh tap/swipe controls already worked.
- [Leaderboard reconnect before](leaderboard-reconnect-before.txt): the actual online reopening fetched global rows but never uploaded the pending local score.
- [Profile language before](profile-language-before.txt): saving a name with country TR changed explicit English to Turkish while the name itself saved normally.

[Before-fix Color Shift board and rotation bar](color-frame-before.png) is an appearance reference, **not an after-fix capture**. Gesture correctness is demonstrated by the native UI event reports. Rotation timing, thresholds and artwork were not redesigned.

## Affected regressions

- [Startup regression: 10 checks](leaderboard-startup-regression.txt): lazy first opening, immediate cached rows, delayed completion, closed-panel ownership and provider cleanup. The [earlier report](../QualityPass2/leaderboard-startup-verification.txt) was preserved.
- [Pending/version regression: 19 checks](leaderboard-pending-regression.txt): changes before service creation, latest multi-mode score/profile, offline submission and both orders of overlapping score results. The [earlier report](../QualityPass2/leaderboard-pending-verification.txt) was preserved.

Cloud fixtures install a deterministic Editor-only provider before singleton Awake. No UGS authentication, live network requests, test accounts or synthetic score uploads occur. Profile fixtures likewise use a scoped offline provider. Exact player data, preference values/key existence, live language, modal state and provider ownership are restored by the focused tests.

The gesture evidence covers the proven touch cancellation and Pause/Retry/Resume paths. It does not claim coverage of every focus-loss, device-reset or physical-device interaction. Live backend behavior and Android device testing remain outside these controlled Editor checks.

## Integration

- [Original Defense101](Defense/defense-verification.txt): **53 assertions** with original HUD, portal/enemies, flight/bounce/life loss, Hint/Freeze, pause/resume and retry. [Fresh graphical capture](Defense/defense-101.png).
- [All100 fresh puzzle playthroughs](Playthroughs/runtime-verification.txt): **50Shapes+50ColorShift**, all cleared with three lives through real pooled coroutine animations, frame/parity/presentation and no-save-write checks.
- [450 interaction assertions](interaction-verification.txt): both modes' Rocket/recovery, rapid Shapes shots, rounded flowing frame, click/swipe rotation, pause and production animation timing checks passed.

[Execution environment](integration-environment.txt) records the full campaign and450-interaction runs as **Unity batchmode/nographics, graphics device Null**. They verify runtime state, geometry/presentation components and coroutine behavior; they do not prove pixel rendering or device performance. Native queued UI-touch evidence and Defense visuals were verified in the normal graphical Editor. Duplicate campaign screenshots were disabled; previous actual captures remain in [QualityPass/Playthroughs](../QualityPass/Playthroughs/runtime-verification.txt).

The graphical Editor ran at roughly3fps in the background. An [earlier Defense check](defense-timing-before.txt) read popup interactivity after a wall-clock delay; the fixture now waits for actual CanvasGroup readiness and player frames while preserving strict immediate pause and Freeze invariants. Final diagnostics show the timer unchanged during pause and decreasing after resume. An [earlier graphical interaction timing gate](interaction-graphical-timing.txt) observed1.184s against the .70s configured cap plus scheduling tolerance. That result is retained as a throttled-Editor diagnostic; the complete suite subsequently passed in batchmode. Defense production code was unchanged.

## Final state

Unity and its local MCP server were **closed at the starting baseline**. The installed Unity6000.3.18f1 project and bridge were reopened for actual MCP verification. Both Editors and the owned bridge are now closed again; port8080 has no listener.

Five font paths were already modified before this pass: `Roboto-Bold SDF.asset`, `Fazo_Font.asset`, `Fazo_Font_Blueoutline.asset`, `Fazo_Font_Settings.asset` and `NotoSansJP_SDF.asset`. [Final verification](final-state-verification.txt) confirms their privately preserved bytes remain intact and outside these commits, all **364** authored hashes match the starting state, and all five pre-Play preference values/key existence were restored exactly. No private player JSON is published. All12 new scripts stay within200lines; whitespace checks pass.

Final batch native console: **0 errors, 11 warnings**. Warnings comprise one existing Device Simulator generation/file-lock warning during startup and10 reports of orphan ConfettiParticle components. The existing authored particle/pool behavior and protected assets were preserved.

The pass is complete in **7 commits**. Implementation/completion records are in [PROJECT_QUALITY_PASS_3.md](../../Docs/PROJECT_QUALITY_PASS_3.md); integration/restoration evidence is committed as `3874d04`. Only the starting font modifications remain outside this pass. No new automation was created.
