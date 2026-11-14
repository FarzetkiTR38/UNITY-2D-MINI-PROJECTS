"""Reusable native-art promo ending; replace only the R2 tail after33.7s."""
import argparse,functools,json,math,re,subprocess,wave,shutil
from pathlib import Path
import render_reference_reel as m
import numpy as np
from PIL import Image,ImageDraw

ROOT=m.ROOT;MEDIA=m.MEDIA;FPS=30;SR=48000
SHARED=MEDIA/'05_Reels/SharedOutro01'
OUT=MEDIA/'05_Reels/ReferencePilot02_R3_20261114'
PREVIOUS=MEDIA/'05_Reels/ReferencePilot02_R2_20261114'
INPUT=PREVIOUS/'ArrowSwarm_Shapes2_R2_FullScene_TR.mp4'
NAME='ArrowSwarm_Shapes2_R3_Promo_TR.mp4'
CONFIG=json.loads((SHARED/'store_config.json').read_text(encoding='utf-8'))
DURATION=6.3;START=33.7;END=40

@functools.lru_cache(maxsize=1)
def artwork():
    atlas=Image.open(ROOT/'Assets/_Project/Art/Sprites/SharedUI/shared_ui_kit.png').convert('RGBA')
    def crop(x,y,w,h):return atlas.crop((x,atlas.height-y-h,x+w,atlas.height-y))
    return Image.open(m.LOGO).convert('RGBA'),crop(33,1333,464,168),crop(653,526,218,247)

def text(im,s,y,size,color=m.WHITE,stroke=4):return m.label(im,s,y,size,color,stroke,cx=540)
@functools.lru_cache(maxsize=1)
def base():
    im=m.closing_background().copy();d=ImageDraw.Draw(im)
    for i in range(16):
        x=90+(i*211)%890;y=200+(i*233)%1460;r=2+i%3
        color=(239,106,191,95) if i%2 else (255,216,92,105)
        d.ellipse((x-r,y-r,x+r,y+r),fill=color)
    return im

def frame(t):
    im=base().copy();logo,plate,play=artwork()
    size=round(820*(.965+.035*m.ease(t/.65)))
    art=logo.resize((size,size),Image.Resampling.LANCZOS)
    im.alpha_composite(art,(round((1080-size)/2),300))
    text(im,CONFIG['headline'],1165,55)
    text(im,CONFIG['download_line'],1245,57,m.GOLD)
    # Shared Unity atlas blank primary plate and play icon, no duplicate raster art.
    press=math.sin(math.pi*(t-3.15)/.28) if 3.15<t<3.43 else 0
    scale=1-.045*press;w,h=round(730*scale),round(206*scale)
    button=Image.new('RGBA',(730,206));button.alpha_composite(plate.resize((730,206),Image.Resampling.LANCZOS))
    icon=play.resize((65,74),Image.Resampling.LANCZOS);button.alpha_composite(icon,(123,65))
    d=ImageDraw.Draw(button);f=m.font(65);s=CONFIG['button_text'];b=d.textbbox((0,0),s,font=f)
    d.text((224,102-(b[3]-b[1])/2-b[1]),s,font=f,fill=m.WHITE,stroke_width=3,stroke_fill=(0,65,157))
    button=button.resize((w,h),Image.Resampling.LANCZOS)
    im.alpha_composite(button,(round((1080-w)/2),round(1395+(206-h)/2)))
    text(im,CONFIG['display_store'],1640,32,(169,221,255),0)
    text(im,'Farzetki Games',1740,27,(143,180,224),0)
    if t>2.1:
        d=ImageDraw.Draw(im);p=m.ease((t-2.1)/.85)
        x=m.lerp(900,806,p);y=m.lerp(1710,1512,p)
        if 3.15<t<3.8:
            rr=20+(t-3.15)*100;alpha=round(170*max(0,1-(t-3.15)/.65))
            glow=Image.new('RGBA',im.size);gd=ImageDraw.Draw(glow)
            gd.ellipse((x-rr,y-rr,x+rr,y+rr),outline=(88,232,255,alpha),width=5);im.alpha_composite(glow);d=ImageDraw.Draw(im)
        # Visible editorial pointer performs a brief press; no installation is simulated.
        pts=[(x,y),(x+6,y+60),(x+23,y+43),(x+39,y+74),(x+53,y+66),(x+36,y+35),(x+61,y+33)]
        d.polygon([(a+3,b+5) for a,b in pts],fill=(0,12,43,140))
        d.polygon(pts,fill=(251,253,255),outline=(28,73,127),width=3)
    return im.convert('RGB')

