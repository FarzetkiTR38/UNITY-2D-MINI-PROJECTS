"""Compose store samples from a genuine Unity keyframe; never synthesizes gameplay."""
from pathlib import Path
import sys,os,json,hashlib
sys.path.insert(0,str(Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Media-Python'))
from PIL import Image,ImageDraw,ImageFont
P=Path(__file__).resolve().parents[2];M=P/'ArrowSwarm_Marketing';OUT=M/'03_GooglePlay'
SRC=M/'04_Raw_Gameplay/Shapes_L2_20261113_081526_913/source.png'
FONT=P/'Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf'
NAVY='#123659';BLUE='#0368df';CYAN='#45d9ef'
def font(n):return ImageFont.truetype(str(FONT),n)
def fitted(s,n,width):
 while font(n).getlength(s)>width:n-=1
 return font(n)
def center(d,s,y,n=66,color=NAVY):
 f=fitted(s,n,900); b=d.textbbox((0,0),s,font=f); d.text(((1080-f.getlength(s))/2,y-b[1]),s,font=f,fill=color)
src=Image.open(SRC).convert('RGB');assert src.size==(1080,1920)
files=[]
for lang,headline,sub,bottom in [('EN','WHICH ARROW FIRST?','Find the order. Clear the shape.','ONE PATH AT A TIME.'),('TR','ÖNCE HANGİ OK?','Sırayı bul. Şekli boşalt.','HER HAMLE BİR YOL AÇAR.')]:
 im=src.copy();d=ImageDraw.Draw(im)
 center(d,'ARROWSWARM  /  SHAPES',274,29,BLUE)
 center(d,headline,323,76)
 center(d,sub,411,31)
 d.rounded_rectangle((128,1520,952,1621),radius=30,fill='#e4f6ff',outline='#89c6ef',width=3)
 center(d,bottom,1551,43)
 path=OUT/f'store_pilot_{lang}.png';im.save(path,optimize=True);files.append(path)
# Native square board crop, uniform resize; no arrows repainted, removed, or rearranged.
w,h=1024,500;im=Image.new('RGB',(w,h));pix=im.load()
for y in range(h):
 for x in range(w):
  k=x/(w-1);pix[x,y]=(int(239-13*k),int(248-8*k),int(254-25*k))
d=ImageDraw.Draw(im)
d.text((64,69),'ARROWSWARM',font=font(30),fill=BLUE)
d.text((64,144),'FIND YOUR',font=font(59),fill=NAVY)
d.text((64,207),'WAY OUT.',font=font(68),fill=NAVY)
d.rounded_rectangle((64,302,397,308),radius=3,fill='#ffc43b')
d.text((64,333),'A shape. A choice.',font=font(27),fill=NAVY)
d.text((64,371),'A path to clear.',font=font(27),fill=NAVY)
board=src.crop((48,470,1032,1454)).resize((392,392),Image.Resampling.LANCZOS)
mask=Image.new('L',(392,392));ImageDraw.Draw(mask).rounded_rectangle((0,0,391,391),radius=20,fill=255)
im.paste(board,(572,54),mask);d=ImageDraw.Draw(im);d.rounded_rectangle((571,53,965,447),radius=22,outline='#83b7db',width=2)
p=OUT/'feature_graphic_pilot_EN_1024x500.png';im.save(p,optimize=True);files.append(p)
qa=[]
for p in files:
 with Image.open(p) as q:
  q.verify()
 with Image.open(p) as q:qa.append({'file':p.name,'width':q.width,'height':q.height,'mode':q.mode,'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
(OUT/'pilot_image_qa.json').write_text(json.dumps({'passed':True,'source':str(SRC.relative_to(P)),'sourceSha256':hashlib.sha256(SRC.read_bytes()).hexdigest(),'sourceType':'Actual Unity screenshot; original gameplay pixels retained','TRcopyOnEnglishHUD':True,'featureCrop':[48,470,1032,1454],'images':qa},indent=2),encoding='utf-8')
(OUT/'PILOT_REVIEW.md').write_text('''# Current first sample · 13 November2026

Fresh native Unity Shapes2 source: 04_Raw_Gameplay/Shapes_L2_20261113_081526_913/source.png,42 arrows. No AI-generated or fabricated gameplay. Store samples are1080x1920RGBPNG; feature graphic1024x500RGBPNG. The feature uses a uniform square crop of the real board. Functional game text was not repainted.

TR and EN differ in editorial text. Current TR sample still has English native HUD; fully localized Turkish gameplay capture remains a release-set task. These are first-review candidates, not a complete uploaded store listing. Existing store_sample files remain archival drafts.

Official technical reference checked13Nov2026: https://support.google.com/googleplay/android-developer/answer/9866151?hl=en . 512px32-bit icon (<1024KB),1024x500RGB feature, RGB screenshots, real gameplay representation. No rankings, fake reviews, install counts, store badges, or download CTA used. Font OFL in FONT_LICENSE.txt. No publication performed.

Alt text: ArrowSwarm Shapes penguin puzzle, with colorful bent arrow paths and its real level2HUD. Feature: A real penguin-shaped arrow puzzle next to the words Find Your Way Out.
''',encoding='utf-8')
print(json.dumps(qa,indent=2))
