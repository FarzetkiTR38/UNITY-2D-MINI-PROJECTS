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
