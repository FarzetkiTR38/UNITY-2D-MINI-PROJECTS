"""Defense17s: natural pressure -> real Freeze -> counterattack -> native victory.

No choice quiz, artificial enemy movement, damage or result. Both original HUDs
remain pixel-identical before encoding until the explicit promotional transition.
"""
import argparse,csv,functools,json,math,re,shutil,subprocess,tempfile,wave
from pathlib import Path
import render_reference_reel as m
import render_promo_outro as promo
from render_creative_short import pcm
from PIL import Image,ImageDraw,ImageChops
import numpy as np

ROOT=m.ROOT;MEDIA=m.MEDIA;OUT=MEDIA/'05_Reels/DefensePilot01_20261115'
SOURCE=MEDIA/'04_Raw_Gameplay/Defense_L10_20261114_214156_530'
NAME='ArrowSwarm_Defense_PanikYok_TR.mp4'
W,H,FPS,SECONDS,SR=1080,1920,30,17,48000
START,SLOW,WIN_RAW,LAST=506,656,718,800
SLOW_AT=5.;WIN_AT=SLOW_AT+(WIN_RAW-SLOW)/15;OUTRO=12.3
VOICE_STARTS=[.1,1.45,2.8,3.85,5.45,9.7,13.1]
HUD=((0,0,W,230),(0,1690,W,H));CYAN=(93,228,255)
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
def csv_rows(p):
    with p.open(encoding='utf-8-sig',newline='') as f:return list(csv.DictReader(f))
PROV=read(SOURCE/'provenance.json');CHECKS=read(SOURCE/'native_capture_checks.json')
EVENTS=csv_rows(SOURCE/'events.csv');TELEMETRY=csv_rows(SOURCE/'telemetry.csv')
def raw_index(t):
    if t<SLOW_AT:return round(START+t*30)
    if t<WIN_AT:return round(SLOW+(t-SLOW_AT)*15)
    return min(LAST,round(WIN_RAW+(t-WIN_AT)*30))
def event_time(frame):
    if frame<SLOW:return (frame-START)/30
    if frame<WIN_RAW:return SLOW_AT+(frame-SLOW)/15
    return WIN_AT+(frame-WIN_RAW)/30
@functools.lru_cache(maxsize=15)
def native(i):return Image.open(SOURCE/f'frame_{i:05d}.jpg').convert('RGB')
def label(im,text,y,size=54,color=m.WHITE,stroke=4):return m.label(im,text,y,size,color,stroke,cx=540)

def threat(im,t,index):
    if not 1.25<=t<3.7:return
    row=TELEMETRY[index];x,y=float(row['leader_x']),float(row['leader_y'])
    alpha=m.ease((t-1.25)/.25)*(1-m.ease((t-3.4)/.3));color=CYAN if t>=3 else (255,128,128)
    layer=Image.new('RGBA',(W,H));d=ImageDraw.Draw(layer);r=74+3*math.sin(t*8)
    for start in [15,105,195,285]:d.arc((x-r,y-r,x+r,y+r),start=start,end=start+52,fill=(*color,round(185*alpha)),width=4)
    im.alpha_composite(layer)

def captions(im,t):
    d=ImageDraw.Draw(im)
    if t<1.25:
        pop=.88+.12*m.ease(t/.18);label(im,'PANİK YOK.',260+round((1-pop)*20),round(66*pop),m.GOLD,5)
        label(im,'SÜRÜ YAKLAŞIYOR.',1580,38,m.WHITE,3)
    elif t<2.8:
        label(im,'BİRAZ DAHA…',262,59,m.WHITE,4)
        label(im,'DOĞRU ANI BEKLE.',1580,38,m.GOLD,3)
    elif t<3.7:
        label(im,'DONDUR!',258,70,CYAN,5)
        width=260*m.ease((t-2.8)/.18);d.line((540-width/2,339,540+width/2,339),fill=CYAN,width=5)
        label(im,'TEK DOKUNUŞ.',1580,38,CYAN,3)
    elif t<SLOW_AT:label(im,'KARŞI SALDIRI!',1580,47,m.GOLD,4)
    elif t<WIN_AT:
        label(im,'SON OK…' if t<5.45 else 'GÖKKUŞAĞI!',1565,47,m.GOLD,4)
        d.rounded_rectangle((464,1632,616,1672),radius=20,fill=m.INK)
        label(im,'0,5×',1638,27,CYAN,0)
    elif 9.7<=t<10.8:label(im,'İŞTE BÖYLE!',238,32,m.GOLD,2)
    elif 10.8<=t<OUTRO:label(im,'BÖLÜM KURTARILDI.',238,30,CYAN,2)

def frame(t):
    if t>=OUTRO+.2:return promo.frame(t-OUTRO)
    index=raw_index(t);source=native(index);im=source.convert('RGBA')
    threat(im,t,index);captions(im,t);rgb=im.convert('RGB')
    if t<OUTRO:
        for band in HUD:assert ImageChops.difference(rgb.crop(band),source.crop(band)).getbbox() is None,f'HUD changed at{t}'
    else:rgb=Image.blend(rgb,promo.frame(t-OUTRO),m.ease((t-OUTRO)/.2))
    return rgb

