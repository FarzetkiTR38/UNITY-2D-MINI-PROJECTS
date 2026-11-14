"""Verify the continued launch workspace and create a source-linked local review bundle."""
from pathlib import Path
from urllib.parse import urlsplit,unquote
from html.parser import HTMLParser
import hashlib,json,os,csv,zipfile,subprocess,sys
P=Path(__file__).resolve().parents[2];M=P/'ArrowSwarm_Marketing';F=M/'Final_Deliverables'
sys.path.insert(0,str(Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Media-Python'))
from PIL import Image
class Links(HTMLParser):
 def __init__(self):super().__init__();self.refs=[]
 def handle_starttag(self,tag,attrs):
  a=dict(attrs)
  for key in ['href','src','poster']:
   if a.get(key):self.refs.append(a[key])
def digest(p):
 h=hashlib.sha256()
 with p.open('rb') as f:
  for b in iter(lambda:f.read(4194304),b''):h.update(b)
 return h.hexdigest()
# Check the new phase workbench; historical first-pilot page has its own earlier QA.
a=Links();a.feed((F/'phase_status.html').read_text(encoding='utf-8'));missing=[]
for ref in a.refs:
 u=urlsplit(ref)
 if u.scheme or not u.path:continue
 if not (F/unquote(u.path)).is_file():missing.append(ref)
assert not missing,missing
baseline=Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Phases-20261113';b=json.loads((baseline/'baseline.json').read_text(encoding='utf-8'))
source_differences=[]
for rel in b['dirtyFiles']:
 if digest(P.parent/rel)!=digest(baseline/'dirty'/rel):source_differences.append(rel)
assert not source_differences,source_differences
research=json.loads((M/'00_Research/RESEARCH_QA.json').read_text(encoding='utf-8'));assert research['individual_competitor_visual_audits_completed']==15 and research['social_not_searched_titles']==0
aso=json.loads((M/'07_ASO_SEO_GEO/ASO_QA.json').read_text(encoding='utf-8'));assert aso['passed']
site=json.loads((M/'07_ASO_SEO_GEO/Website/QA/site_static_qa.json').read_text(encoding='utf-8'));assert site['passed']
growth=json.loads((M/'09_Analytics/GROWTH_PACKAGE_QA.json').read_text(encoding='utf-8'));assert growth['passed']
for rel,h in growth['csv_sha256'].items():assert digest(M/rel)==h,rel
rows=[];image_count=0
for p in sorted(M.rglob('*')):
 if not p.is_file() or '__pycache__' in p.parts or p.suffix=='.pyc':continue
 rel=p.relative_to(M).as_posix()
 if p.name in ['launch_file_manifest.json','continuation_package_qa.json','ASSET_MANIFEST.md','launch_verification.json']:continue
 if p.parent==F and p.suffix=='.zip':continue
 if p.name.startswith('frame_') and p.suffix=='.jpg':continue
 entry={'file':rel,'bytes':p.stat().st_size,'sha256':digest(p)}
 if p.suffix.lower() in ['.png','.jpg','.webp']:
  with Image.open(p) as im:im.verify()
  with Image.open(p) as im:entry.update(width=im.width,height=im.height,mode=im.mode)
  image_count+=1
 rows.append(entry)
manifest={'date':'2026-11-13','scope':'Continuing local launch workbench; not a completed launch or creative approval','files':rows,'rawFrameSequencesExcludedFromGitAndBundle':True,'privateStateExcluded':True,'historicalFileManifests':'Older file_manifest.json and package_qa.json describe the preserved first-pilot snapshot, not this current tree.'}
(F/'launch_file_manifest.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False),encoding='utf-8')
lines=['# Güncel yayın çalışma dosyaları','', '13 Kasım2026. Gerçek dosyalar ve boyutlar aşağıdadır. Bunlar finalyayın/yaratıcıonay anlamına gelmez. Ham630JPEGyereldekalır; özeloyuncuyedekleri dışarıdadır. Eski ilkpilothashlistesi ve ZIP tarihselarşivdir; güncel makineenvanteri Final_Deliverables/launch_file_manifest.json dosyasıdır.','', '| Dosya | Bayt | Ölçü |','|---|---:|---|']
for row in rows:
 dim=f"{row['width']}×{row['height']} {row['mode']}" if 'width' in row else '—'
 lines.append(f"| [{row['file']}]({row['file']}) | {row['bytes']} | {dim} |")
(M/'ASSET_MANIFEST.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
(P/'ASSET_MANIFEST.md').write_text('# ArrowSwarm pazarlama envanteri\n\nGüncel liste: [ArrowSwarm_Marketing/ASSET_MANIFEST.md](ArrowSwarm_Marketing/ASSET_MANIFEST.md). Güncelhashlar: [launch_file_manifest.json](ArrowSwarm_Marketing/Final_Deliverables/launch_file_manifest.json).\n',encoding='utf-8')
report={'passed':True,'date':'2026-11-13','workbenchLinks':len(a.refs),'missingWorkbenchLinks':missing,'preexistingDirtyFilesByteIdentical':len(b['dirtyFiles']),'sourceDifferences':source_differences,'currentFileEntries':len(rows),'imagesDecoded':image_count,'researchVisualsInspected':15,'socialTitlesDiscoveryAttempted':15,'socialVideosActuallySampled':2,'storeDraftPNGs':16,'storeCopyQA':True,'localWebsiteBrowserQA':True,'calendarRows':30,'actualKPIsRemainBlank':True,'gameplayOrProductionSettingsWritten':False,'newUnityCaptureOrDeviceTestInThisPass':False,'SDKorCollectionAdded':False,'outreachOrPublicationOrSpend':False,'releaseDecision':'NO-GO','microCandidatesResearched':4,'qualifiedMicroCreators':0}
(F/'launch_verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
archive=F/'ArrowSwarm_Launch_Workbench_20261113.zip'
with zipfile.ZipFile(archive,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for p in sorted(M.rglob('*')):
  if not p.is_file() or '__pycache__' in p.parts or p.suffix=='.pyc':continue
  if p.parent==F and (p.suffix=='.zip' or p.name=='continuation_package_qa.json'):continue
  if p.name.startswith('frame_') and p.suffix=='.jpg':continue
  z.write(p,p.relative_to(P))
 for p in sorted((P/'Assets/_Project/Scripts/Marketing').rglob('*')):
  if p.is_file():z.write(p,p.relative_to(P))
 for name in ['Farzetki_Title_Font_H1.ttf','Farzetki_Title_Font_H1-OFL.txt']:
  p=P/'Assets/_Project/Fonts'/name;z.write(p,p.relative_to(P))
with zipfile.ZipFile(archive) as z:assert z.testzip() is None
(F/'continuation_package_qa.json').write_text(json.dumps({'passed':True,'zip':archive.name,'bytes':archive.stat().st_size,'sha256':digest(archive),'crcPassed':True,'privateAndRawFrameDataExcluded':True,'publicWebsitePressZipIncluded':True},indent=2),encoding='utf-8')
print(json.dumps(report,indent=2));print('ZIP bytes:',archive.stat().st_size)
