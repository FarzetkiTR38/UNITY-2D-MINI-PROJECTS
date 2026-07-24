using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Creates fully-configured ParticleSystem GameObjects at runtime
    /// for all line clear, board clear, and Nova Cross effects.
    /// 
    /// Uses ProceduralVFXSprites for textures and VFXMaterialFactory
    /// for additive materials. No external assets required.
    /// 
    /// Each factory method returns a disabled GameObject ready for pooling.
    /// </summary>
    public static class LineClearVFXFactory
    {
        // ── Cell Burst ──────────────────────────────────────────

        /// <summary>
        /// Creates a per-cell particle burst effect.
        /// Small neon particles explode outward from a single cell position.
        /// Color is applied at spawn time via VFXPool.Get(pos, color).
        /// </summary>
        public static ParticleSystem CreateCellBurstPS(Transform parent, VFXConfigSO config)
        {
            var go = new GameObject("PS_CellBurst");
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();

            // Main Module
            var main = ps.main;
            main.playOnAwake = false;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.startColor = Color.white; // Overridden at spawn
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.3f;

            // Emission — single burst
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            int burstCount = config != null ? config.cellBurstParticleCount : 15;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, (short)burstCount)
            });

            // Shape — sphere outward
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            // Size Over Lifetime — shrink to nothing
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            // Color Over Lifetime — fade out
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.3f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            // Renderer
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "Dragging Pieces"; renderer.sortingOrder = 50;
            renderer.material = VFXMaterialFactory.Additive;

            // Use procedural soft glow texture
            var tex = ProceduralVFXSprites.SoftGlow;
            if (tex != null && renderer.material != null)
            {
                renderer.material.mainTexture = tex;
            }

            return ps;
        }

        // ── Sweep Line ──────────────────────────────────────────

        /// <summary>
        /// Creates a sweep line effect that travels along a row or column.
        /// A bright streak moves across with trailing sparkle particles.
        /// </summary>
        public static ParticleSystem CreateSweepLinePS(Transform parent, VFXConfigSO config)
        {
            var go = new GameObject("PS_SweepLine");
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();

            // Main Module
            var main = ps.main;
            main.playOnAwake = false;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startColor = config != null ? config.sweepLineColor : new Color(1f, 1f, 1f, 0.8f);
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;

            // Emission — continuous during sweep
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 80;

            // Shape — thin edge (will be rotated for row vs column)
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.5f, 0.5f, 0.01f);

            // Size Over Lifetime — shrink
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            // Color Over Lifetime — bright flash then fade
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 0.3f), new GradientColorKey(new Color(0.6f, 0.8f, 1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.6f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            // Renderer
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.sortingLayerName = "Dragging Pieces";
            renderer.sortingLayerName = "Dragging Pieces"; renderer.sortingOrder = 50;
            renderer.lengthScale = 2f;
            renderer.velocityScale = 0.1f;
            renderer.material = VFXMaterialFactory.Additive;

            var tex = ProceduralVFXSprites.Streak;
            if (tex != null && renderer.material != null)
            {
                renderer.material.mainTexture = tex;
            }

            return ps;
        }

        // ── Nova Cross Shockwave ────────────────────────────────

        /// <summary>
        /// Creates the premium Nova Cross effect.
        /// Expanding ring shockwave + cross-directional particle jets +
        /// central flash burst. The "wow" moment.
        /// </summary>
        public static ParticleSystem CreateNovaCrossPS(Transform parent)
        {
            var go = new GameObject("PS_NovaCross_Premium");
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();

            // Main Module — central burst
            var main = ps.main;
            main.playOnAwake = false;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.1f;

            // Neon cyan-magenta gradient for start color
            var startColorGradient = new Gradient();
            startColorGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0f, 1f, 1f), 0f),   // Cyan
                    new GradientColorKey(new Color(1f, 0f, 1f), 0.5f), // Magenta
                    new GradientColorKey(new Color(0.5f, 0.5f, 1f), 1f) // Violet
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(startColorGradient);

            // Emission — big burst
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 40),
                new ParticleSystem.Burst(0.1f, 20)
            });

            // Shape — sphere for radial explosion
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            // Size Over Lifetime
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            // Color Over Lifetime — glow then fade
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var colGrad = new Gradient();
            colGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colGrad);

            // Renderer
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "Dragging Pieces"; renderer.sortingOrder = 50;
            renderer.material = VFXMaterialFactory.Additive;

            var tex = ProceduralVFXSprites.Star;
            if (tex != null && renderer.material != null)
            {
                renderer.material.mainTexture = tex;
            }

            // ── Sub-emitter: Shockwave Ring ─────────────────────
            var ringGo = new GameObject("PS_NovaCross_Ring");
            ringGo.transform.SetParent(go.transform, false);

            var ringPs = ringGo.AddComponent<ParticleSystem>();
            var ringRenderer = ringGo.GetComponent<ParticleSystemRenderer>();

            var ringMain = ringPs.main;
            ringMain.playOnAwake = false;
            ringMain.duration = 0.6f;
            ringMain.loop = false;
            ringMain.startLifetime = 0.5f;
            ringMain.startSpeed = 0f;
            ringMain.startSize = 1.5f;
            ringMain.startColor = new Color(0f, 1f, 0.9f, 0.8f); // Neon cyan
            ringMain.maxParticles = 2;
            ringMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var ringEmission = ringPs.emission;
            ringEmission.enabled = true;
            ringEmission.rateOverTime = 0;
            ringEmission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 1),
                new ParticleSystem.Burst(0.15f, 1)
            });

            var ringShape = ringPs.shape;
            ringShape.enabled = false;

            // Size over lifetime — expand the ring
            var ringSol = ringPs.sizeOverLifetime;
            ringSol.enabled = true;
            ringSol.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.5f),
                    new Keyframe(0.3f, 8f),
                    new Keyframe(1f, 14f)
                ));

            // Color over lifetime — fade out the ring
            var ringCol = ringPs.colorOverLifetime;
            ringCol.enabled = true;
            var ringGrad = new Gradient();
            ringGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0.4f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            ringCol.color = new ParticleSystem.MinMaxGradient(ringGrad);

            ringRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            ringRenderer.sortingLayerName = "Dragging Pieces";
            ringRenderer.sortingOrder = 50;
            ringRenderer.material = VFXMaterialFactory.SoftAdditive;

            var ringTex = ProceduralVFXSprites.Ring;
            if (ringTex != null && ringRenderer.material != null)
            {
                ringRenderer.material.mainTexture = ringTex;
            }

            return ps;
        }

        // ── Board Clear (MEGA) ──────────────────────────────────

        /// <summary>
        /// Creates the mega supernova effect for when the entire board is cleared.
        /// Massive particle fountain + expanding shockwave + sparkle rain.
        /// This is the most impressive effect in the game.
        /// </summary>
        public static ParticleSystem CreateBoardClearPS(Transform parent, VFXConfigSO config)
        {
            var go = new GameObject("PS_BoardClear_Supernova");
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();

            int particleCount = (config != null ? config.boardClearParticleCount : 120) * 3; // Triple particle count for massive area

            // Main Module — supernova burst
            var main = ps.main;
            main.playOnAwake = false;
            main.duration = 1.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 5f); // Slower speed since they spawn everywhere
            main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 3.0f); // Massive particles
            main.maxParticles = particleCount + 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.2f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            // Rainbow neon color palette
            var colorGradient = new Gradient();
            colorGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0f, 1f, 1f), 0f),     // Cyan
                    new GradientColorKey(new Color(0.5f, 0f, 1f), 0.25f), // Purple
                    new GradientColorKey(new Color(1f, 0f, 0.8f), 0.5f),  // Magenta/Pink
                    new GradientColorKey(new Color(1f, 0.5f, 0f), 0.75f), // Orange
                    new GradientColorKey(new Color(0f, 1f, 0.5f), 1f)     // Green
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(colorGradient);

            // Emission — 3 staggered bursts for cascading feel
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            int burst1 = Mathf.RoundToInt(particleCount * 0.5f);
            int burst2 = Mathf.RoundToInt(particleCount * 0.3f);
            int burst3 = Mathf.RoundToInt(particleCount * 0.2f);
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, (short)burst1),
                new ParticleSystem.Burst(0.15f, (short)burst2),
                new ParticleSystem.Burst(0.35f, (short)burst3)
            });

            // Shape — box spanning the entire 8x8 board
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.scale = new Vector3(4f, 4f, 1f);

            // Size Over Lifetime — expand then shrink
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.3f),
                    new Keyframe(0.2f, 1f),
                    new Keyframe(1f, 0f)
                ));

            // Color Over Lifetime
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var colGrad = new Gradient();
            colGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.7f, 0.4f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colGrad);

            // Rotation Over Lifetime for visual flair
            var rotOverLifetime = ps.rotationOverLifetime;
            rotOverLifetime.enabled = true;
            rotOverLifetime.z = new ParticleSystem.MinMaxCurve(-2f, 2f);

            // Renderer
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "Dragging Pieces"; renderer.sortingOrder = 50;
            renderer.material = VFXMaterialFactory.Additive;

            var tex = ProceduralVFXSprites.Star;
            if (tex != null && renderer.material != null)
            {
                renderer.material.mainTexture = tex;
            }

            // ── Sub-emitter 1: Mega Shockwave Ring ──────────────
            var ringGo = new GameObject("PS_BoardClear_Ring");
            ringGo.transform.SetParent(go.transform, false);

            var ringPs = ringGo.AddComponent<ParticleSystem>();
            var ringRenderer = ringGo.GetComponent<ParticleSystemRenderer>();

            var ringMain = ringPs.main;
            ringMain.playOnAwake = false;
            ringMain.duration = 1f;
            ringMain.loop = false;
            ringMain.startLifetime = 0.8f;
            ringMain.startSpeed = 0f;
            ringMain.startSize = 2f;
            ringMain.startColor = new Color(0.3f, 0.8f, 1f, 0.9f);
            ringMain.maxParticles = 3;
            ringMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var ringEmission = ringPs.emission;
            ringEmission.enabled = true;
            ringEmission.rateOverTime = 0;
            ringEmission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 1),
                new ParticleSystem.Burst(0.2f, 1),
                new ParticleSystem.Burst(0.4f, 1)
            });

            var ringShape = ringPs.shape;
            ringShape.enabled = false;

            float shockwaveSize = config != null ? config.boardClearShockwaveSize : 12f;

            var ringSol = ringPs.sizeOverLifetime;
            ringSol.enabled = true;
            ringSol.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.3f),
                    new Keyframe(0.4f, shockwaveSize * 0.7f),
                    new Keyframe(1f, shockwaveSize)
                ));

            var ringCol = ringPs.colorOverLifetime;
            ringCol.enabled = true;
            var ringGrad = new Gradient();
            ringGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.5f, 0.8f, 1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.3f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            ringCol.color = new ParticleSystem.MinMaxGradient(ringGrad);

            ringRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            ringRenderer.sortingLayerName = "Dragging Pieces";
            ringRenderer.sortingOrder = 50;
            ringRenderer.material = VFXMaterialFactory.SoftAdditive;

            var ringTex = ProceduralVFXSprites.Ring;
            if (ringTex != null && ringRenderer.material != null)
            {
                ringRenderer.material.mainTexture = ringTex;
            }

            // ── Sub-emitter 2: Sparkle Rain ─────────────────────
            var sparkleGo = new GameObject("PS_BoardClear_Sparkle");
            sparkleGo.transform.SetParent(go.transform, false);

            var sparklePs = sparkleGo.AddComponent<ParticleSystem>();
            var sparkleRenderer = sparkleGo.GetComponent<ParticleSystemRenderer>();

            var sparkleMain = sparklePs.main;
            sparkleMain.playOnAwake = false;
            sparkleMain.duration = 1.5f;
            sparkleMain.loop = false;
            sparkleMain.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
            sparkleMain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            sparkleMain.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            sparkleMain.maxParticles = 50;
            sparkleMain.simulationSpace = ParticleSystemSimulationSpace.World;
            sparkleMain.gravityModifier = -0.2f; // Float upward

            // Neon sparkle colors
            var sparkleColorGrad = new Gradient();
            sparkleColorGrad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1f, 1f, 0.5f), 0f),   // Warm yellow
                    new GradientColorKey(new Color(0.5f, 1f, 1f), 0.5f), // Cyan
                    new GradientColorKey(new Color(1f, 0.5f, 1f), 1f)    // Pink
                },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
            sparkleMain.startColor = new ParticleSystem.MinMaxGradient(sparkleColorGrad);

            var sparkleEmission = sparklePs.emission;
            sparkleEmission.enabled = true;
            sparkleEmission.rateOverTime = 0;
            sparkleEmission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0.1f, 25),
                new ParticleSystem.Burst(0.4f, 25)
            });

            var sparkleShape = sparklePs.shape;
            sparkleShape.enabled = true;
            sparkleShape.shapeType = ParticleSystemShapeType.Box;
            sparkleShape.scale = new Vector3(6f, 6f, 0.1f);

            var sparkleSol = sparklePs.sizeOverLifetime;
            sparkleSol.enabled = true;
            sparkleSol.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.1f, 1f),
                    new Keyframe(0.5f, 0.8f),
                    new Keyframe(1f, 0f)
                ));

            var sparkleCol = sparklePs.colorOverLifetime;
            sparkleCol.enabled = true;
            var sparkleGrad2 = new Gradient();
            sparkleGrad2.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            sparkleCol.color = new ParticleSystem.MinMaxGradient(sparkleGrad2);

            sparkleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            sparkleRenderer.sortingLayerName = "Dragging Pieces";
            sparkleRenderer.sortingOrder = 50;
            sparkleRenderer.material = VFXMaterialFactory.Additive;

            var sparkleTex = ProceduralVFXSprites.DiamondShard;
            if (sparkleTex != null && sparkleRenderer.material != null)
            {
                sparkleRenderer.material.mainTexture = sparkleTex;
            }

            return ps;
        }

        // ── Enhanced Line Clear ─────────────────────────────────

        /// <summary>
        /// Creates an enhanced line clear effect (used when config prefab is null).
        /// Particles fly along the line direction with trailing sparkles.
        /// </summary>
        public static ParticleSystem CreateEnhancedLineClearPS(Transform parent)
        {
            var go = new GameObject("PS_LineClear_Enhanced");
            go.transform.SetParent(parent, false);
            go.SetActive(false);

            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.playOnAwake = false;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
            main.startColor = new Color(0.5f, 0.8f, 1f, 1f); // Default neon blue
            main.maxParticles = 25;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.2f;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 20)
            });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(4f, 0.3f, 0.01f); // Wide for row, will be rotated for column

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "Dragging Pieces"; renderer.sortingOrder = 50;
            renderer.material = VFXMaterialFactory.Additive;

            var tex = ProceduralVFXSprites.SoftGlow;
            if (tex != null && renderer.material != null)
            {
                renderer.material.mainTexture = tex;
            }

            return ps;
        }
    }
}
