# Defense Pilot 01 — independent encoded review

**PASS — no blocking visual or editorial issue found in the final encoded samples.** Reviewed 2026-11-15 after `QA_REPORT.json` reported `passed: true`.

File: `ArrowSwarm_Defense_PanikYok_TR.mp4` — 17 seconds, 510 frames, 1080×1920, 30 fps, 7,048,991 bytes. Independently calculated SHA256:

`f38f6e881372f7444062d02e232825d002f464a1af3549850e1c638db1da15de`

The hash matches `QA_REPORT.json`. An independent full FFmpeg decode exits 0, reports 510 frames and exactly 17.000 seconds, with no decode errors or dropped frames.

## Method and observations

Fourteen PNG frames were independently decoded from the final MP4, then visually inspected: 0.4, 2.9, 3.2, 4.2, 5.3, 6.5, 8.4, 9.8, 11.9, 12.2, 12.4, 13.3, 15.6 and 16.9 seconds. These are final encoded frames, not the renderer's planned JPEG stills. Temporary evidence is in `%TEMP%/ArrowSwarm-DefenseEncodedReview-20261115/`.

| Encoded time | Observed result |
| --- | --- |
| 0.4 s | Native top and bottom HUDs are intact. Opening headline sits between top HUD and board; “SÜRÜ YAKLAŞIYOR.” uses the lower gap. No choice quiz or replacement HUD. |
| 2.9–3.2 s | Freeze call anticipates the real action; charge changes from 1 to the native countdown, mobs turn blue and the leader annotation changes color. Caption and underline avoid the top HUD. |
| 4.2 s | “KARŞI SALDIRI!” stays in the lower gap. Arrows, enemy reactions, damage numbers, shot count and Freeze timer remain unobscured. |
| 5.3 s | Rainbow flight is visible, the native counter reads 15/15, enemies remain alive, and “SON OK…” plus the explicit “0,5×” marker fit above the lower HUD. |
| 6.5 s | “GÖKKUŞAĞI!” and “0,5×” remain readable; flights and native hit effects retain the whole gameplay composition. |
| 8.4 s | The leading surviving mob is still visibly present. This preserves the true distinction between projectile kills and later victory cleanup. |
| 9.8 s | Native completion panel and star-award animation are visible. “İŞTE BÖYLE!” is a small line above the panel with a visible gap; it does not touch or cover the panel. |
| 11.9–12.2 s | All three native gold stars are fully visible before promotion starts. “BÖLÜM KURTARILDI.” remains a small heading above the panel; original Turkish result controls are unobscured. |
| 12.4 s | A short, explicit transition blends the completed native result into the separate promotional card. |
| 13.3 s | Promo logo, CTA wording and Play Now button are clear, uncropped and centered. |
| 15.6–16.9 s | Editorial pointer/press treatment is visible on the promo button; the end card remains stable and readable through the final sampled frame. |

Top HUD band y=0–230 and bottom band y=1690–1920 were additionally compared with their corresponding native source frames at ten encoded samples before the promo. Mean absolute RGB differences range from 1.9440 to 3.1329 on the 0–255 scale. These are lossy encoded comparisons, not claims of byte-identical decoded pixels. Visual inspection finds no masked, replaced or obscured native HUD. The renderer separately asserts exact band equality before encoding.

## Timing and truthful narrative

The edit starts at native source frame 506. Real Freeze frame 596 maps to 3.000 s; first shot frame 617 maps to 3.700 s. The visible slow-motion section starts at 5.000 s and is labeled “0,5×”. Rainbow frame 659 maps to 5.200 s; production win frame 718 maps to 9.133 s. The complete three-star native result is visible by 11.9 s, before the 12.3 s promotional transition.

Caption text and narration support one creative sequence: pressure, Freeze, counterattack, rainbow and actual completion. They do not claim a one-life rescue or that the rainbow kills every enemy. The underlying audited source records 78 noncleanup damage callbacks, 22 projectile kills and one normal victory-cleanup kill. Three lives remain throughout. The surviving front mob visible at 8.4 s agrees with that evidence.

## Corrected CTA reuse and audio boundary

`audio/voice_06.wav` is independently verified byte-for-byte equal to the approved corrected-P `SharedOutro01/cta_voice.wav`. Both SHA256 values are:

`3cffc7f86966e7eb7c4c6e7b0524ea7a2ed59c9c013ad2305cfe0adc857e4c23`

The copied input is mono PCM16 at 48 kHz, 110,400 samples / 2.3 seconds. The final MP4's decoded 13.1–15.4 s mono audio has normalized waveform similarity 0.998053 to that input over all 110,400 samples. This supplies an independent technical check that the reused corrected voice appears in the encoded mix; it does not claim AAC bytes are identical or certify perceived pronunciation/listening quality. The CTA ends at 15.4 s before the 15.45 s button press.

The audio timeline explicitly identifies voice, music and SFX as editorial reconstruction using existing game sounds, not a recorded Unity AudioListener track. This review makes no subjective full-playback or audio-mix listening claim.

## Mock CTA and scope

The promo has generic `play.google.com` display text, not a verified published app destination. The preview's Play Now control is a button whose handler reveals the mock-status message; static source inspection finds no external-store navigation in that handler. `QA_REPORT.json` states `storeStatus: mock` and `publicStoreUrl: null`. Browser interaction was not performed during this independent encoded review.

The final file is ready for the user's creative review. This pass did not operate Unity, browse, run Git, or modify the renderer. It authored this report and temporary decoded review frames only.
