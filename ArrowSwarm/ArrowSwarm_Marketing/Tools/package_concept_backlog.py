"""Build the twelve-film review, a final-only ZIP and the complete existing video library."""
import argparse,csv,html,json,os,re,zipfile
from pathlib import Path
from urllib.parse import quote
import build_concept_backlog as c
b=c.b;P=c.PACKAGE;LIB=b.m.MEDIA/'Final_Deliverables/VideoLibrary'
CSS='''*{box-sizing:border-box}html{scroll-behavior:smooth}body{margin:0;background:#091224;color:#edf5ff;font:16px/1.55 system-ui,sans-serif}a{color:#8fe9ff}button,a.pill{font:inherit;cursor:pointer;border:1px solid #496383;background:#14263e;color:#e9f6ff;border-radius:999px;padding:10px 17px;text-decoration:none;display:inline-flex;align-items:center;gap:7px}button:hover,a.pill:hover{background:#244569}button:focus-visible,a:focus-visible{outline:3px solid #ffda5b;outline-offset:3px}button.active,.pill.primary{background:#9fefff;color:#092039;border-color:#9fefff}.wrap{max-width:1240px;padding:28px;margin:auto}nav{display:flex;align-items:center;justify-content:space-between;gap:12px;flex-wrap:wrap}.eyebrow{color:#81d6e9;font-size:12px;letter-spacing:2px;font-weight:800}h1{font-size:clamp(35px,6vw,68px);line-height:1.03;letter-spacing:-2px;margin:25px 0 18px}h1 em{font-style:normal;color:#ffda5b}h2{font-size:24px;line-height:1.2}p{color:#adbed6;max-width:800px}.hero{display:flex;justify-content:space-between;gap:24px;padding:36px 0 22px}.hero img{width:148px;height:148px;border-radius:30px;align-self:center}.actions,.filters{display:flex;gap:9px;flex-wrap:wrap}.stats{display:flex;gap:24px;flex-wrap:wrap;margin:24px 0;color:#b7cae2}.stats strong{font-size:26px;color:white}.view{display:grid;grid-template-columns:minmax(220px,390px) 1fr;gap:38px;background:#111f33;padding:24px;border:1px solid #28445d;border-radius:24px;margin:22px 0 36px;scroll-margin-top:20px}.screen{border-radius:16px;background:#030812;overflow:hidden;display:flex;justify-content:center}.screen video{display:block;width:100%;max-height:690px;object-fit:contain}.details{padding:14px 0;min-width:0}.details h2{font-size:34px;margin:8px 0 18px;overflow-wrap:anywhere}.tag{font-size:12px;padding:5px 9px;border-radius:8px;background:#203951;color:#a6e9ff;display:inline-block;margin:0 5px 6px 0}.grid{display:grid;grid-template-columns:repeat(4,1fr);gap:18px;margin-top:20px}.card{padding:0;border-radius:18px;border:1px solid #2e415c;background:#111f33;display:block;text-align:left;overflow:hidden;color:#eff6ff;min-width:0;width:100%}.card img{display:block;width:100%;height:245px;object-fit:cover;object-position:center 20%;background:#122443}.card .caption{display:block;padding:14px}.card strong{display:block;line-height:1.3;font-size:17px}.card small{display:block;color:#acc6df;margin-top:8px}.card.active{outline:3px solid #8beaff;background:#19344b;color:white}.note{font-size:13px;color:#9eb2cb}details{margin:24px 0;background:#122037;padding:16px;border-radius:12px}summary{cursor:pointer;font-weight:700}.links{display:flex;gap:15px;flex-wrap:wrap;margin:16px 0}footer{margin-top:48px;border-top:1px solid #284056;padding:22px 0;color:#91a6c0;font-size:13px}#empty{padding:20px}input{border:1px solid #55718b;background:#102038;color:white;border-radius:12px;padding:12px 16px;font:inherit;width:min(100%,360px)}@media(max-width:850px){.grid{grid-template-columns:repeat(3,1fr)}.view{gap:24px;grid-template-columns:minmax(180px,300px) 1fr}.hero img{width:100px;height:100px}.details h2{font-size:26px}}@media(max-width:600px){.wrap{padding:18px}.hero{padding-top:20px}.hero img{display:none}.view{display:block;padding:14px}.screen video{max-height:65vh}.details h2{font-size:26px}.grid{grid-template-columns:repeat(2,minmax(0,1fr));gap:12px}.card img{height:205px}.card .caption{padding:11px}.card strong{font-size:15px}.stats{gap:18px}.details{padding:22px 2px 5px}}'''

