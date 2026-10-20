# UI Visual Revision — button feedback and themed transitions

## Scope
Preserve gameplay, campaign data, progress, localization and the existing button squeeze/bounce. Fix pressed feedback bleeding into transparent padding. Replace the plain white iris with a cheerful Arrow Swarm transition, authored in assets/Inspector.

## Phases
1. **Baseline and plan** — inspect the actual MainMenu Play press and transition setup through Unity MCP; safeguard existing local edits and saved data. Commit this plan.
2. **Button transparency** — correct the actual cause, check held/released/exit/disabled feedback in Unity, inspect screenshots and commit the fix with evidence.
3. **Themed iris** — retain circular opening/closing, introduce a cream arrow/star pattern and blue/gold edge; verify fully open/closed states and portrait/aspect behavior in Unity, then commit.
4. **Integration and overview** — exercise real menu/game transitions, check cleanup/input recovery and console, restore saved data and starting Editor scene, publish evidence and commit.

## Baseline
- Starting commit: `e346c8d`.
- Editor: Unity 6000.3.18f1, BootScene, Edit Mode, scene clean.
- Six preexisting modified solution/font assets are privately snapshotted and excluded from these commits.
- User granted implementation and commit authority; no additional approval needed.

## Progress
- [x] Phase 1: repository and Editor baseline inspected; implementation plan recorded.
- [ ] Phase 2: button alpha correction and MCP evidence.
- [ ] Phase 3: themed transition and MCP evidence.
- [ ] Phase 4: integration checks and final overview.
