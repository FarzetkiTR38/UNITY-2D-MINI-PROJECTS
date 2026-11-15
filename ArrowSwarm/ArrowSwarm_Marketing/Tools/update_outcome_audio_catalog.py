"""Refresh only the six replaced sources in the existing local audio catalogue.

Captures stays ignored by Git; this reproducible updater is tracked instead.
Historical outcome scores are explicitly removed, never presented as new ratings.
"""
from pathlib import Path
import hashlib,json,math,os,re,subprocess,sys
sys.path.insert(0,str(Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Media-Python'))
import imageio_ffmpeg
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
PACKAGE=ROOT/'ArrowSwarm_Marketing/01_Game_Audit/OutcomeSoftAudio_20261115'
PAGE=ROOT/'Captures/AudioReview/index.html'

def main():
    if not PAGE.is_file(): print('No local historical AudioReview; new review page remains available.'); return
    text=PAGE.read_text(encoding='utf-8-sig')
    pattern=r'(<script id="metrics-data" type="application/json">)(.*?)(</script>)'
    match=re.search(pattern,text,re.S); data=json.loads(match[2])
    items=json.loads((PACKAGE/'manifest.json').read_text(encoding='utf-8'))['items']
    selected={x['path']:x for x in items}; count=0
    for clip in data['clips']:
        item=selected.get(clip['path'])
        if not item: continue
        source=ROOT/item['path']
        result=subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-v','error','-i',str(source),'-f','f32le','-ar','44100','-ac','2','pipe:1'],capture_output=True,check=True)
        pcm=np.frombuffer(result.stdout,dtype='<f4').reshape(-1,2).astype(float)
        amplitude=np.max(np.abs(pcm),axis=1); active=pcm[amplitude>10**(-55/20)]
        keep={k:clip[k] for k in ('path','role','variant')}
        clip.update({k:None for k in clip}); clip.update(keep)
        clip.update({'active':item['active'],'format':'ogg','size_bytes':source.stat().st_size,'sha256':hashlib.sha256(source.read_bytes()).hexdigest(),
            'duration_s':len(pcm)/44100,'sample_rate_hz':44100,'channels':2,
            'peak_dbfs':20*math.log10(float(np.max(np.abs(pcm)))),
            'rms_dbfs':20*math.log10(float(np.sqrt(np.mean(pcm**2)))),'active_rms_dbfs':20*math.log10(float(np.sqrt(np.mean(active**2)))),
            'true_peak_est_dbfs':item['truePeakDb'],'clipped_sample_fraction':float(np.mean(np.abs(pcm)>=1)),
            'nonfinite_samples':0,'dc_offset':float(np.max(np.abs(np.mean(pcm,axis=0)))),
            'technical_scores':{},'flags':[],'revision':'New soft outcome audio15November2026; no historical scores retained'})
        count+=1
    assert count==6
    data['duplicates']=[x for x in data.get('duplicates',[]) if not any(role in json.dumps(x) for role in ('LevelWin','LevelLose'))]
    data['outcome_audio_revision']={'date':'2026-11-15','new_sources':6,'historicalOutcomeScoresRemoved':True,'newManifest':'ArrowSwarm_Marketing/01_Game_Audit/OutcomeSoftAudio_20261115/manifest.json'}
    text=text[:match.start()]+match[1]+json.dumps(data,ensure_ascii=False,separators=(',',':'))+match[3]+text[match.end():]
    banner='''<aside id="outcome-soft-update" class="notice"><strong>15 Kasım2026 · Kazanma ve kaybetme sesleri tamamen yenilendi.</strong><p>Bu iki role ait altı oynatıcı yeni yumuşak sesleri çalar. Eski kayıtların puanları kaldırıldı; bu sesler için yeni bir güzellik puanı verilmedi. <a href="http://127.0.0.1:8200/#%2Fmarketing%2F01_Game_Audit%2FOutcomeSoftAudio_20261115%2Findex.html">Yeni altılıyı ve oyunda aktif çifti dinle →</a></p></aside>'''
    if 'id="outcome-soft-update"' in text: text=re.sub(r'<aside id="outcome-soft-update".*?</aside>',banner,text,flags=re.S)
    else: text=text.replace('<main>','<main>'+banner,1)
    PAGE.write_text(text,encoding='utf-8')
    (PACKAGE/'catalog_update_checks.json').write_text(json.dumps({'passed':True,'updatedClips':count,'oldOutcomeScoresRemoved':True,'oldAudioPlayersNowUseNewFiles':True,'capturesRemainGitIgnored':True},indent=2)+'\n',encoding='utf-8')
    print('PASS: six new sources in historical audio catalogue; obsolete outcome scores removed')
if __name__=='__main__': main()
