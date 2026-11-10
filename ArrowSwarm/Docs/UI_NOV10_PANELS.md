# Shapes Levels, leaderboard podium and outcome concepts

Baseline is `91e1aa75`.

- [x] Prepare two pairs of centered win and loss popup concepts. Keep them in the review gallery outside Unity Assets.
- [x] Remove the six Shapes-only decorations around the Levels mascots. Retain the mode emblem beside the localized label.
- [x] Improve contrast and weight of the first three leaderboard numerals.
- [x] Compare both themes in Unity, check saved preferences and deliver the offline gallery.

Popup direction A pairs a victory trophy with a replay emblem and blue ribbons. Direction B pairs a blue victory mascot with a pink retry mascot and a glass frame. Both concepts use a centered card with gameplay visible around it. Implementation of an outcome concept awaits the user's choice.

The leaderboard uses the existing bold Fazo face at 70 for ranks 1–3. Deep blue numerals contrast with the three metallic badges in both themes. The highlighted player variant uses white. The Levels scene changes are confined to the six decoration objects and their parent child list.

Unity MCP checked all three Levels routes, the leaderboard layout and localized text, and both themes at 1080×2400. The focused reports are under `Captures/UIRollout/Nov10Panels`. An optional broader test encountered the Editor sample label overriding its expected loading label and later stopped at the unrelated Profile dice assertion. The scoped two-theme layout and navigation run passed. The five player preferences and fifteen pre-existing modified files matched their starting values.

Review from disk at `Captures/UIRollout/Nov10Panels/index.html`.

## Outcome center approved in follow-up

The selected **07 · Oyun Kartları** concept supplies the star display for all three modes.

- [x] Add a warm gold plinth for Win and a cool steel plinth for Lose, with light rays and glints behind the original animated stars.
- [x] Use one shared shader and two authored materials. The decorative graphic has no pointer target and uses no additional textures.
- [x] Keep the panel frame, texts, footer, button artwork, icons and gameplay code as authored.
- [x] Verify real Defense win and loss flows, two-star wins in Shapes and Color Shift, and fatal bounces in all three modes through Unity MCP.
- [x] Inspect Light and Dark, Turkish and English, at 1080×2400 and 720×1280. Confirm pointer hits on the three loss buttons in both puzzle modes.
- [x] Restore the original player preferences, Game View size and all fifteen pre-existing dirty files after verification.

Review from disk at `Captures/OutcomeStarStage/index.html`. Its before/after Defense panels are staged in the native Game View without awarding progress. The puzzle examples are captured after real shots and rotations. This is a focused follow-up, with the numbered rollout review gate still in place.

### Star display follow-up

- [x] Switch each gray placeholder off when its gold star begins to pop. Restore gray slots before the next reveal and on puzzle retry.
- [x] Raise the shared star container by 14 canvas units in both results across all three modes.
- [x] Verify one, two and three stars frame by frame in Unity MCP at 1080×2400 and 720×1280, using both themes for each mode. Check retry and the three gray loss stars. The visible win pedestal clears the first button by about 22 and 15 screen pixels, respectively.
- [x] Add the updated native captures to the top of the existing preview and verify that local player preferences and pre-existing modified files match their starting state.
