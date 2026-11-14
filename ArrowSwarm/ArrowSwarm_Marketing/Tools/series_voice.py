"""Seven editorial Turkish voice clips for each approved Shapes series episode.

Reuses approved generic pilot lines; generates only the shape-specific lines and
one shared direction-neutral explanation. Does not edit Unity or the pilot.
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
OUT = ROOT / "05_Reels/ShapesSeries_20261114"
PILOT = ROOT / "05_Reels/ReferencePilot02_R2_20261114/audio"
EPISODES = [(12,"kaplumbağa",45,"kırk beş"), (16,"ördek",42,"kırk iki"),
            (21,"ahtapot",44,"kırk dört"), (5,"roket",40,"kırk"),
            (29,"flamingo",40,"kırk"), (39,"ayçiçeği",44,"kırk dört")]
COMMON = {1:"Bu üç oktan hangisi önce çıkar?", 2:"A mı, B mi, C mi?",
          3:"Cevap A!", 4:"Çünkü önündeki yol açık.",
          5:"Şimdi geriye kalan okları yollayalım."}

def write_json(path: Path, data: dict) -> None:
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")

def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()

async def main() -> None:
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
    semaphore = asyncio.Semaphore(3)
    shared = OUT / "SharedAudio"
    shared.mkdir(parents=True, exist_ok=True)
    pilot = json.loads((PILOT / "voice_manifest.json").read_text(encoding="utf-8"))
    old = {clip["id"]:clip for clip in pilot["clips"]}

    async def synthesize(folder: Path, index: int, text: str) -> dict:
        folder.mkdir(parents=True, exist_ok=True)
        raw = folder / f"voice_{index:02d}_raw.mp3"
        target = folder / f"voice_{index:02d}.wav"
        async with semaphore:
            if not raw.is_file() or not raw.stat().st_size:
                for attempt in range(3):
                    try:
                        await asyncio.wait_for(edge_tts.Communicate(text, VOICE, rate=RATE).save(str(raw)), timeout=45)
                        break
                    except Exception:
                        if attempt == 2:
                            raise
                        await asyncio.sleep(1 + attempt)
            metrics = trim_to_wav(raw, target, ffmpeg)
        return {"id":index, "requestedStartSeconds":None, "text":text,
                "file":target.name, "rawFile":raw.name,
                "generatedInThisRevision":True, **metrics}

    neutral = await synthesize(shared, 4, COMMON[4])
    write_json(shared / "voice_manifest.json", {"provider":pilot["provider"],
               "voice":VOICE,"rate":RATE,"clips":[neutral]})

    async def episode(level: int, name: str, count: int, words: str) -> dict:
        folder = OUT / f"L{level:02d}/audio"
        folder.mkdir(parents=True, exist_ok=True)
        lines = {0:f"Bu {name} {words} oktan oluşuyor.", **COMMON,
                 6:f"Tebrikler! {words.capitalize()} oktan oluşan {name} bölümünü başarıyla geçtin!"}
        rows = {}
        for index in (1,2,3,5):
            assert old[index]["text"] == lines[index]
            for filename in (old[index]["file"],old[index]["rawFile"]):
                shutil.copy2(PILOT / filename, folder / filename)
            row = dict(old[index])
            row.update(requestedStartSeconds=None, generatedInThisRevision=False,
                       reusedFrom=f"ReferencePilot02_R2_20261114/audio/{row['file']}")
            assert digest(folder / row["file"]) == row["sha256"]
            rows[index] = row
        for filename in (neutral["file"],neutral["rawFile"]):
            shutil.copy2(shared / filename, folder / filename)
        rows[4] = {**neutral,"generatedInThisRevision":False,
                   "reusedFrom":"ShapesSeries_20261114/SharedAudio/voice_04.wav"}
        generated = await asyncio.gather(*(synthesize(folder, i, lines[i]) for i in (0,6)))
        rows.update({row["id"]:row for row in generated})
        clips = [rows[i] for i in range(7)]
        checks = []
        for row in clips:
            path = folder / row["file"]
            with wave.open(str(path),"rb") as wav:
                assert (wav.getframerate(),wav.getnchannels(),wav.getsampwidth()) == (SR,1,2)
                pcm = np.frombuffer(wav.readframes(wav.getnframes()),dtype="<i2")
            assert len(pcm) and np.max(np.abs(pcm.astype(np.int32))) < 32767
            assert digest(path) == row["sha256"]
            assert abs(len(pcm)/SR - row["durationSeconds"]) < .0001
            checks.append({"file":path.name,"durationSeconds":len(pcm)/SR,
                           "sampleRate":SR,"channels":1,"clippedSamples":0,
                           "sha256":row["sha256"],"reused":not row["generatedInThisRevision"]})
        manifest = {"level":level,"shape":name,"arrowCount":count,
                    "provider":pilot["provider"],"voice":VOICE,"rate":RATE,
                    "libraryVersion":edge_tts.__version__,"audioIsEditorial":True,
                    "instagramAudioCopied":False,"generatedFromOriginalScript":True,
                    "sampleRate":SR,"channels":1,"revision":"ShapesSeries_20261114",
                    "timing":"Unscheduled; parent renderer aligns to native first-arrow exit and victory.",
                    "generatedClipIds":[0,6],"reusedClipIds":[1,2,3,4,5],"clips":clips}
        write_json(folder / "voice_manifest.json",manifest)
        write_json(folder / "voice_QA.json",{"passed":True,"clipCount":7,"clips":checks})
        (folder / "VOICE_PROVENANCE.md").write_text(
            f"# Shapes {level}: Turkish editorial narration\n\n"
            f"The {name} episode has {count} arrows, as verified from its authored level. "
            "Its opening and congratulations were synthesized from original Turkish text using "
            "tr-TR-AhmetNeural at +8%. The four unchanged question/answer/resume lines reuse "
            "the approved R2 pilot WAVs byte for byte. One direction-neutral explanation "
            "was generated once for all six episodes and copied unchanged.\n\n"
            "Only the short public script was sent to the speech service; no reference audio, "
            "private player data, credentials or cookies were sent. Narration is editorial, "
            "not native Unity AudioListener capture. Raw MP3 and conservatively trimmed "
            "48kHz mono PCM WAV are retained with duration/hash/no-clipping checks. "
            "The previously approved shared Play Store promo narration is not modified.\n\n"
            "Technical checks do not claim subjective listening or exclusive voice rights.\n",
            encoding="utf-8")
        return {"level":level,"shape":name,"arrowCount":count,"clips":[
            {"id":row["id"],"text":row["text"],"durationSeconds":row["durationSeconds"]} for row in clips]}

    results = await asyncio.gather(*(episode(*item) for item in EPISODES))
    write_json(shared / "series_voice_checks.json", {"passed":True,"order":[e[0] for e in EPISODES],"episodes":results})
    print(json.dumps(results,ensure_ascii=False,indent=2),flush=True)

if __name__ == "__main__":
    asyncio.run(main())
