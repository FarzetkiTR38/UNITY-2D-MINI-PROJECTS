# Defense R2 — independent encoded review

**PASS.** No blocking visual, timing, editorial-truth or decode issue found. Reviewed 2026-11-15 after `QA_REPORT.json` passed.

Final file: `ArrowSwarm_Defense_R2_Sirayla_TR.mp4`, 21.000 seconds, 630 frames, 1080×1920 at 30 fps, 7,219,938 bytes. Independently calculated SHA256 matches the QA report:

`ce1e0492d36dd04b4dc55a0f8f8001f36d80d3bdc67657f64cbdfca843c250f3`

## Actual encoded visual review

Seventeen PNG samples were independently extracted from this final MP4 and visually inspected, rather than using the renderer's planned stills: 0.40, 2.00, 3.40, 4.10, 4.70, 5.10, 8.43, 8.73, 9.30, 9.50, 9.90, 11.10, 12.20, 13.42, 15.57, 17.03 and 19.03 seconds. Temporary evidence: `%TEMP%/ArrowSwarm-DefenseR2EncodedReview-20261115/`.

| Time | Observed encoded result |
| --- | --- |
| 0.40 / 2.00 s | Lighter opening composition; enemies occupy roughly half the perimeter initially rather than already surrounding the board. Opening captions fit below the top HUD. The intro's `2×` speed marker is visible above the lower HUD. |
| 3.40 s | Earlier real Freeze is visible through blue enemy tint and the native timer. `ÖNCE DONDUR.` and `ZAMAN SENDE.` fit the upper/lower gaps. The `2×` marker is gone as action returns to normal speed. |
| 4.10 / 4.70 / 5.10 s | Native counter progresses 1/15 → 2/15 → 4/15. Separate arrow launches and their paths remain readable. `SIRAYLA GÖNDER.` stays above the lower HUD without covering the board, shot count or Freeze timer. |
| 8.43 s | Native counter reaches 14/15 and the remaining arrow is visibly rainbow-colored, establishing its production promotion. |
| 8.73 / 9.30 s | The last arrow has its own readable setup, with `SON OK…` and a thin cyan head halo. The halo identifies the existing native arrow; it does not replace its shape or rainbow appearance. At 9.30 s no ordinary flights remain. |
| 9.50 / 9.90 s | Separate final rainbow launch, 15/15 counter and clear `0,75×` label. The last arrow visibly travels into the perimeter and attacks actual enemies. |
| 11.10 / 12.20 s | Rainbow continues around the real path; newly spawned enemies are still visible at 11.10 s, followed by an empty enemy path at 12.20 s. The edit does not prematurely display the cleaned-up claim. |
| 13.42 s | Actual native completion panel, three gold stars and original Turkish buttons are visible. The small `HEPSİ TEMİZLENDİ!` heading sits above the panel with a visible gap and no overlap. |
| 15.57 s | Native three-gold-star result remains fully visible before the 15.733 s promotional transition. No editorial caption covers this settled result. |
| 17.03 / 19.03 s | Shared logo/promo composition is uncropped and readable. The later sample shows the editorial pointer/press treatment on the separate Play Now promo button. |

Both native HUDs remain visible and unobscured throughout the sampled gameplay and result frames. The top band y=0–230 and bottom band y=1690–1920 were independently compared against matching native source frames at 15 encoded samples. Mean absolute RGB differences range from 1.9396 to 3.1382 on a 0–255 scale, consistent with a lossy encode comparison. This is not a claim that decoded pixels are byte-identical. The renderer separately asserts exact HUD-band equality before encoding.

## Independent native evidence and timing

Source: `04_Raw_Gameplay/DefenseR2_L10_20261115_002652_928/`.

