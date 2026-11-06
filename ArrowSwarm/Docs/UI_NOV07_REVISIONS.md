# November 07 — logo, Profile, ranking and skill follow-up

User-authorized scope: preserve the approved composition while correcting the five supplied revisions. Baseline `77792af9`. Reference images are the five 2026-11-07 attachments; the active approved artwork remains Revision6. No later numbered board is implicitly started.

## Continuing checklist / intended checkpoints

- [x] Read project rules; snapshot source, initially dirty files, original private preferences and Editor state.
- [x] Capture the original Home/Profile/Leaderboard through Unity MCP; inspect the original disabled zero-Rocket source behavior.
- [x] Home: replace the flat TMP wordmark/crest with the existing approved illustrated Boot logo, retaining responsive anchors and other controls. Commit `2a3ae738`.
- [x] Profile: correct the avatar backing/alignment in the real editable name field. Commit `77934503`.
- [ ] Profile: refine the dice and country navigation using existing sprites/feedback. Commit.
- [ ] Leaderboard: remove the tiny top-three star decorations while retaining rank medals. Commit.
- [ ] Leaderboard: refine the status and Refresh plates into rounded controls; preserve rows and data behavior. Commit.
- [ ] Rocket: implement a scoped, duplicate-safe, retry/scene-safe rewarded refill and zero-charge interaction. Commit after native checks.
- [ ] Rocket: author a shared-art exhausted-skill modal in Shapes/Color Shift using live localized TMP, existing card/buttons/rocket art and quiet accents. Commit after native inspection.
- [ ] Defense Hint: prevent repeated spending while the suggested arrow remains unused, including the final available charge. Commit after native checks.
- [ ] Puzzle Hint: preserve a pending arrow/rotation suggestion without further spending until consumed/invalidated; cover lifecycle and rapid input. Commit after native checks.
- [ ] Verify both themes, supported languages, real controls, compact/tall/tablet layouts and existing routes; correct findings and commit each tested adjustment.
- [ ] Restore original private data and initially dirty files; deliver actual Before/After preview, evidence and final checklist commit.

The user requested approximately ten meaningful commits rather than one combined change. Adjacent checkpoints may be combined only when they cannot be independently compiled or tested. Author scene appearance in Edit Mode; runtime code handles only state, control flow and animation. No new image is required when the existing approved logo, UI kit, medal plates, rocket and shared skill-card artwork suffice.

## Acceptance details

- Logo has the supplied glossy illustrated lettering; existing source is shared with Boot and adds no texture duplication. All functional UI wording stays localized TMP.
- Profile remains editable, random name and country cycling work, and Save/Close retain their original behavior.
- Rank rows, mixed Editor sample/live data, cloud and score behavior are preserved. Only requested ornament/control appearance changes are authorized here.
- At zero Rockets, the real HUD control opens an authored modal. Close spends nothing. Failed ads are retryable; successful ads grant exactly one charge to the current attempt. Duplicate/late callbacks, retry/menu/scene changes and spam cannot grant extra charges or revive an old attempt.
- A pending Hint cannot repeatedly consume hints or trigger an ad while its suggested action remains outstanding. Other arrows remain playable; consuming the suggestion or restarting clears it predictably.
- Tests use scoped fake providers in native Unity Editor; never consume real ads or publish test scores. Preserve private preferences/player progress and pre-existing local modifications.

## Resume

Initial snapshot ready at the private temporary pointer `ArrowSwarm-Nov07-path.txt`. Original Editor was running ColorShiftScene at Game View index18 with background=true; its private player payload and five preferences were captured before stopping Play. Home logo and Profile avatar are delivered and verified. Profile dice/country controls are authored and awaiting native interaction checks; leaderboard helpers, Rocket runtime/authoring and all three hint guards/test stages are implemented but not yet committed/tested. Next: native Profile control run, then separate leaderboard stages, then skill checks. Keep unrelated dirty files out of staging.

## Completed evidence

`BeforeHome` and `BeforeMenus` are native captures from unchanged scene appearance. `Logo` and `Avatar` pass paired theme/ten-language layout checks. Home reuses the exact Boot sprite/texture (no raster addition) and its visible mesh stays clear of Play including the existing breathing animation. Avatar inspection found and removed a separate legacy `NameInput/Image` backing; the real input, caret and label remain bound. The two mask materials use existing shaders and no extra texture.
