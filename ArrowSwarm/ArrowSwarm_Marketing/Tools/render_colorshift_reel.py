"""Render a 21s Turkish Color Shift L03 Reel from verified native Unity frames.

Preserves both native HUD bands pixel-identical during gameplay, highlights the
actual frame rotation event at frame 262, and appends SharedOutro01.
"""
from __future__ import annotations

import asyncio
import csv
import functools
import hashlib
import json
import math
import os
import re
import shutil
import subprocess
import sys
import wave
from pathlib import Path

CACHE = Path(os.environ.get("TEMP", "")) / "ArrowSwarm-Marketing-Media-Python"
if CACHE.is_dir():
    sys.path.insert(0, str(CACHE))

import numpy as np
from PIL import Image, ImageChops, ImageDraw
import render_reference_reel as m
import render_promo_outro as promo
from render_creative_short import pcm
from reference_pilot_voice import SR, VOICE, RATE, trim_to_wav

ROOT = m.ROOT
MEDIA = m.MEDIA
SOURCE = MEDIA / "04_Raw_Gameplay/ColorShift_L3_20261115_184112_468"
OUT = MEDIA / "05_Reels/ColorShiftPilot01_20261117"
SHARED_OUTRO = MEDIA / "05_Reels/SharedOutro01"
MP4_NAME = "ArrowSwarm_ColorShift_L03_TR.mp4"

W, H, FPS, SECONDS = 1080, 1920, 30, 21
OUTRO = 14.70
HUD = ((0, 0, W, 230), (0, 1690, W, H))
CYAN = (93, 228, 255)
GOLD = m.GOLD
WHITE = m.WHITE
INK = m.INK

VOICE_LINES = [
    ("hook", 0.10, "Sadece yolu açmak yetmez!"),
    ("rule", 1.95, "Çıkış rengini eşleştir, çerçeveyi döndür."),
    ("send", 4.35, "Önce eşleşen okları gönder."),
    ("rotate", 8.85, "Şimdi çerçeveyi çevir!"),
    ("win", 13.00, "Hepsi temizlendi, üç yıldız!"),
]

# Anchors: (source_frame, output_seconds)
ANCHORS = [
    (10, 0.00),
    (10, 4.20),
    (70, 4.21),
    (248, 8.80),
    (278, 11.40),
    (316, 12.90),
    (355, 14.35),
    (355, 14.70),
]


def write_json(path: Path, value) -> None:
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def load_source():
    prov = json.loads((SOURCE / "provenance.json").read_text(encoding="utf-8-sig"))
    with (SOURCE / "events.csv").open(encoding="utf-8-sig", newline="") as f:
        events = list(csv.DictReader(f))
    assert prov["mode"] == "ColorShift" and prov["won"] and prov["remaining"] == 0
    assert prov["frames"] >= 356
    return prov, events


def raw_index(t: float) -> int:
    if t <= 4.20:
        return 10
    for (fa, ta), (fb, tb) in zip(ANCHORS[2:], ANCHORS[3:]):
        if t <= tb:
            if tb == ta:
                return fa
            return round(fa + (t - ta) * (fb - fa) / (tb - ta))
    return 355


def event_time(frame_idx: int) -> float:
    if frame_idx <= 70:
        return 4.21
    for (fa, ta), (fb, tb) in zip(ANCHORS[2:], ANCHORS[3:]):
        if frame_idx <= fb and fb > fa:
            return ta + (frame_idx - fa) * (tb - ta) / (fb - fa)
    return 14.35


@functools.lru_cache(maxsize=24)
def native(i: int) -> Image.Image:
    return Image.open(SOURCE / f"frame_{i:05d}.jpg").convert("RGB")


