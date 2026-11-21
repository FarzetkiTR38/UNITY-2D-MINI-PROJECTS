"""Typeset the two store locales over the approved-icon-directed generated key art."""
from pathlib import Path
import sys, os, json, hashlib, shutil
sys.path.insert(0,str(Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Media-Python'))
from PIL import Image, ImageDraw, ImageFont, ImageOps
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'ArrowSwarm_Marketing/03_GooglePlay'
SRC=ROOT/'ArrowSwarm_Marketing/02_Branding/Selected_20261115/feature_key_art.png'
FONT=ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf'
def main():
    rows=[]
    for lang,lines,modes in [('EN',['THREE MODES.','ONE CLEVER MOVE.'],'DEFENSE  ·  SHAPES  ·  COLOR SHIFT'),('TR',['ÜÇ OYUN MODU.','HER HAMLE BİR BULMACA.'],'SAVUNMA  ·  ŞEKİLLER  ·  RENK DEĞİŞİMİ')]:
        im=ImageOps.fit(Image.open(SRC).convert('RGB'),(1024,500),Image.Resampling.LANCZOS)
        d=ImageDraw.Draw(im)
        def text(x,y,value,size,color,stroke=0):
            f=ImageFont.truetype(str(FONT),size)
            d.text((x+2,y+4),value,font=f,fill='#04143d',stroke_width=stroke+2,stroke_fill='#04143d')
            d.text((x,y),value,font=f,fill=color,stroke_width=stroke,stroke_fill='#1578d9')
        text(48,78,'ARROW',77,'#f3fcff',2)
        text(48,156,'SWARM',82,'#ffda36',2)
        text(51,276,lines[0],27,'#ffffff')
        text(51,311,lines[1],23 if lang=='TR' else 27,'#7ce9ff')
        text(51,401,modes,13 if lang=='TR' else 14,'#b9d9f3')
        p=OUT/f'feature_graphic_final_{lang}_1024x500.png'; im.save(p,optimize=True)
        rows.append({'path':str(p.relative_to(ROOT)).replace('\\','/'),'language':lang,'width':im.width,'height':im.height,'mode':im.mode,'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
    (OUT/'feature_graphic_final_manifest.json').write_text(json.dumps({'selectedIcon':'ArrowSwarm_Marketing/02_Branding/Selected_20261115/ArrowSwarm_App_Icon_SELECTED.jpeg','artwork':'Built-in image generator, selected icon reference; no AI gameplay','sourceArt':str(SRC.relative_to(ROOT)).replace('\\','/'),'typography':'Localized deterministic title typography; existing licensed Farzetki H1','items':rows},indent=2),encoding='utf-8')
    print(json.dumps(rows))
if __name__=='__main__': main()
