using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Utility class to generate procedural placeholder audio clips at runtime using AudioClip.Create.
    /// This ensures the game has sound effects even without importing audio assets.
    /// Audio direction: "ambient cosmic + soft synth arcade", satisfying but not harsh.
    /// </summary>
    public static class ProceduralAudioGenerator
    {
        private const int SampleRate = 44100;

        /// <summary>
        /// Generates and assigns placeholder audio clips for any null fields in the AudioConfigSO.
        /// </summary>
        public static void GeneratePlaceholderClipsIfNull(AudioConfigSO config)
        {
            if (config == null) return;

            if (config.uiClick == null) config.uiClick = GenerateClick(800f, 0.05f, "Proc_UIClick");
            if (config.uiBack == null) config.uiBack = GenerateClick(400f, 0.08f, "Proc_UIBack");
            if (config.uiNavigate == null) config.uiNavigate = GenerateClick(600f, 0.04f, "Proc_UINavigate");

            if (config.piecePickup == null) config.piecePickup = GenerateSweep(330f, 660f, 0.12f, "Proc_PiecePickup");
            if (config.piecePlace == null) config.piecePlace = GeneratePluck(220f, 110f, 0.15f, "Proc_PiecePlace");
            if (config.pieceReturn == null) config.pieceReturn = GenerateSweep(330f, 165f, 0.15f, "Proc_PieceReturn");

            if (config.lineClear == null) config.lineClear = GenerateChord(new float[] { 523.25f, 659.25f, 783.99f }, 0.4f, "Proc_LineClear"); // C5 Major Triad
            if (config.novaCross == null) config.novaCross = GenerateNovaCross(0.6f, "Proc_NovaCross");

            if (config.comboIncrement == null) config.comboIncrement = GenerateDoubleBeep(660f, 880f, 0.15f, "Proc_ComboIncrement");
            if (config.comboBreak == null) config.comboBreak = GenerateSweep(220f, 80f, 0.35f, "Proc_ComboBreak");

            if (config.gameOver == null) config.gameOver = GenerateGameOverChord(0.9f, "Proc_GameOver");
            if (config.newBestScore == null) config.newBestScore = GenerateFanfare(new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.6f, "Proc_NewBestScore");
            if (config.reviveSuccess == null) config.reviveSuccess = GenerateSwell(220f, 660f, 0.8f, "Proc_ReviveSuccess");
            if (config.batchReady == null) config.batchReady = GenerateSwell(293.66f, 440f, 0.4f, "Proc_BatchReady"); // D4 to A4

            if (config.levelUp == null) config.levelUp = GenerateArpeggio(new float[] { 261.63f, 329.63f, 392.00f, 523.25f, 659.25f, 783.99f }, 0.5f, "Proc_LevelUp");
            if (config.achievementUnlock == null) config.achievementUnlock = GenerateFanfare(new float[] { 587.33f, 739.99f, 880.00f, 1174.66f }, 0.7f, "Proc_Achievement"); // D Major
            if (config.coinEarned == null) config.coinEarned = GenerateCoin(987.77f, 1318.51f, 0.25f, "Proc_CoinEarned"); // B5 to E6
            if (config.purchaseSuccess == null) config.purchaseSuccess = GenerateFanfare(new float[] { 523.25f, 783.99f, 1046.50f }, 0.5f, "Proc_PurchaseSuccess");

            if (config.gameplayMusic == null) config.gameplayMusic = GenerateCosmicDrone(6.0f, "Proc_GameplayMusic");
            if (config.homeMusic == null) config.homeMusic = GenerateCosmicDrone(8.0f, "Proc_HomeMusic");
        }

        // ── Audio Generation Helpers ─────────────────────────────────────

        private static AudioClip CreateClip(float[] samples, string name)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float[] CreateSampleBuffer(float duration)
        {
            int numSamples = Mathf.RoundToInt(SampleRate * duration);
            return new float[numSamples];
        }

        /// <summary>
        /// Generates a simple decaying sine tone.
        /// </summary>
        private static AudioClip GenerateClick(float frequency, float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            float phase = 0f;
            float phaseIncrement = (2f * Mathf.PI * frequency) / SampleRate;

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-t * 80f); // Fast decay
                samples[i] = Mathf.Sin(phase) * envelope * 0.5f;
                phase += phaseIncrement;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates a pitch sweep (frequency slide).
        /// </summary>
        private static AudioClip GenerateSweep(float startFreq, float endFreq, float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            float phase = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / samples.Length;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float phaseIncrement = (2f * Mathf.PI * currentFreq) / SampleRate;
                
                // Exponential decay envelope
                float envelope = 1f - t;
                samples[i] = Mathf.Sin(phase) * envelope * 0.4f;
                phase += phaseIncrement;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates a punchy pluck synth sound.
        /// </summary>
        private static AudioClip GeneratePluck(float startFreq, float endFreq, float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            float phase = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float normTime = (float)i / samples.Length;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, Mathf.Pow(normTime, 0.5f));
                float phaseIncrement = (2f * Mathf.PI * currentFreq) / SampleRate;

                // Strong pluck envelope (fast attack, exponential decay)
                float envelope = Mathf.Exp(-t * 25f);
                
                // Blend sine and a bit of triangle for synth character
                float sine = Mathf.Sin(phase);
                float tri = Mathf.PingPong(phase / Mathf.PI, 1f) * 2f - 1f;
                samples[i] = Mathf.Lerp(sine, tri, 0.2f) * envelope * 0.4f;

                phase += phaseIncrement;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates a chord consisting of multiple frequencies.
        /// </summary>
        private static AudioClip GenerateChord(float[] frequencies, float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float normTime = (float)i / samples.Length;
                float envelope = Mathf.Exp(-t * 6f) * (1f - normTime); // Smooth decay

                float sum = 0f;
                foreach (float freq in frequencies)
                {
                    sum += Mathf.Sin(2f * Mathf.PI * freq * t);
                }

                samples[i] = (sum / frequencies.Length) * envelope * 0.35f;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates a dual-tone coin ring (ting-ting).
        /// </summary>
        private static AudioClip GenerateCoin(float freq1, float freq2, float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            float noteDelay = duration * 0.25f;
            int delaySamples = Mathf.RoundToInt(SampleRate * noteDelay);

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float note1 = 0f;
                float note2 = 0f;

                // First note
                float env1 = Mathf.Exp(-t * 15f);
                note1 = Mathf.Sin(2f * Mathf.PI * freq1 * t) * env1;

                // Second note (delayed)
                if (i >= delaySamples)
                {
                    float t2 = (float)(i - delaySamples) / SampleRate;
                    float env2 = Mathf.Exp(-t2 * 15f);
                    note2 = Mathf.Sin(2f * Mathf.PI * freq2 * t2) * env2;
                }

                samples[i] = (note1 * 0.3f + note2 * 0.4f) * 0.5f;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates double beep effect.
        /// </summary>
        private static AudioClip GenerateDoubleBeep(float freq1, float freq2, float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            int halfPoint = samples.Length / 2;

            for (int i = 0; i < samples.Length; i++)
            {
                if (i < halfPoint)
                {
                    float t = (float)i / SampleRate;
                    float env = Mathf.Exp(-t * 25f);
                    samples[i] = Mathf.Sin(2f * Mathf.PI * freq1 * t) * env * 0.3f;
                }
                else
                {
                    float t = (float)(i - halfPoint) / SampleRate;
                    float env = Mathf.Exp(-t * 25f);
                    samples[i] = Mathf.Sin(2f * Mathf.PI * freq2 * t) * env * 0.3f;
                }
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates Nova Cross clear: high intensity chords + synth sweep + minor noise.
        /// </summary>
        private static AudioClip GenerateNovaCross(float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            float[] freqs = new float[] { 261.63f, 329.63f, 392.00f, 523.25f }; // C major chord C4-E4-G4-C5

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float normTime = (float)i / samples.Length;
                
                // Slow attack, then fade out
                float envelope = 1f;
                if (normTime < 0.15f)
                    envelope = normTime / 0.15f;
                else
                    envelope = Mathf.Exp(-(t - 0.09f) * 5f);

                // Add chord tones
                float chord = 0f;
                for (int j = 0; j < freqs.Length; j++)
                {
                    // Frequency sweeps slightly upward
                    float currentFreq = freqs[j] * (1f + normTime * 0.2f);
                    chord += Mathf.Sin(2f * Mathf.PI * currentFreq * t);
                }
                chord /= freqs.Length;

                // Add subtle filtered noise for explosion feel
                float noise = (Random.value * 2f - 1f) * 0.15f * Mathf.Exp(-t * 12f);

                samples[i] = (chord * 0.7f + noise) * envelope * 0.4f;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates a sad falling game over arpeggio/chord.
        /// </summary>
        private static AudioClip GenerateGameOverChord(float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            // C minor arpeggio C4, Eb4, G4, C3
            float[] freqs = new float[] { 261.63f, 311.13f, 392.00f, 130.81f };

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float normTime = (float)i / samples.Length;
                float envelope = Mathf.Exp(-t * 2.5f) * (1f - normTime);

                float sum = 0f;
                for (int j = 0; j < freqs.Length; j++)
                {
                    // Pitch slides down
                    float currentFreq = freqs[j] * Mathf.Lerp(1f, 0.7f, normTime);
                    sum += Mathf.Sin(2f * Mathf.PI * currentFreq * t);
                }

                samples[i] = (sum / freqs.Length) * envelope * 0.4f;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates a swelling sweep (sound rising in volume and frequency).
        /// </summary>
        private static AudioClip GenerateSwell(float startFreq, float endFreq, float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            float phase = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                float normTime = (float)i / samples.Length;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, normTime);
                float phaseIncrement = (2f * Mathf.PI * currentFreq) / SampleRate;

                // Swell envelope: quiet at start, loud at 80% mark, decays at end
                float envelope = 0f;
                if (normTime < 0.8f)
                    envelope = normTime / 0.8f;
                else
                    envelope = 1.0f - ((normTime - 0.8f) / 0.2f);

                samples[i] = Mathf.Sin(phase) * envelope * 0.35f;
                phase += phaseIncrement;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates a fast ascending arpeggio.
        /// </summary>
        private static AudioClip GenerateArpeggio(float[] frequencies, float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            float noteDuration = duration / frequencies.Length;
            int samplesPerNote = Mathf.RoundToInt(SampleRate * noteDuration);

            for (int i = 0; i < samples.Length; i++)
            {
                int noteIndex = i / samplesPerNote;
                noteIndex = Mathf.Clamp(noteIndex, 0, frequencies.Length - 1);
                float freq = frequencies[noteIndex];

                float t = (float)i / SampleRate;
                float tInNote = (float)(i % samplesPerNote) / SampleRate;
                
                // Exponential decay per note
                float env = Mathf.Exp(-tInNote * 12f);
                float globalEnv = 1f - ((float)i / samples.Length); // Fade out at end

                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * globalEnv * 0.3f;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates an uplifting fanfare (short sequence of chords/notes).
        /// </summary>
        private static AudioClip GenerateFanfare(float[] frequencies, float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            float noteDelay = duration * 0.15f;
            int delaySamples = Mathf.RoundToInt(SampleRate * noteDelay);

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float val = 0f;

                for (int j = 0; j < frequencies.Length; j++)
                {
                    int startSample = j * delaySamples;
                    if (i >= startSample)
                    {
                        float tNote = (float)(i - startSample) / SampleRate;
                        float env = Mathf.Exp(-tNote * 6f);
                        val += Mathf.Sin(2f * Mathf.PI * frequencies[j] * tNote) * env * 0.2f;
                    }
                }

                // Global fade out
                float globalFade = 1f - ((float)i / samples.Length);
                samples[i] = val * globalFade;
            }

            return CreateClip(samples, name);
        }

        /// <summary>
        /// Generates a simple, atmospheric, slow-moving space drone loop.
        /// Uses low frequency sine waves modulated by LFOs.
        /// </summary>
        private static AudioClip GenerateCosmicDrone(float duration, string name)
        {
            float[] samples = CreateSampleBuffer(duration);
            float baseFreq1 = 73.42f; // D2
            float baseFreq2 = 110.00f; // A2
            float baseFreq3 = 146.83f; // D3

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / SampleRate;
                float normTime = (float)i / samples.Length;

                // LFO modulation to keep the drone breathing
                float lfo1 = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.2f * t); // 0.2 Hz LFO
                float lfo2 = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.35f * t); // 0.35 Hz LFO

                float osc1 = Mathf.Sin(2f * Mathf.PI * baseFreq1 * t);
                float osc2 = Mathf.Sin(2f * Mathf.PI * baseFreq2 * t);
                float osc3 = Mathf.Sin(2f * Mathf.PI * baseFreq3 * t);

                // Blend oscillators with LFOs
                float mix = (osc1 * lfo1 * 0.4f) + (osc2 * lfo2 * 0.3f) + (osc3 * (1f - lfo1) * 0.2f);

                // Fade in and fade out at edges to make it loop seamlessly without clicks
                float crossfade = 1f;
                float fadeDur = 0.1f; // 10% fade duration
                if (normTime < fadeDur)
                    crossfade = normTime / fadeDur;
                else if (normTime > (1f - fadeDur))
                    crossfade = (1f - normTime) / fadeDur;

                // Ambient volume is low and pleasant
                samples[i] = mix * crossfade * 0.15f;
            }

            return CreateClip(samples, name);
        }
    }
}
