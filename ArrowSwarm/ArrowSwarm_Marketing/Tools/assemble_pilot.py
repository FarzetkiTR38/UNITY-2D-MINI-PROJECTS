"""Inventory and package the review pilot without private state or bulk raw frames."""
from pathlib import Path
import sys,os,json,hashlib,re,zipfile,html
sys.path.insert(0,str(Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Media-Python'))
from PIL import Image
P=Path(__file__).resolve().parents[2];M=P/'ArrowSwarm_Marketing';F=M/'Final_Deliverables'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for folder in sorted((M/'04_Raw_Gameplay').iterdir()):
 if not folder.is_dir():continue
 frames=sorted(folder.glob('frame_*.jpg'));prov=json.loads((folder/'provenance.json').read_text())
 assert len(frames)==prov['frames']
 items=[]
 for i,p in enumerate(frames):
  assert p.name==f'frame_{i:05d}.jpg'
  with Image.open(p) as im:
   assert im.size==(1080,1920)
  items.append({'file':p.name,'bytes':p.stat().st_size,'sha256':sha(p)})
 (folder/'frame_manifest.json').write_text(json.dumps({'frames':len(items),'fps':30,'localOnly':True,'files':items},indent=2),encoding='utf-8')
# Create the inventory target before link validation; its complete table is written below.
if not (M/'ASSET_MANIFEST.md').exists(): (M/'ASSET_MANIFEST.md').write_text('# Asset inventory\n',encoding='utf-8')
# A media/schema check is not represented as a browser-playback test.
links=[];missing=[]
for href in re.findall(r'(?:href|src|poster)="([^"]+)"',(F/'index.html').read_text(encoding='utf-8')):
 if href.startswith(('http:','https:','#')):continue
 target=(F/html.unescape(href)).resolve();links.append(str(target))
 if not target.is_file():missing.append(href)
assert not missing,missing
(F/'preview_static_qa.json').write_text(json.dumps({'passed':True,'localLinksChecked':len(links),'missing':missing,'browserPlaybackTested':False,'browserBlock':'Built-in browser security policy rejected file:// protocol; no workaround attempted.','imageVisualReview':True,'encodedContactSheetReview':True,'fullVideoDecode':'See05_Reels/Pilot01/QA_REPORT.json'},indent=2),encoding='utf-8')
exclude={'ASSET_MANIFEST.md','package_qa.json'};rows=[]
for p in sorted(M.rglob('*')):
 if not p.is_file() or '__pycache__' in p.parts or re.fullmatch(r'frame_\d+\.jpg',p.name) or p.suffix=='.zip' or p.name in exclude or p.name=='file_manifest.json':continue
 rel=p.relative_to(M).as_posix();entry={'file':rel,'bytes':p.stat().st_size,'sha256':sha(p)}
 if p.suffix.lower() in ('.png','.jpg','.webp'):
  with Image.open(p) as im:im.verify()
  with Image.open(p) as im:entry.update(width=im.width,height=im.height,mode=im.mode)
 entry['status']='historical draft' if p.name.startswith(('store_sample','source_shapes02')) or p.name=='layout_only_ARCHIVE.jpg' else 'first-pilot deliverable/source/QA'
 rows.append(entry)
(F/'file_manifest.json').write_text(json.dumps({'scope':'First approval pilot only; no full launch claim','rawFrameCounts':[{'folder':d.name,'frames':len(list(d.glob('frame_*.jpg')))} for d in sorted((M/'04_Raw_Gameplay').iterdir()) if d.is_dir()],'files':rows},indent=2),encoding='utf-8')
lines=['# GerÃ§ek dosya envanteri','', '13 KasÄ±m2026 Â· Ä°lk onay paketi. Tam global yayÄ±n teslimatÄ± deÄŸildir.','', 'Ham630JPEG kare yereldedir ve Git/ZIP dÄ±ÅŸÄ±nda tutulur;iki frame_manifest.json dosyasÄ± her karenin adÄ±,boyutu veSHA256 deÄŸerini listeler.20s hamMP4 ayrÄ±ca teslim edilir. Ã–zel oyuncu yedekleri ve medya Python baÄŸÄ±mlÄ±lÄ±klarÄ± bu klasÃ¶rÃ¼n dÄ±ÅŸÄ±ndadÄ±r.','', '| Dosya | Bayt | Ã–lÃ§Ã¼ / durum |','|---|---:|---|']
for r in rows:
 dimensions=f"{r['width']}Ã—{r['height']} / {r['mode']}" if 'width' in r else r['status']
 lines.append(f"| [{r['file']}]({r['file']}) | {r['bytes']} | {dimensions} |")
(M/'ASSET_MANIFEST.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
(P/'ASSET_MANIFEST.md').write_text('# ArrowSwarm pazarlama dosyalarÄ±\n\nGÃ¼ncel gerÃ§ek dosya envanteri:[ArrowSwarm_Marketing/ASSET_MANIFEST.md](ArrowSwarm_Marketing/ASSET_MANIFEST.md). Makine okunurhash/Ã¶lÃ§Ã¼ listesi:[file_manifest.json](ArrowSwarm_Marketing/Final_Deliverables/file_manifest.json).\n',encoding='utf-8')
archive=F/'ArrowSwarm_FirstCreative_20261113.zip'
with zipfile.ZipFile(archive,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for p in sorted(M.rglob('*')):
  if not p.is_file() or p==archive or p.name=='package_qa.json' or p.suffix=='.zip' or '__pycache__' in p.parts or re.fullmatch(r'frame_\d+\.jpg',p.name):continue
  z.write(p,p.relative_to(P))
 for p in sorted((P/'Assets/_Project/Scripts/Marketing').rglob('*')):
  if p.is_file():z.write(p,p.relative_to(P))
 for name in ['Farzetki_Title_Font_H1.ttf','Farzetki_Title_Font_H1-OFL.txt']:
  p=P/'Assets/_Project/Fonts'/name;z.write(p,p.relative_to(P))
with zipfile.ZipFile(archive) as z:assert z.testzip() is None
(F/'package_qa.json').write_text(json.dumps({'passed':True,'zip':archive.name,'bytes':archive.stat().st_size,'sha256':sha(archive),'crcPassed':True,'bulkJpegFramesExcluded':True,'privateStateExcluded':True,'purpose':'review package; original frame sequences remain locally available/regenerable using the Editor recorder'},indent=2),encoding='utf-8')
print(json.dumps({'fileEntries':len(rows),'localLinksChecked':len(links),'missingLinks':missing,'archiveBytes':archive.stat().st_size,'zipCrcPassed':True,'browserTest':False},indent=2))
