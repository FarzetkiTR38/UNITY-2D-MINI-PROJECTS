"""Cross-check delivered files/evidence and package the six public MP4s/SRTs."""
import hashlib,json,re,zipfile
from pathlib import Path

MEDIA=Path(__file__).resolve().parents[1]
SERIES=MEDIA/'05_Reels/ShapesSeries_20261114'
ORDER=[12,16,21,5,29,39]
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()

def main():
    shared=MEDIA/'05_Reels/SharedOutro01'
    config=read(shared/'store_config.json');assert config['status']=='mock' and config['store_url'] is None
    shared_hash=sha(shared/'ArrowSwarm_SharedOutro01_TR.mp4')
    episodes=[]
    specs=read(SERIES/'quiz_plan_draft.json')['levels']
    for level in ORDER:
        folder=SERIES/f'L{level:02d}';qa=read(folder/'QA_REPORT.json');plan=read(folder/'edit_plan.json')
        video=folder/f'ArrowSwarm_Shapes{level}_TR.mp4';spec=next(r for r in specs if r['level']==level)
        assert qa['passed'] and qa['sha256']==sha(video) and qa['bytes']==video.stat().st_size
        assert (qa['width'],qa['height'],qa['fps'])==(1080,1920,30)
        assert qa['frames']==round(plan['duration_seconds']*30)
        assert qa['arrows']==spec['arrow_count'] and qa['nativeSolvedWithThreeLives']
        assert qa['hudPixelIdenticalBeforeEncodeEveryGameplayFrame'] and qa['noCropOrReplacementHud']
        assert plan['reused_outro_sha256']==shared_hash
        assert plan['congratulations_end']<plan['outro_start']
        assert plan['first_settled']<plan['voice_starts'][5]<plan['rest_start']
        assert plan['voice_starts'][5]+plan['voice_durations'][5]<plan['rest_start']
        voice=read(folder/'audio/voice_manifest.json')['clips']
        assert len(voice)==7 and voice[0]['text']==spec['opening'] and voice[6]['text']==spec['congratulations']
        for i in [1,2,3,5]:
            p=folder/f'audio/voice_{i:02d}.wav';old=MEDIA/f'05_Reels/ReferencePilot02_R2_20261114/audio/{p.name}'
            assert sha(p)==sha(old)
        assert sha(folder/'audio/voice_04.wav')==sha(SERIES/'SharedAudio/voice_04.wav')
        subtitle=(folder/'captions_TR.srt').read_text(encoding='utf-8')
        assert 'Play Store’dan' in subtitle and 'piley' not in subtitle and len(re.findall(r' --> ',subtitle))==8
        for name in ['cover.jpg','encoded_contact_sheet.jpg','native_capture_checks.json','source_provenance.json','timeline.json']:
            assert (folder/name).is_file()
        source=MEDIA/'04_Raw_Gameplay'/plan['source'];native=read(source/'provenance.json');geometry=read(source/'geometry.json')
        assert native['won'] and native['remaining']==0 and native['frames']==900
        assert native['language']=='tr' and geometry['witness']==spec['solution_steps']
        assert len(read(source/'frame_manifest.json'))==900
        for c in spec['choices']:assert geometry['arrows'][c['arrow_id']]['initiallyRemovable']==(c['label']=='A')
        episodes.append({'level':level,'shape':plan['title'],'arrows':qa['arrows'],'videoDurationSeconds':qa['frames']/30,
            'bytes':qa['bytes'],'sha256':qa['sha256'],'loudnessLUFS':qa['audio']['input_i'],'truePeakDbTP':qa['audio']['input_tp'],
            'congratulationsEnd':plan['congratulations_end'],'outroStart':plan['outro_start'],'passed':True})
    state=read(SERIES/'native_session_checks.json')
    assert state['passed'] and not state['playing'] and not state['recorderArmed'] and state['consoleErrorCount']==0
    assert state['allPreexistingDirtyTrackedFileByteHashesUnchanged']
    result={'passed':True,'order':ORDER,'episodes':episodes,'totalNativeFrames':5400,'totalLegalShots':sum(r['arrows'] for r in episodes),
        'totalFinalBytes':sum(r['bytes'] for r in episodes),'sharedOutroSha256':shared_hash,'storeStatus':'mock','publicStoreUrl':None,
        'scope':'File/hash, exact production evidence, timing and decode/loudness reports. Browser UI checks are recorded separately; no subjective audio or physical-device certification claim.'}
    archive=MEDIA/'Final_Deliverables/ArrowSwarm_Shapes_Series_TR_20261114.zip'
    archive.parent.mkdir(parents=True,exist_ok=True)
    zip_readme='# Arrow Swarm · Altı Türkçe Reels\n\nSıra:12 →16 →21 →5 →29 →39. Her video1080×1920,30fps, yaklaşık41s.\n\n'
    for number,level in enumerate(ORDER,1):
        zip_readme+=f'{number}. Bölüm{level}: {number:02d}_B{level:02d}_ArrowSwarm_TR.mp4; aynı ön ekli SRT yanındadır.\n'
    zip_readme+='\nGerçek Unity oyun görüntüsü ve iki HUD korunur. Anlatım sentetik, ses/müzik ve hız/işaretler kurgudur. Ortak kapanıştaki mağaza bağlantısı MOCK; uygulama henüz yayınlanmadı. Bu dosyalar otomatik paylaşılmaz.\n'
    with zipfile.ZipFile(archive,'w',compression=zipfile.ZIP_STORED) as bundle:
        bundle.writestr('README.md',zip_readme.encode('utf-8'))
        for number,level in enumerate(ORDER,1):
            folder=SERIES/f'L{level:02d}'
            bundle.write(folder/f'ArrowSwarm_Shapes{level}_TR.mp4',f'{number:02d}_B{level:02d}_ArrowSwarm_TR.mp4')
            bundle.write(folder/'captions_TR.srt',f'{number:02d}_B{level:02d}_captions_TR.srt')
    with zipfile.ZipFile(archive) as bundle:
        assert bundle.testzip() is None and len(bundle.namelist())==13
        for number,row in enumerate(episodes,1):assert hashlib.sha256(bundle.read(f"{number:02d}_B{row['level']:02d}_ArrowSwarm_TR.mp4")).hexdigest()==row['sha256']
    result['localZip']={'file':archive.name,'entries':13,'crcAndVideoHashesPassed':True,'bytes':archive.stat().st_size,'sha256':sha(archive),'gitIgnored':True}
    (SERIES/'FINAL_CHECKS.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(result,ensure_ascii=True,indent=2))
if __name__=='__main__':main()
