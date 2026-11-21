# Defense R2 · narration/edit preparation

This is asset preparation, not a locked video timeline or proof of the gameplay outcome. Parent owns the new native capture, final event mapping and render. Work is isolated to this revision; previous pilot/common outro/game assets are untouched.

## Measured clips

| ID | Spoken line | Duration | Placement constraint |
| --- | --- | --- | --- |
|00|Düşmanlar ilerliyor.|1.31s|Opening with enemies around half the path; leave actual HUD/approach readable.|
|01|Ama panik yok.|0.87s|After00 ends, while the natural approach continues.|
|02|Önce dondur.|0.88s|Near the actual earlier Freeze, not a fabricated still/frame recolor.|
|03|Şimdi okları sırayla gönder.|1.67s|Start of sequential legal shots. Let separate native arrow launches read.|
|04|Son ok... Gökkuşağı!|1.76s|Separate last-arrow setup/payoff. The middle silence was shortened0.70s without changing spoken PCM; “Gökkuşağı” starts approximately0.97s into this clip.|
|05|Hepsi temizlendi!|1.21s|**Conditional:** only after actual evidence proves projectile kills for all enemies, no cleanup kills or path finishes.|
|06|Oynamak için Play Store’dan indirin!|2.30s|Existing corrected-P CTA copied exactly. Preserve complete utterance before promo-button click.|

Use existing shared logo/promo art and licensed project font; no new raster artwork is required. A roughly22–26s cut gives the intro, visibly separated shots, a distinct rainbow beat, actual native result and a4.7s shared ending room to read. This is a creative target, not an assertion about capture duration.

## Event-based edit guidance

1. Open before the previous wall of enemies, at an actual frame whose front enemy is around the requested half-path point. Narration00→01 forms one connected sentence with a small natural pause.
2. Show the earlier actual Freeze and its charge/countdown change. Both HUDs remain visible. Do not imply a fabricated one-life rescue.
3. Space legal shots in the source recording. Avoid editing them back into a simultaneous barrage. No additional action caption is needed on every arrow; one lower-gap “SIRAYLA GÖNDER.” can carry the sequence.
4. Give the native last remaining arrow a short readable setup before its shot. A minimal outline/leader annotation can point to the real arrow while keeping the actual board visible. Use the pause in clip04 to separate anticipation and payoff. Its edited quiet region is0.62–0.97s; align the shot around0.97s into that clip, after the real promotion. If slow motion is used, label it; do not use an overlay rainbow as a substitute for the native effect.
5. Verify the real final enemy death before clip05. Keep the native WinPanel/star-award result long enough to show all earned gold stars before promo. Do not use clip05 based only on the previous pilot's normal victory cleanup.
6. Reuse the existing corrected-P closing voice and common promo card. Store destination remains mock; no published app link is invented.

## State/provenance

`audio/voice_manifest.json` intentionally leaves `requestedStartSeconds: null` for every clip. `audio/voice_QA.json` passes format/hash/no-clipping checks but leaves `timingVerified` and `outcomeConditionVerified` false until actual capture alignment is checked. Parent should record event-aligned starts and the successful source proof in the final render/audio timeline; this prep metadata is not a playback certification.

All seven WAVs are48kHz mono PCM16. Six short original public lines were synthesized with tr-TR-AhmetNeural at+8%; no reference-video audio or private account/player data were used. Clip06 SHA256 is `3cffc7f86966e7eb7c4c6e7b0524ea7a2ed59c9c013ad2305cfe0adc857e4c23`, byte-identical to the approved shared corrected-P CTA. Technical checks are not a subjective audio-listening or creative approval claim.

The parent requested a shorter native last-arrow hold. Clip04 therefore removes only silent samples0.75–1.45s from its2.46s original; the removed10ms-block RMS is at most−82.51dBFS. All samples outside that cut are byte-exact. No crossfade is needed in that silent window. Original full WAV and raw MP3 remain in `audio/`; `rainbow_pause_edit.json` proves the operation. Other narration clips and the shared CTA remain unchanged. Reproduction is in `defense_pilot_r2_voice.py`; it refuses a differently synthesized source until its silence is remeasured.
