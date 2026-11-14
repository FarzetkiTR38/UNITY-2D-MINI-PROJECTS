"""Surgical voiced-B-like onset -> aspirated P; preserve every video packet.

Only45ms of the CTA voice changes. Outside that window, the decoded soundtrack
is sample-identical before AAC re-encoding. The old compressed audio cannot be
claimed byte-identical after AAC encoding. No picture rendering occurs.
"""
import hashlib,json,math,os,re,shutil,subprocess,tempfile,wave,sys
from pathlib import Path
from reference_pilot_voice import SR
import numpy as np
import imageio_ffmpeg

MEDIA=Path(__file__).resolve().parents[1];SHARED=MEDIA/'05_Reels/SharedOutro01'
SERIES=MEDIA/'05_Reels/ShapesSeries_20261114';PILOT=MEDIA/'05_Reels/ReferencePilot02_R3_20261114'
ORDER=[12,16,21,5,29,39];START,END=1.015,1.060
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def run(args):return subprocess.run(args,check=True,capture_output=True)
def vhash(ff,p):return run([ff,'-v','error','-i',str(p),'-map','0:v:0','-c','copy','-f','hash','-hash','sha256','-']).stdout.decode().strip()
def decode(ff,p,channels=2):
    return np.frombuffer(run([ff,'-v','error','-i',str(p),'-vn','-ar',str(SR),'-ac',str(channels),'-f','f32le','-']).stdout,dtype='<f4').reshape(-1,channels).copy()
def wav(path,data):
    pcm=np.rint(np.clip(data,-.999969,.999969)*32768).astype('<i2')
    with wave.open(str(path),'wb') as f:
        f.setnchannels(data.shape[1]);f.setsampwidth(2);f.setframerate(SR);f.writeframes(pcm.tobytes())

def patch_track(track,old,new,voice_start):
    base=round(voice_start*SR);left,right=round(START*SR),round(END*SR)
    fit_a,fit_b=round(1.065*SR),round(1.235*SR)
    x=old[fit_a:fit_b,0].astype(float);x-=x.mean()
    target=track[base+fit_a:base+fit_b].mean(axis=1).astype(float);target-=target.mean()
    corr=float(np.dot(x,target)/math.sqrt(np.dot(x,x)*np.dot(target,target)))
    assert corr>.90,f'Unexpected cue alignment: {corr}'
    gains=[];updated=track.copy()
    for channel in range(2):
        y=track[base+fit_a:base+fit_b,channel].astype(float);y-=y.mean()
        gain=float(np.dot(x,y)/np.dot(x,x));assert .2<gain<6,gain;gains.append(gain)
        updated[base+left:base+right,channel]+=(new[left:right,0]-old[left:right,0])*gain
    assert np.array_equal(track[:base+left],updated[:base+left])
    assert np.array_equal(track[base+right:],updated[base+right:])
    assert abs(updated).max()<.99
    return updated,{'patchSeconds':[voice_start+START,voice_start+END],'fittedVoiceGains':gains,
                    'alignmentCorrelation':corr,'decodedPcmOutside45msIdenticalBeforeEncode':True}

