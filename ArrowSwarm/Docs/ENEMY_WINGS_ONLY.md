# Wings-only enemy revision

The user requests a fixed body with only wing flapping. Keep the same character, path movement, collider, HP, damage feedback, freeze, pause and pooled lifetime behavior.

## Phases

1. Record the implementation and protect the ten pre-existing modified files and scene/config/gameplay-source hashes.
2. Import a transparent parts sheet generated with the built-in image tool. Use one fixed body sprite and two detached wing sprites. Author a native looping clip that affects only the two wing child transforms. Bind the production prefab and share damage/freeze tint across all three renderers.
3. Verify through Unity MCP in the real GameScene. Prove the body sprite and local pose remain fixed during normal movement; both wings change pose and stop during freeze and pause. Check hits, HP, death, pool reuse and finish behavior, capture real gameplay and commit the evidence.

The body, face, horns and tail receive no animation curves. Existing hit squash remains impact feedback only. Clip speed and wing amplitudes are authored assets, editable in Unity. Existing scenes and level data remain untouched.

## Art source

Built-in image generation edited the previous character reference into a transparent sheet containing the body, left wing and right wing. The project copy is `Assets/_Project/Art/Sprites/Mobs/MobWingsOnly.png`. The three parts are sliced by Unity without modifying the generated pixels.

### Exact prompt

Use case: precise-object-edit. Asset type: separated sprite rig parts for a Unity 2D mobile game. Input image is the edit target/reference sheet of a glossy cute purple flying imp/ghost with pink horns, forehead pink diamond, purple eyes, tiny arms, cyan-tipped tail and cyan/pink fairy wings. Create ONE transparent PNG PARTS SHEET, NOT an animation sheet: exactly three separated components laid out in a horizontal row with generous transparent gutters. Left cell: the complete FIXED BODY ONLY, front-facing, including pink horns, blue top spike, face, tiny arms and curled cyan-tipped tail, but REMOVE ALL WINGS from behind the body. Center cell: one detached LEFT WING ASSEMBLY ONLY (a cyan upper petal and cyan/pink lower petals), root points to the RIGHT, outer feather/petal tips fan LEFT, retaining dark purple outline and glossy shading. Right cell: matching mirrored RIGHT WING ASSEMBLY ONLY with root to LEFT and tips RIGHT. The left and right wing assemblies must fit behind the round body's shoulders, so their roots may be hidden when assembled. Keep exactly the same character identity, colors, cute angry expression and polished glossy game sprite style as the supplied reference, looking like its first pose. No body deformation, no alternate poses, no blink, no smile change, no motion streaks, no shadows beneath objects, no text, no borders, no labels. Fully transparent background with clean true alpha. All 3 parts unoccluded and separated, no touching. Each part centered in its own third of the canvas, fully within the canvas.
