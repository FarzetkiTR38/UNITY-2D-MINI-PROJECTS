# Approved Defense Reels template

User approved15November2026: `DefensePilot02_20261115/ArrowSwarm_Defense_R2_Sirayla_TR.mp4` (21s, SHA256ce1e0492d36dd04b4dc55a0f8f8001f36d80d3bdc67657f64cbdfca843c250f3). Future requested Defense levels should use this style; do not start more automatically while another task is active.

- Start while the front enemy is around halfway, with breathing room on the path. “Düşmanlar ilerliyor. Ama panik yok.” is the approved opening tone.
- Real charged Freeze before the path is crowded; space legal taps so the arrows read sequentially at normal speed.
- Reserve a legal final arrow whose actual perimeter entry can hit the front group. Wait for ordinary flights to finish, visibly introduce the single remaining rainbow arrow, then emphasize its actual flight. Label any speed change.
- Every spawned enemy must have an actual projectile death. No cleanup9999 deaths, forced win, enemy reposition/HP/speed/damage/spawn cheats or painted-over survivors. Retry legal timing/seed/shot order when necessary.
- Keep native Top/Bottom HUD and scene composition. Show the real three-star result before the existing shared promotional ending.
- Reuse the approved corrected-P CTA byte-exact (`SharedOutro01/cta_voice.wav`, SHA2563cffc7f86966e7eb7c4c6e7b0524ea7a2ed59c9c013ad2305cfe0adc857e4c23). Store destination stays mock until the user provides a published link.
- Actual event timing/counts vary per level. Do not force15arrows/24enemies or reuse hardcoded timings from level10. Keep native provenance, legality/kill checks, scoped private-state recovery and original media.
- Capture through Unity MCP, inspect actual encoded frames, decode/audio checks, browser playback and review; commit completed phases on **ArrowSwarm**. More levels or ColorShift require the next explicit user request.

Reference implementation: `Tools/render_defense_r2.py`, `Tools/check_defense_r2_capture.py`, `Scripts/Marketing/Editor/MarketingCapture.Defense*.cs`. Current source/checks/template delivery are under `DefensePilot02_20261115/`. This approval closes the earlier “await R2 review” checkpoint in `DEFENSE_PILOT_01.md`.