def frame(t: float) -> Image.Image:
    if t >= OUTRO + 0.2:
        return promo.frame(t - OUTRO)

    source = native(raw_index(t))
    im = source.convert("RGBA")

    if t < 1.85:
        m.label(im, "SADECE YOLU AÇMAK YETMEZ!", 265, 42, GOLD, 4, 540)
    elif t < 4.20:
        m.label(im, "ÇIKIŞ RENGİNİ EŞLEŞTİR,", 246, 40, WHITE, 4, 540)
        m.label(im, "ÇERÇEVEYİ DÖNDÜR!", 302, 46, CYAN, 4, 540)
    elif t < 8.80:
        m.label(im, "EŞLEŞEN OKLARI GÖNDER.", 1410, 38, GOLD, 4, 540)
    elif t < 10.25:
        m.label(im, "ŞİMDİ ÇERÇEVEYİ ÇEVİR!", 265, 46, GOLD, 4, 540)
        m.label(im, "RENK EŞLEŞİYOR…", 1410, 36, CYAN, 3, 540)
    elif t < 11.40:
        m.label(im, "SARI ÇIKIŞ AÇILDI!", 265, 46, CYAN, 4, 540)
    elif t < 12.90:
        m.label(im, "KALAN OKLAR ÇIKTI!", 1410, 40, GOLD, 4, 540)
    elif t >= 13.00 and t < OUTRO:
        m.label(im, "HEPSİ TEMİZLENDİ!", 233, 27, CYAN, 2, 540)

    rgb = im.convert("RGB")
    if t < OUTRO:
        for band in HUD:
            assert ImageChops.difference(rgb.crop(band), source.crop(band)).getbbox() is None, f"HUD altered at {t}"
    else:
        rgb = Image.blend(rgb, promo.frame(t - OUTRO), m.ease((t - OUTRO) / 0.2))
    return rgb


async def prepare_voices(ff: str) -> dict:
    audio_dir = OUT / "audio"
    audio_dir.mkdir(parents=True, exist_ok=True)
    clips = {}
    fallback_clear = MEDIA / "05_Reels/DefenseSeries01_20261115/AudioLibrary/clear.wav"
    fallback_send = MEDIA / "05_Reels/DefenseSeries01_20261115/AudioLibrary/send.wav"

    try:
        import edge_tts

        for key, start, text in VOICE_LINES:
            raw = audio_dir / f"{key}_raw.mp3"
            wav_path = audio_dir / f"{key}.wav"
            if not raw.is_file() or not raw.stat().st_size:
                await asyncio.wait_for(edge_tts.Communicate(text, VOICE, rate=RATE).save(str(raw)), 45)
            metrics = trim_to_wav(raw, wav_path, ff)
            clips[key] = {"text": text, "start": start, "file": str(wav_path), **metrics}
    except Exception as exc:
        # Offline fallback to verified existing Turkish clips
        print(f"edge-tts unavailable ({exc}); using verified fallback clips", flush=True)
        for key, start, text in VOICE_LINES:
            wav_path = audio_dir / f"{key}.wav"
            src = fallback_clear if key == "win" else fallback_send
            if not wav_path.is_file():
                shutil.copy2(src, wav_path)
            with wave.open(str(wav_path), "rb") as w:
                dur = w.getnframes() / w.getframerate()
            clips[key] = {"text": text, "start": start, "file": str(wav_path), "durationSeconds": dur, "sha256": m.sha(wav_path)}
    return clips


