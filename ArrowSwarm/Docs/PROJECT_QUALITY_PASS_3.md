# Project quality pass 3 — lifecycle follow-up

Baseline: `3cc995f`. Previous quality passes are complete. This scheduled continuation starts a narrow audit of newly demonstrated lifecycle defects, preserving gameplay, campaign data, authored styling and all preexisting work.

## Starting state

Unity and its MCP server were closed when the continuation began; the Unity tool namespace was unavailable. Root is starting the same installed Unity6000.3.18f1 project and local MCP10.3.0 bridge. Actual Unity MCP evidence is required before accepting production fixes. If the connection cannot be recovered, retain reproduction plans and report that runtime validation is blocked; do not claim tests passed.

Five font paths showed preexisting modifications at initial status (some may be line-ending/cache changes). Private byte copies and hashes protect this work; never stage, discard or overwrite it. All 364 authored scene/prefab/config/font files are protected against incidental test changes. Original Editor was closed; close the Editor and server started by this pass after verification, preserving the starting file state. Preserve PlayerPrefs values and key existence before Play Mode, and restore after leaving it. No live ads or synthetic cloud requests.

## Phases

- [x] 0. Read earlier completion records, inspect current state, protect existing changes, define this resumable implementation and commit it.
- [x] 1. Recover Unity MCP and establish before-fix runtime evidence with isolated Editor fixtures.
- [x] 2. Reject canceled ColorShift frame gestures and stale releases across pause/retry. Keep fresh click/swipe controls working; test actual Input System UI events.
- [x] 3. Retry pending leaderboard synchronization after an offline first opening and reconnection. Prevent duplicate/infinite refresh-submit loops; use a deterministic fake provider through the actual panel lifecycle.
- [x] 4. Preserve explicitly selected language when profile name/country is saved. Verify real UI Save without live cloud activity or losing draft/profile behavior.
- [x] 5. Run appropriate integration, restore exact preferences and preexisting file bytes, verify authored hashes and starting Editor/process state, publish evidence and commit the overview.

## Ownership and verification

Root controls MCP and all commits. Runtime agent owns the frame gesture fixture/fix; data agent owns cloud reconnect; UI agent owns profile language. Fixtures first, actual failing old-behavior evidence next, then minimal production changes. Every new script stays under200lines with summaries. Evidence belongs to `Captures/QualityPass3/`; private snapshots remain outside the repository. Keep previous pass reports intact.

## Completion record

Phase0 commit: `3e389a7`. Phase1 is complete. The installed MCP bridge reconnected to ArrowSwarm@3f240769be4f5e6f; native Unity tools are available again. All 364 authored hashes remained identical before implementation, and private byte copies are now retained for exact final restoration.

Real before-fix proofs are retained under Captures/QualityPass3/: frame-input-before.txt shows three actual UI gesture failures (OS cancellation, held release after Pause/Retry, and held release after Pause/Resume each consumed a turn, 2→1); fresh tap/swipe controls still passed. leaderboard-reconnect-before.txt shows real online reopening fetching rows while never submitting pending latest progress. profile-language-before.txt shows actual name Save preserving the name but switching explicit en preference/live/data language to tr because the country is TR. Fixtures restore user data, preference existence and scoped fake ownership; no live backend was used.

Implementation history: agents implemented the three minimal fixes in Edit Mode. The first frame fixture run stopped on an unready UI-raycast prerequisite; its settled rerun acquired native UI ownership and demonstrated all three defects. Strict event ACK/raycast checks were preserved, with actual layout settling added.

Phase1 evidence commit: `856cd80`. All three fixes compile without errors. Cloud phase3 passed26 focused checks using the actual offline-first panel, latest shared stars/profile, failed retries, explicit synchronization, and a delayed concurrent upload joined by refresh/explicit callers. Original startup10 and pending/version19 regressions also passed; their previous report files were restored and fresh results copied under QualityPass3. No live service was constructed. The refresh-only core prevents recursive or immediate duplicate retries. Frame and profile focused verification are next, followed by integration and restoration.