def audio(ff):
    n=SECONDS*SR;path=ROOT/'Assets/_Project/Audio/Music/GameBGM_alt1.ogg'
    music=pcm(ff,path,SECONDS,8);assert len(music)>=n
    music=music[:n];music*=.05/max(.001,float(np.sqrt(np.mean(music**2))));mix=music.copy();cues=[]
    # A brief dip makes the native freeze audible; speech is independently ducked below.
    a,b=round(3*SR),round(3.7*SR);env=np.ones(b-a)*.35;edge=round(.03*SR)
    env[:edge]=np.linspace(1,.35,edge);env[-edge:]=np.linspace(.35,1,edge);mix[a:b]*=env[:,None]
    def add(t,data):
        start=round(t*SR);end=min(n,start+len(data))
        if start<0 or end<=start:return
        mix[start:end]+=data[:end-start]
    def effect(t,name,rms):
        source=ROOT/'Assets/_Project/Audio/SFX'/name;data=pcm(ff,source)
        data*=rms/max(.001,float(np.sqrt(np.mean(data**2))));data*=min(1,.25/max(.001,float(abs(data).max())))
        add(t,data);cues.append({'seconds':t,'asset':str(source.relative_to(ROOT)).replace('\\','/'),'sha256':m.sha(source)})
    effect(1.45,'ButtonClick_alt2.wav',.02);effect(2.45,'ButtonClick_alt2.wav',.024)
    effect(3.,'SkillFreeze.ogg',.065)
    fired=[e for e in EVENTS if e['event'] in ['fire','rainbow_fire']]
    for i,e in enumerate(fired):
        if i%3==0 or e['event']=='rainbow_fire':effect(event_time(int(e['frame'])),'RainbowArrow.ogg' if e['event']=='rainbow_fire' else 'ArrowFire.wav',.04)
    last=-10
    for e in EVENTS:
        if e['event']!='projectile_damage':continue
        t=event_time(int(e['frame']))
        if t-last>=.22:effect(t,'ArrowHitEnemy.ogg',.023);last=t
    effect(WIN_AT+.10,'LevelWin.ogg',.045);effect(11.2,'StarEarn.wav',.035)
    effect(OUTRO+3.15,'ButtonClick_alt2.wav',.05)
    voice=read(OUT/'audio/voice_manifest.json')['clips'];rows=[]
    for i,start in enumerate(VOICE_STARTS):
        path=OUT/f'audio/voice_{i:02d}.wav';data=pcm(ff,path);data*=.13/max(.001,float(np.sqrt(np.mean(data**2))))
        data*=min(1,.57/max(.001,float(abs(data).max())));a=round(start*SR);b=a+len(data);assert b<=n
        da=max(0,a-round(.08*SR));db=min(n,b+round(.12*SR));env=np.ones(db-da)*.22
        if a>da:env[:a-da]=np.linspace(1,.22,a-da)
        if db>b:env[-(db-b):]=np.linspace(.22,1,db-b)
        mix[da:db]*=env[:,None];add(start,data)
        rows.append({'id':i,'text':voice[i]['text'],'start':start,'end':b/SR,'sha256':m.sha(path)})
    fade=np.minimum(np.arange(n)/(SR*.04),1)*np.minimum((n-1-np.arange(n))/(SR*.18),1)
    mix*=fade[:,None];peak=float(abs(mix).max());mix*=min(1,.88/max(.001,peak))
    with wave.open(str(OUT/'editorial_mix.wav'),'wb') as f:
        f.setnchannels(2);f.setsampwidth(2);f.setframerate(SR);f.writeframes(np.rint(mix*32767).astype('<i2').tobytes())
    write(OUT/'audio_timeline.json',{'music':{'asset':'Assets/_Project/Audio/Music/GameBGM_alt1.ogg','sha256':m.sha(ROOT/'Assets/_Project/Audio/Music/GameBGM_alt1.ogg'),'excerptStartSeconds':8},
        'voices':rows,'cues':cues,'musicDuckedDuringSpeech':True,'nativeAudioListenerRecorded':False,'sourceSoundEffectEventsAreEditorialReconstruction':True})

def preflight():
    assert CHECKS['passed'] and PROV['won'] and PROV['lives']==3 and PROV['arrows']==15
    assert PROV['freezeFrame']==596 and PROV['firstFireFrame']==617 and PROV['winFrame']==718
    assert raw_index(3)==596 and raw_index(3.7)==617 and event_time(659)==5.2
    assert event_time(718)==WIN_AT and raw_index(12.3)==LAST
    voice=read(OUT/'audio/voice_manifest.json')['clips'];assert len(voice)==7
    assert all(VOICE_STARTS[i]+voice[i]['durationSeconds']<VOICE_STARTS[i+1] for i in range(6))
    assert VOICE_STARTS[-1]+voice[-1]['durationSeconds']<OUTRO+3.15<SECONDS
    assert m.sha(OUT/'audio/voice_06.wav')==m.sha(MEDIA/'05_Reels/SharedOutro01/cta_voice.wav')

