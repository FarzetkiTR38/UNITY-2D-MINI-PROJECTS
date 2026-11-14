"""Original compact Turkish narration; reuse the approved P-fixed CTA verbatim."""
from __future__ import annotations
import asyncio,hashlib,json,shutil,wave
from pathlib import Path
from reference_pilot_voice import SR,VOICE,RATE,trim_to_wav
import edge_tts,imageio_ffmpeg,numpy as np

MEDIA=Path(__file__).resolve().parents[1]
OUT=MEDIA/'05_Reels/CreativeShort01_20261114/audio'
SHARED=MEDIA/'05_Reels/SharedOutro01'
LINES=[(.10,'Kırk dört ok. Hangisi önce çıkar?'),(3.50,'Önce A!'),(9.30,'Ve ahtapot çözüldü!')]

def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,obj):p.write_text(json.dumps(obj,ensure_ascii=False,indent=2),encoding='utf-8')

async def main():
    OUT.mkdir(parents=True,exist_ok=True);ff=imageio_ffmpeg.get_ffmpeg_exe()
    async def generate(i,start,text):
        raw=OUT/f'voice_{i:02d}_raw.mp3';target=OUT/f'voice_{i:02d}.wav'
        if not raw.exists() or not raw.stat().st_size:
            await asyncio.wait_for(edge_tts.Communicate(text,VOICE,rate=RATE).save(str(raw)),45)
        metrics=trim_to_wav(raw,target,ff)
        return {'id':i,'requestedStartSeconds':start,'text':text,'file':target.name,'rawFile':raw.name,
                'rawSha256':sha(raw),'generatedThisRevision':True,**metrics}
    rows=await asyncio.gather(*(generate(i,start,text) for i,(start,text) in enumerate(LINES)))
    source=SHARED/'cta_voice.wav';target=OUT/'voice_03.wav';shutil.copy2(source,target)
    base=json.loads((SHARED/'cta_voice_provenance.json').read_text(encoding='utf-8'))
    rows.append({'id':3,'requestedStartSeconds':12.40,'text':'Oynamak için Play Store’dan indirin!',
        'file':target.name,'rawFile':None,'durationSeconds':base['durationSeconds'],'sha256':sha(target),
        'generatedThisRevision':False,'reusedFrom':'SharedOutro01/cta_voice.wav',
        'approvedPOnsetPreserved':True,'sourceSha256':sha(source),
        'sourceCompositeNote':'Edited WAV is copied exactly. Its original base MP3 alone would not reproduce the approved P onset.'})
    assert sha(target)==sha(source)==base['sha256']
    starts=[row['requestedStartSeconds'] for row in rows]
    ends=[start+row['durationSeconds'] for start,row in zip(starts,rows)]
    assert all(end <= (starts[i+1] if i<3 else 16) for i,end in enumerate(ends))
    checks=[]
    for row in rows:
        p=OUT/row['file']
        with wave.open(str(p),'rb') as wav:
            assert (wav.getframerate(),wav.getnchannels(),wav.getsampwidth())==(SR,1,2)
            pcm=np.frombuffer(wav.readframes(wav.getnframes()),dtype='<i2')
        peak=int(np.max(np.abs(pcm.astype(np.int32))));assert len(pcm)>0 and peak<32767
        assert sha(p)==row['sha256']
        assert abs(len(pcm)/SR-row['durationSeconds'])<1/SR
        checks.append({'file':p.name,'durationSeconds':len(pcm)/SR,'sampleRate':SR,'channels':1,
                       'sampleFormat':'PCM16','clippedSamples':0,'samplePeakDbFS':float(20*np.log10(max(peak/32768,1e-12))),
                       'sha256':row['sha256'],'reused':not row['generatedThisRevision']})
    write(OUT/'voice_manifest.json',{'provider':'Microsoft online speech via edge-tts','voice':VOICE,'rate':RATE,
        'libraryVersion':edge_tts.__version__,'audioIsEditorial':True,'instagramAudioCopied':False,
        'sampleRate':SR,'channels':1,'clips':rows,'subjectiveListeningClaimed':False,
        'timing':'Final16s cut: starts0.10,3.50,9.30,12.40s. Each narration finishes before the next.'})
    write(OUT/'voice_QA.json',{'passed':True,'clipCount':4,'approvedCtaPOnsetByteIdentical':True,'checks':checks,
        'lockedCutSeconds':16,'timingNonoverlapPassed':True,'lockedStartsSeconds':starts,'lockedEndsSeconds':ends})
    (OUT/'VOICE_PROVENANCE.md').write_text(
        '# Creative Short01 narration\n\nThree original public Turkish sentences were synthesized with tr-TR-AhmetNeural at+8% using edge-tts. '
        'Only those public creative sentences were sent; no private data, credentials or Instagram audio were used. '
        'Raw MP3s and conservatively trimmed48kHz monoPCM16 WAVs are retained.\n\n'
        'Clip03 is a byte-for-byte copy of the approved, corrected-P SharedOutro01/cta_voice.wav. '
        'It was not regenerated, and no unedited base MP3 is misrepresented as the source of that composite WAV. '
        'The store destination remains mock. All narration is editorial, not Unity AudioListener output. '
        'Technical PCM/hash checks are not a subjective listening certification. Final16s cut starts:0.10,3.50,9.30,12.40s.\n',encoding='utf-8')
    print(json.dumps({'passed':True,'output':str(OUT),'clips':[{'id':r['id'],'durationSeconds':r['durationSeconds'],'text':r['text']} for r in rows]},ensure_ascii=True,indent=2),flush=True)
if __name__=='__main__':asyncio.run(main())
