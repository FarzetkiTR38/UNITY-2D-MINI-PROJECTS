# Arrow Swarm — quality pass 2

Baseline: `10d4919`. This continuation improves confirmed input, localization and leaderboard issues while preserving the existing campaigns, artwork and authored styling. The previous pass remains in [QualityPass](../QualityPass/README.md).

## Fixed behavior

| Area | Result | Unity MCP evidence | Commit |
| --- | --- | --- | --- |
| Defense touch ownership | OS cancellation never fires an arrow or spends a life. Multi-finger gestures cannot become release taps; a subsequent single-finger tap still fires once. | [29 recorded checks](touch-input-verification.txt) | `dd7b1ca` |
| Camera gestures | Pinch and pan use actual pressed fingers, including sparse slots. Finger replacement rebases the gesture; release/cancel after pan-to-pinch clears ownership. | [32 recorded checks](camera-touch-verification.txt) | `d6fdbf3` |
| Camera allocation | HUD-boundary reads reuse corner arrays without changing authored ratios or camera clamps. | [34 recorded checks and native measurements](camera-performance-verification.txt) | `4c4c723` |
| Localization and profile visibility | Defense counters, reopened profile buttons and campaign page ranges follow all ten configured languages. Language refresh preserves the draft, pagination and counter feedback. Profile opening now raises the real modal above the reference menu. | [313 checks](localization-verification.txt) and visual review below | `0f17cd1` |
| First leaderboard opening | The real panel starts its lazy service, displays cached rivals immediately, accepts delayed global results and reuses the service on reopening. Late completion never reveals a closed panel. | [10 checks](leaderboard-startup-verification.txt) | `896fbef` |
| Pending leaderboard scores | Changes made before service creation remain pending. Startup submits the latest level, shared campaign stars and profile. Only the newest submission result can change the pending flag. | [19 checks](leaderboard-pending-verification.txt) | `0262ac9` |

Counts exclude the suite headline. The touch/camera reports include repeated preference-restoration assertions at separate lifecycle boundaries; these counts are not a count of distinct gameplay scenarios.

## Controlled regression evidence

- Real Input System touch events were queued and acknowledged in actual **Dynamic** input updates before assertions. The reports include frame acknowledgements, active device identity and sparse touch-slot state. Editor control reads alone were not accepted as gameplay evidence.
- The [original camera method](camera-performance-before.txt) produced **4,096 native `GC.Alloc` events over 2,048 calls**. The final measurement recorded **empty bracket = 0, original arrays = 4,096, cached HUD ratios = 0, cached camera clamp = 0**. Exact original clamps were checked at three zoom levels. These are warmed, synchronous hot-path measurements, not a claim of zero allocations for an entire frame or game.
- The [unguarded real submission method](leaderboard-pending-before.txt) reproduced the race: a newer failed score left pending work, then an older success incorrectly cleared it. The final method passed both completion orders, including an older failure arriving after the newest success.

The two `before` reports intentionally contain the old failures; they are retained reproduction evidence.

## Visual review

- [German profile after the draw-order fix](profile-de-639280627770566412.png); [before, hidden behind the reference menu](profile-before-draw-order.png).
- [Actual Japanese Defense HUD](defense-ja-639280627729315984.png).
- [Turkish campaign levels](levels-tr-639280627808019540.png) and [actual Korean level range](levels-ko-visual.png).

The localization report retains TMP preferred-height/overflow diagnostics. Some labels use the existing Overflow behavior and report a preferred height larger than their RectTransform. Actual CJK HUD/range text was visually reviewed and remains visible; the functional PASS headline is not a blanket assertion that every diagnostic overflow flag is false. Inspector fonts, colors and bounds remain unchanged.

## Integration

- [Original Defense101](Defense/defense-verification.txt): **50 checks** covering the authored HUD, portal/path/enemies, arrow flight, blocked bounce and life loss, Hint/Freeze, pause and retry. [Gameplay capture](Defense/defense-101.png).
- [All 100 fresh runtime playthroughs](Playthroughs/runtime-verification.txt): **50 Shapes + 50 ColorShift**, completed with three lives through actual pooled animations. Frame parity, presentation, pause and no-save-write checks all passed. `BeginWithoutCaptures` skips only duplicate screenshot creation; the previous hundred captures remain under QualityPass/Playthroughs.
- [450 interaction checks](interaction-verification.txt): rapid Shapes shots, both modes' Rocket/recovery, rounded flowing frame, real click/swipe rotations, pause, timing and visual/logical parity all passed.

## Scope and reruns

Leaderboard fixtures install a private Editor-only provider factory before the real singleton's Awake. Initialization, score results and global rows come from deterministic delayed tasks; no test names, synthetic scores, UGS authentication or network requests are sent to a live backend. The default production provider remains Unity Gaming Services.

Fixtures restore player data, settings, preference values and key existence, input devices/update settings, preview/debug context and provider ownership. Visual fixes use existing scene styling and localization dictionaries. Physical Android touch feel, device performance and live backend behavior are outside these controlled Editor checks.

The phased implementation and completion record are in [PROJECT_QUALITY_PASS_2.md](../../Docs/PROJECT_QUALITY_PASS_2.md). `Quality2LeaderboardStartupVerification.BeginStartup()` and `BeginPending()` require a fresh main-menu Play session before opening the leaderboard; poll `Status` for completion. Input/camera/performance entry points are on `Pass2TouchVerification`; localization uses `Quality2LocalizationVerification`. All fixtures are under `Assets/_Project/Scripts/Modes/Editor`.

## Final state

[Final verification](final-state-verification.txt): all five pre-Play preferences were restored exactly, including key existence. All 364 protected scenes, ScriptableObjects, prefabs and fonts match baseline SHA256. The original BootScene is open with Play Mode off. All 25 new scripts stay within 200 lines; source whitespace checks pass.

Native Unity console counts after the complete integration run: **0 errors, 7 warnings**. The warnings come from existing authored assets: an orphan Editor cleanup component on ConfettiParticle and U+2715 absent from the close label's LiberationSans font. Both were verified in baseline `10d4919`. Native particles and runtime pool cleanup remain intact; the protected assets were preserved.

The pass is complete in **9 commits**, including the initial plan, six focused implementation phases, integration evidence (`8c01ab8`) and this overview. No further automation was created.