JS='''const data=DATA;let selected=0;const $=s=>document.querySelector(s);const player=$('#player');const cards=$('#cards');let filter='Tümü';function esc(s){return s.replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]))}function show(i,play=false){selected=i;const x=data[i];player.pause();player.src=x.src;player.poster=x.poster;player.querySelectorAll('track').forEach(t=>t.remove());if(x.vtt){const t=document.createElement('track');t.kind='subtitles';t.srclang='tr';t.label='Türkçe';t.src=x.vtt;player.append(t)}$('#videoTitle').textContent=x.title;$('#videoMeta').textContent=x.meta;$('#videoNote').textContent=x.note;$('#download').href=x.src;$('#download').download=x.src.split('/').pop();$('#subtitles').hidden=!x.srt;$('#subtitles').href=x.srt||'#';$('#contact').hidden=!x.contact;$('#contact').href=x.contact||'#';$('#position').textContent=`${i+1} / ${data.length}`;document.querySelectorAll('.card').forEach(c=>c.classList.toggle('active',+c.dataset.i===i));if(play){player.play().catch(()=>{});$('#viewer').scrollIntoView({behavior:'smooth',block:'start'})}}function draw(){const q=$('#search')?.value.toLocaleLowerCase('tr')||'';cards.innerHTML='';data.forEach((x,i)=>{if(filter!=='Tümü'&&x.group!==filter)return;if(!`${x.title} ${x.group} ${x.meta}`.toLocaleLowerCase('tr').includes(q))return;const c=document.createElement('button');c.className='card'+(i===selected?' active':'');c.dataset.i=i;c.setAttribute('aria-label',x.title+' videosunu izle');c.innerHTML=`<img loading="lazy" src="${esc(x.poster)}" alt=""><span class="caption"><span class="tag">${esc(x.group)}</span><strong>${esc(x.title)}</strong><small>${esc(x.meta)}</small></span>`;c.onclick=()=>show(i,true);cards.append(c)});$('#empty').hidden=cards.children.length>0}$('#previous').onclick=()=>show((selected-1+data.length)%data.length,true);$('#next').onclick=()=>show((selected+1)%data.length,true);$('#search')?.addEventListener('input',draw);for(const group of ['Tümü',...new Set(data.map(x=>x.group))]){const btn=document.createElement('button');btn.textContent=group;btn.className=group==='Tümü'?'active':'';btn.onclick=()=>{filter=group;document.querySelectorAll('#filters button').forEach(x=>x.classList.toggle('active',x===btn));draw()};$('#filters').append(btn)}draw();show(0);'''

