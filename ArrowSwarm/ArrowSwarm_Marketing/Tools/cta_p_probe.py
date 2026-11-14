"""Generate local pronunciation probes and exact TTS word boundaries only."""
import asyncio,json,os,tempfile
from pathlib import Path
from reference_pilot_voice import trim_to_wav,VOICE,RATE
import edge_tts,imageio_ffmpeg

OUT=Path(tempfile.gettempdir())/'ArrowSwarm-CTA-P-Probes'
async def main():
    OUT.mkdir(exist_ok=True)
    ff=imageio_ffmpeg.get_ffmpeg_exe()
    async def probe(key,text):
        metadata=[];raw=OUT/f'{key}.mp3'
        if not raw.exists():
            speech=edge_tts.Communicate(text,VOICE,rate=RATE,boundary='WordBoundary')
            with raw.open('wb') as f:
                async for chunk in speech.stream():
                    if chunk['type']=='audio':f.write(chunk['data'])
                    else:metadata.append(chunk)
            (OUT/f'{key}_words.json').write_text(json.dumps(metadata,ensure_ascii=False,indent=2),encoding='utf-8')
        else:metadata=json.loads((OUT/f'{key}_words.json').read_text(encoding='utf-8'))
        metrics=trim_to_wav(raw,OUT/f'{key}.wav',ff)
        result={'key':key,'text':text,**metrics,'words':metadata}
        (OUT/f'{key}_metrics.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
        return result
    rows=await asyncio.wait_for(asyncio.gather(probe('original','Oynamak için, piley sıtordan indirin!'),probe('piley','Piley.'),probe('pi_ley','Pi-ley.')),90)
    print(json.dumps(rows,ensure_ascii=True,indent=2))
if __name__=='__main__':asyncio.run(main())
