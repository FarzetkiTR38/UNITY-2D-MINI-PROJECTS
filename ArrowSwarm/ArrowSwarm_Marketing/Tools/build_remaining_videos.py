"""Finish the five planned marketing edits using verified native source frames only."""
from pathlib import Path
import asyncio,csv,hashlib,json,math,os,subprocess,sys,wave
import build_store_launch_videos as m
from reference_pilot_voice import trim_to_wav
import edge_tts
import numpy as np
from PIL import Image,ImageDraw,ImageOps
ROOT=m.ROOT; OUT=m.MEDIA/'06_Trailers_Ads_Tutorials/RemainingVideos_20261117';OUT.mkdir(parents=True,exist_ok=True)
m.OUT=OUT;FF=m.FF;SR=48000
RAW=m.MEDIA/'04_Raw_Gameplay'
SOURCES={'shapes':RAW/'Shapes_L2_20261113_081526_913','fail':RAW/'ShapesFail_L2_20261115_184919_828','color':RAW/'ColorShift_L3_20261115_184112_468','defense':RAW/'DefenseSeries_L3_20261115_120226_359','tip':RAW/'DefenseSeries_L8_20261115_120336_892','freeze':RAW/'DefenseR2_L10_20261115_002652_928'}
TITLES={'shapes':'ŞEKİLLER','fail':'YANLIŞ HAMLE','color':'RENK DEĞİŞİMİ','defense':'SAVUNMA','tip':'İPUCU','freeze':'DONDURMA'}
def dump(p,value):p.write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def seg(key,start,length,title,lines,voice='',speed=1):return {'key':key,'start':start,'seconds':length,'title':title,'lines':lines,'voice':voice,'speed':speed}
def rate_label(speed):return f'{speed:.2f}'.rstrip('0').rstrip('.').replace('.',',')+'× oynanış'
TUTORIAL=[
seg('shapes',0,8,'YOLU KONTROL ET',['Önündeki yol açıksa','oku dışarı gönder.'],'Önce okun baktığı yönü incele. Önündeki yol açıksa, oka dokunarak onu dışarı gönderebilirsin.'),
seg('fail',1,8,'ENGEL VARSA GERİ DÖNER',['Yanlış atışı ve geri dönüşü izle.','Bu bölüm yarı hızda.'],'Önünde bir engel varsa ok çarpar ve geri döner. Böyle bir hamlede bir canını kaybedersin.',.5),
seg('fail',5,6,'ÜÇ CANINA DİKKAT ET',['Kaybedince Tekrar Dene','ile yeniden başlayabilirsin.'],'Canların bittiğinde oyun sona erer. Tekrar dene düğmesiyle bölüme yeniden başlayabilirsin.'),
seg('shapes',8,10,'ŞEKLİ TEMİZLE',['Açılan yolları sırayla kullan.','Bütün okları dışarı gönder.'],'Şekiller modunda, açık yolları sırayla kullan. Oklar çıktıkça yeni yollar açılır. Hedef, bütün okları temizlemek.'),
seg('color',0,6,'RENKLERİ EŞLEŞTİR',['Okun çıkacağı kenara bak.','Renkler de önemlidir.'],'Renk değişimi modunda yalnızca boş yol yetmez. Okun çıkacağı kenarın rengine de dikkat et.'),
seg('color',6,6,'ÇERÇEVEYİ DÖNDÜR',['Gerçek dönüşü izle.','Yeni eşleşmeyle devam et.'],'Yol uygun değilse çerçeveyi döndür. Renkleri yeniden eşleştir ve kalan okları gönder.'),
seg('defense',10,11,'SAVUNMADA ZAMANLA',['Düşmanlar ilerliyor.','Atış sırası ve zamanlama önemli.'],'Savunmada düşmanlar ilerlerken oklarını kullanırsın. Yolu açık okları seç ve atışlarını düşmanlara göre zamanla.'),
seg('tip',8,9,'İPUCU YOL GÖSTERİR',['Önce vurgulanan oku izle.','Sonra o oku gönder.'],'İlk oku bulamıyorsan ipucu kullanabilirsin. Oyun, yolu açık bir oku vurgular. Ardından gösterilen oka dokun.'),
seg('freeze',17,8,'DONDURMA ZAMAN KAZANDIRIR',['Gerçek Dondur kullanımını izle.','Düşmanlar dururken sıranı kur.'],'Düşmanlar yaklaşırken dondurma becerisi zaman kazandırır. Kısa duraklamayı kullanarak atışlarını sıraya koy.'),
seg('freeze',25,7,'SON ATIŞI TAMAMLA',['Son düşman ve gerçek sonuç.','HUD ve yıldızlar korunuyor.'],'Son atışı tamamla ve sonucu gör. Bu kayıtta bütün düşmanlar temizleniyor ve bölüm başarıyla bitiyor.'),
seg('shapes',2,2,'ŞEKİLLER',['Yolları aç.','Şekli temizle.'],'Şekillerde yolu aç.'),
seg('color',8,2,'RENK DEĞİŞİMİ',['Renkleri eşleştir.','Çerçeveyi döndür.'],'Renkleri eşleştir.'),
seg('defense',15,2,'SAVUNMA',['Atışını zamanla.','Savunmanı tamamla.'],'Savunmanı tamamla.')]
assert sum(s['seconds'] for s in TUTORIAL)==85
async def voices():
    folder=OUT/'Voice';folder.mkdir(exist_ok=True);sem=asyncio.Semaphore(3);rows=[]
    async def one(i,s):
        raw=folder/f'{i:02}.mp3';wav=folder/f'{i:02}.wav'
        async with sem:
            if not wav.exists():
                if not raw.exists():await asyncio.wait_for(edge_tts.Communicate(s['voice'],'tr-TR-AhmetNeural',rate='+8%').save(str(raw)),40)
                trim_to_wav(raw,wav,FF)
        with wave.open(str(wav)) as w:dur=w.getnframes()/w.getframerate()
        tempo=1
        if dur>s['seconds']-.08:
            tempo=dur/(s['seconds']-.12)
            if tempo>1.2:raise RuntimeError(f'Voice {i} needs shorter copy: {dur}/{s["seconds"]}')
            fit=folder/f'{i:02}_fit.wav'
            m.run([FF,'-y','-i',str(wav),'-af',f'atempo={tempo:.8f}','-ar',str(SR),'-ac','1','-c:a','pcm_s16le',str(fit)],f'voice_fit_{i}')
            fit.replace(wav)
            with wave.open(str(wav)) as w:dur=w.getnframes()/w.getframerate()
        rows.append({'chapter':i,'text':s['voice'],'file':wav.name,'durationSeconds':dur,'fitTempo':tempo,'sha256':m.sha(wav)})
    await asyncio.gather(*(one(i,s) for i,s in enumerate(TUTORIAL)))
    dump(folder/'manifest.json',{'provider':'Microsoft speech via existing edge-tts','voice':'tr-TR-AhmetNeural','rate':'+8%','onlyPublicEditorialSentencesSent':True,'clips':sorted(rows,key=lambda x:x['chapter'])})