def page(dest,title,heading,desc,data,extras='',foot=''):
    icon=quote(os.path.relpath(b.m.ICON,dest.parent).replace('\\','/'))
    dest.write_text('''<!doctype html><html lang="tr"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>'''+html.escape(title)+'''</title><style>'''+CSS+'''</style><main class="wrap"><nav><a href="http://127.0.0.1:8200/">← Önizleme merkezi</a><span class="eyebrow">FARZETKI GAMES / ARROWSWARM</span></nav><header class="hero"><div><div class="eyebrow">VİDEO KOLEKSİYONU · 18 KASIM 2026</div><h1>'''+heading+'''</h1><p>'''+desc+'''</p>'''+extras+'''</div><img src="'''+icon+'''" alt="Seçilen ArrowSwarm uygulama ikonu"></header><section class="view" id="viewer" aria-label="Video oynatıcı"><div class="screen"><video id="player" controls playsinline preload="metadata"></video></div><div class="details"><span class="tag" id="position"></span><h2 id="videoTitle"></h2><p id="videoMeta"></p><p class="note" id="videoNote"></p><div class="actions"><a class="pill primary" id="download" download>↓ MP4 indir</a><button id="previous" aria-label="Önceki video">← Önceki</button><button id="next" aria-label="Sonraki video">Sonraki →</button></div><div class="links"><a id="subtitles" download>SRT altyazı</a><a id="contact" target="_blank" rel="noopener">Kareleri incele ↗</a></div><p class="note">Video kendi oynatıcısında açılır. Tam ekran ve ses kontrollerini kullanabilirsin.</p></div></section><h2>İzlemek istediğini seç</h2><div class="actions"><input id="search" type="search" placeholder="Video veya mod ara…" aria-label="Videolarda ara"></div><div class="filters" id="filters" style="margin-top:16px" aria-label="Video kategorileri"></div><div class="grid" id="cards"></div><p id="empty" hidden>Bu aramada video bulunamadı.</p>'''+foot+'''<footer>Önceki teslimler korunur. Teknik doğrulama, yaratıcı onay veya mağazada yayın anlamına gelmez. Oyun mekaniği değiştirilmedi.</footer></main><script>'''+JS.replace('DATA',json.dumps(data,ensure_ascii=False))+'''</script></html>''',encoding='utf-8')

NOTES={'R01':'Gerçek Şekiller1 kaydı. Kalp, açık yol ve ilk hamle.','R02':'Şekiller2; 1,33× hız. Arşiv HUD’u İngilizce, anlatım Türkçe.','R03':'İngilizce HUD’lu Şekiller2. Yarı hızdaki yanlış hamleden sonra etiketli ayrı doğru-hamle örneği. Tek bir tekrar deneme gibi sunulmaz.','R04':'Renk Değişimi3; 0,8×. Çerçevenin gerçek dönüşü. Arşiv HUD’u İngilizce.','R05':'Renk Değişimi1: gerçek yanlış renk geri dönüşü, iki gerçek dönüş ve eşleşen açık çıkışlar. Tam bölüm bitişi iddia edilmez.','R06':'Savunma10: gerçek son gökkuşağı oku ve doğrulanmış kazanma. İlk plandaki101 yerine mevcut gerçek10 kaydı kullanıldı.','R07':'Gerçek ilk bölüm eğitimi; gösterilen iki oka gerçek dokunma. Bu kayıt için var olan HUD’lar görünür tutuldu.','R08':'Gerçek Şekiller50; 48 ok, 1,67× hız, doğal çözüm ve kazanma.','R09':'Üç farklı kapalı oka gerçek atış, üç çarpışma/geri dönüş ve doğal kayıp. 0,4× hız etiketi görünür.','R10':'Renk Değişimi5; gerçek sınırlı dönüş hakkı, bir dönüş ve doğal kazanma. 1,67× hız.','R11':'Gerçek üç mod seçimi, Light/Dark ve dil düğmeleri. 1,33×; dil Türkçeden İspanyolcaya değişir.','R12':'Açıkça etiketli iki ayrı Savunma3 denemesi: kazanma ve kaybetme. Kayıp kısmı0,8×; tek deneme veya yeniden oynama gibi sunulmaz.'}

