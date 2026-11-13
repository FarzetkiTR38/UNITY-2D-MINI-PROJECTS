"""Export generated icon concepts and an actual-size review sheet (Pillow13)."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib
D=Path(__file__).resolve().parent
board=Image.new('RGB',(1760,1040),'#F1F4F8'); dr=ImageDraw.Draw(board)
f=ImageFont.truetype(r'C:/Windows/Fonts/arialbd.ttf',26);small=ImageFont.truetype(r'C:/Windows/Fonts/arial.ttf',20)
dr.text((32,24),'ARROWSWARM / ICON CONCEPTS / ACTUAL SIZE CHECKS',fill='#173458',font=f)
dr.text((32,65),'Concept artwork only. No game icon or Player Settings changed. Final choice requires review.',fill='#4F6179',font=small)
manifest=[]
for i,letter in enumerate('ABC'):
 p=D/f'icon_{letter}_source.png';im=Image.open(p).convert('RGB');x=32+i*576
 dr.text((x,112),{'A':'A / MINIMAL','B':'B / VIBRANT','C':'C / PREMIUM'}[letter],fill='#173458',font=f)
 for n in [48,72,128,512]:
  out=D/f'icon_{letter}_{n}.png';resized=im.resize((n,n),Image.Resampling.LANCZOS)
  if n==512:resized=resized.convert('RGBA')
  resized.save(out,optimize=True)
  with Image.open(out) as q:
   assert q.size==(n,n) and q.format=='PNG'
   if n==512:assert q.mode=='RGBA' and q.getchannel('A').getextrema()==(255,255) and out.stat().st_size<1024*1024
  manifest.append({'file':out.name,'width':n,'height':n,'format':'PNG','mode':'RGBA' if n==512 else 'RGB','bytes':out.stat().st_size,'sha256':hashlib.sha256(out.read_bytes()).hexdigest()})
 board.paste(Image.open(D/f'icon_{letter}_512.png'),(x,158))
 for n,dx in [(48,0),(72,80),(128,185)]:
  dr.text((x+dx,710),f'{n}px',fill='#173458',font=small);board.paste(Image.open(D/f'icon_{letter}_{n}.png'),(x+dx,748))
 mask=Image.new('L',(128,128),0);ImageDraw.Draw(mask).ellipse((0,0,127,127),fill=255)
 circle=Image.new('RGBA',(128,128));circle.paste(Image.open(D/f'icon_{letter}_128.png').convert('RGBA'),(0,0),mask)
 circle.save(D/f'icon_{letter}_circle128.png')
 board.paste(circle,(x+365,748),circle);dr.text((x+365,710),'Circle mask',fill='#173458',font=small)
 dr.text((x,914),{'A':'Clean geometry; strongest flat readability.','B':'High energy; check blue saturation.','C':'Close to the existing blue/gold UI.'}[letter],fill='#4F6179',font=small)
dr.text((32,990),'512px masters + 48/72/128 exports / New AI concepts / Not gameplay or installed production assets',fill='#4F6179',font=small)
board.save(D/'icon_comparison.png',optimize=True)
(D/'icon_files.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps({'outputs':len(manifest),'board':str(D/'icon_comparison.png'),'bytes':sum(x['bytes'] for x in manifest)},indent=2))
