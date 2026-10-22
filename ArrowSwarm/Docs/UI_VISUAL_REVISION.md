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
- [x] Phase 2: button alpha correction and MCP evidence.
- [x] Phase 3: themed transition and MCP evidence.
- [ ] Phase 4: integration checks and final overview.

## Button result
- Root cause: `MainMenuTextless.png` has RGB cream padding baked into rectangular sprite slices. Unity's normal pressed tint colored those opaque corners too; `UIButtonFeedback` itself only scales.
- Five main-menu illustrations now use authored, antialiased rounded artwork masks. Coverage multiplies sampled alpha, so outside padding stays clear and white artwork inside remains intact. Source artwork and existing TMP labels, tint colors and squeeze/bounce settings are unchanged.
- Unity MCP: 30 actual Input System mouse checks passed across Play, Game Mode, Levels, Leaderboard and Settings (hold scale/tint, exit recovery, outside release, disabled press). Held screenshots inspected; no dark rectangle remains. Console: no errors.
- Evidence: `Captures/UIVisualRevision/held-*.png`, `pointer-verification.txt`, before/after screenshots.

## Themed iris result
- Replaced the plain iris coverage with a cream arrow/star pattern and a glossy blue/cyan ring with a thin gold edge. Circular closing/opening and the authored one-second duration remain intact.
- All styling is authored in `Mat_SwarmIrisWipe` and the CoreManagers prefab. The serialized material reference also retains the shader through the BootScene build dependency graph.
- The transition clones the authored material once, reuses that runtime clone across scene loads and releases it on destruction. Progress, aspect and center never dirty the authored material asset.
- Radius derives from the actual aspect and farthest corner, including an off-center iris. Open is fully transparent; closed is fully opaque.
- Unity MCP / real GPU: **111 checks passed** for five button materials, sampled texture/material/vertex alpha, UI clipping, and iris endpoints/continuity at 9:16, narrow portrait and landscape with two centers.
- Actual Play Mode captures show the pattern and circular rim. The real three-mode navigation integration has also passed; its restoration and final evidence publication are recorded in Phase 4.
