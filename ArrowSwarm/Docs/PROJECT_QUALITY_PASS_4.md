# Project quality pass 4 — whole-game improvement roadmap

Baseline: `5f057df`. User authorizes a broad, autonomous quality pass: enter every game/UI flow through Unity MCP, verify Light/Dark, fix proven failures, improve visual quality and measured performance, and list valuable future features. Every coherent implementation phase gets a separate commit. Read this file first when asked to continue.

## Preserve
- Existing Defense rules, three separate campaigns, shared stars and current working progress remain protected.
- Previous quality passes 1–3 are complete; do not repeat their fixes. Read their records before selecting a finding.
- Current dirty solution/font/ad-prefab files and five player preferences are privately snapshotted. Never stage preexisting changes or publish player JSON. Restore test-owned state in finally.
- No live ads, synthetic leaderboard submissions or cloud writes in fixtures. Use scoped service adapters. Leaderboard concepts from the previous task remain preview-only until a design is chosen.
- Root owns Unity MCP and commits; agents audit/implement disjoint source areas. No script edits while runtime fixtures are active; compile and inspect console before accepting changes.
- Static visuals remain scene/material/Inspector authored. TMP/localization and existing input effects are retained. New scripts stay under 200 lines.

## Phases / resume checklist
- [ ] 0. Read previous records, snapshot starting state, commit this roadmap and establish current Unity baseline.
- [ ] 1. Sweep all six scenes and visible popup families in both themes. Capture actual renders; inventory missing scripts/references, navigation and localization coverage.
- [ ] 2. Audit data/profile/leaderboard asynchronous behavior and implement only reproducible reliability/UX fixes, with offline before/after evidence.
- [ ] 3. Audit Defense, Shapes and Color Shift lifecycle/input/skills/pooling. Reproduce and fix demonstrated gameplay problems; profile any proposed optimization with positive controls.
- [ ] 4. Improve confirmed visual/readability/localization issues found in the full sweep. Verify each affected screen and input path before its commit.
- [ ] 5. Exercise real Defense, tutorial/pause/win/lose/retry/menu and all 100 puzzle levels plus representative real-input/skill/rotation/camera flows. Distinguish graphical rendering evidence from timing/performance evidence.
- [ ] 6. Publish a prioritized feature list with benefit, scope and dependencies; implement small evidence-backed improvements where appropriate, leaving speculative/new mechanics clearly separate.
- [ ] 7. Final regressions, original preference/file preservation, screenshots and overview. Record finished commits and next resume point explicitly.

## Entry points to verify
Boot/startup; main Play for each mode; mode selector; separate level pages/locked levels; settings/theme/language/audio/haptics; profile name/country/Save/Skip/back; leaderboard/cache/offline/reopen; Defense tutorial/HUD/hint/freeze/pause/retry/win/lose; Shapes rapid shots/bounce/lives/hint/Rocket/progress; Color Shift legal/blocked arrows, wrong wall, clicks/swipes/rotation limit/extra turns/dead ends; pinch/pan/cancel; map-preview tooling. Check source bindings, actual rendered states and real callbacks/input where meaningful.

## Evidence and acceptance
New evidence: `Captures/QualityPass4/`. Each phase records reproduction, final behavior, meaningful MCP checks, restoration and commit. Do not label an untested path as passed. Visual preference changes must fit the game's established glossy blue/gold style and both themes. Avoid redesign solely for novelty.

## Current resume point
Initial read/snapshot complete. Three independent audits (cloud/data, gameplay/performance, UI/theme/localization) are running. Root is starting the 30-screen paired graphical sweep. No new production finding is accepted yet. Continue from phase 1 after inspecting current reports/working tree.

## Findings / commits / ideas
To be updated as evidence is obtained. Previously observed cloud Leaderboards connectivity warning is a candidate for graceful offline UX investigation; it is not automatically a newly introduced defect or permission to hide errors.
