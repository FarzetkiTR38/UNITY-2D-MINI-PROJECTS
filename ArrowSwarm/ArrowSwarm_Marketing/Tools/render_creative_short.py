"""Sixteen-second editorial alternative from authentic Unity Shapes21 frames.

Both native HUDs are guarded before the promo. No gameplay, crops, replacement
HUDs or false solution are generated. Only graphics, timing and sound are edited.
"""
from __future__ import annotations
import argparse,csv,functools,json,math,re,shutil,subprocess,wave,tempfile
from pathlib import Path
import render_reference_reel as m
import render_promo_outro as promo
from PIL import Image,ImageDraw,ImageChops
import numpy as np

ROOT=m.ROOT;MEDIA=m.MEDIA;OUT=MEDIA/'05_Reels/CreativeShort01_20261114'
SOURCE=MEDIA/'04_Raw_Gameplay/Shapes_L21_20261114_181042_624'
PREVIOUS=MEDIA/'05_Reels/ShapesSeries_20261114/L21/ArrowSwarm_Shapes21_TR.mp4'
NAME='ArrowSwarm_Ahtapot_CreativeShort_TR.mp4'
W,H,FPS,SECONDS,SR=1080,1920,30,16,48000
FIRE,REST,FAST_END,OUTRO=3.8,5.0,7.6,11.6
FIRST,SECOND,FAST_LAST,LAST=75,225,803,899
RATE=(FAST_LAST-SECOND)/(FAST_END-REST)/30
VOICE_STARTS=[.1,3.5,9.3,12.4]
HUD=((0,0,W,230),(0,1690,W,H))
GEOM=json.loads((SOURCE/'geometry.json').read_text(encoding='utf-8'))
PROV=json.loads((SOURCE/'provenance.json').read_text(encoding='utf-8'))
with (SOURCE/'events.csv').open(encoding='utf-8-sig',newline='') as f:ALL=list(csv.DictReader(f))
EVENTS=[r for r in ALL if r['event']=='fire']

def write(path,data):path.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
def point(p):
    z,x,y=GEOM['gridZero'],GEOM['gridX'],GEOM['gridY']
    return (z['x']+(x['x']-z['x'])*p['x']+(y['x']-z['x'])*p['y'],z['y']+(x['y']-z['y'])*p['x']+(y['y']-z['y'])*p['y'])
def raw_index(t):
    if t<FIRE-10/30:return FIRST-15
    if t<REST:return round(FIRST+(t-FIRE)*30)
    if t<FAST_END:return round(SECOND+(t-REST)*30*RATE)
    return min(LAST,round(FAST_LAST+(t-FAST_END)*30))
def event_time(f):
    if f<SECOND:return FIRE+(f-FIRST)/30
    if f<FAST_LAST:return REST+(f-SECOND)/(30*RATE)
    return FAST_END+(f-FAST_LAST)/30
@functools.lru_cache(maxsize=15)
def native(i):return Image.open(SOURCE/f'frame_{i:05d}.jpg').convert('RGB')
def label(im,s,y,size=60,color=m.WHITE,stroke=4,cx=540):return m.label(im,s,y,size,color,stroke,cx)

def choices(im,t):
    if not .6<=t<3.76:return
    layer=Image.new('RGBA',(W,H));d=ImageDraw.Draw(layer)
    for order,(letter,index,cx) in enumerate([('A',5,860),('B',1,610)]):
        alpha=m.ease((t-(.6+order*.1))/.18)*(1-m.ease((t-3.58)/.15))
        if alpha<=0:continue
        pts=[point(p) for p in GEOM['arrows'][index]['points']]
        color=(83,238,180) if letter=='A' and t>=3.5 else m.GOLD
        d.line(pts,fill=(*color,round(90*alpha)),width=22,joint='curve')
        hx,hy=pts[0];cy=1510;r=round(37*alpha)
        d.line(((hx,hy),(cx,cy-r)),fill=(*m.INK,round(215*alpha)),width=3)
        d.ellipse((cx-r+2,cy-r+5,cx+r+2,cy+r+5),fill=(*m.INK,round(90*alpha)))
        d.ellipse((cx-r,cy-r,cx+r,cy+r),fill=(*color,round(255*alpha)),outline=(*m.INK,round(255*alpha)),width=4)
        f=m.font(44);b=d.textbbox((0,0),letter,font=f)
        d.text((cx-d.textlength(letter,font=f)/2,cy-(b[3]-b[1])/2-b[1]),letter,font=f,fill=(*m.INK,round(255*alpha)))
    im.alpha_composite(layer)

