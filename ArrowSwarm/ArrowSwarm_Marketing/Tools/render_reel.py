"""Render the approval pilot from genuine Unity frame captures, never from mockups.

Dependencies: pillow, numpy, imageio-ffmpeg. Install outside Unity, then run:
  python render_reel.py --source ../04_Raw_Gameplay/<capture> --output ../05_Reels/Pilot01
"""
from __future__ import annotations
import argparse, csv, hashlib, json, math, os, re, shutil, subprocess, sys, wave
from pathlib import Path

try:
    import numpy as np
    from PIL import Image, ImageDraw, ImageFont, ImageStat
    import imageio_ffmpeg
except ImportError:
    cache = Path(os.environ.get("TEMP", "")) / "ArrowSwarm-Marketing-Media-Python"
    if cache.is_dir():
        sys.path.insert(0, str(cache))
    import numpy as np
    from PIL import Image, ImageDraw, ImageFont, ImageStat
    import imageio_ffmpeg

W, H, FPS, DURATION, SR = 1080, 1920, 30, 18, 48000
TOOLS = Path(__file__).resolve().parent
PROJECT = TOOLS.parent.parent
FONT = PROJECT / "Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf"
FONT_LICENSE = PROJECT / "Assets/_Project/Fonts/Farzetki_Title_Font_H1-OFL.txt"
SECTIONS = [(0, 2.5, "WHICH ARROW FIRST?", "ÖNCE HANGİ OK?"),
            (2.5, 5.0, "WATCH THE EXIT.", "ÇIKIŞI TAKİP ET."),
            (5.0, 12.5, "CLEAR A PATH.", "YOLU AÇ."),
            (12.5, 15.5, "ONE ARROW AT A TIME.", "ADIM ADIM ÇÖZ."),
            (15.5, 18, "WHERE WOULD YOU START?", "SEN NEREDEN BAŞLARDIN?")]

def sha(path):
    h = hashlib.sha256()
    with Path(path).open("rb") as f:
        for b in iter(lambda: f.read(4*1024*1024), b""): h.update(b)
    return h.hexdigest()

def run(args, log):
    result = subprocess.run(args, capture_output=True, text=True, encoding="utf-8", errors="replace")
    Path(log).write_text(result.stdout + result.stderr, encoding="utf-8")
    if result.returncode: raise RuntimeError(f"Command failed; see {log}")
    return result

def fit(text, size, limit=800):
    while size >= 16:
        f = ImageFont.truetype(str(FONT), size)
        if f.getlength(text) <= limit: return f
        size -= 1
    raise ValueError("Text cannot fit safe area")

def centered(draw, y, text, fill, size, stroke=0):
    font = fit(text, size)
    box = draw.textbbox((0,0), text, font=font, stroke_width=stroke)
    x = int(495 - draw.textlength(text, font=font)/2)
    draw.text((x, y-box[1]), text, font=font, fill=fill, stroke_width=stroke,
              stroke_fill=(248,246,239,255))
    return [x-stroke,y,x+int(draw.textlength(text,font=font))+stroke,y+box[3]-box[1]]

def make_overlays(out, language):
    bounds=[]
    for i,(_,_,en,tr) in enumerate(SECTIONS):
        im=Image.new("RGBA",(W,H),(0,0,0,0)); d=ImageDraw.Draw(im)
        d.rounded_rectangle((211,252,779,299),radius=23,fill=(17,49,78,239))
        centered(d,263,"SHAPES MODE  /  ARROWSWARM",(213,244,255,255),28)
        bounds.append(centered(d,326,tr if language=="TR" else en,(16,49,78,255),66,2))
        if i==4:
            d.rounded_rectangle((127,400,863,460),radius=20,fill=(13,48,77,241))
            centered(d,409,"YORUMDA İLK HAMLENİ SÖYLE" if language=="TR" else "COMMENT YOUR FIRST MOVE",(255,255,255,255),30)
            centered(d,440,"GELİŞTİRME AŞAMASINDA" if language=="TR" else "IN DEVELOPMENT",(153,222,249,255),16)
        im.save(out/f"overlay_{language}_{i}.png")
    assert all(90<=b[0] and b[2]<=900 and 230<=b[1] and b[3]<=1500 for b in bounds)
    return bounds