def plate(s,i,total):
    im=m.gradient(1920,1080);icon,mask=m.round_icon(260);im.paste(icon,(190,100),mask)
    m.text(im,'ARROW',(320,450),82,anchor='mm');m.text(im,'SWARM',(320,540),82,fill='#ffdc58',anchor='mm')
    m.text(im,'BAŞLANGIÇ REHBERİ' if total>5 else 'ÜÇ MOD. TEK OYUN.',(320,710),42,fill='#87e6ff',anchor='mm')
    m.text(im,TITLES[s['key']],(320,820),40,anchor='mm')
    words=s['title'].split();lines=[];line=''
    for word in words:
        if len(line+' '+word)>21:lines.append(line);line=word
        else:line=(line+' '+word).strip()
    if line:lines.append(line)
    for j,line in enumerate(lines):m.text(im,line,(1590,310+j*74),57,fill='#85e6ff' if j==0 else '#fff7dc',anchor='mm')
    for j,line in enumerate(s['lines']):m.text(im,line,(1590,610+j*60),36,anchor='mm',stroke=0)
    if s['speed']!=1:m.text(im,rate_label(s['speed']),(1590,850),32,fill='#ffdc58',anchor='mm')
    m.text(im,f'{i+1:02} / {total:02}',(1590,975),31,fill='#95aacc',anchor='mm',stroke=0)
    return m.save(im,f'plate_{total}_{i}.png')
