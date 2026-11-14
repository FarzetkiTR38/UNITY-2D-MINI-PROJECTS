"""Original Defense narration and byte-exact reuse of the approved P-fixed CTA.

Only the six public creative lines below are sent to the speech service.
Outputs are editorial audio; Unity, existing media and player data are untouched.
"""
from __future__ import annotations

import asyncio
import hashlib
import json
import shutil
import wave
from datetime import datetime, timezone
from pathlib import Path

from reference_pilot_voice import SR, VOICE, RATE, trim_to_wav
import edge_tts
import imageio_ffmpeg
import numpy as np

MEDIA = Path(__file__).resolve().parents[1]
OUTPUT = MEDIA / "05_Reels/DefensePilot01_20261115/audio"
SHARED = MEDIA / "05_Reels/SharedOutro01"
APPROVED_CTA_SHA256 = "3cffc7f86966e7eb7c4c6e7b0524ea7a2ed59c9c013ad2305cfe0adc857e4c23"
LINES = [
    (.10, "Panik yok."),
    (1.45, "Biraz daha..."),
    (2.80, "Dondur!"),
    (3.85, "Şimdi sıra sende."),
    (5.45, "Son ok gökkuşağı!"),
    (9.70, "İşte böyle!"),
]
CTA_START = 13.10
CUT_SECONDS = 17


def sha(path: Path) -> str:
    """Return a file hash without changing its content."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write(path: Path, value: object) -> None:
    """Write UTF-8 production metadata only within this pilot's audio folder."""
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")