def caption(im,t):
    d=ImageDraw.Draw(im)
    if t<1.35:
        pop=.85+.15*m.ease(t/.22)
        label(im,'44 OK.',262+round((1-pop)*25),round(94*pop),m.GOLD,6)
        label(im,'İLK HAMLEN?',376,51,m.WHITE,4)
        width=round(325*m.ease(t/.3));d.line((540-width/2,448,540+width/2,448),fill=m.GOLD,width=5)
    elif t<3.5:
        label(im,'HANGİSİ ÖNCE ÇIKAR?',267,52,m.WHITE,4)
        label(im,'A MI, B Mİ?',337,47,m.GOLD,3)
        if t>=1.5:
            cx,cy,r=540,418,28
            d.ellipse((cx-r,cy-r,cx+r,cy+r),fill=m.INK,outline=m.GOLD,width=3)
            m.label(im,str(2-int(t-1.5)),cy-20,37,m.WHITE,1,cx)
    elif t<4.65:
        label(im,'ÖNCE A!',294,84,m.GOLD,6)
        label(im,'ÖNÜ AÇIK.',401,34,(89,235,180),2)
    elif t<REST:label(im,'İLK OK ÇIKTI.',312,55,m.GOLD,4)
    elif t<FAST_END:
        label(im,'HIZLI SARALIM.',274,43,m.WHITE,3)
        d.rounded_rectangle((420,350,660,421),radius=35,fill=m.INK)
        label(im,'7,4×',362,48,m.GOLD,0)
        # Small editorial acceleration marks, confined to the empty header gap.
        for side in [-1,1]:
            for j in range(3):
                x=540+side*(150+j*24);d.line((x,374,x+side*12,386,x,398),fill=(124,195,234),width=3)
    elif t<event_time(835):label(im,'SON OK…',303,56,m.GOLD,4)
    elif 9.3<=t<10.71:label(im,'AHTAPOT ÇÖZÜLDÜ!',1644,32,m.GOLD,2)
    elif 10.71<=t<OUTRO:label(im,'44 OK. 3 YILDIZ.',1644,32,m.GOLD,2)

def tap(im,t):
    if not 3.72<=t<4.10:return
    hx,hy=point(GEOM['arrows'][5]['points'][0]);u=(t-3.72)/.38
    layer=Image.new('RGBA',(W,H));d=ImageDraw.Draw(layer);r=12+48*u
    d.ellipse((hx-r,hy-r,hx+r,hy+r),outline=(255,224,75,round(240*(1-u))),width=5)
    im.alpha_composite(layer)

def frame(t):
    if t>=OUTRO+.2:return promo.frame(t-OUTRO)
    source=native(raw_index(t));im=source.convert('RGBA')
    choices(im,t);caption(im,t);tap(im,t);rgb=im.convert('RGB')
    if t<OUTRO:
        for band in HUD:assert ImageChops.difference(rgb.crop(band),source.crop(band)).getbbox() is None,f'HUD altered at{t}'
    else:rgb=Image.blend(rgb,promo.frame(t-OUTRO),m.ease((t-OUTRO)/.2))
    return rgb

def pcm(ff,path,seconds=None,start=0):
    args=[ff,'-v','error','-i',str(path)]
    if start:args+=['-ss',str(start)]
    if seconds:args+=['-t',str(seconds)]
    r=subprocess.run(args+['-f','f32le','-ar',str(SR),'-ac','2','pipe:1'],capture_output=True,check=True)
    return np.frombuffer(r.stdout,dtype='<f4').reshape(-1,2).astype(float)

