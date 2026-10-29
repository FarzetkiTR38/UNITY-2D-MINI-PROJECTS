# UI rollout 02 - responsive screens, mode selection and Settings

Baseline `c9786852`. The user authorizes board02 after the requested responsive Boot/Home revision. Approval is required again before board03.

## Rules carried forward

- Use the whole safe area and independent top/center/bottom anchors. A taller screen must not push its top HUD away from the top. Keep each related group internally proportional; scale down on compact/tablet/landscape windows and respect notches.
- Keep functional text live TMP and existing localization. Reuse shared blank surfaces, icons and theme materials; do not bake working UI into a full-screen mockup.
- Preserve the original runtime controllers, callbacks, data, settings, input feedback and functioning Defense mode. Geometry helpers may update geometry only.
- Test each implemented phase through Unity MCP, inspect actual Light/Dark screenshots, and commit the completed phase. Work stays in this chat with no further approval before board02 is finished.

## Phases

- [x] P0: Read existing design/controllers; privately snapshot local files, both scenes, player/preferences and Editor state. Pointer: `%TEMP%/ArrowSwarm-UI02-path.txt`.
- [ ] P1: Fix Boot/Home responsive anchors. Verify the top HUD stays at its safe-area inset and illustrations/buttons remain visible and separate across phone/tablet/window ratios, then commit.
- [ ] P2: Implement board02 mode-selection modal with live TMP, three distinct reusable card illustrations, selected state, existing mode/icon/progress behavior and click feedback. MCP check and commit.
- [ ] P3: Implement board02 Settings with existing SFX/VFX/vibration, language and theme controls. Live TMP, responsive modal, reused visual kit and all existing behavior retained. MCP check and commit.
- [ ] P4: Verify actual navigation, every language/theme, settings roundtrips, all three modes and representative resolutions; inspect captures, restore original player/preferences/local files/Editor state, publish preview and commit. Request board02 approval before board03.

Evidence goes under `Captures/UIRollout/Phase02/`. The previous full backup remains available outside the repo; preserve pre-existing dirty font/SDK/solution files and Recovery scenes.
