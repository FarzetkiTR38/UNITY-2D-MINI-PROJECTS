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
- [x] Native `voices-checks.txt`: 8 effect voices/2 reserved, generic burst drops beyond budget, critical feedback can reclaim generic combat only, continuing click pitches stay stable, no new sources per burst, mute/zero stop/prevent voices. Inspector 8→4→8 resize removes owned excess components. Rapid BGM A→B→A reuses its original source/sample position (`mix-checks.txt`).
- [x] All five Defense popup owners pass native `popups-checks.txt`: startup/repeated hides silent, repeated opening silent, one cue per actual transition, real Tip/Freeze Close buttons produce click+close and release input. Reward-ad buttons were inspected but no real ad was requested.
- [x] Lossless edits to ArrowWrong/ToggleSwitch silent tails and MobDie_alt1 3/4ms edge fades; GUIDs, rates, PCM encoding and RIFF metadata retained. Native re-import/decode checks pass; source report and reproducible hash-guarded script provided.
- [x] Four original music sources use Streaming/background load/no preload; source files are retained. Playlist gains 0.50/0.89/0.49 are serialized separately from user music level. Native importer checks pass.
- [x] Bootstrap listener fix: the temporary persistent listener yields to the scene camera; native fallback recovery and one active listener in all three gameplay scenes pass in `playlist-checks.txt`.
- [x] Final native Unity MCP regression: 187 passing assertions across all six groups (voices/mix/events/profile/popups/playlist) after the final runtime change. Three gameplay scenes, Light/Dark result captures, actual next/retry/profile/close controls, pause/mute and fallback listeners covered. Error-console query returns zero errors.
- [x] Deliver reviewable evidence at `Captures/AudioRevisions/index.html`, six native result JPGs and an overview. The original AudioReview page has a dated baseline warning and links this follow-up; its earlier metrics/ratings are not presented as current measurements.
- [x] Original pre-task preference values/key existence restored. All 17 pre-existing tracked dirty files still match original SHA256 values; MainMenuScene remains unsaved/dirty with SharedBackdrop selected. No unrelated file was staged.

Each code phase is compiled and checked in Unity before its commit. Exact related native checks and commits are recorded below as they complete. Changes stay within the requested audio/VFX scope; this is not approval to begin another numbered UI board.

## Verification boundaries

This session cannot assess audio timbre, musical pleasure or aesthetic preference by hearing. Source boundaries, runtime routing, AudioSource state, calls, gains and playback progression can be checked. Technical correctness is not presented as a listening score. Original review files remain a baseline; this revision adds dated follow-up evidence.

## Resume point

The requested revision is complete. No later numbered UI board was started. Next action is user review of the existing three tracks at runtime; aesthetic selection still requires hearing. Do not automatically replace further sounds or begin another board.

| Phase | Commit |
| --- | --- |
| Plan and original-state backup | `d001f8c7` |
| Profile controls | `cd07d746` |
| Background win VFX removal | `bc685916` |
| Three-track per-attempt playlist | `e5a9a5e6` |
| Lossless WAV boundary repair | `24d90f3d` |
| Streaming music imports | `1c8d543c` |
| Zero/mute/contextual gain and crossfade | `3e9e8f71` |
| Defense popup SFX | `88d70ca7` |
| Single life/hint/mob sound ownership | `7cb4e697` |
| Bounded voices, priorities and continuing playback | `341b390e` |
| Scene listener ownership | `bcbf9fd0` |
| Final verification sources, captures and delivery | Same commit as this completed checklist |

Native reports and all-source checks are under `Captures/AudioRevisions`. Test helpers are Editor-only `UI/Editor/AudioRevisionVerification*.cs`; enter initialized MainMenu Play Mode, let domain reload settle, call `Begin("profile")` / `"voices"` / `"mix"` / `"events"` / `"popups"` / `"playlist"`, then poll `Status`. Each group uses cloned player data and scoped offline services; reports explain unexercised real ads/device listening. These are controlled result-state/route checks, not claims of playing the full campaign.

Preserve the 17 pre-existing dirty files and the unsaved MainMenuScene. Exact original dirty-file hashes/copies and private preferences are outside the repository; `%TEMP%/ArrowSwarm-AudioRevisions-backup-path.txt` locates them. Original full-resolution screenshots are also outside the repository; compact deliverable JPGs add no player build assets. No browser/server workaround was used for the earlier local-preview security denial. HTML source/links were checked programmatically; browser playback was not verified. Inspector budget shrinking removes only pool-owned dynamic AudioSources, preserving externally assigned authored components.
