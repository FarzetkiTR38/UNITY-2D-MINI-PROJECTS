# Store trailer and wrong-move ad

Two actual encoded outputs, using the user's selected glossy cyan/purple-monster icon, current final feature graphic and existing Farzetki Games splash assets.

- `ArrowSwarm_StoreTrailer_EN_25s.mp4`: exactly 25 seconds, 1920×1080, 30 fps H.264/AAC. The complete portrait game image remains visible inside the landscape composition; both HUDs are preserved. 1.2s Farzetki background **and foreground logo/developer credit** → 4s Shapes → 7s Color Shift including actual rotation → 9.5s Defense through its genuine completed three-star animation → 3.3s final brand graphic.
- `ArrowSwarm_FailAd_EN_15s.mp4`: exactly 15 seconds, 1080×1920, 30 fps H.264/AAC. Actual Shapes 2 blocked shot/collision/return; the first attempt is slowed to 0.2× so the return can be seen. All three failed shots remain in their actual order, leading to an unforced native loss. Editorial captions disappear at 5.7s, before the result popup, keeping its title readable. Final 5s uses the selected icon and a short brand message.

English editorial copy, native English Shapes/Color Shift. **The reused Defense archive has Turkish HUD labels**; this is declared in the manifest rather than repainting its actual UI. No fabricated gameplay, fake score/leaderboard, store link, installed-app animation, or claim that the app is already published is included.

Audio is an **editorial mix**, not Unity AudioListener output: existing project `GameBGM_alt1.ogg` at a restrained level, short fades, and actual-event-aligned project fire/collision/click/win/loss cues. It uses the newly replaced soft outcome sounds. No new paid music or voice service was used.

`VIDEO_QA.json` records exact frame counts, dimensions, full FFmpeg frame decode, all-stream decode and final SHA256 values. `trailer_manifest.json`, `fail_manifest.json` and aggregate `manifest.json` retain source folders, event/provenance hashes, edit segments and speed. `*_contact_sheet.jpg` previews actual encoded frames; `.srt` files provide the English text. Native capture evidence is also retained in `NativeEvidence/` by the parent task.

Rebuild from the project root with Python 3.13 and the existing TEMP media dependency folder:

```powershell
py -3.13 ArrowSwarm_Marketing/Tools/build_store_launch_videos.py --trailer --fail-source ArrowSwarm_Marketing/04_Raw_Gameplay/ShapesFail_L2_20261115_184919_828
```

Rebuild requires the local native raw JPEG sequences listed in the manifests; these are capture sources, not runtime Unity assets. Intermediate segment MP4s, PNG plates, WAV mixes and logs are regenerable and excluded by this package's `.gitignore`. Final videos, manifests, captions and review images are retained. The tool never writes gameplay, saves, preferences or production Unity assets.

Review through the continuing hub: http://127.0.0.1:8200/#%2Fmarketing%2F06_Trailers_Ads_Tutorials%2FStoreLaunch_20261115%2Findex.html . No upload/publishing has been performed.
