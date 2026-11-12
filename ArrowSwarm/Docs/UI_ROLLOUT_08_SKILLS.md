# Board08 — Defense exhausted-skill popups

Active request, 2026-11-06: integrate only the Hint exhausted and Freeze exhausted popup appearance from `Captures/Revision6/Concepts/08-defense-skills.png`. Baseline `4159e497`.

The backdrop, gameplay, HUD, Pause, outcomes, other modes, rewards and advertisement logic remain outside this change. Use existing illustration sprites, localized TMP and scene-authored styling. Preserve local dirty files and private player/Editor state.

## Continuing checklist

- [x] Read project/architecture rules and inspect board08; verify native Unity MCP connection.
- [x] Privately snapshot all12 existing modified files and original GameScene/overlay source.
- [x] Capture native before views and preserve private Editor/player preferences.
- [x] Author only the two Defense popups with reused illustrations, dimensional TMP, themed cards and original controls.
- [x] Inspect and refine actual Light/Dark captures against the reference.
- [x] Verify exhausted-skill opening, Close and scoped rewarded success/failure through real pointer input; verify localization and responsive safe bounds.
- [x] Verify unchanged source outside the two popup subtrees; restore private state and original dirty files.
- [x] Commit tested implementation and deliver actual before/after preview.

## Resume

Complete. Next action: user review of `Captures/UIRollout/Phase08Skills/index.html`. Do not replay the reverted board06/Pause work or begin another numbered board without a new request.

## Baseline and isolation

`Before/verification.txt`: PASS; four actual1080x1920 captures and existing control/reward behavior verified. The unchanged zero-count HUD uses the direct rewarded-ad route. Manager exhaustion events open these two popup owners; those events and native popup controls are exercised separately. Only a fixture-owned fake provider is used.

The independent source checker is `Captures/UIRollout/Phase08Skills/Tools/check_scope.py`. It checks original scene documents, prefab overrides and ownership of every added object. Final result: PASS, all72 original scene documents preserved outside the explicitly permitted popup overrides, all210 added documents owned by the two popup trees. Three incidental CameraController defaults were removed. All12 original dirty files were restored byte-for-byte; the newly populated Korean fallback font was restored to its initially clean source.

## Implemented appearance and verification

- Reuses the original smiling bulb, snowflake, video icon, blue secondary button, shared text-free gold CTA and blank rounded plate. No new raster artwork, fonts or shaders.
- Uses one small shared `Ui08CardLight` material for the porcelain card interior; existing shared Dark palette and button-fill materials remain unchanged.
- Preserves localized two-line Casual3DTitle, enables width fitting only on these copied titles, and uses existing LilitaOne TMP for message/actions. No functional copy is baked into images.
- Corrected German title overflow found by native glyph-mesh checks; ordinary labels also pass TMP overflow checks. Native tests verify real press squeeze, modal input blocking, failure granting nothing, success granting exactly one charge, Close and generated-board identity.
- Standard1080x1920, Tall1080x2400, Compact720x1280 and Tablet1536x2048 revised runs pass, including both themes and all10 languages:1,165 successful assertions per size,4,660 total, no failed assertions in delivered reports. Native PNG dimensions are checked in `verification-summary.json`.
- Final tablet fitting reserves room for the unchanged HUD chrome. Its wrapper extent is900x1850; phone sizes retain a fit scale of1. Standard and Tablet were rechecked after this adjustment. The earlier Tall/Compact captures remain valid because their fit scale and appearance are unchanged.
- The initial TMP rectangle-only title check was unsuitable for warped title geometry and was replaced with a check of actual visible glyph vertices against the card. A cleanup timing race was also corrected in the Editor-only fixture; production popup/reward code was never changed.

## Restoration and limits

All five original PlayerPrefs keys were compared against the private pre-Play snapshot after stopping. Original MainMenu Edit scene, Game View selection and background setting are restored. No player record, mock reward or cloud provider is retained. Verification used scoped fake ads and native Editor Game View safe rectangles; it does not certify live ad SDKs, physical devices or hardware notches.

The last tablet bootstrap was restarted after an in-flight script reload interrupted Boot's coroutine; the clean compiled Play run passed. Boot source and startup logic were not modified.

Review: `Captures/UIRollout/Phase08Skills/index.html`, served at `http://127.0.0.1:8170/UIRollout/Phase08Skills/index.html`. Original Revision6 gallery remains on8168.

Commits: plan `7cda5e58`; isolated tested implementation `9367619a`; final evidence/checklist are committed separately with this delivery.
