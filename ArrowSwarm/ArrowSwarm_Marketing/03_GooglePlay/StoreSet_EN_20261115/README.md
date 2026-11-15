# Arrow Swarm — English store screenshot set

Eight ordered **1080 × 1920, 24-bit RGB PNGs**, captured from the real English/Light game interface. Bright marketing ribbons are added below the complete native game view. No gameplay pixels, HUD text, stars, arrows, or leaderboard rows are painted into the capture.

Open `index.html` locally for the review gallery, source comparisons, full-size zoom and individual downloads. `overview_EN.jpg` is a review contact sheet. `ArrowSwarm_Store_EN_8.zip` contains the eight final PNGs only.

**Status:** prepared for creative review, not uploaded to Google Play. The leaderboard contains fictional demonstration players and local sample progress, not verified live players or global ranks. Home progress is also demonstration state. Native capture provenance is recorded in `Native/capture_manifest.json`.

## Image order, copy and English alt text

| # | Final PNG | Caption | Alt text |
|---|---|---|---|
| 1 | `01_Home_EN.png` | THREE MODES / ONE BIG CHALLENGE | Arrow Swarm Home with Play and three modes. Cyan ribbon: THREE MODES / ONE BIG CHALLENGE. |
| 2 | `02_Defense_EN.png` | TAP TO / ATTACK! | Defense level 10: arrow puzzle, advancing enemies and full HUD. Orange ribbon: TAP TO / ATTACK! |
| 3 | `03_Shapes_EN.png` | SHAPES TO / SOLVE | Shapes level 12: a turtle made of 45 arrows. Pink ribbon: SHAPES TO / SOLVE. |
| 4 | `04_ColorShift_EN.png` | MATCH THE / COLORS | Color Shift level 3: purple and gold arrow puzzle. Violet ribbon: MATCH THE / COLORS. |
| 5 | `05_Defense_Freeze_EN.png` | FREEZE THE / SWARM! | Defense level 15: Freeze turns the enemies icy blue. Teal ribbon: FREEZE THE / SWARM! |
| 6 | `06_Shapes_Advanced_EN.png` | FIND THE / WAY OUT | Shapes level 21: a purple octopus made of 44 arrows. Green ribbon: FIND THE / WAY OUT. |
| 7 | `07_ColorShift_Rotate_EN.png` | ROTATE TO / ESCAPE | Color Shift level 15 after a quarter-turn. Magenta ribbon: ROTATE TO / ESCAPE. |
| 8 | `08_Leaderboard_EN.png` | EARN STARS / CLIMB THE RANKS | Demo leaderboard with fictional players and sample ranks. Blue ribbon: EARN STARS / CLIMB THE RANKS. |

The same descriptions are available in `alt_text_EN.json` and in the review gallery's image alt attributes.

## Composition and provenance

- Native sources are under `Native/`; finished store images are under `Final/`.
- For a **1080 × 1620 native capture**, all source RGB pixels remain identical at coordinates `(0, 0)` through `(1079, 1619)`. The 300-pixel caption area begins at `y=1620`.
- A supported **1080 × 1920 native capture** is uniformly fitted to `900 × 1600` at `(90, 16)` to retain its entire frame. Its separate caption begins at `y=1632`.
- The title typeface is the existing project font `Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf`. No AI-rendered gameplay or new in-game assets are used.
- `store_set_manifest.json` records order, copy, palettes, source/output SHA-256 hashes, font provenance and the exact body rectangle for each image.
- `image_QA.json` records dimensions, RGB output, native-body integrity and caption bounds. Consult the current JSON after regeneration for the exact final source treatment and hashes.

## Rebuild

From the ArrowSwarm repository directory, with the existing cached marketing Pillow/NumPy environment available:

```powershell
py -3.13 ArrowSwarm_Marketing/Tools/compose_store_set_en.py
```

The composer requires all eight real native PNGs and a nonempty `Native/capture_manifest.json`. It refuses missing inputs. It writes the eight final PNGs, overview, composition manifest and image QA. Regenerate the final-only ZIP after any native source or caption change; review HTML and alt text do not need to be inside the store-image ZIP.

## Boundaries

This package creates marketing artwork only. Game scenes, mechanics, player saves, audio and production services are not modified by the compositor. The page makes no install-link, level-count or verified-rank claim. Final publication requires the user's creative review.
