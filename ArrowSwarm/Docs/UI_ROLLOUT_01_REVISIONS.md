# Board01 revisions

Baseline `febf1768`. These three requested fixes belong to board01; board02 remains behind the user's approval gate. Preserve the existing local changes and player preferences; private revision snapshot is referenced by `%TEMP%/ArrowSwarm-UIRollout-revisions-path.txt`.

- [x] R1: Restore an explicit, legible FARZETKI GAMES TMP studio credit inside Boot's safe area. Native Light/Dark check, then commit.
- [ ] R2: Keep the entire shared cloud footer inside Boot and Home screen bounds, preserving its aspect ratio and texture reuse. Native paired-theme check, then commit.
- [ ] R3: Replace the distorted nested button-shaped progress fill with a rounded, correctly inset fill. Check 0/low/mid/full values and the real loading animation, then commit.
- [ ] Final: Repeat native phone layout checks, update actual previews, restore all original preferences/local bytes and Editor state. Return revised board01 for approval; do not begin board02.

Native evidence will be saved under `Captures/UIRollout/Phase01/Revisions/`.

R1 native check passed in Light/Dark at1080x1920: exact full studio wording and all four credit corners inside safe area; screenshots inspected. Other typography/theme binding checks also passed. R1 changes only Boot credit and its authoring/verification.
