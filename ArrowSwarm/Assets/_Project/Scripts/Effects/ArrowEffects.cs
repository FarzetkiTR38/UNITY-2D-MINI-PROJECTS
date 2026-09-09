namespace ArrowSwarm.Effects
{
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages particle effects for arrow launch bursts, clash sparks on collision,
    /// and sparkle trails with zero GC allocations.
    /// </summary>
    public class ArrowEffects : Singleton<ArrowEffects>
    {
        [Header("Colors")]
        [SerializeField] private Color _clashSparkColor = new Color(1f, 0.92f, 0.45f, 1f);
        [SerializeField] private Color _launchDustColor = new Color(0.9f, 0.9f, 1f, 0.5f);

        private ParticleSystem _launchParticle;
        private ParticleSystem _clashParticle;

        protected override void OnSingletonAwake()
        {
            InitializeParticles();
        }

        private void InitializeParticles()
        {
            if (_launchParticle == null)
            {
                _launchParticle = CreateParticleEffect("ArrowLaunchParticle", 12, 0.25f, 0.15f, 2.5f);
            }
            if (_clashParticle == null)
            {
                _clashParticle = CreateParticleEffect("ArrowClashParticle", 20, 0.35f, 0.2f, 4.0f);
            }
        }

        private ParticleSystem CreateParticleEffect(string name, int maxParticles, float lifetime, float size, float speed)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = grad;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 22; // Render above arrows (5) and portals (15)

            // Use URP Sprite Unlit / Default particle material
            Material mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default"));
            mat.mainTexture = Texture2D.whiteTexture;
            renderer.material = mat;

            return ps;
        }

        /// <summary>
        /// Plays a subtle launch puff / dust burst at the arrow's tail/origin.
        /// </summary>
        public void PlayLaunchBurst(Vector3 position, Color? color = null)
        {
            if (DataManager.Instance?.PlayerData != null && !DataManager.Instance.PlayerData.vfxEnabled) return;
            if (_launchParticle == null) InitializeParticles();

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = position;
            emitParams.startColor = color ?? _launchDustColor;
            _launchParticle.Emit(emitParams, 8);
        }

        /// <summary>
        /// Plays metalic/light clash sparks at the obstacle collision point when blocked.
        /// </summary>
        public void PlayClashSparks(Vector3 collisionPoint, Color? color = null)
        {
            if (DataManager.Instance?.PlayerData != null && !DataManager.Instance.PlayerData.vfxEnabled) return;
            if (_clashParticle == null) InitializeParticles();

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = collisionPoint;
            emitParams.startColor = color ?? _clashSparkColor;
            _clashParticle.Emit(emitParams, 16);
        }
    }
}
