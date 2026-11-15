"""Native Defense revision: earlier charged Freeze, sequential arrows, isolated Rainbow.

Only a passed zero-cleanup source can render. No production artwork/gameplay edits.
"""
import argparse,csv,functools,json,math,re,shutil,wave
from pathlib import Path
import render_reference_reel as m
import render_promo_outro as promo
from render_creative_short import pcm
from PIL import Image,ImageDraw,ImageChops
import numpy as np

ROOT=m.ROOT;MEDIA=m.MEDIA;OUT=MEDIA/'05_Reels/DefensePilot02_20261115'
NAME='ArrowSwarm_Defense_R2_Sirayla_TR.mp4'
W,H,FPS,SR=1080,1920,30,48000
HUD=((0,0,W,230),(0,1690,W,H));CYAN=(93,228,255)

def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
def rows(p):
    with p.open(encoding='utf-8-sig',newline='') as f:return list(csv.DictReader(f))

def configure(source):
    global SOURCE,PROV,CHECKS,EVENTS,GEO,START,FREEZE,RAIN,WIN,FREEZE_AT,RAIN_AT,WIN_AT,OUTRO,SECONDS,LAST,VOICE_STARTS,PROMOTED_AT
    SOURCE=source.resolve();PROV=read(SOURCE/'provenance.json');CHECKS=read(SOURCE/'native_capture_checks.json')
    EVENTS=rows(SOURCE/'events.csv');GEO=read(SOURCE/'geometry.json')
    assert CHECKS['passed'] and PROV['allFramesSaved'] and PROV['cleanupKills']==0
    assert PROV['spawnedMobs']==PROV['projectileKills'] and PROV['lives']==3
    START=CHECKS['halfwayOpeningFrame'];FREEZE=PROV['freezeFrame'];RAIN=CHECKS['rainbowFrame'];WIN=PROV['winFrame']
    FREEZE_AT=(FREEZE-START)/60;RAIN_AT=FREEZE_AT+(RAIN-FREEZE)/30;WIN_AT=RAIN_AT+(WIN-RAIN)/22.5
    OUTRO=math.ceil((WIN_AT+3.2)*30)/30;SECONDS=math.ceil(OUTRO+4.7);LAST=WIN+96
    shots=[e for e in EVENTS if e['event'] in ('fire','rainbow_fire')]
    PROMOTED_AT=event_time(int(shots[-2]['frame']))
    VOICE_STARTS=[.1,1.65,FREEZE_AT-.7,event_time(PROV['firstFireFrame'])+.15,RAIN_AT-.97,WIN_AT+.4,OUTRO+.8]
    voice=read(OUT/'audio/voice_manifest.json')['clips'];assert len(voice)==7
    assert all(VOICE_STARTS[i]+voice[i]['durationSeconds']<VOICE_STARTS[i+1] for i in range(6))
    assert VOICE_STARTS[4]>=PROMOTED_AT and VOICE_STARTS[6]+voice[6]['durationSeconds']<OUTRO+3.15<SECONDS
    assert m.sha(OUT/'audio/voice_06.wav')==m.sha(MEDIA/'05_Reels/SharedOutro01/cta_voice.wav')

def event_time(i):
    if i<FREEZE:return (i-START)/60
    if i<RAIN:return FREEZE_AT+(i-FREEZE)/30
    if i<WIN:return RAIN_AT+(i-RAIN)/22.5
    return WIN_AT+(i-WIN)/30

def raw_index(t):
    if t<FREEZE_AT:return round(START+t*60)
    if t<RAIN_AT:return round(FREEZE+(t-FREEZE_AT)*30)
    if t<WIN_AT:return round(RAIN+(t-RAIN_AT)*22.5)
    return min(LAST,round(WIN+(t-WIN_AT)*30))

@functools.lru_cache(maxsize=15)
def native(i):return Image.open(SOURCE/f'frame_{i:05d}.jpg').convert('RGB')
def label(im,text,y,size=50,color=m.WHITE,stroke=4):return m.label(im,text,y,size,color,stroke,cx=540)

