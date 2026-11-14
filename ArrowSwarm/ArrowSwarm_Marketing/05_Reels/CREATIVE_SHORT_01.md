# Creative Short01 ·16-second Ahtapot Reels

User request: one fully authored, much shorter and more polished Reels; the long clearing sequence may be skipped or greatly accelerated. This is a new alternative, not a replacement of the approved seven videos. No game assets/scenes change.

## Edit decision

Use actual Shapes21 (purple octopus,44 arrows). Keep both native HUDs. Open immediately on the puzzle with a kinetic "44 OK. İLK HAMLEN?" hook; simplify to A/B. A=arrow5 is genuinely free; B=arrow1 hits another arrow. Do not claim A completely frees B: production rules show another blocker beyond it.

|Time|Beat|
|---|---|
|0–3.5s|Immediate hook, two marked choices and a two-second count|
|3.5–5.0s|Answer A, actual normal-speed flight, small idle breath at1/44|
|5.0–7.6s|Remaining solution compressed to2.6s, honestly labelled7.4×|
|7.6–10.8s|Final shot, return to1× and native three-star WinPanel|
|10.8–11.6s|Short breathing room on completed44/44 result|
|11.6–16s|Native-art logo/PlayNow promo, full corrected P-CTA, button press|

New short narration: "Kırk dört ok. Hangisi önce çıkar?" / "Önce A!" / "Ve ahtapot çözüldü!". Reuse the corrected2.3s CTA exactly. Continuous music plus game-owned UI/fire/win sounds replace the old repetitive full-solution pacing. Keep text readable, avoid huge confetti and fake gameplay. Editorial tap ring/labels are explicitly annotations. Store remains mock; no listing invented.

## Continuing checklist

- [x] Read standing rules; snapshot ten pre-existing dirty files outside Git.
- [x] Unity MCP read-only confirmation: Shapes21 has44 arrows, solvable; solverfirst5; A clear, B blocked at3 units. Clean Edit GameScene, no Play/prefs changes.
- [x] Generate three short narration lines; reuse P-corrected CTA. Four48k monoPCM16 clips pass hash/no-clipping QA.
- [ ] Implement the16s render, score, actual-frame timeline, safe captions and native HUD guards.
- [ ] Inspect stills; fix any coverage/readability/pacing issues before final encode.
- [ ] Decode, verify44 real actions/three lives, audio peaks/loudness, source hashes and corrected CTA hash.
- [ ] Review actual encoded opening/answer/rush/win/promo; test native browser play/seek and phone layout.
- [ ] Deliver MP4 and old/new preview, commit completed phases, preserve old outputs and player/project state.

## Exact next action

Implement `Tools/render_creative_short.py` into `05_Reels/CreativeShort01_20261114/`. Raw source is the existing900-frame native Shapes21 recording, not a fresh/fake play. Render stills first, then16s480frames. Original41.3s series video remains intact. Finish with local review and ask for creative feedback only after delivering the finished alternative.