def native(s,name,portrait=False,i=0,total=1):
    source=SOURCES[s['key']];info=m.read(source/'provenance.json');maxsec=info['frames']/30
    assert s['start']+s['seconds']*s['speed']<=maxsec+.04,(name,maxsec,s)
    first=round(s['start']*30);frames=round(s['seconds']*30)
    assert (source/f'frame_{first:05}.jpg').exists()
    args=[FF,'-y','-framerate','30','-start_number',str(first),'-i',str(source/'frame_%05d.jpg')]
    vf=f'setpts={1/s["speed"]:.8f}*(PTS-STARTPTS),fps=30,tpad=stop_mode=clone:stop_duration=0.1'
    if portrait:
        layer=Image.new('RGBA',(1080,1920));d=ImageDraw.Draw(layer);d.rounded_rectangle((90,235,990,365),radius=32,fill='#0c214bed',outline='#66e5ff',width=3)
        m.text(layer,s['title'],(540,300),64,anchor='mm')
        if s['speed']!=1:m.text(layer,rate_label(s['speed']),(540,400),32,fill='#ffdc58',anchor='mm')
        overlay=m.save(layer,name+'_plate.png');args+=['-loop','1','-i',str(overlay)]
        events=list(csv.DictReader((source/'events.csv').open(encoding='utf-8-sig')))
        won=next((float(e['time_seconds']) for e in events if e['event']=='won'),maxsec)
        until=max(0,min(10,(won-s['start'])/s['speed']-.25))
        filt=f"[0:v]{vf}[p];[p][1:v]overlay=shortest=1:enable='lt(t,{until:.5f})',format=yuv420p[v]"
    else:
        bg=plate(s,i,total);args+=['-loop','1','-i',str(bg)]
        filt=f'[0:v]{vf},scale=608:1080,setsar=1[p];[1:v][p]overlay=656:0:shortest=1,format=yuv420p[v]'
    p=OUT/(name+'.mp4');m.run(args+['-filter_complex',filt,'-map','[v]','-frames:v',str(frames),'-r','30','-an','-c:v','libx264','-preset','veryfast','-crf','19','-threads','3',str(p)],name)
    return p
