"""R2: full native scene, isolated A, a real wait, then remaining arrows.

Both HUDs remain untouched in every output frame. No board/HUD compositing,
affine reframing, replacement UI, global blur, or replacement ending is used.
"""
from __future__ import annotations
import argparse,csv,functools,json,math,re,shutil
from pathlib import Path
import render_reference_reel as shared
from PIL import Image,ImageDraw,ImageChops

ROOT=shared.ROOT;MEDIA=shared.MEDIA
SOURCE=MEDIA/'04_Raw_Gameplay/Shapes_L2_20261114_105432_122'
OUT=MEDIA/'05_Reels/ReferencePilot02_R2_20261114'
VIDEO='ArrowSwarm_Shapes2_R2_FullScene_TR.mp4'
W,H,FPS,SECONDS=1080,1920,30,37
VOICE_STARTS=[.15,2.45,4.55,9.30,10.60,12.80,28.80,34.10]
INK,GOLD,WHITE=shared.INK,shared.GOLD,shared.WHITE
HUD_BANDS=((0,0,W,230),(0,1690,W,H))
with (SOURCE/'events.csv').open(encoding='utf-8-sig',newline='') as f:ALL_EVENTS=list(csv.DictReader(f))
EVENTS=[e for e in ALL_EVENTS if e['event']=='fire']
FIRST=int(EVENTS[0]['frame']);SECOND=int(EVENTS[1]['frame'])
SETTLED=int(next(e['frame'] for e in ALL_EVENTS if e['event']=='first_settled'))
RAW_START=FIRST-10;PLAY_START=10.2-10/30
REST_START=10.2+(SECOND-FIRST)/30
FAST_END_RAW=int(EVENTS[-1]['frame'])-10
FAST_END=REST_START+(FAST_END_RAW-SECOND)/45
PROVENANCE=json.loads((SOURCE/'provenance.json').read_text(encoding='utf-8-sig'))
LAST_SOURCE=PROVENANCE['frames']-1
SOURCE_END=FAST_END+(LAST_SOURCE-FAST_END_RAW)/30

def raw_index(t):
    if t<PLAY_START:return FIRST-15
    if t<REST_START:return round(RAW_START+(t-PLAY_START)*30)
    if t<FAST_END:return round(SECOND+(t-REST_START)*45)
    return min(LAST_SOURCE,round(FAST_END_RAW+(t-FAST_END)*30))
def event_time(f):
    if f<SECOND:return PLAY_START+(f-RAW_START)/30
    if f<FAST_END_RAW:return REST_START+(f-SECOND)/45
    return FAST_END+(f-FAST_END_RAW)/30
@functools.lru_cache(maxsize=14)
def native(i):return Image.open(SOURCE/f'frame_{i:05d}.jpg').convert('RGB')
def label(im,text,y,size=54,color=WHITE,stroke=4):return shared.label(im,text,y,size,color,stroke,cx=520)

def quiz(im,t):
    if not 2.55<=t<10.17:return
    fade=1-shared.ease((t-9.9)/.25)
    layer=Image.new('RGBA',(W,H));d=ImageDraw.Draw(layer)
    for order,(letter,index) in enumerate([('A',0),('B',3),('C',1)]):
        alpha=shared.ease((t-(2.55+order*.2))/.2)*fade
        if not alpha:continue
        points=[shared.grid_to_source(p) for p in shared.PATHS[index]]
        color=(94,237,178) if letter=='A' and t>=9.2 else GOLD
        d.line(points,fill=(*color,round(75*alpha)),width=20,joint='curve')
        hx,hy=points[0];cy=1530 if letter!='C' else 1500;r=round(33*alpha)
        d.line(((hx,hy),(hx,cy-r)),fill=(*INK,round(230*alpha)),width=3)
        d.ellipse((hx-r+1,cy-r+4,hx+r+1,cy+r+4),fill=(*INK,round(90*alpha)))
        d.ellipse((hx-r,cy-r,hx+r,cy+r),fill=(*color,round(255*alpha)),outline=(*INK,round(255*alpha)),width=4)
        font=shared.font(37);box=d.textbbox((0,0),letter,font=font)
        d.text((hx-d.textlength(letter,font=font)/2,cy-(box[3]-box[1])/2-box[1]),letter,font=font,fill=(*INK,round(255*alpha)))
    im.alpha_composite(layer)
    if 6.2<=t<9.2:
        d=ImageDraw.Draw(im);cx,cy,r=520,383,42
        d.ellipse((cx-r,cy-r,cx+r,cy+r),fill=(49,34,104),outline=WHITE,width=3)
        progress=1-(t-6.2)%1
        d.arc((cx-r+3,cy-r+3,cx+r-3,cy+r-3),start=-90,end=-90+max(1,progress*360),fill=GOLD,width=6)
        shared.label(im,str(3-int(t-6.2+1e-6)),cy-28,56,WHITE,2,cx)

