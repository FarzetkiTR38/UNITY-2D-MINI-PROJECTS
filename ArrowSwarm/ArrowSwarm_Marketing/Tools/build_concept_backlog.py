"""Twelve distinct original briefs, each with an explicit genuine source witness."""
from pathlib import Path
import argparse,asyncio,csv,json,sys,wave
import build_remaining_videos as b
import master_remaining_audio as mastering
from PIL import Image,ImageDraw
ROOT=b.ROOT; PACKAGE=b.m.MEDIA/'05_Reels/ConceptBacklog_20261117'
RAW=b.RAW; ORIGINAL_NATIVE=b.native; ORIGINAL_AUDIO=b.audio
SOURCES=dict(b.SOURCES)
SOURCE_CASES={'heart':'Heart','advanced':'Advanced','budget':'Budget','match':'Match','tutorial':'Tutorial','wrong':'Wrong','menu':'Menu'}
LABELS={'heart':'ŞEKİLLER','advanced':'ŞEKİLLER50','budget':'RENK DEĞİŞİMİ','match':'RENK EŞLEŞMESİ','tutorial':'İLK BÖLÜM','wrong':'SAVUNMADA HATA','menu':'ÜÇ MOD'}
def prepare_sources():
    p=PACKAGE/'NativeEvidence/capture_batch.csv'
    rows=list(csv.DictReader(p.open(encoding='utf-8-sig'))) if p.exists() else []
    for key,case in SOURCE_CASES.items():
        row=next((x for x in reversed(rows) if x['scenario']==case and x['status']=='PASS'),None)
        if row:SOURCES[key]=ROOT/row['source']
    b.SOURCES=SOURCES;b.TITLES.update(LABELS)
def three(key,start,speed,titles,voices):
    return [b.seg(key,start+i*5*speed,5,titles[i],[],voices[i],speed) for i in range(3)]