async def main() -> None:
    """Generate six speech clips, copy the approved CTA and validate all PCM."""
    OUTPUT.mkdir(parents=True, exist_ok=True)
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
    semaphore = asyncio.Semaphore(3)
    cta_source = SHARED / "cta_voice.wav"
    if sha(cta_source) != APPROVED_CTA_SHA256:
        raise RuntimeError("Approved CTA source hash differs; do not regenerate it.")

    async def generate(index: int, start: float, text: str) -> dict:
        raw = OUTPUT / f"voice_{index:02d}_raw.mp3"
        target = OUTPUT / f"voice_{index:02d}.wav"
        async with semaphore:
            if not raw.is_file() or not raw.stat().st_size:
                await asyncio.wait_for(
                    edge_tts.Communicate(text, VOICE, rate=RATE).save(str(raw)), 45)
            metrics = trim_to_wav(raw, target, ffmpeg)
        return {"id": index, "requestedStartSeconds": start, "text": text,
                "file": target.name, "rawFile": raw.name, "rawSha256": sha(raw),
                "generatedThisRevision": True, **metrics}

    rows = await asyncio.gather(*(generate(i, start, text)
                                 for i, (start, text) in enumerate(LINES)))
    target = OUTPUT / "voice_06.wav"
    shutil.copy2(cta_source, target)
    cta_provenance = json.loads((SHARED / "cta_voice_provenance.json").read_text(encoding="utf-8"))
    write(OUTPUT / "cta_source_provenance.json", cta_provenance)
    if sha(target) != sha(cta_source) or sha(target) != APPROVED_CTA_SHA256:
        raise RuntimeError("CTA copy must remain byte-identical to the approved P-fixed source.")
    rows.append({"id": 6, "requestedStartSeconds": CTA_START,
        "text": cta_provenance["text"], "file": target.name, "rawFile": None,
        "durationSeconds": cta_provenance["durationSeconds"], "sha256": sha(target),
        "generatedThisRevision": False, "reusedFrom": "SharedOutro01/cta_voice.wav",
        "approvedPOnsetPreserved": True, "sourceSha256": sha(cta_source),
        "sourceProvenanceFile": "cta_source_provenance.json",
        "sourceCompositeNote": "The edited WAV is copied exactly. Its base MP3 alone does not reproduce the approved P onset."})

    checks = []
    for row in rows:
        path = OUTPUT / row["file"]
        with wave.open(str(path), "rb") as wav:
            if (wav.getframerate(), wav.getnchannels(), wav.getsampwidth()) != (SR, 1, 2):
                raise RuntimeError(f"Unexpected PCM format: {path.name}")
            pcm = np.frombuffer(wav.readframes(wav.getnframes()), dtype="<i2")
        if not len(pcm):
            raise RuntimeError(f"Empty audio: {path.name}")
        peak = int(np.max(np.abs(pcm.astype(np.int32))))
        clipped = int(np.count_nonzero(np.abs(pcm.astype(np.int32)) >= 32767))
        if clipped or sha(path) != row["sha256"]:
            raise RuntimeError(f"PCM clipping or hash failure: {path.name}")
        duration = len(pcm) / SR
        if abs(duration - row["durationSeconds"]) >= 1 / SR:
            raise RuntimeError(f"Duration metadata mismatch: {path.name}")
        checks.append({"file": path.name, "durationSeconds": duration,
            "sampleRate": SR, "channels": 1, "sampleFormat": "PCM16",
            "clippedSamples": clipped, "samplePeakDbFS": float(20 * np.log10(max(peak / 32768, 1e-12))),
            "sha256": row["sha256"], "reused": not row["generatedThisRevision"]})

    starts = [row["requestedStartSeconds"] for row in rows]
    ends = [start + row["durationSeconds"] for start, row in zip(starts, rows)]
    windows = [{"id": row["id"], "startSeconds": start, "endSeconds": end,
        "gapUntilNextSeconds": (starts[i + 1] if i + 1 < len(rows) else CUT_SECONDS) - end}
        for i, (row, start, end) in enumerate(zip(rows, starts, ends))]
    overlaps = [window for window in windows if window["gapUntilNextSeconds"] < 0]
    created = datetime.now(timezone.utc).isoformat()
    write(OUTPUT / "voice_manifest.json", {
        "provider": "Microsoft online speech via edge-tts", "voice": VOICE, "rate": RATE,
        "libraryVersion": edge_tts.__version__, "generatedUtc": created,
        "audioIsEditorial": True, "instagramAudioCopied": False,
        "generatedFromOriginalScript": True, "sampleRate": SR, "channels": 1,
        "sampleFormat": "PCM16", "clips": rows, "subjectiveListeningClaimed": False,
        "timing": "Requested starts for a 17-second cut; inspect measured windows before locking editorial timing.",
        "storeDestination": "mock; app not published and no live destination verified"})
    write(OUTPUT / "voice_QA.json", {
        "passed": True, "clipCount": len(checks), "checks": checks,
        "approvedCtaPOnsetByteIdentical": True, "approvedCtaSha256": APPROVED_CTA_SHA256,
        "requestedCutSeconds": CUT_SECONDS, "requestedWindows": windows,
        "requestedTimingNonoverlapPassed": not overlaps, "requestedTimingOverlaps": overlaps,
        "subjectiveListeningClaimed": False})
    (OUTPUT / "VOICE_PROVENANCE.md").write_text(
        "# Defense Pilot01 Turkish narration\n\n"
        "Six original public Turkish sentences were synthesized with tr-TR-AhmetNeural at +8% using edge-tts. "
        "Only those six creative sentences were sent to the speech service. No Instagram audio, account cookies, "
        "credentials, player state or private files were used. Raw MP3s and conservatively edge-trimmed 48kHz mono PCM16 WAVs are retained.\n\n"
        "Clip06 is a byte-for-byte copy of the approved corrected-P SharedOutro01/cta_voice.wav, "
        "SHA256 " + APPROVED_CTA_SHA256 + ". It was not regenerated. "
        "The copied cta_source_provenance.json records its composite onset patch; the unedited base MP3 alone "
        "is not misrepresented as the source of that corrected WAV. The store destination remains mock.\n\n"
        "All narration is editorial audio, not Unity AudioListener output. Measured clip durations and requested "
        "17-second-cut windows are in voice_QA.json. PCM and hash checks are technical checks, not a subjective listening certification. "
        "No claim of exclusive rights or cleared hosted-service terms for public release is made.\n\n"
        "Tool source: https://github.com/rany2/edge-tts\n", encoding="utf-8")
    files = [{"file": path.name, "bytes": path.stat().st_size, "sha256": sha(path)}
             for path in sorted(OUTPUT.iterdir()) if path.is_file() and path.name != "file_manifest.json"]
    write(OUTPUT / "file_manifest.json", {"generatedUtc": created, "files": files})
    print(json.dumps({"passed": True, "output": str(OUTPUT), "clips": [
        {"id": row["id"], "text": row["text"], "durationSeconds": row["durationSeconds"],
         "startSeconds": starts[i], "endSeconds": ends[i]}
        for i, row in enumerate(rows)], "requestedTimingNonoverlapPassed": not overlaps,
        "requestedTimingOverlaps": overlaps}, ensure_ascii=True, indent=2), flush=True)


if __name__ == "__main__":
    asyncio.run(main())