def captions(im,t):
    if t<2.4:
        label(im,'BU PENGUEN',268,56)
        label(im,'42 OKTAN OLUŞUYOR.',347,58,GOLD)
    elif t<4.5:
        label(im,'BU ÜÇ OKTAN',268,55)
        label(im,'HANGİSİ ÖNCE ÇIKAR?',347,53,GOLD)
    elif t<6.2:label(im,'A MI, B Mİ, C Mİ?',302,61,GOLD)
    elif t<9.2:label(im,'3 SANİYEN VAR.',267,45,GOLD)
    elif t<10.45:label(im,'CEVAP: A!',295,75,GOLD,6)
    elif t<12.3:
        label(im,'SOLUNDAKİ YOL',268,54)
        label(im,'AÇIK.',347,60,GOLD)
    elif t<12.8:label(im,'A ÇIKTI.',303,48,GOLD)
    elif t<REST_START:
        label(im,'ŞİMDİ GERİYE KALAN',268,52)
        label(im,'OKLARI YOLLAYALIM.',347,54,GOLD)
    elif t<FAST_END:
        d=ImageDraw.Draw(im);d.rounded_rectangle((425,300,615,347),radius=23,fill=INK)
        label(im,'1,5×',306,31,GOLD,0)
    elif t<event_time(int(EVENTS[-1]['frame']))+.55:label(im,'SON OK DA ÇIKIYOR.',300,44,GOLD)
    # The real win popup fills the middle. Use only the gap above BottomHUD.
    elif 28.8<=t<29.75:label(im,'TEBRİKLER!',1644,30,GOLD,2)
    elif 29.75<=t<31.05:label(im,'42 OKTAN OLUŞAN',1644,30,WHITE,2)
    elif 31.05<=t<32.15:label(im,'PENGUEN BÖLÜMÜNÜ',1644,30,WHITE,2)
    elif 32.15<=t<33.7:label(im,'BAŞARIYLA GEÇTİN!',1644,30,GOLD,2)
    elif 34.1<=t:label(im,'İLK SEFERDE BİLDİN Mİ?',1644,30,GOLD,2)

def frame(t):
    source=native(raw_index(t));im=source.convert('RGBA')
    quiz(im,t);captions(im,t);im=im.convert('RGB')
    # This assertion runs on every rendered output frame, including the ending.
    for band in HUD_BANDS:
        assert ImageChops.difference(im.crop(band),source.crop(band)).getbbox() is None,f'HUD changed at {t}'
    return im

