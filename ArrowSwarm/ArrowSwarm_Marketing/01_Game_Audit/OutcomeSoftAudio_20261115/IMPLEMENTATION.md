# Soft outcome audio replacement

User requests complete replacement of all six LevelWin/LevelLose sounds: one active and two alternatives for each. Soft, brief, pleasant musical feedback matching Arrow Swarm. No gameplay/UI changes. Stay on ArrowSwarm.

- [x] Inspect bindings/importers/runtime mix and back up existing dirty files plus all six original clips/metas outside Git.
- [x] Preserve existing SFXLibrary user changes; replace six audio payloads in place so all GUID bindings remain valid.
- [x] Select six distinct coherent soft cues with documented reusable rights and provenance.
- [x] Replace the six payloads, retaining their existing .meta GUIDs; main Win/Lose are the active pair.
- [x] Inspect decoded audio, levels, boundaries, tails and file sizes; verify actual Unity import and playback of both bound events and four alternatives.
- [x] Add a new listening/review page to the shared hub and update stale outcome information in the old audio catalog.
- [x] Verify preservation and commit tested phases; listening page is ready for user review.

All six old payloads are replaced in the existing paths, not retained as extra alternatives in Unity Assets. External backup: `C:/Users/Farzetki/Desktop/GitHub/ArrowSwarm-Backups/SoftOutcomeAudio-20261115-202725`. It includes the six previous payloads/metas, baseline hashes and unsaved SFXLibrary JSON. No new service keys, account, purchase or generation provider were needed.

Active pair: `LevelWin.ogg` (STEEL10, four-note rise) and `LevelLose.ogg` (STEEL11, four-note descent). `alt1` uses shorter two-note STEEL08/09 phrases; `alt2` uses longer STEEL02/14 phrases. Source: Kenney Music Jingles, CC0; independent mirror and exact hashes are documented in `NOTICE.md` and `manifest.json`. All six recordings differ from the rejected six.

Mastering softens onset/presence/upper frequencies and matches approximately -20 LUFS. Six files total 85,487 bytes; 44.1 kHz stereo Vorbis; independently decoded peaks -7.28 to -8.96 dBTP; no nonfinite or clipped samples. Technical decoding, musical contour and playback checks do not claim a subjective listening score.

Verification evidence:

- `audio_QA.json`: independent six-file full decode, loudness and hash/GUID audit.
- `unity_import_checks.json`: actual native imports, channels/rate/loaded PCM, correct main bindings.
- `native_playback_checks.txt`: actual AudioManager main callbacks and all alternatives each accepted exactly once, corresponding AudioSource playing with positive volume; restoration passed.
- `restoration_checks.json`: five private preferences restored without recording values, 27 protected paths unchanged including all six metas, Editor stopped in clean BootScene, capture tooling disarmed. The pre-existing SFXLibrary disk edits were preserved; only two authorized in-memory outcome fields were changed, without saving the whole library.
- `browser_checks.json`, `Browser/`: all six HTML controls started successfully from the hub; metadata, stop control, responsive layout and historical replacement notice verified. Old outcome scores were removed rather than reused for new recordings.

Commits: `bf90c585` scope/backup plan; `5986a071` licensed sources/mastering; `ec57eafd` six installed replacements/native verification; subsequent review commit records the listening page and completed checklist. Branch remains **ArrowSwarm**; no push performed.

Delivery: http://127.0.0.1:8200/#%2Fmarketing%2F01_Game_Audit%2FOutcomeSoftAudio_20261115%2Findex.html . The hub discovers this page automatically under Sesler. `Captures/` remains ignored; `Tools/update_outcome_audio_catalog.py` reproducibly updates just the six changed historical entries when that local archive is present.

Exact next action: user listens to the new active pair and alternatives on this page. This six-sound replacement is complete. If asked to resume the earlier five-video task, use `05_Reels/DEFENSE_SERIES_01.md`; do not infer that its outstanding review/delivery is complete from this audio task.