def bundle():
    manifest=b.m.read(P/'manifest.json');qa=b.m.read(P/'ENCODED_QA.json');assert manifest['complete'] and qa['passed']
    data=[]
    for x in manifest['items']:
        ident=x['id'];folder=P/ident;stem=Path(x['file']).stem
        srt=folder/(stem+'.srt');(folder/(stem+'.vtt')).write_text('WEBVTT\n\n'+re.sub(r'(\d\d:\d\d:\d\d),(\d{3})',r'\1.\2',srt.read_text(encoding='utf-8')),encoding='utf-8')
        data.append({'title':ident+' · '+x['title'],'group':{'R01':'Şekiller','R02':'Şekiller','R03':'Şekiller','R08':'Şekiller','R04':'Renk Değişimi','R05':'Renk Değişimi','R10':'Renk Değişimi','R11':'Menü'}.get(ident,'Savunma'),'src':f'{ident}/{x["file"]}','poster':f'{ident}/{stem}_cover.jpg','srt':f'{ident}/{stem}.srt','vtt':f'{ident}/{stem}.vtt','contact':f'{ident}/review_strip.jpg','meta':'18s · 1080×1920 · 30fps · TR anlatım','note':NOTES[ident]})
    readme='# ArrowSwarm · 12 özgün video\n\n18 Kasım2026. Önceki R01–R12 planının tamamı için ayrı18s dikey MP4 ve Türkçe seslendirme/altyazı. Toplam216s/6480kare. Her iki HUD korunur. Oyun mekaniği, sahneler ve özel kayıtlar değiştirilmedi.\n\n'
    readme+='\n'.join(f'- {x["id"]} — {x["title"]}: {NOTES[x["id"]]}' for x in manifest['items'])
    readme+='\n\nSes: proje müziği/SFX ve Türkçe Microsoft Ahmet stok sesinden editoryal miks; AudioListener kaydı değildir. Tüm dosyalar yaklaşık−18LUFS; teknik QA, sesin öznel beğenisi veya yaratıcı onay sayılmaz. Mağaza bağlantısı uydurulmadı; yeni kapanış “Şimdi sıra sende”.\n\nKaynak kareler yerelde04_Raw_Gameplay altında kalır. NativeEvidence/ içindeki olay/geometry/provenance/restoration kayıtları kompakt kanıttır. R06 için101→10 açık seviye ikamesidir. R12 iki ayrı denemeyi birleştirir.\n\nDevam: kullanıcı bu12videoyu değerlendirir; yeni istek gelmeden oyun/yayın değişikliği veya push yapılmaz.\n'
    (P/'README.md').write_text(readme,encoding='utf-8')
    archive=P/'ArrowSwarm_12_Concepts_TR.zip'
    with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
        for x in manifest['items']:
            for suffix in ('.mp4','.srt'):z.write(P/x['id']/(Path(x['file']).stem+suffix),x['id']+'/'+Path(x['file']).stem+suffix)
        for f in ('README.md','manifest.json','ENCODED_QA.json'):z.write(P/f,f)
    with zipfile.ZipFile(archive) as z:assert z.testzip() is None and len(z.namelist())==27
    b.dump(P/'BUNDLE_QA.json',{'passed':True,'members':27,'finalMP4':12,'srt':12,'intermediatesIncluded':False,'sha256':b.m.sha(archive),'bytes':archive.stat().st_size})
    extras='<div class="actions"><a class="pill primary" href="ArrowSwarm_12_Concepts_TR.zip" download>↓ 12 videoyu indir · ZIP</a><a class="pill" href="../../Final_Deliverables/VideoLibrary/index.html">Tüm video arşivi ↗</a></div><div class="stats"><span><strong>12</strong> farklı konu</span><span><strong>18s</strong> her video</span><span><strong>7</strong> yeni native kayıt</span></div>'
    foot='<details><summary>Kaynak ve kontrol notları</summary><p>Tam native1080×1920 kayıtlar;30fpsH.264/AAC,6480karenin tam çözümü, kaynak hashleri, altyazı süreleri ve ses seviyeleri kontrol edildi. Yavaş/hızlı sahneler etiketli. Menü dil değişimi ve arşiv İngilizceHUD’lar kartlarda açıklanır. Ses editoryal mikstir.</p><div class="links"><a href="ENCODED_QA.json">Medya/kaynak QA</a><a href="manifest.json">Kurgu manifesti</a><a href="IMPLEMENTATION.md">Tikli uygulama</a><a href="BUNDLE_QA.json">ZIP doğrulaması</a></div></details>'
    page(P/'index.html','ArrowSwarm · 12 Yeni Konsept · Tam Video Galerisi','12 konu.<br><em>12 yeni video.</em>','İlk açık yoldan son gökkuşağı okuna. Önceki planda kalan bütün konseptler, ayrı Türkçe anlatım ve gerçek oyun görüntüleriyle hazır.',data,extras,foot)