def main():
    ff=imageio_ffmpeg.get_ffmpeg_exe()
    if (SHARED/'P_PRONUNCIATION_QA.json').exists():raise RuntimeError('P revision already applied; use Git/backup for an intentional new revision.')
    backup=Path(tempfile.mkdtemp(prefix='ArrowSwarm-CTA-P-Backup-'))
    (Path(tempfile.gettempdir())/'ArrowSwarm-CTA-P-backup-path.txt').write_text(str(backup),encoding='utf-8')
    old=decode(ff,SHARED/'cta_voice.wav',1);donor_dir=Path(tempfile.gettempdir())/'ArrowSwarm-CTA-P-Probes'
    donor=decode(ff,donor_dir/'pi_ley.wav',1)[round(.040*SR):round(.085*SR),0].astype(float)
    a,b=round(START*SR),round(END*SR);assert len(donor)==b-a
    # The donor contains P release/aspiration before its voiced vowel, raised14dB.
    donor*=10**(14/20);n=round(.004*SR);blend=np.ones(len(donor));blend[:n]=np.linspace(0,1,n);blend[-n:]=np.linspace(1,0,n)
    new=old.copy();new[a:b,0]=(old[a:b,0]*(1-blend)+donor*blend).astype('<f4')
    assert np.array_equal(old[:a],new[:a]) and np.array_equal(old[b:],new[b:])
    wav(backup/'cta_voice_new.wav',new)
    targets=[{'id':'shared','path':SHARED/'ArrowSwarm_SharedOutro01_TR.mp4','cta':.8,'frames':189},
             {'id':'pilot2','path':PILOT/'ArrowSwarm_Shapes2_R3_Promo_TR.mp4','cta':34.5,'frames':1200}]
    for level in ORDER:
        folder=SERIES/f'L{level:02d}';plan=read(folder/'edit_plan.json');qa=read(folder/'QA_REPORT.json')
        targets.append({'id':f'L{level:02d}','path':folder/f'ArrowSwarm_Shapes{level}_TR.mp4','cta':plan['outro_start']+.8,'frames':qa['frames']})
    for p in [SHARED/'cta_voice.wav',SHARED/'outro_mix.wav']:
        shutil.copy2(p,backup/p.name)
    rows=[]
    for t in targets:
        source=t['path'];copy=backup/source.name;shutil.copy2(source,copy)
        pcm=decode(ff,copy);patched,proof=patch_track(pcm,old,new,t['cta'])
        raw=backup/f"{t['id']}.f32";raw.write_bytes(patched.astype('<f4').tobytes())
        output=backup/f"{t['id']}_fixed.mp4"
        cmd=[ff,'-hide_banner','-y','-i',str(copy),'-f','f32le','-ar',str(SR),'-ac','2','-i',str(raw),'-map','0:v:0','-map','1:a:0','-c:v','copy','-c:a','aac','-b:a','192k','-ar',str(SR),'-t',str(t['frames']/30),'-movflags','+faststart',str(output)]
        r=run(cmd);(backup/f"{t['id']}_remux.log").write_bytes(r.stderr)
        before=vhash(ff,copy);after=vhash(ff,output);assert before==after
        r=run([ff,'-hide_banner','-i',str(output),'-f','null','-','-progress','pipe:1'])
        count=int(re.findall(rb'^frame=(\d+)$',r.stdout,re.M)[-1]);assert count==t['frames']
        r=run([ff,'-hide_banner','-i',str(output),'-vn','-af','loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json','-f','null','-'])
        levels=json.loads(re.findall(rb'\{\s*"input_i".*?\}',r.stderr,re.S)[-1]);assert float(levels['input_tp'])<-1 and -20<float(levels['input_i'])<-12
        rows.append({'id':t['id'],'file':str(source.relative_to(MEDIA)),'passed':True,'videoBitstreamSha256':before,
            'videoBitstreamUnchanged':True,'frames':count,'audio':levels,'oldSha256':sha(copy),'sha256':sha(output),'bytes':output.stat().st_size,**proof})
    # Commit replacements only after every candidate has decoded and passed video/audio checks.
    for t in targets:os.replace(backup/f"{t['id']}_fixed.mp4",t['path'])
    mixed=decode(ff,backup/'outro_mix.wav');mixed,_=patch_track(mixed,old,new,.8);wav(SHARED/'outro_mix.wav',mixed)
    shutil.copy2(backup/'cta_voice.wav',SHARED/'cta_voice_base_before_p.wav')
    shutil.copy2(backup/'cta_voice_new.wav',SHARED/'cta_voice.wav')
    shutil.copy2(donor_dir/'pi_ley.wav',SHARED/'cta_p_donor.wav');shutil.copy2(donor_dir/'pi_ley.mp3',SHARED/'cta_p_donor_raw.mp3')
    patch={'method':'45ms aspirated P onset from isolated Pi-ley; other original phonemes preserved',
        'sourceText':'Pi-ley.','targetVoiceWindow':[START,END],'donorVoiceWindow':[.040,.085],
        'donorGainDb':14,'crossfadeMs':4,'durationSeconds':len(old)/SR,'newVoiceSha256':sha(SHARED/'cta_voice.wav'),
        'baseVoiceSha256':sha(SHARED/'cta_voice_base_before_p.wav'),'voicePcmOutsidePatchUnchanged':True,'subjectiveListeningClaimed':False}
    write(SHARED/'cta_p_patch.json',patch)
    provenance=read(SHARED/'cta_voice_provenance.json');provenance.update({'sha256':patch['newVoiceSha256'],'pronunciationRevision':'P onset revised with isolated Pi-ley release/aspiration; original sentence preserved outside45ms.','onsetPatch':patch})
    write(SHARED/'cta_voice_provenance.json',provenance)
    config=read(SHARED/'store_config.json');config['cta_onset_patch']='cta_p_patch.json';write(SHARED/'store_config.json',config)
    shared_hash=sha(targets[0]['path'])
    for t,row in zip(targets[1:],rows[1:]):
        folder=t['path'].parent;qa=read(folder/'QA_REPORT.json')
        qa.update({'sha256':row['sha256'],'bytes':row['bytes'],'audio':row['audio'],'pPronunciationRevised':True,'approvedVideoStreamBitIdentical':True})
        if t['id']!='pilot2':
            qa['reused_outro_sha256']=shared_hash;plan=read(folder/'edit_plan.json');plan['reused_outro_sha256']=shared_hash;write(folder/'edit_plan.json',plan)
        write(folder/'QA_REPORT.json',qa);write(folder/'P_PRONUNCIATION_QA.json',row)
    report={'passed':True,'targetCount':len(rows),'patch':patch,'targets':rows,
        'pictureOrDisplayedTextChanges':False,'aacReencoded':True,'aacBytesOutsidePatchClaimedIdentical':False,
        'backup':'Full pre-revision media retained outside repository; Git keeps original committed versions.'}
    write(SHARED/'P_PRONUNCIATION_QA.json',report);write(SERIES/'P_PRONUNCIATION_QA.json',report)
    print(json.dumps({'passed':True,'outputs':len(rows),'changedVoiceMilliseconds':45,'videoStreamsUnchanged':True,'backup':str(backup)},indent=2),flush=True)
