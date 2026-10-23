# Dark theme artwork

Built-in ImageGen edit (background-extraction) of `Assets/_Project/Art/Sprites/ModeMenu/MainMenuBackdrop.png`, saved non-destructively to `Assets/_Project/Art/Sprites/ModeMenu/MainMenuNightCutout.png`.

Output: 941×1672 RGBA, genuine transparent background. Light keeps the original backdrop. Dark uses the cutout above an Inspector-authored navy root, with an inexpensive one-sample material that recolors only the stat plaques. This avoids color-keying away white character/letter highlights. TMP owns every localized menu label and selection state.

Final prompt:
> Use case: background-extraction. Edit target: supplied Arrow Swarm main-menu artwork. Create a clean transparent-background cutout for the SAME game UI, same portrait aspect and exact composition. Preserve the existing giant glossy ARROW SWARM logo, central smiling gold star, four blue/pink/green/orange arrow characters, colorful ribbon trails, little blue decorative arrows/dots at their original positions, and the two blank trophy/star stat pills at the top. Remove ONLY the cream background and the pale/white ambient backdrop glow around/between characters/ribbons to true transparent alpha. Keep all glossy highlights INSIDE the characters and letters intact; no dark holes in them. Retain accurate saturated original colors, smooth antialiased edges and original positions/scales. Keep the entire lower half largely empty/transparent exactly like the target; do not add buttons, text, numbers, new decoration, new layout, or new artwork. The only words are the existing ARROW SWARM wordmark; the two stat pill text areas remain blank. No opaque background or checkerboard.

Validation: Unity MCP paired screen captures, unchanged Light sprite/material restoration, 44 GPU checks including source alpha and sampled character/logo/ribbon RGBA preservation. Source alpha/UVs are retained by the stat material; capsule button alpha and held/disabled feedback are separately tested.
