"""Build store/ad films from genuine native captures, without modifying the game.

The landscape film preserves the whole portrait HUD inside a branded frame.
The vertical ad shows actual blocked shots, returns and an unforced loss.
"""
from __future__ import annotations
import argparse, csv, functools, hashlib, json, math, os, subprocess, sys, wave
from pathlib import Path
sys.path.insert(0, str(Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Media-Python'))
import imageio_ffmpeg
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageOps

ROOT=Path(__file__).resolve().parents[2]
MEDIA=ROOT/'ArrowSwarm_Marketing'
OUT=MEDIA/'06_Trailers_Ads_Tutorials/StoreLaunch_20261115'
FONT=ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf'
ICON=MEDIA/'02_Branding/Selected_20261115/ArrowSwarm_App_Icon_SELECTED.jpeg'
SPLASH=ROOT/'Assets/_Project/Art/Sprites/SharedUI/FarzetkiGamesSplash.png'
SPLASH_LOGO=ROOT/'Assets/_Project/Art/Sprites/SharedUI/FarzetkiGamesSplashLogo.png'
FEATURE=MEDIA/'03_GooglePlay/feature_graphic_final_EN_1024x500.png'
FPS,SR=30,48000
FF=imageio_ffmpeg.get_ffmpeg_exe()
OUT.mkdir(parents=True,exist_ok=True)

def sha(path): return hashlib.sha256(Path(path).read_bytes()).hexdigest()
def read(path): return json.loads(Path(path).read_text(encoding='utf-8-sig'))
@functools.lru_cache(None)
def font(size): return ImageFont.truetype(str(FONT),size)
def text(im,s,xy,size=54,fill='#f8fcff',anchor='la',stroke=2):
    ImageDraw.Draw(im).text(xy,s,font=font(size),fill=fill,anchor=anchor,stroke_width=stroke,stroke_fill='#10284e')
def run(args,name):
    r=subprocess.run(args,capture_output=True,text=True,encoding='utf-8',errors='replace')
    (OUT/(name+'.log')).write_text(r.stdout+r.stderr,encoding='utf-8')
    if r.returncode: raise RuntimeError(name+' failed; see log')
def gradient(w,h):
    a=np.zeros((h,w,3),dtype=np.uint8)
    for y in range(h):
        t=y/(h-1);a[y,:,0]=round(8+13*t);a[y,:,1]=round(17+27*t);a[y,:,2]=round(48+34*t)
    im=Image.fromarray(a);d=ImageDraw.Draw(im)
    for i in range(65):
        x=23+(i*397)%(w-46);y=28+(i*251)%(h-56);r=1+i%3
        c=['#4f9cba','#94648d','#b9a46d'][i%3];d.ellipse((x-r,y-r,x+r,y+r),fill=c)
    return im
def round_icon(size):
    im=Image.open(ICON).convert('RGB').resize((size,size),Image.Resampling.LANCZOS)
    mask=Image.new('L',im.size);ImageDraw.Draw(mask).rounded_rectangle((0,0,size-1,size-1),radius=size//5,fill=255)
    return im,mask
def landscape_bg(mode,headline,lines):
    im=gradient(1920,1080);d=ImageDraw.Draw(im)
    icon,mask=round_icon(300);im.paste(icon,(165,130),mask)
    text(im,'ARROW', (315,510),81,anchor='mm');text(im,'SWARM',(315,598),81,fill='#ffdc58',anchor='mm')
    text(im,'THREE WAYS TO PLAY',(315,758),37,fill='#8cdfff',anchor='mm')
    for i,s in enumerate(('DEFENSE','SHAPES','COLOR SHIFT')):
        text(im,s,(315,824+53*i),31,fill='#ffdc58' if s==mode else '#a7b8d9',anchor='mm',stroke=0)
    d.rounded_rectangle((647,0,1273,1079),radius=20,fill='#032b4b',outline='#64ddff',width=4)
    text(im,mode,(1595,270),53,fill='#6be6ff',anchor='mm')
    for i,line in enumerate(headline):text(im,line,(1595,380+i*74),65,anchor='mm')
    for i,line in enumerate(lines):text(im,line,(1595,650+i*52),36,fill='#b4d8f4',anchor='mm',stroke=0)
    p=OUT/(mode.lower().replace(' ','_')+'_frame.png');im.save(p);return p
def splash_frame():
    im=gradient(1920,1080)
    source=Image.open(SPLASH).convert('RGB')
    source=ImageOps.contain(source,(608,1080),Image.Resampling.LANCZOS)
    im.paste(source,((1920-source.width)//2,(1080-source.height)//2))
    logo=ImageOps.contain(Image.open(SPLASH_LOGO).convert('RGBA'),(540,800),Image.Resampling.LANCZOS)
    im.paste(logo,((1920-logo.width)//2,(1080-logo.height)//2),logo)
    return save(im,'splash_landscape.png')
def feature_frame():
    im=gradient(1920,1080)
    feature=ImageOps.contain(Image.open(FEATURE).convert('RGB'),(1840,898),Image.Resampling.LANCZOS)
    im.paste(feature,((1920-feature.width)//2,35))
    text(im,'THREE MODES. ONE SWARM.',(960,1005),66,anchor='mm')
    return save(im,'trailer_outro.png')
def save(im,name):
    p=OUT/name;im.save(p);return p
def still_segment(image,seconds,name,w=1920,h=1080):
    dest=OUT/(name+'.mp4')
    run([FF,'-y','-loop','1','-i',str(image),'-t',str(seconds),'-vf',f'scale={w}:{h},fps=30,format=yuv420p',
         '-an','-c:v','libx264','-preset','fast','-crf','18','-threads','2','-movflags','+faststart',str(dest)],name)
    return dest
def native_segment(source,start,seconds,bg,name):
    dest=OUT/(name+'.mp4')
    run([FF,'-y','-loop','1','-i',str(bg),'-framerate','30','-start_number',str(round(start*30)),
         '-i',str(source/'frame_%05d.jpg'),'-filter_complex','[1:v]scale=608:1080,setsar=1[p];[0:v][p]overlay=656:0:shortest=1,format=yuv420p[v]',
         '-map','[v]','-frames:v',str(round(seconds*30)),'-r','30','-an','-c:v','libx264','-preset','fast','-crf','18','-threads','2',str(dest)],name)
    return dest
def pcm(path):
    r=subprocess.run([FF,'-v','error','-i',str(path),'-ar',str(SR),'-ac','2','-f','f32le','pipe:1'],capture_output=True,check=True)
    return np.frombuffer(r.stdout,dtype='<f4').reshape(-1,2).astype(float)
def mix(seconds,segments,name,fail=False):
    count=round(seconds*SR);music=pcm(ROOT/'Assets/_Project/Audio/Music/GameBGM_alt1.ogg')[:count].copy()
    music*=.055/max(.001,float(np.sqrt(np.mean(music*music))));out=music
    def cue(path,start,gain):
        a=round(start*SR)
        if a<0 or a>=count:return
        data=pcm(path);data*=gain/max(.001,float(abs(data).max()));n=min(len(data),count-a);out[a:a+n]+=data[:n]
    for seg in segments:
        events=seg['source']/'events.csv'
        if not events.exists():continue
        with events.open(encoding='utf-8-sig',newline='') as f:
            for row in csv.DictReader(f):
                event=row['event'];when=(float(row['time_seconds'])-seg['sourceStart'])/seg.get('speed',1)+seg['outputStart']
                if not seg['outputStart']<=when<seg['outputStart']+seg['seconds']:continue
                clip=None;gain=.08
                if event in ('fire','rainbow_fire','wrong_fire'):clip='ArrowFire.wav'
                elif event=='collision':clip='ArrowWrong.wav';gain=.09
                elif event=='won':clip='LevelWin.ogg';gain=.14
                elif event=='lost':clip='LevelLose.ogg';gain=.13
                elif event=='rotate':clip='ButtonClick_alt2.wav';gain=.065
                if clip:cue(ROOT/'Assets/_Project/Audio/SFX'/clip,when,gain)
    fade=np.minimum(np.arange(count)/(SR*.12),1)*np.minimum((count-1-np.arange(count))/(SR*.4),1)
    out*=fade[:,None];peak=float(abs(out).max())
    if peak>.84:out*=.84/peak
    p=OUT/(name+'.wav')
    with wave.open(str(p),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(SR);f.writeframes((out*32767).astype('<i2').tobytes())
    return p
def concatenate(parts,audio,dest,seconds):
    listing=OUT/(dest.stem+'_concat.txt');listing.write_text('\n'.join("file '"+p.name+"'" for p in parts)+'\n',encoding='utf-8')
    run([FF,'-y','-f','concat','-safe','0','-i',str(listing),'-i',str(audio),'-map','0:v:0','-map','1:a:0','-c:v','copy','-c:a','aac','-b:a','160k','-t',str(seconds),'-movflags','+faststart',str(dest)],dest.stem+'_encode')
def srt(rows,path):
    def stamp(t):
        ms=round(t*1000);return f'{ms//3600000:02}:{ms//60000%60:02}:{ms//1000%60:02},{ms%1000:03}'
    path.write_text('\n\n'.join(f'{i}\n{stamp(a)} --> {stamp(b)}\n{s}' for i,(a,b,s) in enumerate(rows,1))+'\n',encoding='utf-8')
def verify(path,frames,width,height):
    info=imageio_ffmpeg.read_frames(str(path));metadata=next(info);count=sum(1 for _ in info)
    run([FF,'-v','error','-i',str(path),'-f','null','-'],path.stem+'_decode')
    assert count==frames,(path,count,frames)
    assert tuple(metadata['size'])==(width,height)
    return {'passed':True,'file':path.name,'frames':count,'fps':metadata['fps'],'dimensions':metadata['size'],'durationSeconds':metadata['duration'],'bytes':path.stat().st_size,'sha256':sha(path),'fullDecode':True}
def contact(path,seconds,name):
    run([FF,'-y','-i',str(path),'-vf',f'fps=1/{seconds/8},scale=480:-1,tile=4x2','-frames:v','1','-q:v','2',str(OUT/name)],name+'_contact')
def trailer():
    shapes=MEDIA/'04_Raw_Gameplay/Shapes_L2_20261113_081526_913'
    color=MEDIA/'04_Raw_Gameplay/ColorShift_L3_20261115_184112_468'
    defense=MEDIA/'04_Raw_Gameplay/DefenseSeries_L3_20261115_120226_359'
    timeline=[{'mode':'Shapes','source':shapes,'sourceStart':2.0,'outputStart':1.2,'seconds':4.0},
              {'mode':'ColorShift','source':color,'sourceStart':4.9,'outputStart':5.2,'seconds':7.0},
              {'mode':'Defense','source':defense,'sourceStart':11.2,'outputStart':12.2,'seconds':9.5}]
    parts=[still_segment(splash_frame(),1.2,'trailer_00_splash')]
    looks=[('SHAPES',('CLEAR THE','PICTURE'),('Find the way out.','One arrow at a time.')),
           ('COLOR SHIFT',('ROTATE.','THINK. TAP.'),('A new perspective.','A clear path.')),
           ('DEFENSE',('TIME YOUR','SHOTS'),('Enemies are moving.','Make every arrow count.'))]
    for i,(seg,look) in enumerate(zip(timeline,looks)):
        parts.append(native_segment(seg['source'],seg['sourceStart'],seg['seconds'],landscape_bg(*look),'trailer_0'+str(i+1)))
    parts.append(still_segment(feature_frame(),3.3,'trailer_04_brand'))
    dest=OUT/'ArrowSwarm_StoreTrailer_EN_25s.mp4';audio=mix(25,timeline,'trailer_mix');concatenate(parts,audio,dest,25)
    rows=[(1.2,5.2,'SHAPES — Clear the picture.'),(5.2,12.2,'COLOR SHIFT — Rotate. Think. Tap.'),(12.2,21.7,'DEFENSE — Time your shots.'),(21.7,25,'THREE MODES. ONE SWARM.')]
    srt(rows,OUT/'trailer_captions_EN.srt');contact(dest,25,'trailer_contact_sheet.jpg')
    report=verify(dest,750,1920,1080)
    manifest={'file':dest.name,'language':'en','editorialLanguage':'en','nativeHUDsPreserved':True,'sourceAudioRecorded':False,'audio':'Editorial mix of existing GameBGM_alt1 and event-aligned project SFX; no voiceover','selectedIcon':str(ICON.relative_to(ROOT)),'selectedIconSha256':sha(ICON),'splash':str(SPLASH.relative_to(ROOT)),'splashSha256':sha(SPLASH),'segments':[{**{k:v for k,v in s.items() if k!='source'},'source':str(s['source'].relative_to(ROOT)),'provenanceSha256':sha(s['source']/'provenance.json'),'eventsSha256':sha(s['source']/'events.csv')} for s in timeline],'qa':report,'claimLimit':'Actual captured gameplay. Defense archival HUD is Turkish; exterior editorial titles are English. No installed-store or live-leaderboard claim.'}
    manifest.update(splashLogo=str(SPLASH_LOGO.relative_to(ROOT)),splashLogoSha256=sha(SPLASH_LOGO))
    (OUT/'trailer_manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    return report
def fail_ad(source):
    proof=read(source/'provenance.json')
    assert proof['mode']=='Shapes' and proof['frames']==450 and proof['wrongShots']==proof['returnedShots']==3 and proof['lost'] and not proof['forcedOutcome']
    timeline=[{'source':source,'sourceStart':.2,'outputStart':0,'seconds':1.8,'speed':1,'caption':'ONE WRONG MOVE?'},
              {'source':source,'sourceStart':2,'outputStart':1.8,'seconds':2,'speed':.2,'caption':'BLOCKED.'},
              {'source':source,'sourceStart':2.4,'outputStart':3.8,'seconds':3.1,'speed':1,'caption':'LOOK BEFORE YOU TAP.'},
              {'source':source,'sourceStart':5.5,'outputStart':6.9,'seconds':3.1,'speed':1,'caption':'TRY A CLEAR PATH.'}]
    parts=[]
    for i,seg in enumerate(timeline):
        layer=Image.new('RGBA',(1080,1920));d=ImageDraw.Draw(layer)
        d.rounded_rectangle((105,255,975,380),radius=38,fill=(9,27,58,232),outline=(91,217,255,255),width=3)
        text(layer,seg['caption'],(540,315),58,anchor='mm');overlay=save(layer,f'fail_overlay_{i}.png')
        native=OUT/f'fail_native_{i}.mp4'
        caption_until=max(0,min(seg['seconds'],5.7-seg['outputStart']))
        run([FF,'-y','-framerate','30','-start_number',str(round(seg['sourceStart']*30)),'-i',str(source/'frame_%05d.jpg'),'-loop','1','-i',str(overlay),
             '-filter_complex',f"[0:v]setpts={(1/seg['speed']):.6f}*(PTS-STARTPTS),fps=30[p];[p][1:v]overlay=0:0:shortest=1:enable='lt(t,{caption_until:.3f})',format=yuv420p[v]",
             '-map','[v]','-frames:v',str(round(seg['seconds']*30)),'-an','-c:v','libx264','-preset','fast','-crf','19','-threads','2',str(native)],f'fail_native_{i}')
        parts.append(native)
    end=gradient(1080,1920);icon,mask=round_icon(540);end.paste(icon,(270,260),mask)
    text(end,'ARROW',(540,965),130,anchor='mm');text(end,'SWARM',(540,1102),130,fill='#ffdc58',anchor='mm')
    text(end,'THINK BEFORE YOU TAP.',(540,1350),58,anchor='mm')
    text(end,'THREE MODES.',(540,1480),48,fill='#85e5ff',anchor='mm');text(end,'ONE CLEVER MOVE.',(540,1550),48,fill='#85e5ff',anchor='mm')
    endpath=save(end,'fail_outro.png');parts.append(still_segment(endpath,5,'fail_brand',1080,1920))
    audio=mix(15,timeline,'fail_mix',True);dest=OUT/'ArrowSwarm_FailAd_EN_15s.mp4';concatenate(parts,audio,dest,15)
    srt([(0,1.8,'ONE WRONG MOVE?'),(1.8,3.8,'BLOCKED.'),(3.8,6.9,'LOOK BEFORE YOU TAP.'),(6.9,10,'TRY A CLEAR PATH.'),(10,15,'ARROW SWARM — Think before you tap.')],OUT/'fail_captions_EN.srt')
    contact(dest,15,'fail_contact_sheet.jpg');report=verify(dest,450,1080,1920)
    manifest={'file':dest.name,'language':'en','source':str(source.relative_to(ROOT)),'provenanceSha256':sha(source/'provenance.json'),'eventsSha256':sha(source/'events.csv'),'nativeHUDsPreserved':True,'editorialCaptionHiddenAfterSeconds':5.7,'sourceAudioRecorded':False,'audio':'Editorial project music and event-aligned fire/collision/loss cues','segments':[{k:(str(v.relative_to(ROOT)) if k=='source' else v) for k,v in s.items()} for s in timeline],'slowMotion':'First genuine blocked shot plays at 0.2x; all three native collisions/returns retained in original order, then unforced native loss.','qa':report,'claimLimit':'Actual native blocked-shot/collision/return/loss capture; no simulated touch or unverified gameplay.'}
    (OUT/'fail_manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8');return report
def main():
    p=argparse.ArgumentParser();p.add_argument('--trailer',action='store_true');p.add_argument('--fail-source',type=Path);a=p.parse_args();reports=[]
    if a.trailer:reports.append(trailer())
    if a.fail_source:reports.append(fail_ad(a.fail_source.resolve()))
    old=read(OUT/'VIDEO_QA.json') if (OUT/'VIDEO_QA.json').exists() else {'items':[]}
    byname={x['file']:x for x in old['items']};byname.update({x['file']:x for x in reports})
    (OUT/'VIDEO_QA.json').write_text(json.dumps({'passed':all(x['passed'] for x in byname.values()),'items':list(byname.values()),'method':'FFmpeg complete frame decode and separate all-stream null decode, native provenance retained'},indent=2)+'\n',encoding='utf-8')
    manifests=[]
    for name in ('trailer_manifest.json','fail_manifest.json'):
        path=OUT/name
        if path.exists():
            item=read(path)
            if name=='trailer_manifest.json':
                item.update(splashLogo=str(SPLASH_LOGO.relative_to(ROOT)),splashLogoSha256=sha(SPLASH_LOGO))
                path.write_text(json.dumps(item,indent=2)+'\n',encoding='utf-8')
            manifests.append(item)
    (OUT/'manifest.json').write_text(json.dumps({'project':'Arrow Swarm','date':'2026-11-15','selectedIcon':str(ICON.relative_to(ROOT)),'selectedIconSha256':sha(ICON),'features':['../../03_GooglePlay/feature_graphic_final_EN_1024x500.png','../../03_GooglePlay/feature_graphic_final_TR_1024x500.png'],'videos':manifests,'uploaded':False,'storeURL':None,'audioNotRecordedNatively':True},indent=2)+'\n',encoding='utf-8')
    print(json.dumps(reports,indent=2))
if __name__=='__main__':main()
