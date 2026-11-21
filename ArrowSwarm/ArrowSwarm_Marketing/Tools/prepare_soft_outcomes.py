"""Master six new CC0 acoustic cues; install only explicitly, retaining Unity GUIDs."""
from pathlib import Path
import argparse, hashlib, json, os, re, shutil, subprocess, sys
sys.path.insert(0, str(Path(os.environ['TEMP'])/'ArrowSwarm-Marketing-Media-Python'))
import imageio_ffmpeg
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'ArrowSwarm_Marketing/01_Game_Audit/OutcomeSoftAudio_20261115'
RESEARCH=Path(os.environ['TEMP'])/'ArrowSwarm-OutcomeAudio-Research'
SFX=ROOT/'Assets/_Project/Audio/SFX'
SPECS=[('LevelWin','Win','main','Yumuşak yükseliş','10'),
 ('LevelWin_alt1','Win','alt1','Kısa ve hafif','08'),
 ('LevelWin_alt2','Win','alt2','Melodik yükseliş','02'),
 ('LevelLose','Lose','main','Nazik iniş','11'),
 ('LevelLose_alt1','Lose','alt1','Kısa tekrar dene','09'),
 ('LevelLose_alt2','Lose','alt2','Melodik kapanış','14')]

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def run(args):
    result=subprocess.run(args,capture_output=True)
    if result.returncode: raise RuntimeError(result.stderr.decode(errors='replace'))
    return result
def stats(ff,path):
    r=run([ff,'-hide_banner','-i',str(path),'-af','loudnorm=I=-20:TP=-6:LRA=7:print_format=json','-f','null','-'])
    return json.loads(re.findall(r'\{\s*"input_i".*?\}',r.stderr.decode(),re.S)[-1])
def main():
    parser=argparse.ArgumentParser(); parser.add_argument('--install',action='store_true'); args=parser.parse_args()
    ff=imageio_ffmpeg.get_ffmpeg_exe(); (OUT/'Source').mkdir(parents=True,exist_ok=True); (OUT/'Prepared').mkdir(exist_ok=True)
    source_rows=json.loads((RESEARCH/'selected_six_sources.json').read_text())
    items=[]
    for name,role,variant,title,index in SPECS:
        source=RESEARCH/'candidates'/f'jingles_STEEL{index}.ogg'
        row=next(x for x in source_rows if x['file_ogg'].endswith(source.name))
        assert sha(source)==row['sha256_ogg'] and row['license']=='CC0-1.0'
        archived=OUT/'Source'/source.name; shutil.copy2(source,archived)
        # Float decoding preserves original Vorbis overshoot until headroom mastering.
        pcm=run([ff,'-v','error','-i',str(source),'-f','f32le','-ar','44100','-ac','2','pipe:1']).stdout
        duration=len(pcm)/(4*2*44100)
        filters=f'highpass=f=65,lowpass=f=5200,equalizer=f=3000:t=q:w=0.7:g=-2.5,afade=t=in:d=0.016,afade=t=out:st={max(0,duration-.12):.6f}:d=0.12,apad=pad_dur=0.08'
        first=run([ff,'-hide_banner','-i',str(source),'-af',filters+',loudnorm=I=-20:TP=-6:LRA=7:print_format=json','-f','null','-'])
        measured=json.loads(re.findall(r'\{\s*"input_i".*?\}',first.stderr.decode(),re.S)[-1])
        assert float(measured['input_i'])>-70
        norm='loudnorm=I=-20:TP=-6:LRA=7:linear=true:'+':'.join([
            'measured_I='+measured['input_i'],'measured_TP='+measured['input_tp'],
            'measured_LRA='+measured['input_lra'],'measured_thresh='+measured['input_thresh'],
            'offset='+measured['target_offset']])
        target=OUT/'Prepared'/(name+'.ogg')
        run([ff,'-v','error','-y','-i',str(source),'-af',filters+','+norm,'-ar','44100','-ac','2','-c:a','libvorbis','-q:a','5',str(target)])
        data=np.frombuffer(run([ff,'-v','error','-i',str(target),'-f','f32le','-ar','44100','-ac','2','pipe:1']).stdout,dtype='<f4').reshape(-1,2)
        result=stats(ff,target); peak=float(np.abs(data).max())
        assert np.isfinite(data).all() and 0<peak<.57 and -23<float(result['input_i'])<-18
        assert np.max(np.abs(data[:100]))<.06 and np.max(np.abs(data[-100:]))<.005
        item={'name':name,'role':role,'variant':variant,'title':title,'durationSeconds':len(data)/44100,
            'lufs':float(result['input_i']),'truePeakDb':float(result['input_tp']),'samplePeak':peak,
            'bytes':target.stat().st_size,'sha256':sha(target),'sourceFile':source.name,'sourceSha256':sha(source),
            'sourceUrl':'https://kenney.nl/assets/music-jingles','downloadUrl':row['download_url_ogg'],
            'license':'CC0-1.0','active':variant=='main','path':'Assets/_Project/Audio/SFX/'+name+'.ogg'}
        items.append(item)
    assert len({x['sha256'] for x in items})==6
    if args.install:
        old={sha(SFX/(name+'.ogg')) for name,*_ in SPECS}
        assert not any(x['sourceSha256'] in old for x in items),'Never reuse one of the rejected six recordings'
        metas={name:sha(SFX/(name+'.ogg.meta')) for name,*_ in SPECS}
        for item in items: shutil.copyfile(OUT/'Prepared'/(item['name']+'.ogg'),ROOT/item['path'])
        assert all(sha(SFX/(name+'.ogg.meta'))==digest for name,digest in metas.items())
    manifest={'passed':True,'installed':args.install,'family':'Soft acoustic steelpan / bell-like plucked cues',
        'publisher':'Kenney','officialLicense':'https://kenney.nl/assets/music-jingles',
        'downloadMirror':'https://github.com/Mcamento8/open-game-sfx-index',
        'mastering':'65Hz high-pass,5.2kHz low-pass,2.5dB presence reduction,16ms attack,120ms tail fade,80ms silence,2-pass -20LUFS/-6dBTP; stereo44.1kHz Vorbis quality5',
        'subjectiveListeningClaimed':False,'items':items}
    (OUT/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'passed':True,'installed':args.install,'totalBytes':sum(x['bytes'] for x in items),'items':items},ensure_ascii=False))
if __name__=='__main__': main()
