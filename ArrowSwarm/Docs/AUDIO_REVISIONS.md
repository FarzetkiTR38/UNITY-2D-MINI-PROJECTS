# Audio revisions · 2026-11-12

## Authorized scope

Profile controls need the same SFX coverage as the other menus. Remove the win fireworks/confetti behind the result popup. Rotate the three existing gameplay tracks once per newly loaded attempt, including retries, while looping the selected track until that attempt ends. Correct the actionable issues from `Captures/AudioReview`; preserve gameplay, approved visuals, private saves and pre-existing work.

## Checklist and phase commits

- [x] Inspect project rules, previous audio report and actual Unity editor context. Back up all 17 initially modified tracked files and private preferences outside the project. Initial Editor scene is unsaved MainMenuScene; preserve it.
- [x] Profile avatar/opening/dismissal, dice/country, Save and Skip controls pass native `profile-checks.txt`, including repeated Show/Hide guards and isolated offline services. This scene has no separate optional Close field; shared dismissal is covered.
- [x] Remove automatic and explicit result confetti from Defense, Shapes and ColorShift; native result checks find no playing confetti/fireworks. Other particle APIs and star animations are preserved.
- [x] Add serialized three-track gameplay playlist using GameBGM, GameBGM_alt1, GameBGM_alt2.
- [x] Real Next/Retry buttons in all three scenes advance 0→1→2→0; pause/resume, setting changes and clip-end loop retain their current track. `playlist-checks.txt` passes.
- [x] Exact zero/mute, both crossfade channels, pause/result attenuation across data refresh, unscaled crossfade and non-destructive track matching pass native `mix-checks.txt`.
- [x] Native `events-checks.txt`: one HeartBreak per actual Defense life change and one after a real Shapes blocked flight; initial/reset lives silent. Defense actual hint spends once/sounds once; pending repeat is silent. Real pooled surviving/fatal damage produces MobHit/MobDie once, including synchronous pool health reset. ArrowHit remains a compatible explicit collision hook.
- [ ] Bound overlapping effects without changing the pitch of earlier tails; keep high-priority UI/feedback usable during bursts.
- [x] All five Defense popup owners pass native `popups-checks.txt`: startup/repeated hides silent, repeated opening silent, one cue per actual transition, real Tip/Freeze Close buttons produce click+close and release input. Reward-ad buttons were inspected but no real ad was requested.
- [x] Lossless edits to ArrowWrong/ToggleSwitch silent tails and MobDie_alt1 3/4ms edge fades; GUIDs, rates, PCM encoding and RIFF metadata retained. Native re-import/decode checks pass; source report and reproducible hash-guarded script provided.
- [x] Four original music sources use Streaming/background load/no preload; source files are retained. Playlist gains 0.50/0.89/0.49 are serialized separately from user music level. Native importer checks pass.
- [ ] Native Unity MCP regression across all modes, Light/Dark, real controls, retry/next/pause/mute and original state restoration.
- [ ] Deliver reviewable evidence, updated audio report status and phase commit list.

Each code phase is compiled and checked in Unity before its commit. Exact related native checks and commits are recorded below as they complete. Changes stay within the requested audio/VFX scope; this is not approval to begin another numbered UI board.

## Verification boundaries

This session cannot assess audio timbre, musical pleasure or aesthetic preference by hearing. Source boundaries, runtime routing, AudioSource state, calls, gains and playback progression can be checked. Technical correctness is not presented as a listening score. Original review files remain a baseline; this revision adds dated follow-up evidence.

## Resume point

Eight phases are committed through `88d70ca7`: plan `d001f8c7`, Profile `cd07d746`, win VFX `bc685916`, attempt playlist `e5a9a5e6`, WAV boundaries `24d90f3d`, music streaming `1c8d543c`, mute/mix/crossfade `3e9e8f71`, Defense popup audio `88d70ca7`.

Current next action: compile/test the ready AudioManager.GameplayEvents partial together with the root's HUDFeedbackEffects duplicate removal, using the new Events verification partial from the metrics agent. Then commit life/hint/mob ownership and implement/verify bounded voices. The AudioManager agent is holding edits; release it for the voice phase only after the event phase is validated. New delivery HTML exists; event/voice statuses and six native result JPGs still need final evidence before delivery. Final Profile/Playlist/Mix/Popups reruns should follow the voice change.

Root owns Unity, narrow commits and restoration. Do not reset or stage the 17 pre-existing dirty files. Original Editor MainMenuScene is unsaved/dirty with SharedBackdrop selected; keep its in-memory work. Exact original dirty-file hashes/copies and private preferences are outside the repository; `%TEMP%/ArrowSwarm-AudioRevisions-backup-path.txt` locates them. Restore private preferences and font-cache/file drift after final Play, then verify every original hash. MainMenuScene and BootScene must retain original bytes. No browser/server workaround is permitted for the earlier local-preview security denial.
