# Project quality pass 3 — lifecycle follow-up

Baseline: `3cc995f`. Previous quality passes are complete. This scheduled continuation starts a narrow audit of newly demonstrated lifecycle defects, preserving gameplay, campaign data, authored styling and all preexisting work.

## Starting state

Unity and its MCP server were closed when the continuation began; the Unity tool namespace was unavailable. Root is starting the same installed Unity6000.3.18f1 project and local MCP10.3.0 bridge. Actual Unity MCP evidence is required before accepting production fixes. If the connection cannot be recovered, retain reproduction plans and report that runtime validation is blocked; do not claim tests passed.

Five font paths showed preexisting modifications at initial status (some may be line-ending/cache changes). Private byte copies and hashes protect this work; never stage, discard or overwrite it. All 364 authored scene/prefab/config/font files are protected against incidental test changes. Original Editor was closed; close the Editor and server started by this pass after verification, preserving the starting file state. Preserve PlayerPrefs values and key existence before Play Mode, and restore after leaving it. No live ads or synthetic cloud requests.

## Phases

- [x] 0. Read earlier completion records, inspect current state, protect existing changes, define this resumable implementation and commit it.
- [x] 1. Recover Unity MCP and establish before-fix runtime evidence with isolated Editor fixtures.
- [ ] 2. Reject canceled ColorShift frame gestures and stale releases across pause/retry. Keep fresh click/swipe controls working; test actual Input System UI events.
- [ ] 3. Retry pending leaderboard synchronization after an offline first opening and reconnection. Prevent duplicate/infinite refresh-submit loops; use a deterministic fake provider through the actual panel lifecycle.
- [ ] 4. Preserve explicitly selected language when profile name/country is saved. Verify real UI Save without live cloud activity or losing draft/profile behavior.
- [ ] 5. Run appropriate integration, restore exact preferences and preexisting file bytes, verify authored hashes and starting Editor/process state, publish evidence and commit the overview.

## Ownership and verification

Root controls MCP and all commits. Runtime agent owns the frame gesture fixture/fix; data agent owns cloud reconnect; UI agent owns profile language. Fixtures first, actual failing old-behavior evidence next, then minimal production changes. Every new script stays under200lines with summaries. Evidence belongs to `Captures/QualityPass3/`; private snapshots remain outside the repository. Keep previous pass reports intact.

## Resume point

Phase0 commit: `3e389a7`. Phase1 is complete. The installed MCP bridge reconnected to ArrowSwarm@3f240769be4f5e6f; native Unity tools are available again. All 364 authored hashes remained identical before implementation, and private byte copies are now retained for exact final restoration.

Real before-fix proofs are retained under Captures/QualityPass3/: frame-input-before.txt shows three actual UI gesture failures (OS cancellation, held release after Pause/Retry, and held release after Pause/Resume each consumed a turn, 2→1); fresh tap/swipe controls still passed. leaderboard-reconnect-before.txt shows real online reopening fetching rows while never submitting pending latest progress. profile-language-before.txt shows actual name Save preserving the name but switching explicit en preference/live/data language to tr because the country is TR. Fixtures restore user data, preference existence and scoped fake ownership; no live backend was used.

Current resume point: agents are implementing the three minimal fixes in Edit Mode. Wait for stable files, compile, run each focused fixture and older affected regressions before its commit. The first frame fixture run stopped on an unready UI-raycast prerequisite; its settled rerun acquired native UI ownership and demonstrated all three defects. Preserve strict event ACK/raycast checks and improve the initial layout settling rather than accepting the unready run as evidence.
