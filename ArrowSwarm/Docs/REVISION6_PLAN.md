# Revision 6 — sample leaderboard, focused Dark repairs and AI concept gallery

Baseline: `c8966057`. This is the user's four-part request after Quality Pass 5. Read this first when resuming. Root owns Unity MCP and commits; agents work in disjoint source areas. No questions or extra approvals are needed inside the authorized scope.

## Strict scope
- Implement only: clearly marked, opt-in Editor leaderboard sample data; Defense death-effect Dark repair; gameplay HUD Pause artwork Dark repair.
- All AI redesigns are **preview only**. Do not replace any game scene, prefab, font, sprite, material or runtime layout with concept artwork. Keep concepts outside Assets and provide a before/after gallery.
- Existing Defense mechanics, campaigns, shared stars, player data, authored Light visuals and original dirty files are protected. Private TEMP snapshot includes initial tracked-file bytes/hashes, five preferences and Editor state. Do not publish player JSON.
- Sample accounts must never reach production UGS, saved player data or the persistent leaderboard cache. Default builds retain real rankings. Provide a discoverable Editor action to open/close samples and restore service ownership on exit/reload.
- Use scene/prefab/material-authored theme choices; avoid generic global recoloring. Native tests must exercise actual pooled death particles and real Pause input/appearance in Light and Dark.
- Use the built-in image generator for concept art. The available tool does not expose a DALL·E 3 model selector; this limitation was explained. Do not claim a model that cannot be selected.

## Phases and commits
- [x] 0. Read architecture/imagegen skills, inspect Git/Unity state, snapshot originals and commit this implementation record.
- [ ] 1. Capture the complete current UI inventory in both themes through Boot; record a screen-to-concept manifest. Include menus, level pages, three game modes, HUDs, gameplay popups, tutorial, transition and map-preview UI. Record real baseline death/Pause examples before repairs.
- [ ] 2. Add a reversible, labeled ten-row Editor leaderboard preview. Test actual UI, repeat activation, close, reload/stop cleanup and zero external operations/persistent sample writes. Commit.
- [ ] 3. Repair Dark Defense death effect using the existing Light effect appearance; test real particle emission, theme changes, pool reuse/VFX option and no changes to Light timing/material/color. Commit.
- [ ] 4. Repair Dark gameplay Pause artwork; verify real button feedback, pause/resume and all affected scene/prefab instances, with Light preservation. Commit.
- [ ] 5. Generate coordinated AI concepts for every captured UI state in both themes. Each generated concept/board must be inspected and mapped to its matching original screen. Build an offline before/after gallery with readable full-size previews, prompts and explicit concept-only labels. Commit completed concept batches separately; keep exact coverage pending until each output exists and passes review.
- [ ] 6. Review gallery completeness and all three implemented changes with Unity MCP. Restore original data/files/Editor state, publish overview and next continuation point, commit final evidence.

## Deliverables
Evidence and originals: `Captures/Revision6/Before/`, targeted comparisons in `Captures/Revision6/Checks/`. Concepts and gallery: `Captures/Revision6/Concepts/` and `Captures/Revision6/index.html`; generated art remains outside Assets. Screenshots may be staged for reproducible inspection without awarding progress. Isolated test adapters are not a live UGS connectivity test.

## Current resume point
Phase 0 complete after this plan commit. Three source audits are running: reversible sample-provider tooling, death-effect theme ownership and Pause artwork theme ownership. No production repair or AI concept is complete yet. The original source Unity instance is `ArrowSwarm@3f240769be4f5e6f`, stopped on BootScene with background false. Preserve the 11 pre-existing dirty files and original `.codex/` / recovery scene files.
