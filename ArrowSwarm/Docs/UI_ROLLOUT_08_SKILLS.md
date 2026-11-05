# Board08 — Defense exhausted-skill popups

Active request, 2026-11-06: integrate only the Hint exhausted and Freeze exhausted popup appearance from `Captures/Revision6/Concepts/08-defense-skills.png`. Baseline `4159e497`.

The backdrop, gameplay, HUD, Pause, outcomes, other modes, rewards and advertisement logic remain outside this change. Use existing illustration sprites, localized TMP and scene-authored styling. Preserve local dirty files and private player/Editor state.

## Continuing checklist

- [x] Read project/architecture rules and inspect board08; verify native Unity MCP connection.
- [x] Privately snapshot all12 existing modified files and original GameScene/overlay source.
- [ ] Capture native before views and preserve private Editor/player preferences.
- [ ] Author only the two Defense popups with reused illustrations, dimensional TMP, themed cards and original controls.
- [ ] Inspect and refine actual Light/Dark captures against the reference.
- [ ] Verify exhausted-skill opening, Close and scoped rewarded success/failure through real pointer input; verify localization and responsive safe bounds.
- [ ] Verify unchanged source outside the two popup subtrees; restore private state and original dirty files.
- [ ] Commit tested implementation and deliver actual before/after preview.

## Resume

Next action: capture the original two popups in Play with a private, restored verification scope; then apply an isolated scene-only board08 authoring helper. Do not replay the reverted board06/Pause work.