- Source frame 375 opens the edit with leader progress **0.50000** and **13 living mobs**, versus the previous pilot's 20 at its opening. The 0.40-second encoded sample is later than that initial source frame, so 13 is not asserted as the count at every opening sample.
- Leader progress strictly increases through source frame 578. Freeze at frame 579 occurs at **0.77066**, compared with the previous pilot's 0.90132: approximately **3.2665 seconds earlier in natural route traversal**. The first 21 frozen telemetry samples, 579–599, retain the exact leading position `(118.46540, 1304.70700)` and progress 0.77066. Charge changes 1 → 0.
- All 15 recorded shots were independently replayed against the captured 8×10 arrow occupancy. Every shot has a clear path to the grid edge, and the occupancy is empty afterward. Ordinary shot frames are **600, 610, 620, 630, 640, 650, 660, 670, 680, 690, 700, 710, 720, 730**. At normal action speed this is one shot every **0.333 seconds**, starting at encoded 4.100 s.
- Final-arrow promotion is established by the fourteenth shot at encoded **8.433 s**. The last ordinary flight completes at source frame **745 / encoded 8.933 s**, before the separate rainbow fire at source frame **762 / encoded 9.500 s**.
- There are **15 flight completions**, with rainbow arrow index 10 completing last. Native production victory is source frame **830 / encoded 12.522 s**. The last projectile kill precedes this at source frame **823 / encoded 12.211 s**.
- The actual source log contains **60 noncleanup damage callbacks**, **24 projectile-kill events with 24 unique IDs 0–23**, **zero cleanup kills** and **zero mob finishes**. The source provenance records 24 total spawned mobs. Three lives remain throughout. Therefore the post-outcome `HEPSİ TEMİZLENDİ!` wording is supported. It means the complete arrow sequence clears all spawned enemies; it does not claim the rainbow personally kills all 24.

The final action is labeled `0,75×` from 9.500 s until victory. The main sequential-arrow action is 1×. The intro's 2× acceleration is explicitly labeled. No crop or replacement HUD is apparent in the encoded samples.

## Audio input, alignment and decode checks

An independent full FFmpeg decode exits 0, reports **630 frames / 21.000 seconds**, no errors, and zero duplicate/dropped frames in that decode. This verifies the encoded file can decode; it does not certify subjective listening quality.

The shortened rainbow voice input was independently checked at the PCM-byte level. `voice_04.wav` is exactly `voice_04_full_pause.wav` with only samples from **0.75–1.45 seconds** removed: 33,600 mono samples / 0.700 seconds at 48 kHz. Every sample outside that cut is unchanged; the resulting clip is 1.76 seconds. `rainbow_pause_edit.json` records a revised quiet interval ending around 0.97 seconds into the clip. Final placement at **8.53 seconds + approximately 0.97 seconds** aligns the approximate “Gökkuşağı” onset with the **9.50-second** rainbow shot. This is technical PCM/timing evidence, not a subjective pronunciation claim.

`audio/voice_06.wav` is byte-for-byte identical to the existing approved corrected-P `SharedOutro01/cta_voice.wav`, SHA256:

`3cffc7f86966e7eb7c4c6e7b0524ea7a2ed59c9c013ad2305cfe0adc857e4c23`

The final MP4's decoded CTA window, **16.533–18.833 seconds**, has normalized waveform similarity **0.998062** to that input over all **110,400 samples**. The full 2.3-second utterance ends before the **18.883-second** promo-button press. This verifies reuse and strong presence in the encoded mix; it does not claim AAC bytes are unchanged.

Music, narration and SFX are identified as editorial audio reconstruction rather than a recorded Unity AudioListener stream. No full subjective listening or browser-playback certification is claimed by this review.

## Review boundary

The store destination remains a mock: `QA_REPORT.json` records `storeStatus: mock` and `publicStoreUrl: null`. The encoded promo has only generic `play.google.com` display text, with no verified app listing claimed by this report. Parent owns live preview/control testing.

Ready for the user's creative review. This pass changed only this new report and temporary decoded review frames; it did not operate Unity, browse, run Git, change the renderer, or modify the previous pilot.
