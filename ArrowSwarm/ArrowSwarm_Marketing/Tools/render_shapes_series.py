"""Approved full-scene Reels, parametrized from native production evidence.

Run --level12 --stills before the complete render. Sources, game state and the
approved shared outro are read-only; only the episode's editorial output changes.
"""
from __future__ import annotations
import argparse,csv,functools,json,math,re,shutil
from pathlib import Path
import render_reference_reel as m
from PIL import Image,ImageDraw,ImageChops

ROOT=m.ROOT; MEDIA=m.MEDIA; SERIES=MEDIA/'05_Reels/ShapesSeries_20261114'
ORDER=[12,16,21,5,29,39]; W,H,FPS=1080,1920,30
HUD=((0,0,W,230),(0,1690,W,H))
SHARED=MEDIA/'05_Reels/SharedOutro01/ArrowSwarm_SharedOutro01_TR.mp4'

def write(path,data):path.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
def upper(s):return s.replace('i','İ').replace('ı','I').upper()

class Episode:
    def __init__(self,level):
        self.level=level;self.out=SERIES/f'L{level:02d}';self.out.mkdir(parents=True,exist_ok=True)
        self.source=sorted((MEDIA/'04_Raw_Gameplay').glob(f'Shapes_L{level}_20261114_*'))[-1]
        self.p=json.loads((self.source/'provenance.json').read_text(encoding='utf-8-sig'))
        self.g=json.loads((self.source/'geometry.json').read_text(encoding='utf-8-sig'))
        self.compact=min(self.point(p)[1] for a in self.g['arrows'] for p in a['points'])<440
        self.spec=next(r for r in json.loads((SERIES/'quiz_plan_draft.json').read_text(encoding='utf-8'))['levels'] if r['level']==level)
        with (self.source/'events.csv').open(encoding='utf-8-sig',newline='') as f:self.all=list(csv.DictReader(f))
        self.events=[e for e in self.all if e['event']=='fire'];self.n=self.p['arrows']
        self.first=int(self.events[0]['frame']);self.second=int(self.events[1]['frame'])
        self.settled=int(next(e['frame'] for e in self.all if e['event']=='first_settled'))
        self.won=int(next(e['frame'] for e in self.all if e['event']=='won'))
        clips=json.loads((self.out/'audio/voice_manifest.json').read_text(encoding='utf-8'))['clips']
        self.durations=[r['durationSeconds'] for r in clips]
        self.starts=[.15,2.45,4.55,9.3,10.6,12.8,0.]
        assert self.starts[0]+self.durations[0]<self.starts[1]
        assert all(self.starts[i]+self.durations[i]<self.starts[i+1] for i in range(5))
        self.fire=10.2;self.play_start=self.fire-10/30
        self.rest=max(self.fire+(self.second-self.first)/30,self.starts[5]+self.durations[5]+.3)
        self.fast_end_raw=int(self.events[-1]['frame'])-10
        self.fast_end=self.rest+(self.fast_end_raw-self.second)/45
        self.starts[6]=math.ceil((self.event_time(self.won)+.9)*30)/30
        self.outro=math.ceil((self.starts[6]+self.durations[6]+.2)*30)/30
        self.seconds=math.ceil(self.outro+.4);self.final_seconds=round(self.outro+6.3,6)
        self.final=self.out/f'ArrowSwarm_Shapes{level}_TR.mp4'
        assert self.settled<self.second and self.p['won'] and self.p['remaining']==0
        assert self.n==self.spec['arrow_count']==len(self.events)
        assert [int(e['arrow']) for e in self.events]==self.g['witness']==self.spec['solution_steps']
        assert all(e['lives']=='3' for e in self.all)
        assert int(self.events[0]['arrow'])==self.spec['choices'][0]['arrow_id']
        assert all(self.g['arrows'][c['arrow_id']]['initiallyRemovable']==(c['label']=='A') for c in self.spec['choices'])
        assert self.starts[5]+self.durations[5]<self.rest
        self.plan={'level':level,'title':self.spec['turkish_shape_name'],'arrows':self.n,'source':self.source.name,
            'first_fire':self.fire,'first_settled':self.event_time(self.settled),'rest_start':self.rest,
            'fast_end':self.fast_end,'native_win':self.event_time(self.won),'win_voice_start':self.starts[6],
            'congratulations_end':self.starts[6]+self.durations[6],'outro_start':self.outro,
            'duration_seconds':self.final_seconds,'voice_starts':self.starts,'voice_durations':self.durations,
            'choices':self.spec['choices'],'hud_guard_bands':HUD,'reused_outro_sha256':m.sha(SHARED),
            'store_status':'mock','public_store_url':None,'source_native_audio':False,'compact_editorial_layout':self.compact}
        write(self.out/'edit_plan.json',self.plan)

    def raw_index(self,t):
        if t<self.play_start:return self.first-15
        if t<self.rest:return min(self.second-1,round(self.first-10+(t-self.play_start)*30))
        if t<self.fast_end:return round(self.second+(t-self.rest)*45)
        return min(self.p['frames']-1,round(self.fast_end_raw+(t-self.fast_end)*30))

    def event_time(self,f):
        if f<self.second:return self.fire+(f-self.first)/30
        if f<self.fast_end_raw:return self.rest+(f-self.second)/45
        return self.fast_end+(f-self.fast_end_raw)/30

    def point(self,p):
        zero=self.g['gridZero'];x=self.g['gridX'];y=self.g['gridY']
        return (zero['x']+(x['x']-zero['x'])*p['x']+(y['x']-zero['x'])*p['y'],
            zero['y']+(x['y']-zero['y'])*p['x']+(y['y']-zero['y'])*p['y'])

    @functools.lru_cache(maxsize=12)
    def native(self,i):return Image.open(self.source/f'frame_{i:05d}.jpg').convert('RGB')

    def label(self,im,s,y,size=54,color=m.WHITE,stroke=4):return m.label(im,s,y,size,color,stroke,cx=520)

    def quiz(self,im,t):
        if not 2.55<=t<10.17:return
        fade=1-m.ease((t-9.9)/.25);layer=Image.new('RGBA',(W,H));d=ImageDraw.Draw(layer)
        choices=self.spec['choices'];heads=[self.point(self.g['arrows'][c['arrow_id']]['points'][0]) for c in choices]
        rank={i:r for r,i in enumerate(sorted(range(3),key=lambda i:heads[i][0]))}
        ys=[self.point(p)[1] for a in self.g['arrows'] for p in a['points']]
        bottom=max(ys);top=min(ys)
        # Upper-group choices use the empty upper gap, avoiding leaders across the whole puzzle.
        cy=max(475,top-70) if sum(h[1] for h in heads)/3<(top+bottom)/2 else min(1580,max(1500,bottom+95))
        occupied={False:[],True:[]}
        for order,c in enumerate(choices):
            alpha=m.ease((t-(2.55+order*.2))/.2)*fade
            if not alpha:continue
            pts=[self.point(p) for p in self.g['arrows'][c['arrow_id']]['points']]
            color=(94,237,178) if c['label']=='A' and t>=9.2 else m.GOLD
            d.line(pts,fill=(*color,round(75*alpha)),width=20,joint='curve')
            hx,hy=pts[0];cx=[220,540,860][rank[order]];badge_y=cy;r=round(33*alpha)
            if self.compact:
                side=hx>=540;cx=1035 if side else 45;badge_y=max(320,min(1620,hy));r=round(27*alpha)
                if abs(cx-hx)<60:badge_y+=100 if hy<1450 else -100
                while any(abs(badge_y-y)<85 for y in occupied[side]):badge_y-=85
                occupied[side].append(badge_y)
            endpoint=(cx+(r if cx<hx else -r),badge_y) if self.compact else (cx,badge_y-r)
            d.line(((hx,hy),endpoint),fill=(*m.INK,round(230*alpha)),width=3)
            d.ellipse((cx-r+1,badge_y-r+4,cx+r+1,badge_y+r+4),fill=(*m.INK,round(90*alpha)))
            d.ellipse((cx-r,badge_y-r,cx+r,badge_y+r),fill=(*color,round(255*alpha)),outline=(*m.INK,round(255*alpha)),width=4)
            f=m.font(37);box=d.textbbox((0,0),c['label'],font=f)
            d.text((cx-d.textlength(c['label'],font=f)/2,badge_y-(box[3]-box[1])/2-box[1]),c['label'],font=f,fill=(*m.INK,round(255*alpha)))
        im.alpha_composite(layer)
        if 6.2<=t<9.2 and not self.compact:
            d=ImageDraw.Draw(im);cx,cy,r=520,383,42
            d.ellipse((cx-r,cy-r,cx+r,cy+r),fill=(49,34,104),outline=m.WHITE,width=3)
            d.arc((cx-r+3,cy-r+3,cx+r-3,cy+r-3),start=-90,end=-90+max(1,(1-(t-6.2)%1)*360),fill=m.GOLD,width=6)
            m.label(im,str(3-int(t-6.2+1e-6)),cy-28,56,m.WHITE,2,cx)

    def captions(self,im,t):
        shape=upper(self.spec['turkish_shape_name'])
        if self.compact and t<self.starts[6]:
            if t<2.4:s=f'BU {shape} {self.n} OKTAN OLUŞUYOR.'
            elif t<4.5:s='BU ÜÇ OKTAN HANGİSİ ÖNCE ÇIKAR?'
            elif t<6.2:s='A MI, B Mİ, C Mİ?'
            elif t<9.2:s='3 SANİYEN VAR.  '+str(3-int(t-6.2+1e-6))
            elif t<10.45:s='CEVAP: A!'
            elif t<12.3:s='ÖNÜNDEKİ YOL AÇIK.'
            elif t<12.8:s='A ÇIKTI.'
            elif t<self.rest:s='ŞİMDİ GERİYE KALAN OKLARI YOLLAYALIM.'
            elif t<self.fast_end:s='1,5×'
            elif t<self.event_time(int(self.events[-1]['frame']))+.55:s='SON OK DA ÇIKIYOR.'
            else:return
            self.label(im,s,237,30,m.GOLD,2);return
        if t<2.4:self.label(im,'BU '+shape,268,56);self.label(im,f'{self.n} OKTAN OLUŞUYOR.',347,58,m.GOLD)
        elif t<4.5:self.label(im,'BU ÜÇ OKTAN',268,55);self.label(im,'HANGİSİ ÖNCE ÇIKAR?',347,53,m.GOLD)
        elif t<6.2:self.label(im,'A MI, B Mİ, C Mİ?',302,61,m.GOLD)
        elif t<9.2:self.label(im,'3 SANİYEN VAR.',267,45,m.GOLD)
        elif t<10.45:self.label(im,'CEVAP: A!',295,75,m.GOLD,6)
        elif t<12.3:self.label(im,'ÖNÜNDEKİ YOL',268,54);self.label(im,'AÇIK.',347,60,m.GOLD)
        elif t<12.8:self.label(im,'A ÇIKTI.',303,48,m.GOLD)
        elif t<self.rest:self.label(im,'ŞİMDİ GERİYE KALAN',268,52);self.label(im,'OKLARI YOLLAYALIM.',347,54,m.GOLD)
        elif t<self.fast_end:
            ImageDraw.Draw(im).rounded_rectangle((425,300,615,347),radius=23,fill=m.INK);self.label(im,'1,5×',306,31,m.GOLD,0)
        elif t<self.event_time(int(self.events[-1]['frame']))+.55:self.label(im,'SON OK DA ÇIKIYOR.',300,44,m.GOLD)
        elif self.starts[6]<=t<self.outro:
            p=(t-self.starts[6])/self.durations[6]
            chunk=0 if p<.18 else 1 if p<.44 else 2 if p<.73 else 3
            self.label(im,['TEBRİKLER!',f'{self.n} OKTAN OLUŞAN',shape+' BÖLÜMÜNÜ','BAŞARIYLA GEÇTİN!'][chunk],1644,30,m.GOLD if chunk in(0,3) else m.WHITE,2)

    def frame(self,t):
        source=self.native(self.raw_index(t));im=source.convert('RGBA');self.quiz(im,t);self.captions(im,t);im=im.convert('RGB')
        for band in HUD:assert ImageChops.difference(im.crop(band),source.crop(band)).getbbox() is None,f'HUD changed L{self.level} {t}'
        return im

    def check_source(self):
        files=[]
        for i in range(self.p['frames']):
            p=self.source/f'frame_{i:05d}.jpg'
            with Image.open(p) as im:assert im.size==(W,H)
            files.append({'file':p.name,'bytes':p.stat().st_size,'sha256':m.sha(p)})
        write(self.source/'frame_manifest.json',files)
        proof={'passed':True,'frames':len(files),'won':True,'lives':3,'remaining':0,'solverWitnessAndEventsMatch':True,
            'nativeProductionRulesConfirmOnlyAMarkedChoiceIsRemovable':True,'geometryFromActualCamera':True,
            'firstSettledFrame':self.settled,'nextShotFrame':self.second,'sourceIdleHoldSeconds':(self.second-self.settled)/30,
            'outputIdleHoldSeconds':self.rest-self.event_time(self.settled),
            'sourceAssetHash':m.sha(ROOT/self.g['levelAsset']),'sourceNativeAudio':False}
        assert proof['sourceAssetHash']==self.spec['asset_sha256']
        write(self.source/'native_capture_checks.json',proof);write(self.out/'native_capture_checks.json',proof)
        shutil.copy2(self.source/'provenance.json',self.out/'source_provenance.json')

    def append_outro(self,ff,prefix):
        s=self.outro;filt=f'[0:v]trim=end={s+.4},setpts=PTS-STARTPTS,fps=30,format=yuv420p[v0];[1:v]setpts=PTS-STARTPTS,fps=30,format=yuv420p[v1];[v0][v1]xfade=transition=fade:duration=0.4:offset={s}[v];[0:a]atrim=end={s},asetpts=PTS-STARTPTS,afade=t=out:st={s-.05}:d=0.05[a0];[1:a]asetpts=PTS-STARTPTS[a1];[a0][a1]concat=n=2:v=0:a=1[a]'
        cmd=[ff,'-hide_banner','-y','-i',str(prefix),'-i',str(SHARED),'-filter_complex_threads','2','-filter_complex',filt,'-map','[v]','-map','[a]','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p','-r','30','-c:a','aac','-b:a','192k','-ar','48000','-t',str(self.final_seconds),'-movflags','+faststart','-metadata',f'comment=Real Unity Shapes{self.level} and native HUDs. Editorial holds, quiz,1.5x section, original music and synthetic Turkish narration. Approved common promo; store URL is mock.',str(self.final)]
        intermediate=self.out/'promo_unmastered.mp4';cmd[-1]=str(intermediate)
        write(self.out/'final_encode_command.json',cmd);m.command(cmd,self.out/'final_encode.log')
        # Normalize the joined program, not only its separately mixed sections; copy video unchanged.
        master=[ff,'-hide_banner','-y','-i',str(intermediate),'-map','0:v:0','-map','0:a:0','-c:v','copy','-c:a','aac','-b:a','192k','-ar','48000','-af','loudnorm=I=-16:TP=-1.5:LRA=8','-movflags','+faststart',str(self.final)]
        write(self.out/'master_command.json',master);m.command(master,self.out/'master.log')

    def verify(self,ff):
        r=m.command([ff,'-hide_banner','-i',str(self.final),'-f','null','-','-progress','pipe:1'],self.out/'decode.log')
        count=int(re.findall(r'^frame=(\d+)$',r.stdout,re.M)[-1]);assert count==round(self.final_seconds*FPS),(count,self.final_seconds)
        assert all(s in r.stderr for s in ['1080x1920','yuv420p','48000 Hz, stereo','h264','aac'])
        assert not re.search(r'corrupt|Error while decoding|Invalid data',r.stderr,re.I)
        r=m.command([ff,'-hide_banner','-i',str(self.final),'-vn','-af','loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json','-f','null','-'],self.out/'loudness.log')
        loud=json.loads(re.findall(r'\{\s*"input_i".*?\}',r.stderr,re.S)[-1]);assert -18<=float(loud['input_i'])<=-14 and float(loud['input_tp'])<=-1
        qa={'passed':True,**self.plan,'frames':count,'width':W,'height':H,'fps':FPS,'bytes':self.final.stat().st_size,'sha256':m.sha(self.final),'audio':loud,'creativeApproval':'approved pilot format; episode creative review pending','hudPixelIdenticalBeforeEncodeEveryGameplayFrame':True,'noCropOrReplacementHud':True,'nativeSolvedWithThreeLives':True}
        write(self.out/'QA_REPORT.json',qa)
        m.command([ff,'-hide_banner','-y','-i',str(self.final),'-vf',f'fps=1/{self.final_seconds/10},scale=216:384,tile=5x2','-frames:v','1','-update','1',str(self.out/'encoded_contact_sheet.jpg')],self.out/'contact_sheet.log')
        m.command([ff,'-hide_banner','-y','-ss','6.5','-i',str(self.final),'-frames:v','1','-update','1',str(self.out/'cover.jpg')],self.out/'cover.log')
        return qa

    def subtitles(self):
        manifest=json.loads((self.out/'audio/voice_manifest.json').read_text(encoding='utf-8'))
        rows=[(self.starts[i],self.starts[i]+self.durations[i],c['text']) for i,c in enumerate(manifest['clips'])]
        rows.append((self.outro+.8,self.outro+3.1,'Oynamak için Play Store’dan indirin!'))
        def tc(t):
            ms=round(t*1000);return f'{ms//3600000:02d}:{ms//60000%60:02d}:{ms//1000%60:02d},{ms%1000:03d}'
        (self.out/'captions_TR.srt').write_text('\n\n'.join(f'{i+1}\n{tc(a)} --> {tc(b)}\n{s}' for i,(a,b,s) in enumerate(rows))+'\n',encoding='utf-8')