def frame(t):
    if t>=OUTRO+.2:return promo.frame(t-OUTRO)
    i=raw_index(t);source=native(i);im=source.convert('RGBA');d=ImageDraw.Draw(im)
    if t<1.6:label(im,'DÜŞMANLAR İLERLİYOR.',260,47,m.WHITE,4)
    elif t<FREEZE_AT-.7:label(im,'AMA PANİK YOK.',260,59,m.GOLD,4)
    elif t<event_time(PROV['firstFireFrame']):label(im,'ÖNCE DONDUR.',260,60,CYAN,4)
    if t<FREEZE_AT:
        d.rounded_rectangle((472,1627,608,1670),radius=20,fill=m.INK);label(im,'2×',1634,29,CYAN,0)
    elif t<VOICE_STARTS[4]:
        label(im,'SIRAYLA GÖNDER.' if t>=event_time(PROV['firstFireFrame']) else 'ZAMAN SENDE.',1580,43,m.GOLD,3)
    elif t<RAIN_AT:
        label(im,'SON OK…',1575,52,m.GOLD,4)
        point=GEO['arrows'][PROV['reservedLastArrow']]['screenHead'];x,y=point['x'],point['y'];r=59+3*math.sin(t*8)
        for a in [0,90,180,270]:d.arc((x-r,y-r,x+r,y+r),a+8,a+73,fill=CYAN,width=5)
    elif t<WIN_AT:
        label(im,'GÖKKUŞAĞI!',1565,53,m.GOLD,4)
        d.rounded_rectangle((450,1632,630,1672),radius=20,fill=m.INK);label(im,'0,75×',1638,27,CYAN,0)
    elif WIN_AT+.4<=t<WIN_AT+2:
        label(im,'HEPSİ TEMİZLENDİ!',238,29,CYAN,2)
    rgb=im.convert('RGB')
    if t<OUTRO:
        for b in HUD:assert ImageChops.difference(rgb.crop(b),source.crop(b)).getbbox() is None,f'HUD changed at{t}'
    else:rgb=Image.blend(rgb,promo.frame(t-OUTRO),m.ease((t-OUTRO)/.2))
    return rgb

def audio(ff):
    n=SECONDS*SR;music=pcm(ff,ROOT/'Assets/_Project/Audio/Music/GameBGM_alt1.ogg',SECONDS,8)[:n]
    music*=.05/max(.001,float(np.sqrt(np.mean(music**2))));mix=music.copy();cues=[]
    def add(t,data):
        a=round(t*SR);b=min(n,a+len(data))
        if 0<=a<b:mix[a:b]+=data[:b-a]
    def effect(t,name,rms):
        p=ROOT/'Assets/_Project/Audio/SFX'/name;data=pcm(ff,p);data*=rms/max(.001,float(np.sqrt(np.mean(data**2))))
        data*=min(1,.25/max(.001,float(abs(data).max())));add(t,data);cues.append({'seconds':t,'asset':str(p.relative_to(ROOT)).replace('\\','/'),'sha256':m.sha(p)})
    effect(FREEZE_AT,'SkillFreeze.ogg',.065)
    for e in EVENTS:
        if e['event'] in ('fire','rainbow_fire'):effect(event_time(int(e['frame'])),'RainbowArrow.ogg' if e['event']=='rainbow_fire' else 'ArrowFire.wav',.05 if e['event']=='rainbow_fire' else .029)
    last=-10
    for e in EVENTS:
        if e['event']=='projectile_damage':
            t=event_time(int(e['frame']))
            if t-last>=.18:effect(t,'ArrowHitEnemy.ogg',.022);last=t
    effect(WIN_AT+.1,'LevelWin.ogg',.043);effect(WIN_AT+2.4,'StarEarn.wav',.035);effect(OUTRO+3.15,'ButtonClick_alt2.wav',.05)
    voices=read(OUT/'audio/voice_manifest.json')['clips'];timeline=[]
    for j,start in enumerate(VOICE_STARTS):
        p=OUT/f'audio/voice_{j:02d}.wav';data=pcm(ff,p);data*=.13/max(.001,float(np.sqrt(np.mean(data**2))));data*=min(1,.57/max(.001,float(abs(data).max())))
        a=round(start*SR);b=a+len(data);assert b<=n;da=max(0,a-round(.08*SR));db=min(n,b+round(.12*SR));env=np.ones(db-da)*.22
        if a>da:env[:a-da]=np.linspace(1,.22,a-da)
        if db>b:env[-(db-b):]=np.linspace(.22,1,db-b)
        mix[da:db]*=env[:,None];add(start,data);timeline.append({'text':voices[j]['text'],'start':start,'end':b/SR,'sha256':m.sha(p)})
    fade=np.minimum(np.arange(n)/(SR*.04),1)*np.minimum((n-1-np.arange(n))/(SR*.18),1);mix*=fade[:,None];mix*=min(1,.88/max(.001,float(abs(mix).max())))
    with wave.open(str(OUT/'editorial_mix.wav'),'wb') as f:
        f.setnchannels(2);f.setsampwidth(2);f.setframerate(SR);f.writeframes(np.rint(mix*32767).astype('<i2').tobytes())
    write(OUT/'audio_timeline.json',{'voices':timeline,'cues':cues,'music':'Assets/_Project/Audio/Music/GameBGM_alt1.ogg','nativeAudioListenerRecorded':False,'editorialReconstruction':True,'correctedPCtaCopiedByteExact':True})
    def tc(t):
        ms=round(t*1000);return f'00:00:{ms//1000:02d},{ms%1000:03d}'
    (OUT/'captions_TR.srt').write_text('\n\n'.join(f'{i+1}\n{tc(r["start"])} --> {tc(r["end"])}\n{r["text"]}' for i,r in enumerate(timeline))+'\n',encoding='utf-8')