def audio(out, events):
    n=SR*DURATION; stereo=np.zeros((n,2),dtype=np.float64)
    def tone(start,duration,freq,gain,pan=0,kind="bell"):
        a=int(start*SR); m=min(int(duration*SR),n-a)
        if a<0 or m<=0:return
        t=np.arange(m)/SR
        env=np.minimum(t/.012,1)*np.exp(-t*(5 if kind=="bell" else 1.4))
        if kind=="pad":env=np.sin(np.minimum(t/duration,1)*math.pi)**1.5
        sig=(np.sin(2*math.pi*freq*t)+.15*np.sin(2*math.pi*freq*2*t))*env*gain
        stereo[a:a+m,0]+=sig*math.sqrt((1-pan)/2)
        stereo[a:a+m,1]+=sig*math.sqrt((1+pan)/2)
    # Newly synthesized light pentatonic score; no samples or imported music.
    progression=[(50,57,62),(47,54,59),(43,50,55),(45,52,57)]
    melody=[74,69,78,76,71,74,81,78]
    beat=.625
    for beat_index in range(math.ceil(DURATION/beat)):
        t=beat_index*beat
        tone(t,.6,440*2**((melody[(beat_index*3+beat_index//4)%8]-69)/12),.035,(-.25 if beat_index%2 else .25))
        if beat_index%4==0:
            for note in progression[(beat_index//4)%4]:tone(t,2.4,440*2**((note-69)/12),.023,0,"pad")
    cues=[]
    for event in events:
        t=float(event["time_seconds"])
        if event["event"] not in ("fire","rotate") or not 0<=t<DURATION:continue
        tone(t,.15,1100 if event["event"]=="fire" else 660,.08,0)
        tone(t+.018,.12,1580 if event["event"]=="fire" else 880,.028,.15)
        cues.append({"seconds":t,"sourceEvent":event["event"],"sourceFrame":int(event["frame"]),"editorialCue":True})
    fade=np.minimum(np.arange(n)/(SR*.25),1)*np.minimum((n-1-np.arange(n))/(SR*.65),1)
    stereo*=fade[:,None]
    peak=float(abs(stereo).max()); stereo*=.68/max(peak,.68)
    pcm=(stereo*32767).astype("<i2")
    with wave.open(str(out/"original_editorial_audio.wav"),"wb") as f:
        f.setnchannels(2);f.setsampwidth(2);f.setframerate(SR);f.writeframes(pcm.tobytes())
    (out/"audio_cues.json").write_text(json.dumps(cues,indent=2),encoding="utf-8")
    return {"cues":len(cues),"samplePeakDbFS":20*math.log10(max(float(abs(stereo).max()),1e-12)),"sampleRate":SR,"channels":2}

def timestamp(t):
    ms=round(t*1000);return f"{ms//3600000:02}:{ms//60000%60:02}:{ms//1000%60:02},{ms%1000:03}"

def render(source, out, lang, ffmpeg):
    args=[ffmpeg,"-hide_banner","-y","-framerate",str(FPS),"-start_number","0","-i",str(source/"frame_%05d.jpg"),"-i",str(out/"original_editorial_audio.wav")]
    for i in range(len(SECTIONS)):args += ["-loop","1","-framerate",str(FPS),"-i",str(out/f"overlay_{lang}_{i}.png")]
    filters=["[0:v]fps=30,format=rgba[base]"]
    last="base"
    for i,(start,end,_,_) in enumerate(SECTIONS):
        # Captions softly appear; the captured gameplay itself is not retimed.
        filters.append(f"[{i+2}:v]format=rgba,fade=t=in:st={start}:d=0.16:alpha=1,fade=t=out:st={end-.12}:d=0.12:alpha=1[cap{i}]")
        nxt=f"v{i}";filters.append(f"[{last}][cap{i}]overlay=0:0:enable='between(t,{start},{end})'[{nxt}]");last=nxt
    filters.append(f"[{last}]scale=iw:ih:out_color_matrix=bt709:out_range=tv,format=yuv420p,setparams=range=limited:color_primaries=bt709:color_trc=bt709:colorspace=bt709[video]")
    args += ["-filter_complex",";".join(filters),"-map","[video]","-map","1:a","-t",str(DURATION),"-c:v","libx264","-preset","medium","-crf","18","-r","30","-pix_fmt","yuv420p","-color_range","tv","-colorspace","bt709","-color_primaries","bt709","-color_trc","bt709","-c:a","aac","-b:a","192k","-ar","48000","-ac","2","-af","loudnorm=I=-18:TP=-1.5:LRA=9","-movflags","+faststart","-metadata","comment=Genuine Unity Editor gameplay; captions and original synthesized audio added in post. No gameplay fabrication.",str(out/f"ArrowSwarm_Pilot01_{lang}.mp4")]
    (out/f"ffmpeg_command_{lang}.json").write_text(json.dumps(args,indent=2),encoding="utf-8")
    run(args,out/f"encode_{lang}.log")

def verify(path, out, ffmpeg):
    result=run([ffmpeg,"-hide_banner","-i",str(path),"-map","0:v:0","-map","0:a:0","-f","null","-","-progress","pipe:1"],out/(path.stem+"_decode.log"))
    frames=re.findall(r"^frame=(\d+)$",result.stdout,re.M)
    assert frames and int(frames[-1])==DURATION*FPS,"Incorrect decoded frame count"
    assert "1080x1920" in result.stderr and "h264" in result.stderr and "yuv420p" in result.stderr and "aac" in result.stderr
    assert "48000 Hz, stereo" in result.stderr
    assert not re.search(r"corrupt|Error while decoding|Invalid data",result.stderr,re.I)
    levels=run([ffmpeg,"-hide_banner","-i",str(path),"-vn","-af","loudnorm=I=-18:TP=-1.5:LRA=9:print_format=json","-f","null","-"],out/(path.stem+"_loudness.log"))
    match=re.findall(r'\{\s*"input_i".*?\}',levels.stderr,re.S); assert match
    metrics=json.loads(match[-1]); assert float(metrics["input_tp"])<=-1.0
    assert -19.5<=float(metrics["input_i"])<=-16.5,"Unexpected integrated loudness"
    return {"passed":True,"decodedFrames":int(frames[-1]),"durationSeconds":DURATION,"width":W,"height":H,"fps":FPS,"videoCodec":"h264","pixelFormat":"yuv420p","audioCodec":"aac","audioSampleRate":SR,"audioChannels":2,"audioLoudness":metrics,"sha256":sha(path),"bytes":path.stat().st_size}

def main():
    p=argparse.ArgumentParser();p.add_argument("--source",required=True,type=Path);p.add_argument("--output",type=Path,default=TOOLS.parent/"05_Reels/Pilot01");p.add_argument("--language",choices=["EN","TR","both"],default="both");a=p.parse_args()
    source=a.source.resolve();out=a.output.resolve();out.mkdir(parents=True,exist_ok=True)
    provenance=json.loads((source/"provenance.json").read_text(encoding="utf-8-sig"))
    assert provenance["width"]==W and provenance["height"]==H and provenance["fps"]==FPS
    assert provenance["frames"]>=DURATION*FPS
    files=sorted(source.glob("frame_*.jpg"));assert len(files)==provenance["frames"]
    assert all((source/f"frame_{i:05d}.jpg").is_file() for i in range(DURATION*FPS))
    for index in (0,75,180,360,539):
        with Image.open(files[index]) as im:
            assert im.size==(W,H);assert sum(ImageStat.Stat(im.convert("RGB")).mean)>20,"Black input frame"
    with (source/"events.csv").open(encoding="utf-8-sig",newline="") as f:events=list(csv.DictReader(f))
    assert any(e["event"]=="fire" and float(e["time_seconds"])<DURATION for e in events),"No genuine gameplay action logged"
    ffmpeg=imageio_ffmpeg.get_ffmpeg_exe();audio_qa=audio(out,events)
    languages=["EN","TR"] if a.language=="both" else [a.language];qa={}
    for language in languages:
        bounds=make_overlays(out,language); render(source,out,language,ffmpeg)
        video=out/f"ArrowSwarm_Pilot01_{language}.mp4";qa[language]=verify(video,out,ffmpeg);qa[language]["captionBounds"]=bounds
        srt="\n\n".join(f"{i+1}\n{timestamp(s)} --> {timestamp(e)}\n{tr if language=='TR' else en}" for i,(s,e,en,tr) in enumerate(SECTIONS))+"\n"
        (out/f"Pilot01_{language}.srt").write_text(srt,encoding="utf-8")
        first=Image.open(source/"frame_00000.jpg").convert("RGBA");first.alpha_composite(Image.open(out/f"overlay_{language}_0.png"));first.convert("RGB").save(out/f"cover_{language}.jpg",quality=95)
    # Contact sheet from the actual encoded deliverable, not its source stills.
    sample=out/f"ArrowSwarm_Pilot01_{languages[0]}.mp4"
    run([ffmpeg,"-hide_banner","-y","-i",str(sample),"-vf","fps=1/3,scale=270:480,tile=6x1","-frames:v","1",str(out/"encoded_contact_sheet.jpg")],out/"contact_sheet.log")
    shutil.copy2(FONT_LICENSE,out/"FONT_LICENSE.txt")
    shutil.copy2(source/"provenance.json",out/"source_provenance.json")
    (out/"CAPTIONS.md").write_text("""# Pilot01 posting copy — draft, not published

## English
Which arrow would you tap first? Watch this little shape clear, one path at a time. ArrowSwarm is in development. Real Unity gameplay; editorial captions and an original synthesized soundtrack added in post. Tell us your first move.

## Türkçe
İlk hangi oka dokunurdun? Bu şeklin adım adım açılışını izle. ArrowSwarm geliştirme aşamasında. Gerçek Unity oynanışı; başlıklar ve özgün sentezlenmiş sesler montajda eklendi. İlk hamleni yorumda söyle.

Suggested relevant tags: #ArrowSwarm #PuzzleGame #IndieGameDev #ArrowPuzzle

No public store link or release claim is included because production availability has not been verified.
""",encoding="utf-8")
    (out/"AUDIO_PROVENANCE.md").write_text("""# Audio provenance

The WAV is a newly generated, deterministic instrumental bed and short tonal cues synthesized by render_reel.py. No stock recordings, third-party songs, commercial tracks, samples or project music were imported. Farzetki Games may use and adapt this supplied synthesis/output commercially; no additional sampled-audio licence is involved. This is provenance rather than a guarantee of exclusive rights in simple musical patterns.

This is **editorial audio**, not a recording of Unity's AudioListener output. Cues align to the production action times recorded by events.csv; audio_cues.json retains those timestamps. No collision/win sounds are invented. The isolated nature of the audio is also declared in the MP4 metadata and posting copy.

Typography uses Farzetki_Title_Font_H1, an existing renamed Lilita One derivative supplied in the project under SIL OFL1.1. Its full notice is included in FONT_LICENSE.txt. No font file needs to be redistributed with the rendered MP4.
""",encoding="utf-8")
    report={"passed":True,"genuineUnitySource":str(source),"sourceProvenanceSha256":sha(source/"provenance.json"),"editorialAudio":True,"audio":audio_qa,"noGameplayRetiming":True,"platformTextSafeAreaAssumption":"1080x1920; important text x90..900, y230..460. Requires live Instagram UI review before publishing; no platform overlay guarantee.","videos":qa,"ffmpegVersion":subprocess.check_output([ffmpeg,"-version"],text=True).splitlines()[0]}
    (out/"QA_REPORT.json").write_text(json.dumps(report,indent=2),encoding="utf-8")
    print(json.dumps(report,indent=2),flush=True)

if __name__=="__main__":main()
