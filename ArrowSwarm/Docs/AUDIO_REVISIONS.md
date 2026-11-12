# Audio revisions · 2026-11-12

## Authorized scope

Profile controls need the same SFX coverage as the other menus. Remove the win fireworks/confetti behind the result popup. Rotate the three existing gameplay tracks once per newly loaded attempt, including retries, while looping the selected track until that attempt ends. Correct the actionable issues from `Captures/AudioReview`; preserve gameplay, approved visuals, private saves and pre-existing work.

## Checklist and phase commits

- [x] Inspect project rules, previous audio report and actual Unity editor context. Back up all 17 initially modified tracked files and private preferences outside the project. Initial Editor scene is unsaved MainMenuScene; preserve it.
- [x] Profile avatar/opening/dismissal, dice and country controls produce one appropriate UI sound; native `profile-checks.txt` passes including repeated Show/Hide guards. Save/skip/close final regression follows isolated provider setup.
- [x] Remove automatic and explicit result confetti from Defense, Shapes and ColorShift; native result checks find no playing confetti/fireworks. Other particle APIs and star animations are preserved.
- [x] Add serialized three-track gameplay playlist using GameBGM, GameBGM_alt1, GameBGM_alt2.
- [x] Real Next/Retry buttons in all three scenes advance 0→1→2→0; pause/resume, setting changes and clip-end loop retain their current track. `playlist-checks.txt` passes.
- [ ] Accept exact zero volumes, respect mute everywhere, and retain pause/result gain after data refresh.
- [ ] Own each life-loss sound once; wire Defense hint and distinguish surviving/fatal enemy damage.
- [ ] Bound overlapping effects without changing the pitch of earlier tails; keep high-priority UI/feedback usable during bursts.
- [ ] Complete Defense popup/action SFX coverage without double clicks.
- [x] Lossless edits to ArrowWrong/ToggleSwitch silent tails and MobDie_alt1 3/4ms edge fades; GUIDs, rates, PCM encoding and RIFF metadata retained. Native re-import/decode checks pass; source report and reproducible hash-guarded script provided.
- [ ] Configure existing music import/mix costs and per-track levels for three-track operation.
- [ ] Native Unity MCP regression across all modes, Light/Dark, real controls, retry/next/pause/mute and original state restoration.
- [ ] Deliver reviewable evidence, updated audio report status and phase commit list.

Each code phase is compiled and checked in Unity before its commit. Exact related native checks and commits are recorded below as they complete. Changes stay within the requested audio/VFX scope; this is not approval to begin another numbered UI board.

## Verification boundaries

This session cannot assess audio timbre, musical pleasure or aesthetic preference by hearing. Source boundaries, runtime routing, AudioSource state, calls, gains and playback progression can be checked. Technical correctness is not presented as a listening score. Original review files remain a baseline; this revision adds dated follow-up evidence.

## Resume point

Plan recorded. Agents are implementing Profile audio, win-effect removal and AudioManager/playlist in separate owned files. Root owns Unity validation, the HUD duplicate-heartbreak removal, test harness, source-boundary decisions, narrow staging and commits. Do not reset or stage the 17 pre-existing dirty files. MainMenuScene and BootScene must retain their original on-disk bytes and unsaved Editor work.
