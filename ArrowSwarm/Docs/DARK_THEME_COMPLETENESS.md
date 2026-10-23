# Dark theme completeness — 23 October 2026

## Objective
Audit every player-facing screen, fix remaining light-only surfaces and low-contrast content in Dark, and preserve the authored Light appearance, localization, press feedback and gameplay. Use serialized scene/prefab/material variants through the existing theme architecture; do not style static UI by runtime name searches.

## Protected baseline
- Start: `fe55e85`; clean BootScene in Edit Mode, Unity 6000.3.18f1.
- Six preexisting modified solution/font files and all five data-service preferences privately snapshotted. Exclude these existing edits from commits and restore exact bytes/preferences after verification.
- Source art, campaign generation/data and gameplay rules stay unchanged unless a demonstrated theme defect requires a narrowly scoped correction.

## Phases and completion
- [x] 1. **Inventory and visual baseline**: inspect six build scenes, shared UI prefabs, material families and runtime theme owners; capture paired Light/Dark menu, modal, gameplay and tutorial states. Commit the plan, audit and findings.
- [ ] 2. **Menu and mode selection**: cover the illustrated backdrop/stat surfaces, five buttons, localized labels, mode modal/cards and dynamic menu screens. Verify dark contrast and exact return to authored Light; commit.
- [x] 3. **Gameplay and shared overlays**: inspect Defense, Shapes and Color Shift HUDs, boards, rotation controls, pause/results/hint/skill prompts, tutorial and reusable list/profile elements. Fix only demonstrated gaps, verify both themes, commit.
- [ ] 4. **Boot and scene transitions**: apply saved Dark from startup through shared circular wipes; preserve progress/input ownership when the theme changes during animation. Verify Light unchanged, commit.
- [ ] 5. **Complete regression and overview**: inspect all paired captures, test live switching, scene entry, pooled/list refresh and portrait/tablet layouts; correct remaining issues in tested commits. Restore original data/editor state and publish a visual overview and final commit.

## Verification expectations
- Actual Unity MCP screenshots in both themes, including inactive/popups when shown and scroll/list reuse.
- Native console and shader compilation checks after each phase.
- Light→Dark→Light must restore original material, sprite, color, opacity, layout and interactivity; fades, selected/locked states and press masks remain intact.
- Keep compilation/domain reload complete before entering Play Mode. Use real-frame/popup readiness predicates, unique capture paths and acknowledged player input for interaction checks.

## Findings / continuation log
Initial source review: theme infrastructure and legacy dark material families already exist. New illustrated menu assets and the recently added Swarm iris require explicit coverage review; global tinting would damage character/logo artwork and button transparency, so author targeted variants instead of repainting every graphic indiscriminately.

Phase 3: Added the eight missing MapTest bindings (pause/results/skill/tutorial panels and zoom track) from Defense's existing authored palette. All 21 gameplay/boot pairs restore exact Light materials/sprites/colors. Runtime maps and gameplay rules unchanged. MCP visual validation used an isolated byte-for-byte project copy after the original editor's external-scene reload dialog became inaccessible to desktop input.
