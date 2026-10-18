# Enemy flutter integration

Use the approved second concept, **Yaramaz Vızıldayan**, in Defense's existing `Mob` prefab. Keep the complete current motion for this review. A future wings-only revision can replace the authored clip/art without changing movement or HP logic.

## Commits

1. Record implementation and protect existing changes.
2. Import the approved transparent atlas with its actual sixteen frame rectangles. Author a native one-second, 16 fps sprite-only loop and controller. Match the previous visible width and retain stable centered pivots.
3. Bind the Animator to `MobSprite`; connect pooling and freeze lifecycle while preserving HP, hit flashes, facing, colliders and path movement. Keep static-sprite fallback. Address any directly related lifecycle regression found during verification.
4. Compile and verify through Unity MCP in real GameScene: frame progression, distinct phases, HP, movement, pause/freeze/resume, damage, pool reuse, arrow hits and finish handling. Capture actual gameplay and commit the evidence.

The Animator changes only `SpriteRenderer.m_Sprite`. Transforms, UI, tint, collision geometry and movement remain owned by their existing systems. All visual assets and loop speed are authored in the Inspector. No dynamic sprite creation or per-frame managed allocation is needed.

Baseline: `1ae2880`. The ten existing modified files are backed up outside the project. Existing scenes, level/config data and original enemy artwork are protected by hashes. All commits use explicitly selected task files.

## Asset authoring

The approved second atlas is imported unchanged. Its measured frame rectangles avoid neighboring-frame tail fragments. Sixteen centered sprites use 385.918 pixels per unit, matching the original enemy's maximum visible width (1051/1200 world units). The native clip contains sixteen sprite keys and loops in exactly one second at 16 fps; it has no transform, color, HP or event curves. Unity MCP verified the resulting sprites and clip before binding the prefab.

## Runtime binding

The existing `Mob` prefab retains its root collider and HP sibling exactly. `MobAnimation` starts the child Animator at a deterministic phase per spawn ID; freeze stops its clock and pool release resets it. Existing static variants remain available for unconfigured prefabs. Hit feedback keeps its authored scale and returns to the icy tint when damaged during freeze; stale hit coroutines are cancelled on reuse.

The directly related pool audit found that `PathFollower` retained its paused flag after a frozen enemy died. A single line in `MobMovement.ResetMovement` now clears that flag before resetting the follower. Path geometry, speed, health, difficulty, spawn timing and all scene/config/level assets are unchanged. This intentional lifecycle fix is the sole exception to the initially protected movement-source hash.
