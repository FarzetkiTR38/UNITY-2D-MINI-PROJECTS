"""Revise three original Turkish lines while reusing five unchanged pilot clips.

Writes only the new R2 audio folder. Timing is intentionally left to the editor.
"""
from __future__ import annotations

import asyncio
import hashlib
import json
import shutil
import wave
from pathlib import Path

from reference_pilot_voice import SR, VOICE, RATE, trim_to_wav
import edge_tts
import imageio_ffmpeg
import numpy as np

ROOT = Path(__file__).resolve().parents[1]
OLD = ROOT / "05_Reels/ReferencePilot02_20261114/audio"
OUTPUT = ROOT / "05_Reels/ReferencePilot02_R2_20261114/audio"
LINES = [
    "Bu penguen kırk iki oktan oluşuyor.",
    "Bu üç oktan hangisi önce çıkar?",
    "A mı, B mi, C mi?",
    "Cevap A!",
    "Çünkü solundaki yol açık.",
    "Şimdi geriye kalan okları yollayalım.",
    "Tebrikler! Kırk iki oktan oluşan penguen bölümünü başarıyla geçtin!",
    "Sen ilk seferde bildin mi?",
]
REGENERATE = {0, 5, 6}


async def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    old_manifest = json.loads((OLD / "voice_manifest.json").read_text(encoding="utf-8"))
    old_clips = {row["id"]: row for row in old_manifest["clips"]}
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
    semaphore = asyncio.Semaphore(3)

    async def produce(index: int, text: str) -> dict:
        raw = OUTPUT / f"voice_{index:02d}_raw.mp3"
        target = OUTPUT / f"voice_{index:02d}.wav"
        if index not in REGENERATE:
            previous = old_clips[index]
            assert previous["text"] == text, "Reuse text must be identical"
            for name in (raw.name, target.name):
                shutil.copy2(OLD / name, OUTPUT / name)
            row = dict(previous)
            row["requestedStartSeconds"] = None
            row["reusedFrom"] = f"ReferencePilot02_20261114/audio/{target.name}"
            row["generatedInThisRevision"] = False
            assert hashlib.sha256(target.read_bytes()).hexdigest() == row["sha256"]
            return row
        async with semaphore:
            # Fresh R2 output is a separate cache; no previous pilot is overwritten.
            if not raw.is_file() or not raw.stat().st_size:
                speech = edge_tts.Communicate(text, VOICE, rate=RATE)
                await asyncio.wait_for(speech.save(str(raw)), timeout=45)
            metrics = trim_to_wav(raw, target, ffmpeg)
        return {"id": index, "requestedStartSeconds": None, "text": text,
                "file": target.name, "rawFile": raw.name,
                "generatedInThisRevision": True, **metrics}

    clips = await asyncio.gather(*(produce(i, text) for i, text in enumerate(LINES)))
    manifest = {
        "provider": "Microsoft online speech via edge-tts", "voice": VOICE,
        "rate": RATE, "libraryVersion": edge_tts.__version__,
        "audioIsEditorial": True, "instagramAudioCopied": False,
        "generatedFromOriginalScript": True, "sampleRate": SR, "channels": 1,
        "revision": "R2", "regeneratedClipIds": sorted(REGENERATE),
        "reusedClipIds": sorted(set(range(len(LINES))) - REGENERATE),
        "timing": "Unscheduled: align narration after observing first-arrow exit and pause.",
        "clips": clips,
        "releaseNote": "Prototype narration; no exclusive-rights or hosted-service licensing claim.",
    }
    (OUTPUT / "voice_manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    checks = []
    for row in clips:
        path = OUTPUT / row["file"]
        with wave.open(str(path), "rb") as wav:
            assert (wav.getframerate(), wav.getnchannels(), wav.getsampwidth()) == (SR, 1, 2)
            pcm = np.frombuffer(wav.readframes(wav.getnframes()), dtype="<i2")
        assert len(pcm) and np.max(np.abs(pcm.astype(np.int32))) < 32767
        assert hashlib.sha256(path.read_bytes()).hexdigest() == row["sha256"]
        checks.append({"file": path.name, "durationSeconds": len(pcm) / SR,
                       "sampleRate": SR, "channels": 1, "clippedSamples": 0,
                       "sha256": row["sha256"], "reused": not row["generatedInThisRevision"]})
    (OUTPUT / "voice_QA.json").write_text(json.dumps({"passed": True, "clipCount": len(checks), "clips": checks}, indent=2), encoding="utf-8")
    (OUTPUT / "VOICE_PROVENANCE.md").write_text(
        "# R2 Turkish narration\n\n"
        "Three updated original sentences (clips 00, 05, 06) were synthesized with tr-TR-AhmetNeural at +8% using edge-tts. "
        "Five unchanged clips (01, 02, 03, 04, 07) are byte-for-byte copies of the prior pilot; their hashes are retained. "
        "Only the three public creative sentences were sent to the speech service; no Instagram audio, account cookies, credentials or private files were used.\n\n"
        "Narration is editorial, not Unity AudioListener output. Each raw MP3 and conservatively trimmed 48kHz mono PCM WAV is retained. "
        "Timing is deliberately unscheduled so the editor can show A exiting alone, pause, and then introduce the remaining arrows. "
        "No claim of exclusive rights or cleared hosted-service terms for public release is made.\n\n"
        "Tool source: https://github.com/rany2/edge-tts\n", encoding="utf-8")
    print(json.dumps({"output": str(OUTPUT), "voice": VOICE, "clips": [
        {"id": row["id"], "text": row["text"], "durationSeconds": row["durationSeconds"], "reused": not row["generatedInThisRevision"]}
        for row in clips]}, ensure_ascii=False, indent=2), flush=True)


if __name__ == "__main__":
    asyncio.run(main())