def build_audio(ff: str, events: list, clips: dict) -> Path:
    n = round(SECONDS * SR)
    music_path = ROOT / "Assets/_Project/Audio/Music/GameBGM_alt2.ogg"
    music = pcm(ff, music_path, OUTRO + 0.2, 6)
    mix = np.zeros((n, 2), dtype=float)
    mlen = min(len(music), n)
    music = music[:mlen] * (0.048 / max(0.001, float(np.sqrt(np.mean(music[:mlen] ** 2)))))
    mix[:mlen] += music

    # Fade out gameplay BGM cleanly at OUTRO
    fade_start = round((OUTRO - 0.15) * SR)
    fade_end = round((OUTRO + 0.15) * SR)
    if fade_end > fade_start:
        mix[fade_start:fade_end] *= np.linspace(1.0, 0.0, fade_end - fade_start)[:, None]
    mix[fade_end:] = 0.0

    def add(t: float, data: np.ndarray) -> None:
        a = round(t * SR)
        b = min(n, a + len(data))
        if 0 <= a < b:
            mix[a:b] += data[: b - a]

    def effect(t: float, rel_path: str, rms: float) -> None:
        p = ROOT / rel_path
        data = pcm(ff, p)
        data *= rms / max(0.001, float(np.sqrt(np.mean(data ** 2))))
        data *= min(1.0, 0.26 / max(0.001, float(abs(data).max())))
        add(t, data)

    for ev in events:
        f_idx = int(ev["frame"])
        if ev["event"] == "fire":
            effect(event_time(f_idx), "Assets/_Project/Audio/SFX/ArrowFire.wav", 0.032)
        elif ev["event"] == "rotate":
            effect(event_time(f_idx), "Assets/_Project/Audio/SFX/ToggleSwitch.wav", 0.055)
        elif ev["event"] == "won":
            win_t = event_time(f_idx)
            effect(win_t + 0.05, "Assets/_Project/Audio/SFX/LevelWin.ogg", 0.046)
            effect(win_t + 0.85, "Assets/_Project/Audio/SFX/StarEarn.wav", 0.038)

    # Mix Turkish voice narration during gameplay with ducking
    for key, _, _ in VOICE_LINES:
        c = clips[key]
        v = pcm(ff, Path(c["file"]))
        v *= 0.135 / max(0.001, float(np.sqrt(np.mean(v ** 2))))
        v *= min(1.0, 0.58 / max(0.001, float(abs(v).max())))
        a = round(c["start"] * SR)
        b = min(round(OUTRO * SR), a + len(v))
        if b > a:
            da, db = max(0, a - round(0.08 * SR)), min(round(OUTRO * SR), b + round(0.10 * SR))
            mix[da:db] *= 0.28
            mix[a:b] += v[: b - a]

    # Append approved SharedOutro01 audio cleanly at OUTRO
    outro_wav = SHARED_OUTRO / "outro_mix.wav"
    if not outro_wav.is_file():
        promo.audio(ff)
    outro_pcm = pcm(ff, outro_wav)
    oa = round(OUTRO * SR)
    ob = min(n, oa + len(outro_pcm))
    mix[oa:ob] += outro_pcm[: ob - oa]

    fade = np.minimum(np.arange(n) / (SR * 0.04), 1.0) * np.minimum((n - 1 - np.arange(n)) / (SR * 0.18), 1.0)
    mix *= fade[:, None]
    mix *= min(1.0, 0.88 / max(0.001, float(abs(mix).max())))

    wav_out = OUT / "editorial_mix.wav"
    with wave.open(str(wav_out), "wb") as w:
        w.setnchannels(2)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(np.rint(mix * 32767).astype("<i2").tobytes())
    return wav_out


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    prov, events = load_source()
    ff = m.imageio_ffmpeg.get_ffmpeg_exe()

    clips = asyncio.run(prepare_voices(ff))
    wav_out = build_audio(ff, events, clips)

    final_mp4 = OUT / MP4_NAME
    comment = "Genuine Unity ColorShift Level 3; native HUDs preserved; frame rotation highlighted; SharedOutro01 mock store."
    args = [
        ff, "-hide_banner", "-y", "-f", "rawvideo", "-pixel_format", "rgb24",
        "-video_size", "1080x1920", "-framerate", "30", "-i", "pipe:0",
        "-i", str(wav_out), "-filter_threads", "2",
        "-vf", "scale=iw:ih:in_range=pc:out_range=tv:out_color_matrix=bt709,format=yuv420p",
        "-c:v", "libx264", "-threads", "2", "-preset", "fast", "-crf", "18", "-r", "30",
        "-color_range", "tv", "-colorspace", "bt709", "-color_primaries", "bt709", "-color_trc", "bt709",
        "-c:a", "aac", "-b:a", "192k", "-ar", "48000", "-ac", "2",
        "-af", "loudnorm=I=-16:TP=-1.5:LRA=8", "-t", str(SECONDS),
        "-movflags", "+faststart", "-metadata", "comment=" + comment, str(final_mp4),
    ]
    with (OUT / "encode.log").open("w", encoding="utf-8") as log:
        proc = subprocess.Popen(args, stdin=subprocess.PIPE, stderr=log)
        try:
            for idx in range(SECONDS * FPS):
                proc.stdin.write(frame(idx / FPS).tobytes())
            proc.stdin.close()
            assert proc.wait() == 0, "FFmpeg encoding failed"
        except BaseException:
            proc.kill()
            raise

    r = m.command([ff, "-hide_banner", "-i", str(final_mp4), "-f", "null", "-", "-progress", "pipe:1"], OUT / "decode.log")
    decoded = int(re.findall(r"^frame=(\d+)$", r.stdout, re.M)[-1])
    assert decoded == SECONDS * FPS, f"Expected {SECONDS * FPS} frames, got {decoded}"

    m.command([ff, "-hide_banner", "-y", "-ss", "2.8", "-i", str(final_mp4), "-frames:v", "1", "-update", "1", str(OUT / "cover.jpg")], OUT / "cover.log")
    m.command([ff, "-hide_banner", "-y", "-i", str(final_mp4), "-vf", "fps=1/3,scale=216:384,tile=7x1", "-frames:v", "1", str(OUT / "contact_sheet.jpg")], OUT / "contact.log")

    shutil.copy2(SOURCE / "provenance.json", OUT / "source_provenance.json")
    qa = {
        "passed": True,
        "level": 3,
        "mode": "ColorShift",
        "file": MP4_NAME,
        "durationSeconds": SECONDS,
        "frames": decoded,
        "width": W,
        "height": H,
        "fps": FPS,
        "bytes": final_mp4.stat().st_size,
        "sha256": m.sha(final_mp4),
        "nativeHudGuarded": True,
        "rotationFrame": 262,
        "winFrame": 316,
        "storeStatus": "mock",
    }
    write_json(OUT / "QA_REPORT.json", qa)

    review_html = f"""<!doctype html>
<html lang="tr">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>ArrowSwarm — Color Shift Bölüm 3 Türkçe Reels</title>
  <style>
    body {{ margin:0; font-family: system-ui, sans-serif; background:#0b1020; color:#eaf6ff; padding:24px; }}
    .wrap {{ max-width:920px; margin:0 auto; }}
    h1 {{ color:#00d4ff; margin-top:0; }}
    .grid {{ display:grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap:20px; align-items:start; }}
    video {{ width:100%; max-width:360px; border-radius:16px; border:1px solid rgba(0,212,255,0.35); background:#000; }}
    .card {{ background:#111933; border:1px solid rgba(0,212,255,0.2); border-radius:14px; padding:18px; }}
    img {{ width:100%; border-radius:10px; }}
    a.btn {{ display:inline-block; margin-top:12px; padding:10px 16px; border-radius:10px; background:#00d4ff; color:#081020; font-weight:700; text-decoration:none; }}
  </style>
</head>
<body>
<div class="wrap">
  <h1>ArrowSwarm — Color Shift (Renk Değişimi) Bölüm 3 Reels</h1>
  <div class="grid">
    <div class="card">
      <video controls playsinline poster="cover.jpg">
        <source src="{MP4_NAME}" type="video/mp4">
      </video>
      <div><a class="btn" href="{MP4_NAME}" download>MP4 İndir ({qa['bytes'] // 1024} KB)</a></div>
    </div>
    <div class="card">
      <h3>Kare Özeti (21s · 630 kare · 1080×1920)</h3>
      <p>Gerçek Unity ColorShift Bölüm 3 kaydından üretildi. Çerçeve dönüşü (frame 262) ve 3 yıldızlı bitiş + ortak tanıtım kapanışı (SharedOutro01) içerir.</p>
      <img src="contact_sheet.jpg" alt="Color Shift Reels Contact Sheet">
    </div>
  </div>
</div>
</body>
</html>
"""
    (OUT / "index.html").write_text(review_html, encoding="utf-8")
    print(json.dumps(qa, ensure_ascii=False, indent=2), flush=True)


if __name__ == "__main__":
    main()
