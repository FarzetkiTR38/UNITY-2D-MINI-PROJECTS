"""One reference-led Shapes 2 pilot, rendered from genuine Unity frames.

No Unity assets are modified. Zoom, hold, annotations, speed and audio are editorial.
Run with --stills first, then without it. Dependencies reuse the TEMP media environment.
"""
from __future__ import annotations
import argparse, csv, functools, hashlib, json, math, os, re, shutil, subprocess, sys, wave
from pathlib import Path
sys.path.insert(0, str(Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Media-Python'))
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import imageio_ffmpeg

ROOT=Path(__file__).resolve().parents[2]
MEDIA=ROOT/'ArrowSwarm_Marketing'
SOURCE=MEDIA/'04_Raw_Gameplay/Shapes_L2_20261114_101115_693'
OUT=MEDIA/'05_Reels/ReferencePilot02_20261114'
FONT=ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf'
LOGO=ROOT/'Assets/_Project/Art/Sprites/SharedUI/home_logo.png'
W,H,FPS,SECONDS,SR=1080,1920,30,28,48000
CREAM=(245,240,228); INK=(18,31,66); GOLD=(255,215,49); WHITE=(255,255,255)
VOICE_STARTS=[.10,1.90,3.95,8.60,9.60,11.80,21.65,24.45]
SAMPLE_TIMES=[.5,2.8,5.6,8.1,9.0,11.6,15,20.7,22.9,25.5]

def sha(path):return hashlib.sha256(Path(path).read_bytes()).hexdigest()
def ease(t):
    t=max(0,min(1,t));return t*t*(3-2*t)
def lerp(a,b,t):return a+(b-a)*t
def command(args, logfile):
    r=subprocess.run(args,capture_output=True,text=True,encoding='utf-8',errors='replace')
    Path(logfile).write_text(r.stdout+r.stderr,encoding='utf-8')
    if r.returncode:raise RuntimeError(f'FFmpeg failed: {logfile}')
    return r
@functools.lru_cache(maxsize=40)
def font(size):return ImageFont.truetype(str(FONT),round(size))
def label(im,text,y,size=64,color=WHITE,stroke=6,cx=510):
    d=ImageDraw.Draw(im);f=font(size)
    while d.textlength(text,font=f)>840:f=font(f.size-1)
    b=d.textbbox((0,0),text,font=f,stroke_width=stroke)
    x=cx-d.textlength(text,font=f)/2
    if stroke:
        shadow=Image.new('RGBA',im.size);sd=ImageDraw.Draw(shadow)
        sd.text((x+2,y-b[1]+4),text,font=f,fill=(7,17,40,100),stroke_width=stroke+1,stroke_fill=(7,17,40,100))
        im.alpha_composite(shadow)
        d=ImageDraw.Draw(im)
    d.text((x,y-b[1]),text,font=f,fill=color,stroke_width=stroke,stroke_fill=INK)
    return [round(x-stroke),round(y),round(x+d.textlength(text,font=f)+stroke),round(y+b[3]-b[1])]

def raw_index(t):
    if t<8.5:return 60
    if t<11.2:return round(65+(t-8.5)*30)
    if t<12:return round(146+(t-11.2)*30)
    if t<20:return round(170+(t-12)*370/8)
    return min(599,round(540+(t-20)*30))
def event_time(frame):
    if frame<146:return 8.5+(frame-65)/30
    if frame<170:return 11.2+(frame-146)/30
    if frame<540:return 12+(frame-170)*8/370
    return 20+(frame-540)/30
def camera(t):
    z=ease((t-1.2)/1.05)*(1-ease((t-11.1)/1.1))
    return lerp(1,1.3,z),lerp(1040,1340,z),lerp(960,940,z)
def mapping(t,point):
    scale,cy,ty=camera(t)
    return (540+(point[0]-540)*scale,ty+(point[1]-cy)*scale)
def grid_to_source(p):return (70+32.37*p[0],1430-32.37*p[1])
def read_paths():
    text=(ROOT/'Assets/_Project/ScriptableObjects/Puzzles/Shapes_2.asset').read_text()
    blocks=text.split('  - _points:')[1:]
    return [[tuple(map(int,p)) for p in re.findall(r'- \{x: (-?\d+), y: (-?\d+)\}',b.split('    _direction:')[0])] for b in blocks]
PATHS=read_paths()
@functools.lru_cache(maxsize=16)
def native_frame(i):
    im=Image.open(SOURCE/f'frame_{i:05d}.jpg').convert('RGB')
    # Editorial vertical crop removes native HUD, leaving genuine board and popup pixels.
    matte=im.getpixel((5,300))
    d=ImageDraw.Draw(im);d.rectangle((0,0,W,268),fill=matte);d.rectangle((0,1650,W,H),fill=matte)
    return im
@functools.lru_cache(maxsize=20)
def transformed_frame(index,s,cy,ty):
    source=native_frame(index)
    return source.transform((W,H),Image.Transform.AFFINE,(1/s,0,540-540/s,0,1/s,cy-ty/s),Image.Resampling.BICUBIC,fillcolor=source.getpixel((5,300))).convert('RGBA')
def scene_frame(t):return transformed_frame(raw_index(t),*camera(t)).copy()

def header(im,t):
    if 1.2<=t<12.3 or t>=20.2:return
    d=ImageDraw.Draw(im)
    d.rounded_rectangle((250,173,770,228),radius=27,fill=INK)
    label(im,'ŞEKİLLER  /  BÖLÜM 2',185,31,WHITE,0)
def quiz(im,t):
    if not 2.35<=t<8.85:return
    overlay=Image.new('RGBA',(W,H));d=ImageDraw.Draw(overlay)
    # Annotation is gone before the genuine first shot at 8.833s: no ghost path.
    visibility=1-ease((t-8.5)/.3)
    for order,(letter,idx) in enumerate([('A',0),('B',3),('C',1)]):
        pop=ease((t-(2.35+order*.25))/.25)*visibility
        if not pop:continue
        points=[mapping(t,grid_to_source(p)) for p in PATHS[idx]]
        chosen=letter=='A' and t>=8.5
        tint=(93,238,183) if chosen else GOLD
        alpha=round(90*pop)
        d.line(points,fill=(*tint,alpha),width=26,joint='curve')
        head=points[0];cx=min(880,max(160,head[0]));cy=1180 if letter!='C' else 1142
        d.line((head,(cx,cy-47)),fill=(*INK,round(230*pop)),width=3)
        r=round(43*pop)
        d.ellipse((cx-r+2,cy-r+5,cx+r+2,cy+r+5),fill=(*INK,round(120*pop)))
        d.ellipse((cx-r,cy-r,cx+r,cy+r),fill=(*tint,round(255*pop)),outline=(*INK,round(255*pop)),width=5)
        f=font(47);box=d.textbbox((0,0),letter,font=f)
        d.text((cx-d.textlength(letter,font=f)/2,cy-(box[3]-box[1])/2-box[1]),letter,font=f,fill=(*INK,round(255*pop)))
    im.alpha_composite(overlay)
    if 5.5<=t<8.5:
        d=ImageDraw.Draw(im);cx,cy,r=510,540,80
        d.ellipse((cx-r+3,cy-r+9,cx+r+3,cy+r+9),fill=(18,31,66,70))
        d.ellipse((cx-r,cy-r,cx+r,cy+r),fill=(49,34,104),outline=WHITE,width=5)
        progress=1-(t-5.5)%1
        d.arc((cx-r+6,cy-r+6,cx+r-6,cy+r-6),start=-90,end=-90+max(1,progress*360),fill=GOLD,width=10)
        label(im,str(3-int(t-5.5+1e-6)),cy-48,100,WHITE,2,cx)

def captions(im,t):
    if t<1.2:
        im.paste(im.filter(ImageFilter.GaussianBlur(6)),(0,0))
        im.alpha_composite(Image.new('RGBA',(W,H),(36,17,66,75)))
        card=Image.new('RGBA',(W,300));label(card,'BİL BAKALIM!',50,105,GOLD,9)
        card=card.rotate(-3,Image.Resampling.BICUBIC)
        im.alpha_composite(card,(0,690))
        label(im,'BİR PENGUEN. 42 OK.',1390,50,WHITE,5)
    elif t<3.9:
        label(im,'BU ÜÇ OKTAN',1360,66,WHITE)
        label(im,'HANGİSİ ÇIKAR?',1445,72,GOLD)
    elif t<5.5:label(im,'A MI, B Mİ, C Mİ?',1400,70,GOLD)
    elif t<8.5:label(im,'3 SANİYEN VAR.',1400,68,GOLD)
    elif t<9.55:label(im,'CEVAP: A!',1375,100,GOLD,8)
    elif t<11.3:
        label(im,'SOLUNDAKİ YOL',1370,67,WHITE)
        label(im,'AÇIK.',1450,80,GOLD)
    elif t<13.3:label(im,'ŞİMDİ SIRAYI TAKİP ET.',1500,55,WHITE)
    elif t<20:
        d=ImageDraw.Draw(im);d.rounded_rectangle((385,1520,635,1570),radius=24,fill=INK)
        label(im,'1,54×',1527,33,GOLD,0)
    elif t<21.65:label(im,'SON OK DA ÇIKTI.',1510,54,WHITE)
    else:label(im,'42 OK. 3 YILDIZ.',1510,58,GOLD)

@functools.lru_cache(maxsize=1)
def closing_background():
    y=np.arange(H)[:,None,None]/H
    top=np.array([10,25,57])[None,None,:];bottom=np.array([22,57,105])[None,None,:]
    arr=np.repeat(top*(1-y)+bottom*y,W,axis=1).astype('uint8')
    bg=Image.fromarray(arr).convert('RGBA');d=ImageDraw.Draw(bg)
    for i in range(28):
        x=70+(i*167)%940;yy=190+(i*271)%1490;r=2+i%3
        d.ellipse((x-r,yy-r,x+r,yy+r),fill=(90,194,255,90))
    return bg
@functools.lru_cache(maxsize=40)
def closing_pose(scale):
    im=closing_background().copy();logo=Image.open(LOGO).convert('RGBA')
    size=round(830*scale)
    logo=logo.resize((size,size),Image.Resampling.LANCZOS)
    im.alpha_composite(logo,(round((W-size)/2)-25,390))
    label(im,'İLK SEFERDE',1260,78,WHITE,5)
    label(im,'BİLDİN Mİ?',1360,93,GOLD,6)
    label(im,'YORUMDA SÖYLE.',1510,43,(170,222,255),0)
    label(im,'Farzetki Games',1640,27,(143,180,224),0)
    return im
def closing(t):return closing_pose(round(lerp(.96,1,ease((t-23.5)/.8)),5))
def frame(t):
    im=scene_frame(t);header(im,t);quiz(im,t);captions(im,t)
    if t>=23.5:
        c=closing(t);alpha=ease((t-23.5)/.35)
        im=Image.blend(im,c,alpha)
    return im.convert('RGB')

def make_audio(ffmpeg,events,countdown_times=(5.5,6.5,7.5),answer_time=8.55,victory_time=21.8):
    n=round(SECONDS*SR);mix=np.zeros((n,2));rng=np.random.default_rng(20826)
    def add(start,sig,gain=1,pan=0):
        a=round(start*SR)
        if a<0:return
        m=min(len(sig),n-a)
        if m<=0:return
        mix[a:a+m,0]+=sig[:m]*gain*math.sqrt((1-pan)/2)
        mix[a:a+m,1]+=sig[:m]*gain*math.sqrt((1+pan)/2)
    def tone(start,dur,freq,gain,kind='pluck',pan=0):
        tt=np.arange(round(dur*SR))/SR
        if kind=='kick':
            env=np.exp(-tt*18);sig=np.sin(2*np.pi*(48*tt+3*(1-np.exp(-tt*30))))*env
        elif kind=='pad':sig=np.sin(2*np.pi*freq*tt)*np.sin(np.pi*tt/dur)**2
        else:
            env=np.minimum(tt/.006,1)*np.exp(-tt*9)
            sig=(np.sin(2*np.pi*freq*tt)+.11*np.sin(4*np.pi*freq*tt))*env
        add(start,sig,gain,pan)
    beat=60/112
    for i in range(math.ceil(SECONDS/beat)):
        start=i*beat
        tone(start,.22,50,.075,'kick')
        note=[50,47,43,45][(i//8)%4]
        if i%2==0:tone(start,.44,440*2**((note-69)/12),.045)
        if i%4==0:
            for interval in [0,4,7]:tone(start,beat*3.8,440*2**((note+12+interval-69)/12),.013,'pad')
        if i%2==1:
            tt=np.arange(round(.065*SR))/SR;noise=rng.normal(0,1,len(tt));noise=np.convolve(noise,np.ones(7)/7,'same')
            add(start,noise*np.exp(-tt*70),.032)
        melody=[74,78,81,78,76,74,69,73]
        if i%2==0:tone(start,.45,440*2**((melody[(i//2)%8]-69)/12),.018,pan=(-.25 if i%4 else .25))
    for t in countdown_times:tone(t,.16,700,.11)
    for j,note in enumerate([74,78,81]):tone(answer_time+j*.075,.45,440*2**((note-69)/12),.08)
    cues=[]
    for e in events:
        if e['event']!='fire':continue
        t=event_time(int(e['frame']));tone(t,.12,1040,.045)
        cues.append({'sourceFrame':int(e['frame']),'sourceArrow':int(e['arrow']),'outputSeconds':round(t,6),'editorial':True})
    for j,note in enumerate([74,78,81,86]):tone(victory_time+j*.13,.8,440*2**((note-69)/12),.065)
    manifest=json.loads((OUT/'audio/voice_manifest.json').read_text(encoding='utf-8'))
    voice_rows=[]
    for i,start in enumerate(VOICE_STARTS):
        path=OUT/f'audio/voice_{i:02d}.wav'
        r=subprocess.run([ffmpeg,'-v','error','-i',str(path),'-f','f32le','-ar',str(SR),'-ac','1','pipe:1'],capture_output=True,check=True)
        mono=np.frombuffer(r.stdout,dtype='<f4').astype(float)
        assert start+len(mono)/SR<=SECONDS
        # Music ducks under narration; speech gets an independently normalized gentle level.
        rms=math.sqrt(np.mean(mono**2));mono*=.125/max(rms,.001)
        peak=abs(mono).max();mono*=min(1,.58/max(peak,.001))
        a=round(start*SR);b=min(n,a+len(mono));duck_a=max(0,a-round(.12*SR));duck_b=min(n,b+round(.2*SR))
        env=np.ones(duck_b-duck_a)*.32
        attack=a-duck_a;release=duck_b-b
        if attack:env[:attack]=np.linspace(1,.32,attack)
        if release:env[-release:]=np.linspace(.32,1,release)
        mix[duck_a:duck_b]*=env[:,None];add(start,mono,1)
        voice_rows.append({'clip':path.name,'start':start,'duration':len(mono)/SR,'sha256':sha(path)})
    fade=np.minimum(np.arange(n)/(SR*.1),1)*np.minimum((n-1-np.arange(n))/(SR*.35),1)
    mix*=fade[:,None];peak=float(abs(mix).max());mix*=min(1,.86/max(peak,.001))
    path=OUT/'editorial_mix.wav'
    with wave.open(str(path),'wb') as f:
        f.setnchannels(2);f.setsampwidth(2);f.setframerate(SR);f.writeframes((mix*32767).astype('<i2').tobytes())
    (OUT/'audio_timeline.json').write_text(json.dumps({'voice':voice_rows,'fireCues':cues,'countdown':list(countdown_times),'samplePeakBeforeEncode':peak,'voiceProvider':manifest.get('provider','see voice_manifest.json')},indent=2),encoding='utf-8')

def render(ffmpeg,filename='ArrowSwarm_Shapes2_ReferencePilot_TR.mp4',metadata_comment=None):
    path=OUT/filename
    args=[ffmpeg,'-hide_banner','-y','-f','rawvideo','-pixel_format','rgb24','-video_size',f'{W}x{H}','-framerate',str(FPS),'-i','pipe:0','-i',str(OUT/'editorial_mix.wav'),'-vf','scale=iw:ih:in_range=pc:out_range=tv:out_color_matrix=bt709,format=yuv420p','-c:v','libx264','-preset','fast','-crf','18','-r','30','-color_range','tv','-colorspace','bt709','-color_primaries','bt709','-color_trc','bt709','-c:a','aac','-b:a','192k','-ar',str(SR),'-ac','2','-af','loudnorm=I=-16:TP=-1.5:LRA=8','-t',str(SECONDS),'-movflags','+faststart','-metadata','comment=Genuine Unity Shapes2 footage. Editorial crop, hold, zoom, speed, annotations, original score and synthetic Turkish narration. No gameplay change.',str(path)]
    (OUT/'ffmpeg_command.json').write_text(json.dumps(args,indent=2),encoding='utf-8')
    if metadata_comment:
        index=args.index('-metadata');args[index+1]='comment='+metadata_comment
        (OUT/'ffmpeg_command.json').write_text(json.dumps(args,indent=2),encoding='utf-8')
    with (OUT/'encode.log').open('w',encoding='utf-8') as log:
        p=subprocess.Popen(args,stdin=subprocess.PIPE,stderr=log)
        try:
            for i in range(SECONDS*FPS):
                p.stdin.write(frame(i/FPS).tobytes())
                if i%90==0:print(f'Encoded input {i}/{SECONDS*FPS}',flush=True)
            p.stdin.close();code=p.wait()
        except BaseException:p.kill();raise
    if code:raise RuntimeError('Encoding failed; see encode.log')
    return path
def verify(ffmpeg,path):
    r=command([ffmpeg,'-hide_banner','-i',str(path),'-f','null','-','-progress','pipe:1'],OUT/'decode.log')
    count=int(re.findall(r'^frame=(\d+)$',r.stdout,re.M)[-1]);assert count==SECONDS*FPS
    assert '1080x1920' in r.stderr and 'yuv420p' in r.stderr and '48000 Hz, stereo' in r.stderr
    assert not re.search(r'corrupt|Error while decoding|Invalid data',r.stderr,re.I)
    r=command([ffmpeg,'-hide_banner','-i',str(path),'-vn','-af','loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json','-f','null','-'],OUT/'loudness.log')
    levels=json.loads(re.findall(r'\{\s*"input_i".*?\}',r.stderr,re.S)[-1])
    assert float(levels['input_tp'])<=-1 and -18<=float(levels['input_i'])<=-14
    # Every logged action is represented by an output-frame-index mapping in chronological order.
    output_frames=[round(event_time(int(e['frame']))*FPS) for e in EVENTS]
    assert len(output_frames)==42 and output_frames==sorted(set(output_frames))
    assert raw_index(22)==599 and raw_index(20)<547
    qa={'passed':True,'width':W,'height':H,'fps':FPS,'durationSeconds':SECONDS,'decodedFrames':count,'audio':levels,'bytes':path.stat().st_size,'sha256':sha(path),'source':SOURCE.name,'nativeActionCount':42,'correctCandidate':'A / arrow0','sourceWinShown':True,'retiming':'initial freeze, then 1x and 1.54x; timeline.json records each output frame','newUnityCapture':True,'sourceLanguage':'tr','creativeApproved':False}
    (OUT/'QA_REPORT.json').write_text(json.dumps(qa,indent=2),encoding='utf-8')
    command([ffmpeg,'-hide_banner','-y','-i',str(path),'-vf','fps=1/2.8,scale=216:384,tile=5x2','-frames:v','1',str(OUT/'encoded_contact_sheet.jpg')],OUT/'contact_sheet.log')
    command([ffmpeg,'-hide_banner','-y','-ss','5.3','-i',str(path),'-frames:v','1','-update','1',str(OUT/'cover.jpg')],OUT/'cover.log')
    return qa

with (SOURCE/'events.csv').open(encoding='utf-8-sig',newline='') as f:EVENTS=[e for e in csv.DictReader(f) if e['event']=='fire']
def main():
    parser=argparse.ArgumentParser();parser.add_argument('--stills',action='store_true');a=parser.parse_args()
    OUT.mkdir(parents=True,exist_ok=True)
    for t in SAMPLE_TIMES:frame(t).save(OUT/f'still_{t:04.1f}.jpg',quality=94)
    if a.stills:print('Review stills rendered.');return
    ffmpeg=imageio_ffmpeg.get_ffmpeg_exe();make_audio(ffmpeg,EVENTS);path=render(ffmpeg);qa=verify(ffmpeg,path)
    (OUT/'timeline.json').write_text(json.dumps([{'outputFrame':i,'seconds':i/FPS,'sourceFrame':raw_index(i/FPS),'camera':camera(i/FPS),'closing':i/FPS>=23.5} for i in range(SECONDS*FPS)],indent=2),encoding='utf-8')
    shutil.copy2(SOURCE/'provenance.json',OUT/'source_provenance.json')
    shutil.copy2(ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1-OFL.txt',OUT/'FONT_LICENSE.txt')
    print(json.dumps(qa,indent=2))
if __name__=='__main__':main()
