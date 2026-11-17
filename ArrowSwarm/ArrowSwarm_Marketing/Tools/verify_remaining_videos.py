"""Independent final-file audio/decode/source and ZIP checks for the five edits."""
import json,subprocess,zipfile
from pathlib import Path
import build_remaining_videos as b
def main():
    manifest=b.m.read(b.OUT/'manifest.json');checks=[]
    for item in manifest['items']:
        p=b.OUT/item['file'];q=item['qa']
        assert b.m.sha(p)==q['sha256'] and q['fps']==30 and q['frames']==item['durationSeconds']*30 and q['durationSeconds']==item['durationSeconds']
        r=subprocess.run([b.FF,'-hide_banner','-i',str(p),'-af','loudnorm=I=-18:TP=-2:LRA=11:print_format=json','-f','null','-'],capture_output=True,text=True,encoding='utf8',errors='replace',check=True)
        levels=json.JSONDecoder().raw_decode(r.stderr[r.stderr.rfind('{'):])[0]
        assert float(levels['input_tp'])<0
        checks.append({'file':p.name,'hashMatches':True,'fullyDecodedAgain':True,'integratedLUFS':float(levels['input_i']),'truePeakDbTP':float(levels['input_tp'])})
    for row in manifest['sources'].values():
        src=b.ROOT/row['path'];assert b.m.sha(src/'provenance.json')==row['provenanceSha256'] and b.m.sha(src/'events.csv')==row['eventsSha256']
    voice=b.m.read(b.OUT/'Voice/manifest.json')
    tutorial=next(x for x in manifest['items'] if x['narration'])
    for row in voice['clips']:
        import wave
        p=b.OUT/'Voice'/row['file'];assert b.m.sha(p)==row['sha256']
        with wave.open(str(p)) as w:duration=w.getnframes()/w.getframerate()
        assert duration+.05<=tutorial['timeline'][row['chapter']]['seconds']
    report={'passed':True,'fiveActualMP4s':True,'exactSeconds':[x['durationSeconds'] for x in manifest['items']],'sourceEventAndProvenanceHashesUnchanged':True,'audio':checks,'renderFrameChecks':[x['qa'] for x in manifest['items']],'narrationFitsChapters':True,'noNativeCaptureClaimedThisSession':True,'subjectiveListeningClaimed':False}
    b.dump(b.OUT/'ENCODED_QA.json',report)
    with zipfile.ZipFile(b.OUT/'ArrowSwarm_Remaining_Videos_TR.zip','w',zipfile.ZIP_DEFLATED) as z:
        for row in manifest['items']:
            for suffix in ['.mp4','.srt']:
                p=b.OUT/(Path(row['file']).stem+suffix);z.write(p,p.name)
        for name in ['README.md','manifest.json','ENCODED_QA.json']:z.write(b.OUT/name,name)
    with zipfile.ZipFile(b.OUT/'ArrowSwarm_Remaining_Videos_TR.zip') as z:assert z.testzip() is None and len(z.namelist())==13
    print(json.dumps(report,ensure_ascii=False));print('ZIP verified')
if __name__=='__main__':main()
