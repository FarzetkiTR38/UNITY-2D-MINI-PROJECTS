# Defense Pilot 01 — native capture editorial audit

Audited 2026-11-15. Source: `04_Raw_Gameplay/Defense_L10_20261114_214156_530/` within `ArrowSwarm_Marketing`.

## Result

PASS for the truthful sequence **natural approach → one real Freeze charge → legal arrow chain → production win**. This is actual Unity GameScene level 10, Turkish, 1080×1920, 30 fps. There are 1,140 JPEG frames and 1,140 matching telemetry rows: 38 seconds of source material. The capture starts after gameplay has already begun; source frame 0 is not the level-start instant.

This audit reads the saved event/telemetry/geometry files, independently replays shot legality, reads the capture helper, and visually inspects six native stills. It does not certify a complete edited video, audio recording, soundtrack mix, or physical-device gameplay.

## Observed event timeline

Times below are source frame / 30, before editorial trimming or retiming.

| Frame | Source time | Evidence |
| --- | --- | --- |
| 0 | 0.000 s | 15 arrows remain, three lives, three living mobs; leader progress 0.10800; Freeze charge 1. |
| 506 | 16.867 s | Untouched board, 20 living mobs, leader progress 0.78266; charge still 1. |
| 595 | 19.833 s | Leader naturally reaches 0.90132; 23 living mobs; not frozen. |
| 596 | 19.867 s | Actual HUD Freeze button route; charge becomes 0 and frozen becomes true. No arrows have fired. |
| 617 | 20.567 s | First legal production shot, arrow index 2; 14 arrows remain. |
| 630 | 21.000 s | Five arrows have fired; native still shows flights, hit flashes, damage numbers and live Freeze timer. |
| 659 | 21.967 s | Fifteenth shot is the promoted rainbow arrow, index 10; no arrows remain on the grid. Several flights are still outstanding. |
| 676 | 22.533 s | Final projectile kill; only the leading mob remains alive. |
| 718 | 23.933 s | Last flight completes; normal victory cleanup removes mob 0, Freeze ends and the production win event is recorded. Three lives remain. |
| 800 | 26.667 s | Native three-star completion panel is visibly settled. |

The approach is continuous in telemetry: leading progress strictly increases in every pair of consecutive samples from frame 0 through 595, from 0.10800 to 0.90132. The per-frame increment is 0.00133–0.00134 at the recorded precision. No shots occur before frame 617 and no `mob_finish` event occurs anywhere in the recording.

The first 21 frozen samples, frames 596–616 inclusive, all retain leader screen position **(118.46540, 818.82650)**, progress **0.90132**, 23 living mobs, charge 0 and frozen true. First firing begins 21 frames / 0.700 seconds after Freeze. The leader position and progress remain exactly the same through frame 717, including the projectile sequence. At win it disappears rather than advancing to the finish.

## Shot and kill verification

The geometry identifies production `Map2.asset`, an 8×10 board, 15 arrows, path length 34.36444, base mob speed 1.37458 and configured Freeze duration 5 seconds. The capture helper selects arrows using the live `GridManager.IsPathClear` check and calls production `Arrow.OnPlayerClick`. Freeze uses the existing HUD button's `onClick` event, not a physical pointer gesture. Debug infinite lives/tips are disabled and the speed multiplier is 1.

An independent occupancy replay starts with every captured arrow point. For each recorded shot it checks all cells from the head in the shot direction to the grid edge, then removes that arrow's points. All 15 shots pass, with no occupied points remaining afterward. The firing witness, expressed as `frame:arrow index`, is:

`617:2, 620:13, 623:9, 626:4, 629:11, 632:8, 635:14, 638:3, 641:0, 644:12, 647:6, 650:1, 653:7, 656:5, 659:10`.

There are 15 unique flight-completion events covering indices 0–14. The final completion and production victory share frame 718. In event-callback order the last flight's logging appears after victory; this does not mean victory occurred without its completion.

The log contains **78 noncleanup damage callbacks**, **22 projectile kills**, and **one separate victory-cleanup kill**. Damage callbacks are not a count of unique targets or a guarantee that every callback reduced living HP. Five damage-999 projectile events occur at frames 665, 668, 671, 673 and 676. The front mob, ID 0, survives the projectile sequence and receives the normal damage-9999 victory cleanup at frame 718.

Supported wording: “Dondur. Zinciri çöz.” / “15 ok. Tek akış.” / “Gerçek oyun görüntüsü.” / “Bölüm tamamlandı.” A near-escape setup is supported by the leader reaching 90.132% of its route. Three lives remain throughout; this is not a one-life rescue. Do not claim that the rainbow arrow kills every enemy, that all 23 mobs die from projectiles, or that Freeze alone wins the level.

## Native visual inspection and caption space

- `frame_00506.jpg`: populated perimeter, untouched arrows, charge 1 and `Oklar: 0/15`; clear tension setup.
- `frame_00596.jpg`: blue frozen enemy appearance and real Freeze timer around 4.9 seconds; board still untouched.
- `frame_00630.jpg`: active arrow flights, red hit reactions, damage figures, `Oklar: 5/15` and timer around 3.8 seconds.
- `frame_00659.jpg`: rainbow flight is visibly present, `Oklar: 15/15`, enemies still alive and timer around 2.8 seconds. This frame must not be presented as the already completed win.
- `frame_00718.jpg`: completion panel has just begun entering and remains translucent. It is an entrance frame, not a settled result card.
- `frame_00800.jpg`: settled native three-star completion panel with original Turkish controls.

For gameplay, the broad empty band between the top HUD and board is approximately y=230–345; the lower gap between board and HUD is approximately y=1545–1690. Keep the lower HUD and its Freeze timer visible. During the hit sequence, floating damage text reaches roughly y=305, so a compact top caption should stay near y=238–285, or use the lower gap. Avoid text over the portal, mobs, arrows or damage figures.

The settled win panel occupies approximately y=280–1645. Prefer no editorial caption over this panel. If one is essential, use a single small line near y=238, above the panel; the gameplay lower gap is no longer free space.

## Restoration evidence and scope

The source `state_checks.txt` reports restored ad/cloud providers, the original data object, autosave and all five data-service preferences, language, theme, timing, launch context, random state and background setting. These are saved capture-run assertions, not independently repeated Editor checks in this audit. The parent capture task separately reports clean Edit Mode in GameScene, Arm false and the same ten pre-existing dirty-file hashes. This audit did not access private preference values or operate Unity.

Only this audit file was authored by this audit pass. No source, Unity assets, player preferences or Git state were changed.