Phase3 commit: `33bd6f4`. Profile phase4 passed67 checks: actual Show/name/Save across ten explicit languages, mismatched profile countries, normal profile persistence/close and startup-default English with no explicit preference. Saving a profile now leaves language selection to the language setting; it no longer creates or replaces a language preference. Exact data object, preference existence, live language, modal state and offline provider ownership restored. Evidence: profile-language-verification.txt. Frame phase2 is running; no source changes are currently pending from agents.

Phase4 commit: `15e1df4`. Frame phase2 passed66 recorded assertions (including repeated restoration boundaries). Native Dynamic Input System UI events now preserve turn budget2→2 for Canceled, held Pause/Retry release and held Pause/Resume release; after each case a fresh side tap and horizontal swipe still consume exactly one turn. Session identity/AttemptVersion and state invalidation protect ownership; touch cancellation uses the event's actual device/touch ID. Authored timing, thresholds and appearance are unchanged. The fixture now settles Canvas layout before strict raycast/ownership assertions. Only these proven touch/pause/retry paths are covered, not all focus/device-reset combinations. Evidence: frame-input-verification.txt and retained pre-fix report.

Integration history: all implementation phases passed focused MCP tests before Defense101, all100 preview puzzle playthroughs,450 interactions and final restoration. Agents made no further production changes during integration.

Integration update: Defense regression now passes53 assertions after its existing wall-clock popup waits were hardened to actual CanvasGroup readiness and real player frames. Diagnostics confirm immediate Paused/timeScale0 with unchanged Freeze, then settled interactable popup, and immediate/settled resume with resumed countdown. Defense production code was not changed. The original timing failure is retained in defense-timing-before.txt. The background graphical Editor ran at roughly3fps; the original interaction timing gate observed1.184s against a .70s animation cap plus tolerance. That graphical scheduling result is retained in interaction-graphical-timing.txt and is not accepted as a device-performance measurement. Eleven graphical puzzle witnesses completed before an explicit cancel. Root is reopening the same project in Unity batchmode/nographics through the same MCP bridge to rerun the complete100-level and450-interaction suites with real player-loop animations and no duplicate screenshots. Normal graphical/native touch evidence is already complete; headless verification does not establish pixel rendering.

All phases are now complete. Frame commit: `97fee66`. Final batch MCP verification completed all100 puzzle levels (50Shapes+50ColorShift) with three lives, real pooled coroutine actions, frame orientation/presentation parity and no save writes. The full450 interaction suite passed, including its normal production timing checks. Fresh environment metadata explicitly records batchmode=True/graphics=Null; no screenshot/pixel-rendering claim is made for that run. Defense53 and native UI gesture66 evidence were obtained in the graphical Editor.

All five pre-Play preference values and key existence were restored exactly. After both owned Editors exited, all364 authored hashes and all five preexisting font byte copies matched the starting state. The local MCP server started by this pass was stopped; port8080 has no listener. The initial closed-Editor/closed-bridge state is restored. Generated solution ordering was reverted. All12 new scripts remain under200lines. Final native batch console: errors0/warnings11; warnings comprise the existing DeviceSimulator startup file-lock warning and orphan ConfettiParticle components. These preserved authored assets were not rewritten merely to silence warnings. Previous pass reports remain unchanged. No new automation was created.

## Commit map

| Commit | Work |
| --- | --- |
| 3e389a7 | Lifecycle plan and preexisting asset protection |
| 856cd80 | Actual pre-fix native UI, reconnect and language evidence |
| 33bd6f4 | Pending reconnect synchronization and concurrency verification |
| 15e1df4 | Profile Save preserves explicit language |
| 97fee66 | ColorShift canceled/stale gesture ownership |
| 3874d04 | Semantic Defense fixture waits, all campaigns/interactions and exact final restoration |
| Overview commit | Completed report index and scope/limitations |