def series_library_details():
    """Use titles and existing covers from the current short-series manifests."""
    details = {}
    reels = b.m.MEDIA / '05_Reels'
    for folder, mode in (('ShapesShorts_20261118', 'Şekiller'), ('ColorShiftSeries_20261118', 'Renk Değişimi')):
        series = reels / folder
        manifest = series / 'manifest.json'
        if not manifest.is_file():
            continue
        for entry in b.m.read(manifest):
            if entry.get('existing'):
                continue
            title = f"{mode} · Bölüm {entry['level']} · {entry['title']}"
            note = entry.get('note', 'Bölümün 16 saniyelik Türkçe anlatımlı kısa kurgusu.')
            details[(series / entry['file']).resolve()] = (title, series / entry['poster'], note)
    return details

def library():
    LIB.mkdir(parents=True,exist_ok=True);items=[];archive_names=('Pilot01/','ReferencePilot02_20261114/','ReferencePilot02_R2_20261114/','DefensePilot01_20261115/')
    series = series_library_details()
    for base in (b.m.MEDIA/'05_Reels',b.m.MEDIA/'06_Trailers_Ads_Tutorials'):
        for p in sorted(base.rglob('ArrowSwarm_*.mp4')):
            rel=p.relative_to(b.m.MEDIA).as_posix()
            if re.search(r'_part\d|_end\.|_splash\.|_pre_audio\.|_audio_master\.',p.name) or 'SharedOutro01/' in rel:continue
            archive=any(x in rel for x in archive_names)
            group='Arşiv / önceki sürüm' if archive else 'Yeni12 konsept' if 'ConceptBacklog_' in rel else 'Fragman / reklam / rehber' if '06_Trailers' in rel else 'Savunma' if 'Defense' in rel else 'Renk Değişimi' if 'ColorShift' in rel else 'Şekiller'
            inf=b.m.imageio_ffmpeg.read_frames(str(p));meta=next(inf);inf.close()
            stem = p.stem
            title, poster, series_note = series.get(p.resolve(), (stem.replace('ArrowSwarm_', '').replace('_', ' '), p.with_name(stem + '_cover.jpg'), None))
            if not poster.exists():
                poster=LIB/'Posters'/f'{b.m.sha(p)[:12]}.jpg';poster.parent.mkdir(exist_ok=True)
                if not poster.exists():b.m.run([b.FF,'-y','-ss',str(min(2,meta['duration']/3)),'-i',str(p),'-frames:v','1','-vf','scale=320:-1','-q:v','3',str(poster)],'library_poster')
            src=quote(os.path.relpath(p,LIB).replace('\\','/'));pos=quote(os.path.relpath(poster,LIB).replace('\\','/'))
            srt=p.with_suffix('.srt');contact=p.with_name('review_strip.jpg')
            if not contact.exists():contact=p.with_name(stem+'_contact.jpg')
            concept=re.search(r'ConceptBacklog_.*/(R\d\d)/',rel)
            if concept:title=concept[1]+' · '+next(x['title'] for x in b.m.read(P/'manifest.json')['items'] if x['id']==concept[1])
            note = series_note or 'Kurgu ayrıntıları ve teknik kontroller ilgili teslim klasöründedir.'
            if archive:
                note = 'Önceki kurgu sürümü.'
            elif concept:
                note = NOTES.get(concept[1], note)
            items.append({'title':title,'group':group,'src':src,'poster':pos,'srt':quote(os.path.relpath(srt,LIB).replace('\\','/')) if srt.exists() else None,'contact':quote(os.path.relpath(contact,LIB).replace('\\','/')) if contact.exists() else None,'meta':f'{meta["duration"]:g}s · {meta["size"][0]}×{meta["size"][1]} · '+('Arşiv' if archive else 'Teslim edilmiş dosya'),'note':note,'path':rel,'sha256':b.m.sha(p),'durationSeconds':meta['duration'],'archive':archive})
    items.sort(key=lambda x:(x['archive'],'Yeni12' not in x['group'],x['group'],x['title']))
    active=[x for x in items if not x['archive']];b.dump(LIB/'inventory.json',{'date':'2026-11-18','total':len(items),'currentFiles':len(active),'archiveFiles':len(items)-len(active),'originalRenderQueueRemaining':0,'items':items,'componentsExcluded':['SharedOutro01 reusable ending','render segments','pre-master copies'],'published':False})
    with (LIB/'inventory.csv').open('w',newline='',encoding='utf-8-sig') as f:
        w=csv.DictWriter(f,fieldnames=['title','group','path','durationSeconds','archive','sha256']);w.writeheader();w.writerows({k:x[k] for k in w.fieldnames} for x in items)
    extras='<div class="actions"><a class="pill primary" href="../../05_Reels/ConceptBacklog_20261117/index.html">Yeni12videoya geç ↗</a><a class="pill" href="inventory.csv" download>↓ Dosya envanteri</a></div><div class="stats"><span><strong>'+str(len(active))+'</strong> güncel teslim</span><span><strong>'+str(len(items)-len(active))+'</strong> arşiv sürümü</span><span><strong>0</strong> planda eksik video</span></div>'
    page(LIB/'index.html','ArrowSwarm · Tüm Videolar · Tek Arşiv','Bütün videolar.<br><em>Tek yerde.</em>','Şekiller kısa kurguları, Renk Değişimi ve Savunma serileri, mağaza fragmanı ve rehber.',items,extras,'<details><summary>Envanterin kapsamı</summary><p>Reels, fragman, reklam ve rehber klasörlerindeki tamamlanmış MP4 dosyaları. Önceki kurgu sürümlerine Arşiv filtresinden ulaşabilirsin.</p><a href="inventory.json">Dosyalar ve SHA256 envanteri</a></details>')
    polish_page(LIB)
    print('LIBRARY',len(items),'CURRENT',len(active),'ARCHIVE',len(items)-len(active))