def brand(portrait=False):
    im=m.gradient(1080,1920) if portrait else m.gradient(1920,1080)
    if portrait:
        icon,mask=m.round_icon(500);im.paste(icon,(290,230),mask)
        m.text(im,'ARROW',(540,930),130,anchor='mm');m.text(im,'SWARM',(540,1070),130,fill='#ffdc58',anchor='mm')
        m.text(im,'ŞİMDİ SIRA SENDE.',(540,1360),65,anchor='mm')
    else:
        f=ImageOps.contain(Image.open(m.MEDIA/'03_GooglePlay/feature_graphic_final_TR_1024x500.png'),(1800,880),Image.Resampling.LANCZOS);im.paste(f,((1920-f.width)//2,35));m.text(im,'ŞİMDİ SIRA SENDE.',(960,1000),65,anchor='mm')
    return m.save(im,'brand_portrait.png' if portrait else 'brand_landscape.png')
def audio(seconds,segments,narrated=False):
    n=round(seconds*SR);music=m.pcm(ROOT/'Assets/_Project/Audio/Music/GameBGM_alt1.ogg');music=np.tile(music,(math.ceil(n/len(music)),1))[:n].copy();music*=.027/max(.001,float(np.sqrt(np.mean(music**2))));out=music
    def add(data,t,gain):
        start=round(t*SR)
        if start<0 or start>=n:return
        data=data*gain/max(.001,float(abs(data).max()));count=min(len(data),n-start);out[start:start+count]+=data[:count]
    for item in segments:
        if narrated and 'chapter' in item:add(m.pcm(OUT/'Voice'/f'{item["chapter"]:02}.wav'),item['at']+.05,.64)
        if 'key' not in item:continue
        for e in csv.DictReader((SOURCES[item['key']]/'events.csv').open(encoding='utf-8-sig')):
            t=(float(e['time_seconds'])-item['start'])/item['speed']+item['at']
            if not item['at']<=t<item['at']+item['seconds']:continue
            clip={'fire':'ArrowFire.wav','rainbow_fire':'RainbowArrow.ogg','wrong_fire':'ArrowFire.wav','collision':'ArrowWrong.wav','lost':'LevelLose.ogg','won':'LevelWin.ogg','rotate':'ButtonClick_alt2.wav','tip':'SkillTips.ogg','freeze':'SkillFreeze.ogg'}.get(e['event'])
            if clip:add(m.pcm(ROOT/'Assets/_Project/Audio/SFX'/clip),t,.045 if narrated else .09)
    fade=np.minimum(np.arange(n)/(SR*.08),1)*np.minimum((n-1-np.arange(n))/(SR*.3),1);out*=fade[:,None];peak=float(abs(out).max())
    if peak>.87:out*=.87/peak
    p=OUT/'mix.wav'
    with wave.open(str(p),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(SR);w.writeframes((out*32767).astype('<i2').tobytes())
    return p
def build(name,title,items,seconds,portrait=False,intro=0,narrated=False):
    print('RENDER',name,flush=True);parts=[];timeline=[];at=0
    if intro:parts.append(m.still_segment(m.splash_frame(),intro,name+'_splash'));at=intro
    for i,s in enumerate(items):
        parts.append(native(s,name+f'_part{i}',portrait,i,len(items)));timeline.append(dict(s,at=at,chapter=i));at+=s['seconds']
    outro=round(seconds-at,5);assert outro>0
    parts.append(m.still_segment(brand(portrait),outro,name+'_end',1080 if portrait else 1920,1920 if portrait else 1080))
    dest=OUT/(name+'.mp4');m.concatenate(parts,audio(seconds,timeline,narrated),dest,seconds)
    qa=m.verify(dest,round(seconds*30),1080 if portrait else 1920,1920 if portrait else 1080)
    assert qa['fps']==30 and abs(qa['durationSeconds']-seconds)<.04
    m.contact(dest,seconds,name+'_contact.jpg');m.run([FF,'-y','-ss','3','-i',str(dest),'-frames:v','1','-vf','scale=960:-1','-q:v','2',str(OUT/(name+'_cover.jpg'))],name+'_cover')
    m.srt([(s['at'],s['at']+s['seconds'],s['voice'] if narrated else s['title']) for s in timeline]+[(seconds-outro,seconds,'ARROW SWARM — Şimdi sıra sende.')],OUT/(name+'.srt'))
    meta={'title':title,'file':dest.name,'durationSeconds':seconds,'portrait':portrait,'narration':narrated,'editorialLanguage':'tr','nativeHUDsPreserved':True,'timeline':timeline,'qa':qa};dump(OUT/(name+'_manifest.json'),meta);print('PASS',name,qa['frames'],flush=True);return meta
async def main():
    import argparse
    p=argparse.ArgumentParser();p.add_argument('--only',choices=['voice','shorts','tutorial','all'],default='all');args=p.parse_args()
    if args.only in ['voice','tutorial','all']:await voices()
    if args.only=='voice':return
    rows=[]
    if args.only in ['shorts','all']:
        compare=[seg('color',6.5,5.5,'RENKLERİ EŞLEŞTİR',['Çerçeveyi döndür.','Yeni çıkışı bul.']),seg('defense',12,5.5,'ATIŞINI ZAMANLA',['Düşmanlar ilerliyor.','Açık oklarla savun.']),seg('shapes',2,5.5,'ŞEKLİ TEMİZLE',['Önce açık yollar.','Sonra kalan oklar.'])]
        rows.append(build('ArrowSwarm_ModeComparison_TR_20s','Üç mod · 20 saniyelik karşılaştırma',compare,20))
        for key,start,speed in [('defense',7.7,1),('shapes',0,4/3),('color',0,.8)]:
            s=seg(key,start,15,TITLES[key],['Gerçek oyun kaydı.','İki HUD da korunuyor.'],speed=speed)
            rows.append(build('ArrowSwarm_'+key.title()+'_TR_18s',TITLES[key]+' · 18 saniye',[s],18,portrait=True))
    if args.only in ['tutorial','all']:rows.append(build('ArrowSwarm_BeginnerGuide_TR_90s','Başlangıç rehberi · Türkçe anlatımlı',TUTORIAL,90,intro=2,narrated=True))
    old=m.read(OUT/'manifest.json')['items'] if (OUT/'manifest.json').exists() else [];byname={r['file']:r for r in old};byname.update({r['file']:r for r in rows})
    proof={k:{'path':str(v.relative_to(ROOT)),'provenanceSha256':m.sha(v/'provenance.json'),'eventsSha256':m.sha(v/'events.csv'),'nativeLanguage':m.read(v/'provenance.json').get('language','en')} for k,v in SOURCES.items()}
    dump(OUT/'manifest.json',{'date':'2026-11-17','items':list(byname.values()),'sources':proof,'audio':'Editorial project music/SFX and Turkish speech, not native AudioListener recording','noNewGameplayRecording':True,'published':False,'storeURL':None})
    print('DONE',len(byname),flush=True)
if __name__=='__main__':asyncio.run(main())
