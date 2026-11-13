from pathlib import Path
import sys,os,json,hashlib
sys.path.insert(0,str(Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Media-Python'))
from PIL import Image,ImageDraw,ImageFont
P=Path.cwd();M=P/'ArrowSwarm_Marketing';O=M/'03_GooglePlay/DraftSet01';O.mkdir(parents=True,exist_ok=True)
fontpath=P/'Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf'
raw='ArrowSwarm_Marketing/04_Raw_Gameplay/Shapes_L2_20261113_081526_913/'
items=[
('01','Shapes',raw+'source.png','2026-11-13','WHICH ARROW FIRST?','ÖNCE HANGİ OK?','Read the path before you tap.','Dokunmadan önce yolu incele.'),
('02','Shapes',raw+'frame_00180.jpg','2026-11-13','MAKE ROOM TO MOVE','HAREKETE YER AÇ','One exit opens the next choice.','Her çıkış yeni bir seçenek açar.'),
('03','Defense','Captures/UIRevision/defense-verified.png','historical-native-date-unconfirmed','PUZZLE MEETS DEFENSE','BULMACA VE SAVUNMA','Open a path. Face the swarm.','Bir yol aç. Sürüye karşı koy.'),
('04','Shapes','Captures/UIRollout/Nov10/Shapes/shapes-03-light.webp','2026-11-10','A SHAPE TO SOLVE','ÇÖZÜLECEK BİR ŞEKİL','Find a way through the rabbit.','Tavşanın içindeki yolları keşfet.'),
('05','ColorShift','Captures/UIRollout/Nov09/FatalBounce/ColorShift-returning.png','2026-11-09','MATCH THE WAY OUT','ÇIKIŞIN RENGİNİ EŞLEŞTİR','Rotate the frame. Keep arrows still.','Çerçeveyi döndür. Oklar sabit kalsın.'),
('06','Shapes','Captures/UIRollout/Nov10/Shapes/shapes-11-light.webp','2026-11-10','LOOK A LITTLE CLOSER','BİRAZ DAHA YAKINDAN BAK','Follow the turns, then choose.','Kıvrımları izle, sonra karar ver.'),
('07','Shapes','Captures/UIRollout/Nov10/Shapes/shapes-02-dark.webp','2026-11-10','A DIFFERENT MOOD','FARKLI BİR ATMOSFER','A real puzzle in the dark theme.','Aynı bulmaca, koyu tema.'),
('08','Home','Captures/UIRollout/Nov10/After/home-light.png','2026-11-10','CHOOSE YOUR CHALLENGE','MEYDAN OKUMANI SEÇ','Defense. Shapes. Color Shift.','Savunma. Şekiller. Renk Değişimi.')]
def font(n):return ImageFont.truetype(str(fontpath),n)
def centered(d,text,y,n,width=936,fill='#123751'):
 while font(n).getlength(text)>width:n-=1
 f=font(n);b=d.textbbox((0,0),text,font=f);d.text(((1080-f.getlength(text))/2,y-b[1]),text,font=f,fill=fill)
manifest=[]
for key,mode,source,date,en,tr,suben,subtr in items:
 path=P/source;assert path.is_file(),source
 with Image.open(path) as base:
  base=base.convert('RGB');w,h=base.size
  assert abs(w/h-9/16)<.025,(source,w,h)
  shot=base.resize((832,round(h*832/w)),Image.Resampling.LANCZOS)
 for lang,title,sub in [('EN',en,suben),('TR',tr,subtr)]:
  im=Image.new('RGB',(1080,1920),'#f4f0e5');d=ImageDraw.Draw(im)
  centered(d,'ARROWSWARM / '+mode.upper(),64,28,fill='#0879b6')
  centered(d,title,123,62);centered(d,sub,220,31)
  x=(1080-shot.width)//2;y=315
  d.rounded_rectangle((x-5,y-5,x+shot.width+5,y+shot.height+5),radius=26,fill='#c4e2f0')
  im.paste(shot,(x,y))
  centered(d,'VISUAL DRAFT / UNITY '+('CAPTURE' if date=='2026-11-13' else 'ARCHIVE'),1841,20,fill='#4e6572')
  target=O/f'{key}_{mode}_{lang}.png';im.save(target,optimize=True)
  with Image.open(target) as check:check.verify()
  manifest.append({'file':target.name,'language':lang,'mode':mode,'source':source,'sourceDate':date,'sourceSha256':hashlib.sha256(path.read_bytes()).hexdigest(),'outputSha256':hashlib.sha256(target.read_bytes()).hexdigest(),'bytes':target.stat().st_size,'width':1080,'height':1920,'modePixels':'RGB','releaseReady':False,'reason':'Visual revision deferred; non-current archived captures require recapture; source HUD language may differ from copy.'})
canvas=Image.new('RGB',(1440,1420),'#e9edf2');d=ImageDraw.Draw(canvas);d.text((30,22),'ARROWSWARM / STORE DRAFT SET / NOT RELEASE-READY',font=font(31),fill='#123751')
for i,(key,mode,*_) in enumerate(items):
 pic=Image.open(O/f'{key}_{mode}_EN.png').resize((320,569),Image.Resampling.LANCZOS);x=30+(i%4)*350;y=90+(i//4)*650;canvas.paste(pic,(x,y));d.text((x,y+578),key+' / '+mode,font=font(23),fill='#123751')
canvas.save(O/'overview_EN.jpg',quality=90)
(O/'source_manifest.json').write_text(json.dumps({'status':'UNAPPROVED_DRAFT','newUnityCaptureThisTurn':False,'gameplayPixelsFabricated':False,'items':manifest},ensure_ascii=False,indent=2),encoding='utf-8')
(O/'README.md').write_text('''# Eight-frame store narrative · unapproved drafts

13 November2026. Eight real raster layouts inTR+EN (16PNG),1080x1920RGB, with an overview and per-file source/SHA256 ledger. First three display real gameplay. Existing rejected icons/Reel are not used as approved branding. Typography and art direction still need final user revision.

Sources1–2 are from the13November verified native capture. Sources3–8 are historical actual Unity archives; dates/uncertainty are recorded individually. They are not fresh runtime captures. Color Shift frame5 records a genuine rejected-shot return; it is not represented as a perfect run. No arrows, hearts, progress or ranks were repainted. The complete source screenshot is uniformly scaled into the layout; marketing copy is added outside it.

These files are NOT final Play uploads: native recapture is required to confirm current UI/geometry and fully localizedHUDs, plus visual review after the user's final revision. All files visibly sayVISUAL DRAFT; remove that marker only for validated final exports. No Classic/fake daily challenge/invented offline/health/download-count claim. This is phase3 production scaffolding, not phase3 approval/completion.
''',encoding='utf-8')
print(json.dumps({'draftPNGs':len(manifest),'overview':str(O/'overview_EN.jpg'),'releaseReady':False}))
