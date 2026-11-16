# Store launch creative completion · 15 November 2026

- [x] Confirm selected icon from user; preserve source and supersede old rejected pilot identity notes.
- [x] Generate matching marketing key art; export EN/TR RGB feature graphics at exactly 1024×500.
- [x] Capture actual English Color Shift with both HUDs, legal shots and rotation using isolated Unity MCP recorder.
- [x] Complete 25-second three-mode store trailer including Farzetki splash.
- [x] Complete 15-second wrong-move ad with actual collision/return animation.
- [x] Decode both outputs, inspect frames, verify preview controls and mobile layout.
- [x] Restore private player/preferences/Editor state; commit each completed phase; deliver through shared preview hub.

User's latest supplied icon is authoritative. No store upload, account connection, fake download URL or production gameplay/UI modification is requested. Branch: ArrowSwarm. Source capture scope uses existing offline isolation, cloned player data and preference restoration; no live progress or leaderboard writes.

Completed 16 November: EN/TR final feature graphics are exactly 1024×500 RGB PNG, about 622 KB each. Selected icon source is preserved under `02_Branding/Selected_20261115/`. The illustration uses the built-in image generator with that identity reference; localized typography is separately typeset with the existing licensed title font. Original pilot graphic is preserved.

Trailer: 25.0s, 750 frames, 1920×1080. Actual splash background plus foreground logo/developer credit; English Shapes and Color Shift capture with real rotation, then Defense through its natural three-star result; final selected-brand key art. Defense archival HUD is Turkish; surrounding copy is English. Full portrait gameplay and both HUDs are retained in the landscape frame.

Ad: 15.0s, 450 frames, 1080×1920. Three actual blocked Shapes 2 shots and full returns, first attempt at editorial 0.2× speed, unforced loss, then selected-icon end card. Caption overlay is hidden before the Game Over title. No fake download address or publication claim. Audio is an editorial mix of existing project music and event-aligned SFX, including the new soft outcome cues.

`VIDEO_QA.json` records full frame/all-stream decoding and hashes. `DELIVERY_QA.json` independently verifies final hashes, HTTP206 video seeking, HTTP200 evidence, a real TR download/hash match and actual browser playback/exclusive play. TR/EN toggle/full-size image and 390×844 layout pass with no horizontal overflow; temporary viewport reset. Browser screenshots are in `Browser/final-desktop.jpg` and `Browser/final-mobile.jpg`. The landscape player was corrected to 16:9 so the preview does not squeeze it into a portrait box.

Native MCP evidence: `NativeEvidence/` contains legal Color Shift fire/rotation/win and all three wrong-shot collision/return/loss events. All five private preferences and original runtime providers restored; 21 existing dirty paths unchanged at capture completion. Editor returned stopped/disarmed to clean BootScene. No production game visuals, mechanics or Player Settings icons changed.

Phase commits: `61aec231` final feature graphics/selected identity; `ffd2ac32` native capture evidence/helper; `eb737c91` both final films and encoded verification; final preview/checklist commit follows. Branch remains ArrowSwarm; no push/upload performed.

Continuing preview: http://127.0.0.1:8200/#%2Fmarketing%2F06_Trailers_Ads_Tutorials%2FStoreLaunch_20261115%2Findex.html . Server command: `py -3.13 ReviewHub/serve.py --port 8200`. Both individual PNG and MP4 download controls are available.

Exact next action: user reviews these two graphics and two videos in the hub; apply requested creative revisions without rerendering unrelated media. This task is complete; no next numbered UI board or publishing starts automatically. A future fully English Defense capture may replace the declared Turkish archival segment if requested. The earlier five-video delivery has its own remaining checkpoint in `05_Reels/DEFENSE_SERIES_01.md`.
