"""Generate original Turkish narration for the reference-style Shapes2 pilot.

Only the eight short original sentences below are sent to Microsoft's online
speech service via edge-tts. No Instagram audio, user data or credentials are used.
Outputs are editorial narration, not audio captured from Unity.
"""
from __future__ import annotations

import asyncio
import hashlib
import json
import os
import subprocess
import sys
from pathlib import Path

CACHE = Path(os.environ.get("TEMP", "")) / "ArrowSwarm-Marketing-Media-Python"
if CACHE.is_dir():
    sys.path.insert(0, str(CACHE))

import edge_tts
import imageio_ffmpeg
import numpy as np

OUTPUT = Path(__file__).resolve().parents[1] / "05_Reels/ReferencePilot02_20261114/audio"
VOICE = "tr-TR-AhmetNeural"
RATE = "+8%"
LINES = [
    (0.15, "Bir penguen, kırk iki ok."),
    (1.75, "Bu üç oktan hangisi önce çıkar?"),
    (3.80, "A mı, B mi, C mi?"),
    (7.90, "Cevap A!"),
    (8.90, "Çünkü solundaki yol açık."),
    (11.10, "Şimdi sırayı takip et."),
    (21.80, "Kırk iki ok. Üç yıldız."),
    (24.10, "Sen ilk seferde bildin mi?"),
]
SR = 48000


def trim_to_wav(raw: Path, target: Path, ffmpeg: str) -> dict:
    """Trim only edge silence, keeping 50ms pre-roll and 100ms post-roll."""
    command = [ffmpeg, "-hide_banner", "-loglevel", "error", "-i", str(raw),
               "-vn", "-ar", str(SR), "-ac", "1", "-f", "f32le", "-"]
    result = subprocess.run(command, check=True, capture_output=True)
    pcm = np.frombuffer(result.stdout, dtype="<f4")
    # 10ms RMS frames; -48dB only locates speech edges and does not gate speech.
    block = SR // 100
    usable = pcm[:len(pcm) // block * block].reshape(-1, block)
    rms = np.sqrt(np.mean(usable * usable, axis=1))
    active = np.flatnonzero(rms > 10 ** (-48 / 20))
    if not len(active):
        raise RuntimeError(f"No narration signal in {raw}")
    start = max(0, int(active[0]) * block - int(.05 * SR))
    end = min(len(pcm), (int(active[-1]) + 1) * block + int(.10 * SR))
    trimmed = pcm[start:end]
    import wave
    with wave.open(str(target), "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(SR)
        wav.writeframes((np.clip(trimmed, -1, 1) * 32767).astype("<i2").tobytes())
    return {
        "durationSeconds": len(trimmed) / SR,
        "untrimmedDurationSeconds": len(pcm) / SR,
        "trimStartSeconds": start / SR,
        "trimEndSeconds": (len(pcm) - end) / SR,
        "samplePeakDbFS": float(20 * np.log10(max(float(np.max(np.abs(trimmed))), 1e-12))),
        "sha256": hashlib.sha256(target.read_bytes()).hexdigest(),
    }


async def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    semaphore = asyncio.Semaphore(3)
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()

    async def generate(index: int, start: float, text: str) -> dict:
        raw = OUTPUT / f"voice_{index:02d}_raw.mp3"
        target = OUTPUT / f"voice_{index:02d}.wav"
        async with semaphore:
            if not raw.is_file() or not raw.stat().st_size:
                speech = edge_tts.Communicate(text, VOICE, rate=RATE)
                await asyncio.wait_for(speech.save(str(raw)), timeout=45)
            metrics = trim_to_wav(raw, target, ffmpeg)
        return {"id": index, "requestedStartSeconds": start, "text": text,
                "file": target.name, "rawFile": raw.name, **metrics}

    clips = await asyncio.gather(*(generate(i, start, text) for i, (start, text) in enumerate(LINES)))
    manifest = {
        "provider": "Microsoft online speech via edge-tts",
        "voice": VOICE, "rate": RATE, "libraryVersion": edge_tts.__version__,
        "audioIsEditorial": True, "instagramAudioCopied": False,
        "generatedFromOriginalScript": True, "sampleRate": SR, "channels": 1,
        "clips": clips,
        "releaseNote": "Prototype narration. Provider output terms require verification before public/commercial release; package licensing does not license the hosted voice service.",
    }
    (OUTPUT / "voice_manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    (OUTPUT / "VOICE_PROVENANCE.md").write_text(
        "# Turkish pilot narration\n\n"
        "Eight original Turkish sentences were synthesized with tr-TR-AhmetNeural using edge-tts. "
        "Only this public creative script was sent to the speech service. No reference-video audio, credentials, account cookies or private files were used.\n\n"
        "The narration is added in editing and is not Unity AudioListener output. "
        "Raw MP3 and conservatively edge-trimmed 48kHz mono PCM WAV are retained with durations and hashes. "
        "There is no claim of exclusive rights or cleared third-party voice-service terms for a public release.\n\n"
        "Tool source: https://github.com/rany2/edge-tts\n", encoding="utf-8")
    print(json.dumps(manifest, ensure_ascii=False, indent=2), flush=True)


if __name__ == "__main__":
    asyncio.run(main())