def concepts():
    return [
    ('R01','Kalp · Önce hangi ok?',three('heart',0,1,['KALBİ NEREDEN AÇARSIN?','ÖNCE AÇIK YOL','SIRAYI TAKİP ET'],['Bu kalbi hangi okla açarsın?','Önce yolu açık olan oku bul.','Oklar çıktıkça yeni yollar açılır.'])),
    ('R02','Bir ok, yeni bir yol',three('shapes',0,4/3,['BİR OK. YENİ BİR YOL.','ZİNCİRİ DEVAM ETTİR','SIRAYLA TEMİZLE'],['Bir ok çıkar. Yeni bir yol açılır.','Önce açık yolu bul, sonra zinciri devam ettir.','Bütün okları sırayla temizle.'])),
    ('R03','Kapalı yol neden geri döner?',[b.seg('fail',1,10,'KAPALI YOL GERİ DÖNER',[],'Önünde engel varsa ok çarpar ve geri döner. Yanlış hamle canını azaltır.',.5),b.seg('shapes',0,5,'AYRI ÖRNEK: AÇIK YOL',[],'Bu ayrı örnekte ise yol açık. Önce kontrol et.')]),
    ('R04','Okları değil, çerçeveyi döndür',three('color',0,.8,['OKLARI DEĞİL...','ÇERÇEVEYİ DÖNDÜR','YENİ ÇIKIŞI BUL'],['Okları değil, çerçeveyi döndür.','Çıkışın rengini ve açık yolu birlikte düşün.','Dönüşten sonra kalan okları gönder.'])),
    ('R05','Çıkış rengini eşleştir',[b.seg('match',0,2,'RENK EŞLEŞMESİ',[],'Önce çıkışa bak.'),b.seg('match',2,4,'YANLIŞ RENK GERİ DÖNER',[],'Yanlış renk, oku geri döndürür.'),b.seg('match',6,9,'DOĞRU ÇIKIŞI SEÇ',[],'Rengi eşleştir, açık yolu kullan.')]),
    ('R06','Gerçek son gökkuşağı oku',three('freeze',18.3,1,['GERÇEK SON OKU BEKLE','GÖKKUŞAĞI SAHNEDE','GERÇEK SONUÇ'],['Gerçek son oku bekle.','Gökkuşağı oku düşmanlara doğru ilerliyor.','Bu denemede bütün düşmanlar temizlendi.'])),
    ('R07','İlk bölümde ilk açık yol',three('tutorial',0,1,['İLK BÖLÜM','GÖSTERİLEN OKA DOKUN','YOLU AÇ'],['İlk bölüm, ilk açık yol.','Oyun serbest oku sana gösteriyor.','Gösterilen oku gönder ve yolu aç.'])),
    ('R08','50. seviye · Kıvrımları takip et',three('advanced',0,5/3,['BÖLÜM50: YOLU BUL','KIVRIMLARI TAKİP ET','ACELE ETMEDEN DÜŞÜN'],['Elli numaralı bölümde yolu bulabilir misin?','Dokunmadan önce kıvrımları takip et.','Önü açık oklarla yeni yollar oluştur.'])),
    ('R09','Savunmada üç farklı yanlış hamle',three('wrong',0,.4,['BU OK NEDEN DÖNÜYOR?','FARKLI ÜÇ YANLIŞ HAMLE','CANLARINA DİKKAT ET'],['Savunmada bu ok neden geri dönüyor?','Üç farklı kapalı ok, üç yanlış hamle.','Oklar geri döndü. Canlar tükendi.'])),
    ('R10','Her dönüş bir karar',three('budget',0,5/3,['HER DÖNÜŞ BİR KARAR','HAKKINI PLANLA','YENİ EŞLEŞMEYİ BUL'],['Her dönüş bir karar.','Dönüş hakkın sınırlı. Rengi ve yolu birlikte düşün.','Çerçeveyi doğru anda döndür ve devam et.'])),
    ('R11','Üç mod ve gerçek menü kontrolleri',three('menu',0,4/3,['ÜÇ FARKLI SONRAKİ HAMLE','MODUNU SEÇ','TEMANI SEÇ'],['Üç farklı oyun modu seni bekliyor.','Savunma, şekiller ve renk değişimi. Modunu seç.','Ayarlardan açık veya koyu temanı seçebilirsin.'])),
    ('R12','İki ayrı deneme, iki gerçek sonuç',[b.seg('defense',15.2,7.5,'İLK DENEME: KAZANÇ',[],'Bu denemede savunma tamamlandı. Sonuç, gerçek oyun kaydı.'),b.seg('wrong',0,7.5,'AYRI DENEME: KAYIP',[],'Bu ayrı denemede ise yanlış hamlelerle canlar bitti. İki ayrı oyun, iki gerçek sonuç.',.8)])]
