"""Independent encoded-file, source-witness and preserved-state checks for R01–R12."""
import csv,hashlib,json,os,re,shutil,subprocess,wave
from pathlib import Path
import build_concept_backlog as c
b=c.b; P=c.PACKAGE

def rows(p):return list(csv.DictReader(p.open(encoding='utf-8-sig')))
def check_native():
    c.prepare_sources();checks=[]
    for key,case in c.SOURCE_CASES.items():
        source=c.SOURCES[key];proof=b.m.read(source/'provenance.json');ev=rows(source/'events.csv')
        assert proof['width']==1080 and proof['height']==1920 and proof['fps']==30
        if key in ('heart','advanced','budget'):
            assert proof['level']=={'heart':1,'advanced':50,'budget':5}[key] and proof['won'] and proof['remaining']==0
        else:assert proof['forcedOutcome'] is False
        if key=='budget':assert any(e['event']=='rotate' for e in ev)
        if key=='tutorial':assert proof['level']==1 and len([e for e in ev if e['event']=='fire'])==2
        if key=='match':
            assert len([e for e in ev if e['event']=='wrong_fire'])==1 and len([e for e in ev if e['event']=='returned'])==1
            shots=[e for e in ev if e['event']=='fire'];assert shots and int(shots[-1]['remaining'])<int(shots[0]['remaining'])
        if key=='wrong':
            shots=[e for e in ev if e['event']=='wrong_fire'];returns=[e for e in ev if e['event']=='returned']
            assert len(shots)==len(returns)==3 and len({e['arrow'] for e in shots})==3
            assert [int(e['lives']) for e in returns]==[2,1,0]
            assert all(int(a['frame'])<int(z['frame']) for a,z in zip(shots,returns))
            assert all(int(returns[i]['frame'])<int(shots[i+1]['frame']) for i in range(2))
        if key=='menu':assert {'mode_open','mode_select','settings_open','dark_theme','light_theme','language_next','menu_return'}<={e['event'] for e in ev}
        folder=P/'NativeEvidence'/case;folder.mkdir(exist_ok=True)
        for filename in ('provenance.json','events.csv','geometry.json','state_checks.txt'):
            if (source/filename).exists():shutil.copy2(source/filename,folder/filename)
        state=(source/'state_checks.txt').read_text(encoding='utf-8-sig');assert 'FAIL' not in state and 'PASS' in state
        checks.append({'scenario':case,'passed':True,'source':source.relative_to(c.ROOT).as_posix(),'frames':proof['frames'],'stateRestored':True})
    return checks

def check_preserved():
    backup=Path(Path(os.environ['TEMP'],'ArrowSwarm-VideoBacklog-backup.txt').read_text().strip())
    original=b.m.read(backup/'baseline.json');checks=[]
    for r in original:
        p=Path(r['path']);assert p.exists()==r['exists']
        if p.exists():assert b.m.sha(p)==r['sha256'],p.name
        checks.append({'file':p.relative_to(c.ROOT).as_posix() if p.is_relative_to(c.ROOT) else p.name,'unchanged':True})
    return checks

def main():
    manifest=b.m.read(P/'manifest.json');assert manifest['complete'] and len(manifest['items'])==12 and not manifest['missing']
    checks=[]
    for row in manifest['items']:
        folder=P/row['id'];path=folder/row['file'];q=row['qa'];assert b.m.sha(path)==q['sha256']
        r=subprocess.run([b.FF,'-hide_banner','-i',str(path),'-af','loudnorm=I=-18:TP=-2:LRA=11:print_format=json','-progress','pipe:1','-f','null','-'],capture_output=True,text=True,encoding='utf-8',errors='replace',check=True)
        levels=json.JSONDecoder().raw_decode(r.stderr[r.stderr.rfind('{'):])[0]
        frames=int(re.findall(r'^frame=(\d+)',r.stdout,re.M)[-1]);assert frames==540
        assert abs(float(levels['input_i'])+18)<.4 and float(levels['input_tp'])<-1.5
        assert 'Video: h264' in r.stderr and 'Audio: aac' in r.stderr
        info=b.m.imageio_ffmpeg.read_frames(str(path));meta=next(info);info.close()
        assert tuple(meta['size'])==(1080,1920) and meta['fps']==30 and abs(meta['duration']-18)<.04
        audio=b.m.read(folder/(path.stem+'_audio.json'));assert audio['encodedVideoPacketsUnchanged'] and audio['finalSha256']==q['sha256']
        pm=b.m.read(folder/'manifest.json')
        for src in pm['sources'].values():
            source=c.ROOT/src['path'];assert b.m.sha(source/'provenance.json')==src['provenanceSha256'] and b.m.sha(source/'events.csv')==src['eventsSha256']
        voice=b.m.read(folder/'Voice/manifest.json')
        for clip in voice['clips']:
            wav=folder/'Voice'/clip['file'];assert b.m.sha(wav)==clip['sha256']
            with wave.open(str(wav)) as w:duration=w.getnframes()/w.getframerate()
            assert duration+.05<=row['timeline'][clip['chapter']]['seconds']
        srt=folder/(path.stem+'.srt');assert srt.exists() and '00:00:18,000' in srt.read_text(encoding='utf-8-sig')
        # Explicitly sample the real final MP4 rather than a pre-master/intermediate.
        samples=[]
        for sec in (1,4,7,10,13,16.7):
            r=subprocess.run([b.FF,'-v','error','-ss',str(sec),'-i',str(path),'-frames:v','1','-vf','scale=270:480','-f','rawvideo','-pix_fmt','rgb24','-'],capture_output=True,check=True)
            samples.append(b.Image.frombytes('RGB',(270,480),r.stdout))
        sheet=b.Image.new('RGB',(270*6,480));
        for i,img in enumerate(samples):sheet.paste(img,(i*270,0))
        sheet.save(folder/'review_strip.jpg',quality=87)
        checks.append({'id':row['id'],'file':row['file'],'passed':True,'frames':frames,'durationSeconds':18,'sha256':q['sha256'],'integratedLUFS':float(levels['input_i']),'truePeakDbTP':float(levels['input_tp']),'nativeSourceHashesMatch':True,'narrationFits':True,'h264AAC':True})
        print('VERIFIED',row['id'],flush=True)
    native=check_native();preserved=check_preserved()
    report={'date':'2026-11-18','passed':True,'videos':checks,'nativeWitnesses':native,'protectedFiles':preserved,'subjectiveListeningClaimed':False,'sourceAudioCaptured':False,'audio':'Editorial music, event-aligned project SFX and public Turkish stock speech','nativeHUDLanguage':'TR except archival EN Shapes2/error/Color3; Menu switches TR→ES through native Settings','rainbowLevelSubstitution':'D-rainbow101 planned; verified actual Defense10 used and labelled10. No level101 recording claimed.','outcomes':'R12 contains two separate attempts. Loss event at end of Wrong CSV is terminal validation, not popup onset; final return identifies natural zero-life transition.','creativeApproval':'Awaiting user review','published':False}
    b.dump(P/'ENCODED_QA.json',report);print('PASS all12 /6480frames /7native witnesses /protected original files')
if __name__=='__main__':main()