def verify(ff,path):
    r=m.command([ff,'-hide_banner','-i',str(path),'-f','null','-','-progress','pipe:1'],OUT/'decode.log')
    count=int(re.findall(r'^frame=(\d+)$',r.stdout,re.M)[-1]);assert count==SECONDS*FPS
    assert all(s in r.stderr for s in ['1080x1920','yuv420p','48000 Hz, stereo','h264','aac'])
    r=m.command([ff,'-hide_banner','-i',str(path),'-vn','-af','loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json','-f','null','-'],OUT/'loudness.log')
    levels=json.loads(re.findall(r'\{\s*"input_i".*?\}',r.stderr,re.S)[-1]);assert -18<=float(levels['input_i'])<=-14 and float(levels['input_tp'])<-1
    qa={'passed':True,'revision':'Defense R2','durationSeconds':SECONDS,'frames':count,'width':W,'height':H,'fps':FPS,'bytes':path.stat().st_size,'sha256':m.sha(path),
        'source':SOURCE.name,'arrows':PROV['arrows'],'lives':3,'freezeAt':FREEZE_AT,'firstShotAt':event_time(PROV['firstFireFrame']),'rainbowAt':RAIN_AT,'naturalWinAt':WIN_AT,'promoAt':OUTRO,
        'introSpeed':2,'actionSpeed':1,'rainbowSpeed':.75,'speedChangesLabelled':True,'nativeHudBandsIdenticalBeforeEncode':HUD,
        'spawnedMobs':PROV['spawnedMobs'],'projectileKills':PROV['projectileKills'],'victoryCleanupKills':0,'sourceHalfwayOpeningFrame':START,
        'ctaNarration':[VOICE_STARTS[-1],VOICE_STARTS[-1]+2.3],'buttonPressAt':OUTRO+3.15,'storeStatus':'mock','publicStoreUrl':None,'audio':levels}
    write(OUT/'QA_REPORT.json',qa)
    write(OUT/'timeline.json',[{'frame':i,'seconds':i/FPS,'sourceFrame':raw_index(i/FPS) if i/FPS<OUTRO+.2 else None,'nativeHudGuard':i/FPS<OUTRO,'promo':i/FPS>=OUTRO} for i in range(SECONDS*FPS)])
    m.command([ff,'-hide_banner','-y','-i',str(path),'-vf',f'fps=1/{SECONDS/10},scale=216:384,tile=5x2','-frames:v','1','-update','1',str(OUT/'encoded_contact_sheet.jpg')],OUT/'contact_sheet.log')
    m.command([ff,'-hide_banner','-y','-ss','0.4','-i',str(path),'-frames:v','1','-update','1',str(OUT/'cover.jpg')],OUT/'cover.log')
    return qa

def main():
    p=argparse.ArgumentParser();p.add_argument('--source',type=Path,required=True);p.add_argument('--stills',action='store_true');a=p.parse_args();configure(a.source)
    times=[.4,2.,FREEZE_AT-.1,FREEZE_AT+.2,event_time(PROV['firstFireFrame'])+.6,PROMOTED_AT+.3,RAIN_AT-.2,RAIN_AT+.4,RAIN_AT+1.6,WIN_AT+.9,WIN_AT+3.05,OUTRO+1.3,OUTRO+3.3]
    for t in times:frame(t).save(OUT/f'still_{t:05.2f}.jpg',quality=94)
    if a.stills:print(json.dumps({'stillsPassed':True,'duration':SECONDS,'freeze':FREEZE_AT,'rainbow':RAIN_AT,'win':WIN_AT,'promo':OUTRO}));return
    ff=m.imageio_ffmpeg.get_ffmpeg_exe();audio(ff);m.OUT=OUT;m.SECONDS=SECONDS;m.frame=frame
    video=m.render(ff,NAME,'New native Defense10 source. Earlier charged Freeze; sequential legal shots; separate final Rainbow; every enemy killed by projectiles with zero victory cleanup. Both native HUDs preserved. Intro2x and Rainbow0.75x labelled. Editorial sound/text; store mock.')
    qa=verify(ff,video);shutil.copy2(SOURCE/'provenance.json',OUT/'source_provenance.json');shutil.copy2(ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1-OFL.txt',OUT/'FONT_LICENSE.txt');print(json.dumps(qa,indent=2),flush=True)
    for p in OUT.glob('*.log'):p.write_text('\n'.join(x.rstrip() for x in p.read_text(encoding='utf-8').splitlines())+'\n',encoding='utf-8')
if __name__=='__main__':main()