def native(s,name,portrait=False,i=0,total=1):
    source=SOURCES[s['key']];info=b.m.read(source/'provenance.json');frame=round(s['start']*30)
    assert s['start']+s['seconds']*s['speed']<=info['frames']/30+.04,(name,s,info['frames'])
    if not portrait:return ORIGINAL_NATIVE(s,name,portrait,i,total)
    args=[b.FF,'-y','-framerate','30','-start_number',str(frame),'-i',str(source/'frame_%05d.jpg')]
    vf=f'setpts={1/s["speed"]:.9f}*(PTS-STARTPTS),fps=30,tpad=stop_mode=clone:stop_duration=0.1'
    if s['key']=='menu':
        layer=b.m.gradient(1080,1920);b.m.text(layer,s['title'],(540,76),50,anchor='mm')
        if s['speed']!=1:b.m.text(layer,b.rate_label(s['speed']),(540,128),26,fill='#ffdc58',anchor='mm')
        p=b.m.save(layer,name+'_plate.png');args+=['-loop','1','-i',str(p)]
        filt=f'[0:v]{vf},scale=990:1760,setsar=1[p];[1:v][p]overlay=45:160:shortest=1,format=yuv420p[v]'
    else:
        layer=Image.new('RGBA',(1080,1920));d=ImageDraw.Draw(layer)
        d.rounded_rectangle((65,235,1015,365),radius=32,fill='#0c214bed',outline='#66e5ff',width=3)
        size=58
        while b.m.font(size).getlength(s['title'])>900:size-=2
        b.m.text(layer,s['title'],(540,300),size,anchor='mm')
        if s['speed']!=1:b.m.text(layer,b.rate_label(s['speed']),(540,400),32,fill='#ffdc58',anchor='mm')
        overlay=b.m.save(layer,name+'_plate.png');args+=['-loop','1','-i',str(overlay)]
        events=list(csv.DictReader((source/'events.csv').open(encoding='utf-8-sig')))
        terminal=next((float(e['time_seconds']) for e in events if e['event'] in ('won','lost') or e['event']=='returned' and e.get('lives')=='0'),info['frames']/30)
        until=max(0,min(10,(terminal-s['start'])/s['speed']-.25))
        filt=f"[0:v]{vf}[p];[p][1:v]overlay=shortest=1:enable='lt(t,{until:.5f})',format=yuv420p[v]"
    p=b.OUT/(name+'.mp4');b.m.run(args+['-filter_complex',filt,'-map','[v]','-frames:v',str(round(s['seconds']*30)),'-r','30','-an','-c:v','libx264','-preset','veryfast','-crf','19','-threads','2',str(p)],name);return p
async def render(ready_only):
    prepare_sources();all_rows=[];missing=[]
    for ident,title,items in concepts():
        if any(s['key'] not in SOURCES for s in items):missing.append(ident);continue
        folder=PACKAGE/ident;folder.mkdir(parents=True,exist_ok=True);b.OUT=folder;b.m.OUT=folder;b.TUTORIAL=items
        final=folder/f'ArrowSwarm_{ident}_TR_18s.mp4';manifest=folder/'manifest.json'
        if manifest.exists():
            existing=b.m.read(manifest)['items'][0]
            unchanged=[{k:v for k,v in s.items() if k not in ('at','chapter')} for s in existing['timeline']]==items
            if unchanged and final.exists() and existing['qa']['sha256']==b.m.sha(final):all_rows.append({'id':ident,**existing});continue
        voice_manifest=folder/'Voice/manifest.json'
        if voice_manifest.exists():
            old={x['chapter']:x['text'] for x in b.m.read(voice_manifest)['clips']}
            for i,s in enumerate(items):
                if old.get(i)!=s['voice']:
                    for suffix in ('.mp3','.wav','_fit.wav'):
                        (folder/'Voice'/f'{i:02}{suffix}').unlink(missing_ok=True)
        await b.voices();b.native=native
        result=b.build(final.stem,title,items,18,portrait=True,narrated=True)
        proof={key:{'path':str(SOURCES[key].relative_to(ROOT)),'provenanceSha256':b.m.sha(SOURCES[key]/'provenance.json'),'eventsSha256':b.m.sha(SOURCES[key]/'events.csv')} for key in {s['key'] for s in items}}
        b.dump(manifest,{'items':[result],'sources':proof});mastering.main();result=b.m.read(manifest)['items'][0]
        all_rows.append({'id':ident,**result})
    b.dump(PACKAGE/'manifest.json',{'date':'2026-11-18','count':len(all_rows),'complete':len(all_rows)==12,'items':all_rows,'missing':missing,'language':'tr','sourcesAreNative':True,'audioIsEditorial':True,'published':False})
    print(json.dumps({'rendered':len(all_rows),'missing':missing}),flush=True)
    if missing and not ready_only:raise RuntimeError('Missing genuine capture witnesses: '+','.join(missing))
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--ready-only',action='store_true');args=parser.parse_args();asyncio.run(render(args.ready_only))