def verify(ff,path):
    r=m.command([ff,'-hide_banner','-i',str(path),'-f','null','-','-progress','pipe:1'],OUT/'decode.log')
    count=int(re.findall(r'^frame=(\d+)$',r.stdout,re.M)[-1]);assert count==SECONDS*FPS
    assert all(s in r.stderr for s in ['1080x1920','yuv420p','48000 Hz, stereo','h264','aac'])
    r=m.command([ff,'-hide_banner','-i',str(path),'-vn','-af','loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json','-f','null','-'],OUT/'loudness.log')
    levels=json.loads(re.findall(r'\{\s*"input_i".*?\}',r.stderr,re.S)[-1]);assert -18<=float(levels['input_i'])<=-14 and float(levels['input_tp'])<-1
    report={'passed':True,'durationSeconds':17,'frames':count,'width':W,'height':H,'fps':FPS,'bytes':path.stat().st_size,'sha256':m.sha(path),
        'source':SOURCE.name,'mode':'Defense','level':10,'arrows':15,'lives':3,'realChargeUsed':[1,0],
        'naturalApproachFrame':START,'freezeAt':3,'leadingProgressAtFreeze':.90132,'firstShotAt':3.7,'rainbowAt':5.2,'naturalWinAt':WIN_AT,
        'slowMotion':{'start':5,'end':WIN_AT,'rate':.5,'label':'0,5×'},'promoAt':OUTRO,'ctaNarration':[13.1,15.4],'buttonPressAt':15.45,
        'nativeHudBandsIdenticalBeforeEncode':HUD,'nativeGameplayZoomCropOrReplacementHud':False,'nativeDamageEventsExcludingCleanup':78,
        'projectileKills':22,'victoryCleanupKills':1,'noForcedOutcomeOrEnemyStatManipulation':True,
        'nativeSourceRecordedThisTask':True,'storeStatus':'mock','publicStoreUrl':None,'audio':levels,
        'creativeApproval':'One Defense concept for user review; no further Defense/ColorShift batch authorized yet'}
    write(OUT/'QA_REPORT.json',report)
    write(OUT/'timeline.json',[{'frame':i,'seconds':i/FPS,'sourceFrame':raw_index(i/FPS) if i/FPS<OUTRO+.2 else None,'nativeHudGuard':i/FPS<OUTRO,'promo':i/FPS>=OUTRO} for i in range(SECONDS*FPS)])
    m.command([ff,'-hide_banner','-y','-i',str(path),'-vf','fps=1/1.7,scale=216:384,tile=5x2','-frames:v','1','-update','1',str(OUT/'encoded_contact_sheet.jpg')],OUT/'contact_sheet.log')
    m.command([ff,'-hide_banner','-y','-ss','3.2','-i',str(path),'-frames:v','1','-update','1',str(OUT/'cover.jpg')],OUT/'cover.log')
    return report

def main():
    p=argparse.ArgumentParser();p.add_argument('--stills',action='store_true');a=p.parse_args();OUT.mkdir(parents=True,exist_ok=True);preflight()
    for t in [.4,2.,2.9,3.2,4.2,5.3,6.5,8.4,9.8,11.9,13.3,15.6]:frame(t).save(OUT/f'still_{t:04.1f}.jpg',quality=94)
    if a.stills:print('Defense stills passed native HUD guards.');return
    ff=m.imageio_ffmpeg.get_ffmpeg_exe();audio(ff);m.OUT=OUT;m.SECONDS=SECONDS;m.frame=frame
    video=m.render(ff,NAME,'Genuine new Unity Defense10 capture. Natural enemy approach, real charged Freeze and15 legal shots with native rainbow/victory. Both HUDs preserved; labelled0.5x final action. Editorial captions/sound; existing corrected CTA; store mock.')
    qa=verify(ff,video);shutil.copy2(SOURCE/'provenance.json',OUT/'source_provenance.json');shutil.copy2(ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1-OFL.txt',OUT/'FONT_LICENSE.txt')
    voice=read(OUT/'audio/voice_manifest.json')['clips']
    def tc(t):
        ms=round(t*1000);return f'00:00:{ms//1000:02d},{ms%1000:03d}'
    (OUT/'captions_TR.srt').write_text('\n\n'.join(f'{i+1}\n{tc(VOICE_STARTS[i])} --> {tc(VOICE_STARTS[i]+r["durationSeconds"])}\n{r["text"]}' for i,r in enumerate(voice))+'\n',encoding='utf-8')
    print(json.dumps(qa,ensure_ascii=True,indent=2),flush=True)
if __name__=='__main__':main()
