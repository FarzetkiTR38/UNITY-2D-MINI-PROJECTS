# November menu and skill revisions

The five requested revisions are delivered on the ArrowSwarm branch. The original captures and source comparison use baseline `77792af9`. The review opens at `Captures/UIRollout/Nov07/index.html`.

## Implementation

| Checkpoint | Commit |
| --- | --- |
| Snapshot and implementation plan | `60704c9f` |
| Home reuses the existing illustrated Boot logo | `2a3ae738` |
| Profile avatar aligned and duplicate backing removed | `77934503` |
| Profile dice and country controls refined | `6e3ca4d5` |
| Rank stars removed and the first three numbers centered | `ede9ee7f` |
| Leaderboard status and Refresh use round capsules | `091f4a91` |
| Defense retains a paid suggestion until the arrow is used | `fa7c61c2` |
| Puzzle modes retain a paid arrow or rotation suggestion | `b0764a43` |
| Shapes and Color Shift offer a rewarded Rocket refill | `7853bf2e` |
| Profile controls share the glossy blue button artwork | `84ffa2fc` |

The final verification commit adds the reproducible editor fixtures, source scope check and review captures. Home and Boot share the same logo sprite. Profile uses the existing avatar, dice, arrows and two materials that clip the art to a round outline. The Rocket popup shares the existing skill card art, effects, title font and buttons. Its text uses the ten existing language tables.

## Verified in Unity

- [x] Light and Dark at 1080 × 1920, 1080 × 2400, 720 × 1280 and 1536 × 2048. Home, Profile, Leaderboard and the Rocket cards fit the native view. The preview uses actual Game View captures.
- [x] Profile name editing, random name, both country arrows, country wrapping, and Save and Close. The native leaderboard Refresh and Back checks passed in both themes using cached rows and an offline provider. The screenshots use the original sample presentation.
- [x] Shapes and Color Shift spend a real Rocket, open the exhausted card and accept native button input. Failed test ads can be retried. A successful result adds one charge. Duplicate and stale callbacks leave the current attempt intact.
- [x] All three modes spend one hint for an outstanding suggestion. Repeated requests reuse it. The final-charge HUD path also avoids opening a reward prompt. Suggested shots and Color Shift rotation consume their pending suggestion.
- [x] Five private player preferences matched the snapshot. The fourteen original dirty files match their initial bytes. The source guard reports zero violations and the existing recovery files remain present.

`Captures/UIRollout/Nov07/finalsummary.json` records the exact captures and reports. The ad tests run in the Unity Editor with a controlled test provider. The editor was returned to the Color Shift scene in Edit Mode at the original Game View size.

## Reproduce

Start from Boot and let the editor reach idle Home in Play Mode. Run the `UiRollout01Verification.RocketAt` and `HintAt` entry points through Unity MCP with an evidence folder. Use `Stage03ProfileAt` for the profile controls and `Stage03At` for the menu captures. The original private snapshot is referenced by the temporary `ArrowSwarm-Nov07-path.txt` pointer. The local preview is available at `http://127.0.0.1:8170/UIRollout/Nov07/index.html`.