def verify(ffmpeg,path):
    r=shared.command([ffmpeg,'-hide_banner','-i',str(path),'-f','null','-','-progress','pipe:1'],OUT/'decode.log')
    count=int(re.findall(r'^frame=(\d+)$',r.stdout,re.M)[-1]);assert count==SECONDS*FPS
    assert all(s in r.stderr for s in ['1080x1920','yuv420p','48000 Hz, stereo','h264','aac'])
    assert not re.search(r'corrupt|Error while decoding|Invalid data',r.stderr,re.I)
    r=shared.command([ffmpeg,'-hide_banner','-i',str(path),'-vn','-af','loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json','-f','null','-'],OUT/'loudness.log')
    levels=json.loads(re.findall(r'\{\s*"input_i".*?\}',r.stderr,re.S)[-1])
    assert float(levels['input_tp'])<=-1 and -18<=float(levels['input_i'])<=-14
    assert len(EVENTS)==42 and int(EVENTS[0]['arrow'])==0 and SETTLED<SECOND
    voice=json.loads((OUT/'audio/voice_manifest.json').read_text(encoding='utf-8'))['clips']
    assert VOICE_STARTS[5]+voice[5]['durationSeconds']<REST_START
    assert all(VOICE_STARTS[i]+voice[i]['durationSeconds']<=VOICE_STARTS[i+1] for i in range(7))
    assert all(e['remaining']=='41' for e in ALL_EVENTS if e['event']=='first_settled')
    qa={'passed':True,'width':W,'height':H,'fps':FPS,'durationSeconds':SECONDS,'decodedFrames':count,'audio':levels,'sha256':shared.sha(path),'bytes':path.stat().st_size,'nativeActionCount':42,'nativeSource':SOURCE.name,'sourceLanguage':'tr','fullNativeScene':True,'hudMaskOrReplacement':False,'wholeFrameCropOrZoom':False,'hudBandsPixelIdenticalBeforeEncodeEveryFrame':list(HUD_BANDS),'firstArrow':0,'firstShotAt':event_time(FIRST),'firstShotSettledAt':event_time(SETTLED),'remainingSequenceAt':REST_START,'idleWaitSecondsBeforeOtherShots':(SECOND-SETTLED)/30,'resumeNarrationEndsAt':VOICE_STARTS[5]+voice[5]['durationSeconds'],'correctCandidate':'A','sourceWon':PROVENANCE['won'],'sourceNativeAudio':False,'creativeApproved':False}
    (OUT/'QA_REPORT.json').write_text(json.dumps(qa,indent=2),encoding='utf-8')
    shared.command([ffmpeg,'-hide_banner','-y','-i',str(path),'-vf','fps=1/3.7,scale=216:384,tile=5x2','-frames:v','1',str(OUT/'encoded_contact_sheet.jpg')],OUT/'contact_sheet.log')
    shared.command([ffmpeg,'-hide_banner','-y','-ss','6.5','-i',str(path),'-frames:v','1','-update','1',str(OUT/'cover.jpg')],OUT/'cover.log')
    return qa

def main():
    p=argparse.ArgumentParser();p.add_argument('--stills',action='store_true');args=p.parse_args()
    OUT.mkdir(parents=True,exist_ok=True)
    for t in [.5,3.7,6.5,9.5,10.7,12.5,14.5,16,25,29.5,32.8,35.5]:frame(t).save(OUT/f'still_{t:04.1f}.jpg',quality=94)
    if args.stills:print('R2 full-scene stills passed HUD guards');return
    shared.OUT=OUT;shared.SOURCE=SOURCE;shared.SECONDS=SECONDS;shared.VOICE_STARTS=VOICE_STARTS
    shared.frame=frame;shared.event_time=event_time
    ffmpeg=shared.imageio_ffmpeg.get_ffmpeg_exe()
    shared.make_audio(ffmpeg,EVENTS,countdown_times=[6.2,7.2,8.2],answer_time=9.3,victory_time=28.8)
    path=shared.render(ffmpeg,VIDEO,'Full native Unity Shapes2 scene, both HUDs preserved. One real A shot, completed flight, idle hold, narration, then the remaining 41 arrows. Editorial annotations, hold, 1.5x segment and original/synthetic audio. No gameplay modification.')
    qa=verify(ffmpeg,path)
    timeline=[{'outputFrame':i,'seconds':i/FPS,'sourceFrame':raw_index(i/FPS),'fullScene':True,'topHudPreserved':True,'bottomHudPreserved':True} for i in range(SECONDS*FPS)]
    (OUT/'timeline.json').write_text(json.dumps(timeline,indent=2),encoding='utf-8')
    shutil.copy2(SOURCE/'provenance.json',OUT/'source_provenance.json')
    shutil.copy2(ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1-OFL.txt',OUT/'FONT_LICENSE.txt')
    print(json.dumps(qa,indent=2))
if __name__=='__main__':main()