def polish_page(folder):
    file=folder/'index.html';text=file.read_text(encoding='utf-8')
    icon=quote(os.path.relpath(b.m.ICON,folder).replace('\\','/'))
    text=text.replace('<style>',f'<link rel="icon" href="{icon}"><style>',1)
    for old,new in [('Yeni12','Yeni 12'),('yeni12','yeni 12'),('12konsept','12 konsept'),('12videoya','12 videoya'),('Şekiller1','Şekiller 1'),('Şekiller2','Şekiller 2'),('Şekiller50','Şekiller 50'),('Savunma10','Savunma 10'),('Savunma3','Savunma 3'),('Değişimi1','Değişimi 1'),('Değişimi3','Değişimi 3'),('Değişimi5','Değişimi 5')]:text=text.replace(old,new)
    file.write_text(text,encoding='utf-8')

def polish():
    polish_page(P)
    overview=b.Image.new('RGB',(1440,920),'#0b1830')
    for i,x in enumerate(b.m.read(P/'manifest.json')['items']):
        img=b.Image.open(P/x['id']/(Path(x['file']).stem+'_cover.jpg')).convert('RGB').resize((234,416),b.Image.Resampling.LANCZOS)
        left=(i%6)*240+3;top=(i//6)*460+6;overview.paste(img,(left,top))
        b.m.text(overview,x['id'],(left+117,top+435),24,anchor='mm',fill='#92e9ff')
    overview.save(P/'overview.jpg',quality=90)
if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--library-only', action='store_true', help='Refresh the combined library without rebuilding the twelve-film bundle')
    args = parser.parse_args()
    if not args.library_only:
        bundle()
        polish()
    library()