def finalize():
    """Refresh current file references after the independently checked audio-only edit."""
    report=read(SHARED/'P_PRONUNCIATION_QA.json');assert report['passed'] and len(report['targets'])==8
    pilot_row=next(r for r in report['targets'] if r['id']=='pilot2')
    prior=read(PILOT/'PRONUNCIATION_QA.json');prior.update({'newVideoSha256':pilot_row['sha256'],'pOnsetRevised':True,'pOnsetQa':'P_PRONUNCIATION_QA.json'})
    write(PILOT/'PRONUNCIATION_QA.json',prior)
    checks=read(PILOT/'FINAL_CHECKS.json');checks.update({'pOnsetRevised':True,'currentVideoSha256':pilot_row['sha256'],'videoBitstreamUnchangedAfterPRevision':True})
    write(PILOT/'FINAL_CHECKS.json',checks)
    patch=read(SHARED/'cta_p_patch.json');patch['donorRawSha256']=sha(SHARED/'cta_p_donor_raw.mp3');patch['donorWavSha256']=sha(SHARED/'cta_p_donor.wav');write(SHARED/'cta_p_patch.json',patch)
    for folder in [SHARED,PILOT]:
        manifest=[{'file':str(p.relative_to(folder)).replace('\\','/'),'bytes':p.stat().st_size,'sha256':sha(p)}
                  for p in sorted(folder.rglob('*')) if p.is_file() and p.name!='file_manifest.json']
        write(folder/'file_manifest.json',manifest)
    import check_shapes_series
    check_shapes_series.main()
if __name__=='__main__':finalize() if '--finalize' in sys.argv else main()
