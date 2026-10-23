# Dark theme completeness — 23 October 2026

## Objective
Audit every player-facing screen, fix remaining light-only surfaces and low-contrast content in Dark, and preserve the authored Light appearance, localization, press feedback and gameplay. Use serialized scene/prefab/material variants through the existing theme architecture; do not style static UI by runtime name searches.

## Protected baseline
- Start: `fe55e85`; clean BootScene in Edit Mode, Unity 6000.3.18f1.
- Six preexisting modified solution/font files and all five data-service preferences privately snapshotted. Exclude these existing edits from commits and restore exact bytes/preferences after verification.
- Source art, campaign generation/data and gameplay rules stay unchanged unless a demonstrated theme defect requires a narrowly scoped correction.

## Phases and completion
- [x] 1. **Inventory and visual baseline**: inspect six build scenes, shared UI prefabs, material families and runtime theme owners; capture paired Light/Dark menu, modal, gameplay and tutorial states. Commit the plan, audit and findings.
- [x] 2. **Menu and mode selection**: cover the illustrated backdrop/stat surfaces, five buttons, localized labels, mode modal/cards and dynamic menu screens. Verify dark contrast and exact return to authored Light; commit.
- [x] 3. **Gameplay and shared overlays**: inspect Defense, Shapes and Color Shift HUDs, boards, rotation controls, pause/results/hint/skill prompts, tutorial and reusable list/profile elements. Fix only demonstrated gaps, verify both themes, commit.
- [x] 4. **Boot and scene transitions**: apply saved Dark from startup through shared circular wipes; preserve progress/input ownership when the theme changes during animation. Verify Light unchanged, commit.
- [ ] 5. **Complete regression and overview**: inspect all paired captures, test live switching, scene entry, pooled/list refresh and portrait/tablet layouts; correct remaining issues in tested commits. Restore original data/editor state and publish a visual overview and final commit.

## Verification expectations
- Actual Unity MCP screenshots in both themes, including inactive/popups when shown and scroll/list reuse.
- Native console and shader compilation checks after each phase.
- Light→Dark→Light must restore original material, sprite, color, opacity, layout and interactivity; fades, selected/locked states and press masks remain intact.
- Keep compilation/domain reload complete before entering Play Mode. Use real-frame/popup readiness predicates, unique capture paths and acknowledged player input for interaction checks.

## Findings / continuation log
Initial source review: theme infrastructure and legacy dark material families already exist. New illustrated menu assets and the recently added Swarm iris require explicit coverage review; global tinting would damage character/logo artwork and button transparency, so author targeted variants instead of repainting every graphic indiscriminately.

Phase 3: Added the eight missing MapTest bindings (pause/results/skill/tutorial panels and zoom track) from Defense's existing authored palette. All 21 gameplay/boot pairs restore exact Light materials/sprites/colors. Runtime maps and gameplay rules unchanged. MCP visual validation used an isolated byte-for-byte project copy after the original editor's external-scene reload dialog became inaccessible to desktop input.

Phase 2: Added 27 illustrated-menu bindings including the underlying cream root, localized labels, rounded button/card materials, stat plaques and a clean transparent Night cutout. Light source art/material settings are retained. Corrected safe-area fitting when CanvasScaler updates after OnEnable. Verified 111 existing GPU checks, 44 Dark artwork checks, and all nine menu Light/Dark/Light pairs. Artwork extraction/prompt recorded in DARK_THEME_ART.md.

Phase 4: CoreManagers now authors a separate Dark wipe palette. Palette events update existing runtime instances only, retaining progress, center/aspect and input/coroutine ownership. Real Light and Dark transition suites each passed all six production scene loads, one paused callback and ten rendered capture frames; source material/config/player/preferences remained unchanged. The earlier 49-check live suite also passed switches during closing/opening in both palettes.

Final verification found a legacy Levels close label using U+2715, unsupported by its LiberationSans font. Replaced that static symbol with supported ASCII X; checking both palettes/layouts again. The final thirty paired captures (after source synchronization and font preservation) passed all Light restoration checks.

Responsive correction: actual 1536×2048 captures revealed clipped legacy panels. Added three serialized SafeAreaPanelFit wrappers around Levels/Leaderboard/Settings, retaining each original animation target and all styling/references. Profile already fits and retains its direct Card hierarchy/auto-wiring. The range badge now auto-fits long localized labels on one line. Tablet nine-pair round trips passed and panels were visually rechecked.

Final close-control correction: the mode modal's source slice contained a baked white rectangle. Dark now uses the existing capsule-mask shader with a dedicated rounded-square Inspector material; the white cross/source alpha and held tint remain intact. All 47 Dark GPU checks and all nine tablet menu pairs passed after correction.
