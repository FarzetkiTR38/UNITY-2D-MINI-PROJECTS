# Reproducible pilot edit

`render_reel.py` consumes a real `MarketingCapture` folder: `frame_00000.jpg` onward, `events.csv`, and `provenance.json`. It refuses missing frames, wrong resolution/frame rate, or a capture with no logged gameplay action. It does not generate, alter or replace the gameplay board, arrows, HUD, outcome or shooting logic.

## Environment

Python3.13 was used for this run. Install `requirements-media.txt` in a separate Python environment, not in Unity. The verified local dependency cache for the current computer is `%TEMP%/ArrowSwarm-Marketing-Media-Python`; the script detects that cache if normal imports are unavailable. FFmpeg is obtained from `imageio-ffmpeg`; no FFmpeg binary is copied into the game or delivered package. The exact executable build is recorded in the QA report.

```powershell
py ArrowSwarm_Marketing/Tools/render_reel.py --source ArrowSwarm_Marketing/04_Raw_Gameplay/<capture-folder> --output ArrowSwarm_Marketing/05_Reels/Pilot01
```

`--language EN` or `--language TR` renders a single language. The default creates both. Reusing an output folder deliberately regenerates its edit files; preserve an earlier approved version in a separately named folder before iterating.

## Design

- Native1080×1920 capture remains undistorted and runs at its original30fps. The board and HUD are never cropped or retimed.
- The first2.5seconds invite a choice before the recorder's first shot. Later captions describe the genuine unfolding gameplay, without promising a unique solution or fabricating a win.
- Five caption sections softly fade, reusing the project's licensed Farzetki title font. The headline is constrained to x90..900 above the board; the final CTA occupies the existing upper gap at y400..460, above the board and away from bottom caption controls.
- No release/download claim is made. Current CTA asks for the viewer's first move, and the film identifies the game as in development.
- The newly synthesized audio is editorial, not captured Unity sound. Source-action timestamps drive its light cues. No external song, recording, sample or game-library music is included.

## Outputs and verification

Each language produces an H.264/yuv420p MP4 with48kHz stereo AAC, cover and SRT. Shared outputs include caption copy, exact FFmpeg commands, synthesis WAV, event cue list, source provenance, font/audio notices and a contact sheet extracted from the encoded video. The script performs a full audio/video decode, checks540 decoded frames, stream formats and dimensions, and measures integrated loudness/true peak on the encoded audio.

`QA_REPORT.json` records technical checks and file hashes. It does not certify subjective music taste, actual Instagram UI overlap on every device, store compliance or publication approval. Listen to the film and check it in the intended platform preview before publishing. The first creative approval gate remains in place.

`layout_only_ARCHIVE.jpg`, if present, is a discarded typography proof composited over the previously archived November10 still. It is not a new gameplay capture, video source or final deliverable.


## Updated user direction · 13 November 2026
The user disliked the icon concepts and pilot Reel and asked to continue phase work, deferring visual revisions until later. The first creative approval gate is no longer a reason to pause work. Existing media remains an unapproved draft; technical checks do not imply creative acceptance.
