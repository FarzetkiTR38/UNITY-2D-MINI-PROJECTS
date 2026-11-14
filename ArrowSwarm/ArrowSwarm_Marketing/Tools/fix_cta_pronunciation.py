"""Replace only CTA narration; copy the approved video streams bit for bit."""
import asyncio,hashlib,json,os,shutil,subprocess,tempfile
from pathlib import Path
from reference_pilot_voice import trim_to_wav,VOICE,RATE
import edge_tts
import render_promo_outro as p

def video_hash(ff,path):
    r=subprocess.run([ff,'-v','error','-i',str(path),'-map','0:v:0','-c','copy','-f','hash','-hash','sha256','-'],check=True,capture_output=True,text=True)
    return r.stdout.strip()

async def main():
    ff=p.m.imageio_ffmpeg.get_ffmpeg_exe()
    backup=Path(tempfile.mkdtemp(prefix='ArrowSwarm-CTA-Pronunciation-'))
    final=p.OUT/p.NAME;outro=p.SHARED/'ArrowSwarm_SharedOutro01_TR.mp4'
    old_hashes={x.name:video_hash(ff,x) for x in [final,outro]}
    for x in [final,outro,p.SHARED/'cta_voice.wav',p.SHARED/'cta_voice_raw.mp3']:
        shutil.copy2(x,backup/x.name)
    spoken=p.CONFIG.get('synthesis_cta','Oynamak için, piley sıtordan indirin!')
    raw=p.SHARED/'cta_voice_raw.mp3'
    await asyncio.wait_for(edge_tts.Communicate(spoken,VOICE,rate=RATE).save(str(raw)),45)
    metrics=trim_to_wav(raw,p.SHARED/'cta_voice.wav',ff)
    assert 1.6<metrics['durationSeconds']<3.2
    provenance=json.loads((p.SHARED/'cta_voice_provenance.json').read_text(encoding='utf-8'))
    provenance.update(metrics)
    provenance.update({'text':'Oynamak için Play Store’dan indirin!','synthesisText':spoken,'pronunciationRevision':'Turkish phonetic spelling prevents English Store plus Turkish suffix from being merged incorrectly.','rawSha256':p.m.sha(raw),'subjectiveListeningClaimed':False})
    (p.SHARED/'cta_voice_provenance.json').write_text(json.dumps(provenance,ensure_ascii=False,indent=2),encoding='utf-8')
    p.audio(ff)
    audio_only='[0:a]atrim=end=33.7,asetpts=PTS-STARTPTS[a0];[1:a]asetpts=PTS-STARTPTS[a1];[a0][a1]concat=n=2:v=0:a=1[a]'
    fixed=p.OUT/'pronunciation_fixed.mp4'
    p.m.command([ff,'-hide_banner','-y','-i',str(backup/final.name),'-i',str(p.SHARED/'outro_mix.wav'),'-filter_complex',audio_only,'-map','0:v:0','-map','[a]','-c:v','copy','-c:a','aac','-b:a','192k','-ar','48000','-t','40','-movflags','+faststart',str(fixed)],p.OUT/'pronunciation_remux.log')
    assert video_hash(ff,fixed)==old_hashes[final.name]
    os.replace(fixed,final)
    fixed_outro=p.SHARED/'pronunciation_fixed.mp4'
    p.m.command([ff,'-hide_banner','-y','-i',str(backup/outro.name),'-i',str(p.SHARED/'outro_mix.wav'),'-map','0:v:0','-map','1:a:0','-c:v','copy','-c:a','aac','-b:a','192k','-ar','48000','-t','6.3','-movflags','+faststart',str(fixed_outro)],p.SHARED/'pronunciation_remux.log')
    assert video_hash(ff,fixed_outro)==old_hashes[outro.name]
    os.replace(fixed_outro,outro)
    r=p.m.command([ff,'-hide_banner','-i',str(final),'-f','null','-','-progress','pipe:1'],p.OUT/'pronunciation_decode.log')
    import re
    assert int(re.findall(r'^frame=(\d+)$',r.stdout,re.M)[-1])==1200
    r=p.m.command([ff,'-hide_banner','-i',str(final),'-vn','-af','loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json','-f','null','-'],p.OUT/'pronunciation_loudness.log')
    levels=json.loads(re.findall(r'\{\s*"input_i".*?\}',r.stderr,re.S)[-1]);assert float(levels['input_tp'])<-1
    qa=json.loads((p.OUT/'QA_REPORT.json').read_text(encoding='utf-8'))
    qa.update({'sha256':p.m.sha(final),'bytes':final.stat().st_size,'audio':levels,'promoNarration':[34.5,round(34.5+metrics['durationSeconds'],3)],'pronunciationFixed':True,'approvedVideoStreamBitIdentical':True})
    (p.OUT/'QA_REPORT.json').write_text(json.dumps(qa,indent=2),encoding='utf-8')
    report={'passed':True,'synthesisText':spoken,'displayTextUnchanged':True,'videoStreamHashesUnchanged':old_hashes,'decodedFrames':1200,'durationSeconds':40,'newVoiceDuration':metrics['durationSeconds'],'newVideoSha256':p.m.sha(final),'subjectiveListeningClaimed':False}
    (p.OUT/'PRONUNCIATION_QA.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(report,ensure_ascii=False,indent=2))
if __name__=='__main__':asyncio.run(main())