def audio(ff):
    n=SECONDS*SR;music_path=ROOT/'Assets/_Project/Audio/Music/GameBGM_alt2.ogg'
    music=pcm(ff,music_path,SECONDS,8);assert len(music)>=n
    music=music[:n];music*=.045/max(.001,float(np.sqrt(np.mean(music**2))))
    mix=music.copy();cues=[]
    def add(t,data,gain=1):
        a=round(t*SR);b=min(n,a+len(data))
        if a<0 or b<=a:return
        mix[a:b]+=data[:b-a]*gain
    def effect(t,name,rms=.05):
        path=ROOT/'Assets/_Project/Audio/SFX'/name;data=pcm(ff,path)
        data*=rms/max(.001,float(np.sqrt(np.mean(data**2))))
        data*=min(1,.22/max(.001,float(abs(data).max())))
        add(t,data);cues.append({'seconds':t,'asset':str(path.relative_to(ROOT)),'sha256':m.sha(path)})
    effect(.05,'ButtonClick_alt2.wav',.035)
    for t in [1.5,2.5]:effect(t,'ButtonClick_alt2.wav',.025)
    effect(3.5,'StarEarn.wav',.06);effect(FIRE,'ArrowFire.wav',.045)
    for i,e in enumerate(EVENTS[1:-1]):
        if i%4==0:effect(event_time(int(e['frame'])),'ArrowFire.wav',.028)
    effect(event_time(int(EVENTS[-1]['frame'])),'ArrowFire.wav',.045)
    effect(event_time(835)+.12,'LevelWin.ogg',.045)
    effect(10.25,'StarEarn.wav',.035);effect(OUTRO+3.15,'ButtonClick_alt2.wav',.05)
    # Short broadband lift emphasises the speed change without drowning the game cues.
    rng=np.random.default_rng(2116);count=round(.32*SR);tt=np.arange(count)/SR
    whoosh=rng.normal(0,1,(count,2));whoosh-=np.vstack((np.zeros((1,2)),whoosh[:-1]))*.82
    whoosh*=np.sin(np.pi*tt/.32)[:,None]**2*.012;add(4.84,whoosh)
    manifest=json.loads((OUT/'audio/voice_manifest.json').read_text(encoding='utf-8'));rows=[]
    for i,start in enumerate(VOICE_STARTS):
        path=OUT/f'audio/voice_{i:02d}.wav';data=pcm(ff,path)
        data*=.13/max(.001,float(np.sqrt(np.mean(data**2))));data*=min(1,.57/max(.001,float(abs(data).max())))
        a=round(start*SR);b=a+len(data);assert b<=n
        da=max(0,a-round(.08*SR));db=min(n,b+round(.12*SR));env=np.ones(db-da)*.22
        if a>da:env[:a-da]=np.linspace(1,.22,a-da)
        if db>b:env[-(db-b):]=np.linspace(.22,1,db-b)
        mix[da:db]*=env[:,None];add(start,data)
        rows.append({'id':i,'text':manifest['clips'][i]['text'],'start':start,'end':b/SR,'sha256':m.sha(path)})
    fade=np.minimum(np.arange(n)/(SR*.04),1)*np.minimum((n-1-np.arange(n))/(SR*.18),1)
    mix*=fade[:,None];peak=float(abs(mix).max());mix*=min(1,.88/max(.001,peak))
    with wave.open(str(OUT/'editorial_mix.wav'),'wb') as f:
        f.setnchannels(2);f.setsampwidth(2);f.setframerate(SR);f.writeframes(np.rint(mix*32767).astype('<i2').tobytes())
    write(OUT/'audio_timeline.json',{'music':{'asset':str(music_path.relative_to(ROOT)),'sha256':m.sha(music_path),'excerptStartSeconds':8,'continuousUnderPromo':True},'voice':rows,'cues':cues,'nativeAudioListenerCaptured':False,'musicDuckedDuringSpeech':True})

def preflight():
    assert PROV['arrows']==44 and PROV['won'] and PROV['remaining']==0 and PROV['frames']==900
    assert len(EVENTS)==44 and [int(e['arrow']) for e in EVENTS]==GEOM['witness']
    assert GEOM['arrows'][5]['initiallyRemovable'] and not GEOM['arrows'][1]['initiallyRemovable']
    assert all(e['lives']=='3' for e in ALL)
    assert m.sha(OUT/'audio/voice_03.wav')==m.sha(MEDIA/'05_Reels/SharedOutro01/cta_voice.wav')
    assert m.sha(ROOT/GEOM['levelAsset'])==json.loads((SOURCE/'native_capture_checks.json').read_text())['sourceAssetHash']
    assert raw_index(FIRE)==FIRST and raw_index(REST)==SECOND and raw_index(FAST_END)==FAST_LAST
    assert event_time(96)<REST and event_time(835)<10.8 and raw_index(10.8)==LAST

def preservation_checks():
    base=json.loads((Path(tempfile.gettempdir())/'ArrowSwarm-CreativeShort-DirtyBaseline.json').read_text(encoding='utf-8-sig'))
    assert len(base)==10 and all(m.sha(Path(r['path'])).upper()==r['sha256'].upper() for r in base)
    prior=json.loads((MEDIA/'05_Reels/SharedOutro01/P_PRONUNCIATION_QA.json').read_text(encoding='utf-8'))
    assert all(m.sha(MEDIA/r['file'])==r['sha256'] for r in prior['targets'])
    assert m.sha(OUT/'before.mp4')==m.sha(PREVIOUS)
    write(OUT/'PRESERVATION_CHECKS.json',{'passed':True,'preexistingDirtyFilesUnchanged':10,
        'previousVideoOutputsUnchanged':8,'beforeComparisonIsExactCurrentPreviousVideo':True,
        'beforeSha256':m.sha(PREVIOUS),'newSha256':m.sha(OUT/NAME),'nativeEditorChanges':'Read-only asset/rules query; no Play, saves or scenes changed.'})

