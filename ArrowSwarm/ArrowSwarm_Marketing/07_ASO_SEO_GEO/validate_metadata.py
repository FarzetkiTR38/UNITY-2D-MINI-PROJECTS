"""Validate public store copy mechanically; does not publish or promise policy approval."""
from pathlib import Path
import csv,hashlib,json,unicodedata,re
ROOT=Path(__file__).resolve().parent
LIMITS={'title':30,'short':80,'long':4000}
BANNED=('ad-free','no ads','100% offline','play offline','multiplayer','unlimited levels','improve your iq','best game','million downloads','reklamsız','internetsiz','çevrimdışı','çok oyunculu','sınırsız bölüm','iq artır','en iyi oyun')
def validate():
 document=json.loads((ROOT/'store_metadata.json').read_text(encoding='utf-8'))
 results={};lines=[]
 for lang,item in document['languages'].items():
  counts={}
  md=(ROOT/f'PLAY_STORE_{lang}.md').read_text(encoding='utf-8')
  blocks=re.findall(r'```text\n(.*?)\n```',md,re.S)
  assert len(blocks)==3,f'{lang}: expectedexactthreecopyblocks'
  for i,(field,limit) in enumerate(LIMITS.items()):
   value=item[field];txt=(ROOT/f'play_{lang.lower()}_{field}.txt').read_text(encoding='utf-8')
   assert value==txt==blocks[i],f'{lang}/{field}: source/copypaste mismatch'
   assert value and value==value.strip() and unicodedata.normalize('NFC',value)==value
   assert '\ufeff' not in value and not any(unicodedata.category(c)=='Cf' for c in value)
   assert not any(c not in '\n' and unicodedata.category(c)=='Cc' for c in value)
   if field!='long':assert '\n' not in value
   points=len(value);units=len(value.encode('utf-16-le'))//2
   assert points<=limit and units<=limit,f'{lang}/{field} exceeds{limit}'
   found=[term for term in BANNED if term in value.casefold()]
   assert not found,f'{lang}/{field} unsupportedclaim:{found}'
   counts[field]={'unicodeCodePoints':points,'utf16Units':units,'limit':limit,'passed':True}
   lines.append([lang,field,points,units,limit])
  assert item['long'].count('50')==2
  assert 'Classic' not in item['long']
  results[lang]={'locale':item['locale'],'counts':counts,'copySha256':hashlib.sha256(json.dumps(item,ensure_ascii=False,sort_keys=True).encode()).hexdigest()}
 with (ROOT/'metadata_counts.csv').open('w',encoding='utf-8-sig',newline='') as f:
  w=csv.writer(f);w.writerow(['language','field','unicode_codepoints','utf16_units','limit']);w.writerows(lines)
 sources=(ROOT/'SOURCES.md').read_text(encoding='utf-8');assert sources.count('https://')>=8
 keyword_rows=list(csv.DictReader((ROOT/'KEYWORD_MAP.csv').open(encoding='utf-8-sig')));assert len(keyword_rows)==13
 website=json.loads((ROOT/'website_copy.json').read_text(encoding='utf-8'))
 assert website['public_domain'] is None and website['play_store_url'] is None and website['support_email'] is None
 report={'passed':True,'preparedDate':'2026-11-13','status':'Local documents/copy ready; unpublished; final visual revision pending','metadata':results,'keywordIntentRows':len(keyword_rows),'sourcePolicyLinks':8,'keywordVolumeMeasured':False,'visualDesignApproved':False,'userAuthorizedContinuationWithoutVisualApproval':True,'unsupportedClaimLintPassed':True,'copyMarkdownTxtJsonMatch':True,'unicodeNfcAndNoHiddenControls':True,'publicWebsiteDeployed':False,'playConsoleAccessed':False,'storeExperimentsRun':0,'newUnityChecksInThisPass':False,'scope':'Mechanical copy/intent/source checks only; not a policy decision,device test,SEOindexing result or measuredASOperformance.'}
 (ROOT/'ASO_QA.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 print(json.dumps(report,ensure_ascii=False,indent=2))
if __name__=='__main__':validate()
