"""Prepare original Defense R2 speech without assuming unverified capture timing.

Only six original public creative lines go to the speech service. Previous
videos, common CTA, Unity assets, player data and private preferences are untouched.
"""
from __future__ import annotations

import asyncio
import argparse
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
OUTPUT = MEDIA / "05_Reels/DefensePilot02_20261115/audio"
SHARED = MEDIA / "05_Reels/SharedOutro01"
CTA_HASH = "3cffc7f86966e7eb7c4c6e7b0524ea7a2ed59c9c013ad2305cfe0adc857e4c23"
RAINBOW_FULL_HASH = "da2ef86c0aeff54d803e3690d9e041ebf741268846a73f959d6dfd52504fdf5b"
LINES = [
    "Düşmanlar ilerliyor.",
    "Ama panik yok.",
    "Önce dondur.",
    "Şimdi okları sırayla gönder.",
    "Son ok... Gökkuşağı!",
    "Hepsi temizlendi!",
]


def sha(path: Path) -> str:
    """Hash a local media input without modifying it."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write(path: Path, value: object) -> None:
    """Save this revision's public production metadata."""
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")


def shorten_rainbow_pause(target: Path) -> dict:
    """Remove only0.70s of verified middle silence; all spoken samples stay exact."""
    backup=OUTPUT/"voice_04_full_pause.wav";source=backup if backup.is_file() else target
    assert sha(source)==RAINBOW_FULL_HASH,"Unexpected full source: remeasure its pause before trimming"
    if not backup.is_file():shutil.copy2(source,backup)
    with wave.open(str(backup),"rb") as wav:
        assert (wav.getframerate(),wav.getnchannels(),wav.getsampwidth())==(SR,1,2)
        pcm=np.frombuffer(wav.readframes(wav.getnframes()),dtype="<i2").copy()
    a,b=round(.75*SR),round(1.45*SR);removed=pcm[a:b].astype(float)/32768
    rms=np.sqrt(np.mean(removed.reshape(-1,SR//100)**2,axis=1))
    assert float(np.max(rms))<10**(-43/20),"Cut window is not verified silence"
    result=np.concatenate((pcm[:a],pcm[b:]))
    with wave.open(str(target),"wb") as wav:
        wav.setnchannels(1);wav.setsampwidth(2);wav.setframerate(SR);wav.writeframes(result.astype("<i2").tobytes())
    exact=bool(np.array_equal(result[:a],pcm[:a]) and np.array_equal(result[a:],pcm[b:]));assert exact
    proof={"passed":True,"sourceFile":backup.name,"sourceSha256":sha(backup),"resultFile":target.name,
           "resultSha256":sha(target),"sourceDurationSeconds":len(pcm)/SR,"durationSeconds":len(result)/SR,
           "removedWindowSeconds":[.75,1.45],"removedSeconds":(b-a)/SR,"removedSamples":b-a,
           "cutMax10msRmsDbFS":float(20*np.log10(max(float(np.max(rms)),1e-12))),
           "outsideCutPcmSamplesByteExact":exact,"crossfadeApplied":False,
           "originalQuietRunSeconds":[.62,1.67],"revisedQuietRunSeconds":[.62,.97],
           "approximateGokkusagiOnsetSeconds":.97,"subjectiveListeningClaimed":False}
    write(OUTPUT/"rainbow_pause_edit.json",proof)
    return {"durationSeconds":len(result)/SR,"sha256":sha(target),"pauseEdit":proof}


def shorten_existing() -> None:
    """Update only clip04 and its metadata; other clips and CTA are not rewritten."""
    update=shorten_rainbow_pause(OUTPUT/"voice_04.wav")
    path=OUTPUT/"voice_manifest.json";manifest=json.loads(path.read_text(encoding="utf-8"))
    row=next(r for r in manifest["clips"] if r["id"]==4);row.update(update);write(path,manifest)
    path=OUTPUT/"voice_QA.json";qa=json.loads(path.read_text(encoding="utf-8"))
    row=next(r for r in qa["checks"] if r["file"]=="voice_04.wav");row.update({k:update[k] for k in ["durationSeconds","sha256"]})
    qa["rainbowMiddleSilenceEdit"]=update["pauseEdit"];write(path,qa)
    write(OUTPUT/"file_manifest.json",{"generatedUtc":datetime.now(timezone.utc).isoformat(),"files":[
        {"file":p.name,"bytes":p.stat().st_size,"sha256":sha(p)}
        for p in sorted(OUTPUT.iterdir()) if p.is_file() and p.name!="file_manifest.json"]})
    print(json.dumps(update,ensure_ascii=True,indent=2),flush=True)


async def main() -> None:
    """Generate independently placeable lines and copy the corrected CTA exactly."""
    OUTPUT.mkdir(parents=True, exist_ok=True)
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
    cta_source = SHARED / "cta_voice.wav"
    if sha(cta_source) != CTA_HASH:
        raise RuntimeError("Approved corrected-P CTA hash changed; do not regenerate it.")
    semaphore = asyncio.Semaphore(3)

    async def generate(index: int, text: str) -> dict:
        raw = OUTPUT / f"voice_{index:02d}_raw.mp3"
        target = OUTPUT / f"voice_{index:02d}.wav"
        async with semaphore:
            if not raw.is_file() or not raw.stat().st_size:
                await asyncio.wait_for(edge_tts.Communicate(text, VOICE, rate=RATE).save(str(raw)),45)
            metrics = trim_to_wav(raw, target, ffmpeg)
            if index==4:metrics.update(shorten_rainbow_pause(target))
        row = {"id": index,"requestedStartSeconds": None,"text": text,
               "file": target.name,"rawFile": raw.name,"rawSha256": sha(raw),
               "generatedThisRevision": True,**metrics}
        if index == 5:
            row["useCondition"] = "Use only after actual source verifies every enemy dies from projectiles, zero victory-cleanup kills and zero path finishes."
            row["useConditionVerified"] = False
        return row

    rows = await asyncio.gather(*(generate(i,text) for i,text in enumerate(LINES)))
    target = OUTPUT / "voice_06.wav"
    shutil.copy2(cta_source,target)
    cta_prov = json.loads((SHARED / "cta_voice_provenance.json").read_text(encoding="utf-8"))
    write(OUTPUT / "cta_source_provenance.json",cta_prov)
    assert sha(target) == CTA_HASH
    rows.append({"id":6,"requestedStartSeconds":None,"text":cta_prov["text"],
                 "file":target.name,"rawFile":None,"durationSeconds":cta_prov["durationSeconds"],
                 "sha256":sha(target),"generatedThisRevision":False,
                 "reusedFrom":"SharedOutro01/cta_voice.wav","approvedPOnsetPreserved":True,
                 "sourceSha256":CTA_HASH,"sourceProvenanceFile":"cta_source_provenance.json"})
    checks=[]
    for row in rows:
        path=OUTPUT / row["file"]
        with wave.open(str(path),"rb") as wav:
            assert (wav.getframerate(),wav.getnchannels(),wav.getsampwidth()) == (SR,1,2)
            pcm=np.frombuffer(wav.readframes(wav.getnframes()),dtype="<i2").astype(np.int32)
        assert len(pcm) and sha(path)==row["sha256"]
        clipped=int(np.count_nonzero(np.abs(pcm)>=32767))
        assert clipped==0 and abs(len(pcm)/SR-row["durationSeconds"])<1/SR
        checks.append({"file":path.name,"durationSeconds":len(pcm)/SR,"sampleRate":SR,
                       "channels":1,"sampleFormat":"PCM16","clippedSamples":clipped,
                       "samplePeakDbFS":float(20*np.log10(max(float(np.max(np.abs(pcm)))/32768,1e-12))),
                       "sha256":sha(path)})
    created=datetime.now(timezone.utc).isoformat()
    write(OUTPUT / "voice_manifest.json",{
        "provider":"Microsoft online speech via edge-tts","voice":VOICE,"rate":RATE,
        "libraryVersion":edge_tts.__version__,"generatedUtc":created,"audioIsEditorial":True,
        "instagramAudioCopied":False,"generatedFromOriginalScript":True,
        "sampleRate":SR,"channels":1,"sampleFormat":"PCM16","clips":rows,
        "timing":"Unassigned. Align measured clips only after the new native source proves the requested sequence.",
        "outcomeLineRequiresActualZeroCleanupProof":True,"subjectiveListeningClaimed":False,
        "storeDestination":"mock; no published app destination verified"})
    write(OUTPUT / "voice_QA.json",{
        "passed":True,"clipCount":len(rows),"checks":checks,"timingVerified":False,
        "approvedCtaPOnsetByteIdentical":True,"approvedCtaSha256":CTA_HASH,
        "outcomeConditionVerified":False,"rainbowMiddleSilenceEdit":rows[4]["pauseEdit"],"subjectiveListeningClaimed":False})
    (OUTPUT / "VOICE_PROVENANCE.md").write_text(
        "# Defense R2 narration\n\nSix original public Turkish sentences were synthesized with "
        "tr-TR-AhmetNeural at +8% via edge-tts. No reference-video audio, account cookies, private files, "
        "player state or credentials were sent. Raw MP3s and conservatively edge-trimmed 48kHz mono PCM16 "
        "WAVs are retained. Narration is editorial audio, not Unity AudioListener output.\n\n"
        "Clip04 retains its complete spoken samples but removes0.70s from the verified silent middle "
        "window0.75–1.45s. voice_04_full_pause.wav retains the original2.46s version. The edited clip is "
        "1.76s; the approximate Gökkuşağı onset is0.97s. rainbow_pause_edit.json records exact PCM checks.\n\n"
        "The seventh clip (voice_06.wav) is a byte-identical copy of the approved corrected-P CTA; SHA256 "
        +CTA_HASH+". Its source provenance records the composite onset patch. The store remains mock.\n\n"
        "Clip05, Hepsi temizlendi!, is conditional: do not use it until actual source event evidence proves "
        "all enemies died from projectiles with zero cleanup kills and zero path finishes. No start times "
        "are locked by this preparation step. Technical checks certify format/hash/no PCM clipping, "
        "not subjective listening, creative approval or hosted-service release rights.\n\n"
        "Tool source: https://github.com/rany2/edge-tts\n",encoding="utf-8")
    write(OUTPUT / "file_manifest.json",{"generatedUtc":created,"files":[
        {"file":p.name,"bytes":p.stat().st_size,"sha256":sha(p)}
        for p in sorted(OUTPUT.iterdir()) if p.is_file() and p.name!="file_manifest.json"]})
    print(json.dumps({"passed":True,"output":str(OUTPUT),"clips":[
        {"id":r["id"],"text":r["text"],"durationSeconds":r["durationSeconds"]} for r in rows],
        "timelineLocked":False,"outcomeConditionVerified":False},ensure_ascii=True,indent=2),flush=True)


if __name__ == "__main__":
    parser=argparse.ArgumentParser();parser.add_argument('--shorten-existing',action='store_true');args=parser.parse_args()
    if args.shorten_existing:shorten_existing()
    else:asyncio.run(main())