def verify(ff,path):
    r=m.command([ff,'-hide_banner','-i',str(path),'-f','null','-','-progress','pipe:1'],OUT/'decode.log')
    count=int(re.findall(r'^frame=(\d+)$',r.stdout,re.M)[-1]);assert count==480
    assert all(s in r.stderr for s in ['1080x1920','yuv420p','48000 Hz, stereo','h264','aac'])
    r=m.command([ff,'-hide_banner','-i',str(path),'-vn','-af','loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json','-f','null','-'],OUT/'loudness.log')
    levels=json.loads(re.findall(r'\{\s*"input_i".*?\}',r.stderr,re.S)[-1]);assert -18<=float(levels['input_i'])<=-14 and float(levels['input_tp'])<-1
    timeline=[{'frame':i,'seconds':i/FPS,'sourceFrame':raw_index(i/FPS) if i/FPS<OUTRO+.2 else None,'nativeHudGuard':i/FPS<OUTRO,'promo':i/FPS>=OUTRO} for i in range(480)]
    write(OUT/'timeline.json',timeline)
    report={'passed':True,'durationSeconds':16,'frames':count,'width':W,'height':H,'fps':FPS,'bytes':path.stat().st_size,'sha256':m.sha(path),
        'previousDurationSeconds':41.3,'shorterPercent':round((1-16/41.3)*100,1),'firstFire':FIRE,'firstSettled':event_time(96),'remainingAt':REST,
        'speedSegment':{'start':REST,'end':FAST_END,'speed':RATE,'label':'7,4×'},'nativeWinAt':event_time(835),'promoAt':OUTRO,
        'source':SOURCE.name,'sourceActions':44,'nativeLives':3,'nativeResult':'44/44, three stars','hudBandsIdenticalBeforeEncode':HUD,
        'nativeGameplayZoomCropOrReplacementHud':False,'ctaVoiceReusedByteIdentical':True,'storeStatus':'mock','publicStoreUrl':None,
        'audio':levels,'gameplayCapturedThisTask':False,'nativeEvidenceReused':True,'creativeApproval':'new alternative awaiting user review'}
    write(OUT/'QA_REPORT.json',report)
    m.command([ff,'-hide_banner','-y','-i',str(path),'-vf','fps=1/1.6,scale=216:384,tile=5x2','-frames:v','1','-update','1',str(OUT/'encoded_contact_sheet.jpg')],OUT/'contact_sheet.log')
    m.command([ff,'-hide_banner','-y','-ss','2.0','-i',str(path),'-frames:v','1','-update','1',str(OUT/'cover.jpg')],OUT/'cover.log')
    return report

def main():
    p=argparse.ArgumentParser();p.add_argument('--stills',action='store_true');p.add_argument('--checks-only',action='store_true');a=p.parse_args();OUT.mkdir(parents=True,exist_ok=True);preflight()
    if a.checks_only:preservation_checks();print('Previous eight outputs and ten pre-existing dirty files unchanged.');return
    for t in [.1,.5,1.8,2.6,3.55,4.0,4.7,5.6,7.7,9.5,10.9,12.4,14.9]:frame(t).save(OUT/f'still_{t:04.2f}.jpg',quality=94)
    if a.stills:print('Creative short stills passed native HUD guards.');return
    ff=m.imageio_ffmpeg.get_ffmpeg_exe();audio(ff);m.OUT=OUT;m.SECONDS=SECONDS;m.frame=frame
    video=m.render(ff,NAME,'16s creative alternative. Genuine Unity Shapes21 footage, both native HUDs. Two-choice quiz, real first shot and idle hold, marked7.4x full middle solution, normal final flight/win, shortened native-art promo. Editorial soundtrack/tap annotations; store mock.')
    qa=verify(ff,video);shutil.copy2(SOURCE/'provenance.json',OUT/'source_provenance.json');shutil.copy2(ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1-OFL.txt',OUT/'FONT_LICENSE.txt')
    manifest=json.loads((OUT/'audio/voice_manifest.json').read_text(encoding='utf-8'))
    def tc(t):
        ms=round(t*1000);return f'00:00:{ms//1000:02d},{ms%1000:03d}'
    lines=[]
    for i,row in enumerate(manifest['clips']):lines.append(f'{i+1}\n{tc(VOICE_STARTS[i])} --> {tc(VOICE_STARTS[i]+row["durationSeconds"])}\n{row["text"]}')
    (OUT/'captions_TR.srt').write_text('\n\n'.join(lines)+'\n',encoding='utf-8')
    print(json.dumps(qa,ensure_ascii=True,indent=2),flush=True)
if __name__=='__main__':main()