def audio(ff):
    n=round(SR*DURATION);out=np.zeros((n,2))
    def tone(start,dur,freq,gain):
        a=round(start*SR);count=min(round(dur*SR),n-a)
        if count<=0:return
        tt=np.arange(count)/SR;env=np.minimum(tt/.015,1)*np.exp(-tt*4)
        sig=np.sin(2*np.pi*freq*tt)*env*gain;out[a:a+count]+=sig[:,None]
    for i in range(12):
        for note in [50,54,57]:tone(i*60/112,1.3,440*2**((note-69)/12),.012)
        if i%2==0:tone(i*60/112,.45,440*2**(([74,78,81,78,76,74][i//2]-69)/12),.022)
    tone(3.15,.12,1080,.075);tone(3.19,.13,1540,.03)
    raw=subprocess.check_output([ff,'-v','error','-i',str(SHARED/'cta_voice.wav'),'-f','f32le','-ar',str(SR),'-ac','1','pipe:1'])
    voice=np.frombuffer(raw,dtype='<f4').astype(float);voice*=.14/max(.001,float(np.sqrt(np.mean(voice**2))));voice*=min(1,.60/max(.001,float(abs(voice).max())))
    start=round(.8*SR);out[start:start+len(voice)]+=voice[:,None]*.7071
    fade=np.minimum(np.arange(n)/(SR*.10),1)*np.minimum((n-1-np.arange(n))/(SR*.3),1);out*=fade[:,None]
    with wave.open(str(SHARED/'outro_mix.wav'),'wb') as f:
        f.setnchannels(2);f.setsampwidth(2);f.setframerate(SR);f.writeframes((out*32767).astype('<i2').tobytes())

def render_outro(ff):
    args=[ff,'-hide_banner','-y','-f','rawvideo','-pixel_format','rgb24','-video_size','1080x1920','-framerate','30','-i','pipe:0','-i',str(SHARED/'outro_mix.wav'),'-vf','scale=in_range=pc:out_range=tv:out_color_matrix=bt709,format=yuv420p','-c:v','libx264','-preset','fast','-crf','18','-color_range','tv','-colorspace','bt709','-color_primaries','bt709','-color_trc','bt709','-c:a','aac','-b:a','192k','-ar',str(SR),'-t',str(DURATION),'-movflags','+faststart',str(SHARED/'ArrowSwarm_SharedOutro01_TR.mp4')]
    with (SHARED/'encode.log').open('w') as log:
        p=subprocess.Popen(args,stdin=subprocess.PIPE,stderr=log)
        for i in range(round(DURATION*FPS)):p.stdin.write(frame(i/FPS).tobytes())
        p.stdin.close();assert p.wait()==0

def main():
    p=argparse.ArgumentParser();p.add_argument('--stills',action='store_true');a=p.parse_args();OUT.mkdir(parents=True,exist_ok=True)
    frame(1.5).save(SHARED/'poster.jpg',quality=95);frame(3.3).save(SHARED/'press.jpg',quality=95)
    if a.stills:return
    ff=m.imageio_ffmpeg.get_ffmpeg_exe();audio(ff);render_outro(ff)
    filt='[0:v]trim=end=34.1,setpts=PTS-STARTPTS,fps=30,format=yuv420p[v0];[1:v]setpts=PTS-STARTPTS,fps=30,format=yuv420p[v1];[v0][v1]xfade=transition=fade:duration=0.4:offset=33.7[v];[0:a]atrim=end=33.7,asetpts=PTS-STARTPTS,afade=t=out:st=33.65:d=0.05[a0];[1:a]asetpts=PTS-STARTPTS[a1];[a0][a1]concat=n=2:v=0:a=1[a]'
    cmd=[ff,'-hide_banner','-y','-i',str(INPUT),'-i',str(SHARED/'ArrowSwarm_SharedOutro01_TR.mp4'),'-filter_complex',filt,'-map','[v]','-map','[a]','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p','-r','30','-c:a','aac','-b:a','192k','-ar',str(SR),'-t','40','-movflags','+faststart','-metadata','comment=R3: R2 edit preserved through33.7s; only ending replaced by reusable promotional mock-store outro. No public store destination exists yet.',str(OUT/NAME)]
    (OUT/'ffmpeg_command.json').write_text(json.dumps(cmd,indent=2),encoding='utf-8');m.command(cmd,OUT/'encode.log')
    r=m.command([ff,'-hide_banner','-i',str(OUT/NAME),'-f','null','-','-progress','pipe:1'],OUT/'decode.log')
    count=int(re.findall(r'^frame=(\d+)$',r.stdout,re.M)[-1]);assert count==1200,count
    r=m.command([ff,'-hide_banner','-i',str(OUT/NAME),'-vn','-af','loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json','-f','null','-'],OUT/'loudness.log')
    levels=json.loads(re.findall(r'\{\s*"input_i".*?\}',r.stderr,re.S)[-1]);assert float(levels['input_tp'])<-1
    qa={'passed':True,'frames':count,'durationSeconds':40,'width':1080,'height':1920,'unchangedEditUntil':33.7,'congratulationsVoiceEndsAt':33.52,'transition':[33.7,34.1],'promoNarration':[34.5,36.54],'clickAnimationAt':36.85,'storeStatus':'mock','publicStoreUrl':None,'audio':levels,'sha256':m.sha(OUT/NAME),'bytes':(OUT/NAME).stat().st_size,'sourceR2Sha256':m.sha(INPUT)}
    (OUT/'QA_REPORT.json').write_text(json.dumps(qa,indent=2),encoding='utf-8')
    m.command([ff,'-hide_banner','-y','-ss','35.4','-i',str(OUT/NAME),'-frames:v','1','-update','1',str(OUT/'cover.jpg')],OUT/'cover.log')
    m.command([ff,'-hide_banner','-y','-i',str(OUT/NAME),'-vf','fps=1/4,scale=216:384,tile=5x2','-frames:v','1',str(OUT/'encoded_contact_sheet.jpg')],OUT/'contact_sheet.log')
    print(json.dumps(qa,indent=2))
if __name__=='__main__':main()