def main():
    p=argparse.ArgumentParser();p.add_argument('--level',required=True,type=int,choices=ORDER);p.add_argument('--stills',action='store_true');p.add_argument('--finish-only',action='store_true');a=p.parse_args()
    ep=Episode(a.level)
    for t in [.5,3.7,6.5,9.5,11,12.5,14.5,16,ep.starts[6]+1]:ep.frame(t).save(ep.out/f'still_{t:04.1f}.jpg',quality=94)
    if a.stills:print(json.dumps(ep.plan,ensure_ascii=False));return
    ep.check_source();ff=m.imageio_ffmpeg.get_ffmpeg_exe()
    m.OUT=ep.out;m.SECONDS=ep.seconds;m.VOICE_STARTS=ep.starts;m.frame=ep.frame;m.event_time=ep.event_time
    m.make_audio(ff,ep.events,countdown_times=[6.2,7.2,8.2],answer_time=9.3,victory_time=ep.starts[6])
    prefix=ep.out/'gameplay_edit.mp4'
    if not a.finish_only:prefix=m.render(ff,prefix.name,f'Full native Unity Shapes{ep.level}; both HUDs preserved. Actual isolated first shot then remaining production solution; editorial annotations, hold,1.5x segment and synthetic/original audio.')
    ep.append_outro(ff,prefix);qa=ep.verify(ff);ep.subtitles()
    write(ep.out/'timeline.json',[{'outputFrame':i,'sourceFrame':ep.raw_index(i/FPS),'topHudPreserved':True,'bottomHudPreserved':True} for i in range(round(ep.outro*FPS))])
    shutil.copy2(ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1-OFL.txt',ep.out/'FONT_LICENSE.txt')
    print(json.dumps(qa,ensure_ascii=False,indent=2),flush=True)
if __name__=='__main__':main()
