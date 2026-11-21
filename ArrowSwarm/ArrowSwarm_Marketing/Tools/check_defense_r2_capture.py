"""Strict native Defense R2 evidence: spaced legal shots and zero cleanup kills.

Only writes manifests/checks in the supplied new full-capture directory and an
optional new review directory. Never updates the original Defense pilot proof.
"""
import argparse,csv,hashlib,json,re
from pathlib import Path
from render_reference_reel import Image

ROOT=Path(__file__).resolve().parents[2]
MEDIA=ROOT/'ArrowSwarm_Marketing'
OLD=MEDIA/'04_Raw_Gameplay/Defense_L10_20261114_214156_530'
OLD_VIDEO=MEDIA/'05_Reels/DefensePilot01_20261115/ArrowSwarm_Defense_PanikYok_TR.mp4'
OLD_SHA='f38f6e881372f7444062d02e232825d002f464a1af3549850e1c638db1da15de'

def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def rows(p):
    with p.open(encoding='utf-8-sig',newline='') as f:return list(csv.DictReader(f))
def write(p,data):p.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')

def verify(source,output=None):
    """Verify completed full source before publishing any positive proof."""
    assert source!=OLD.resolve(),'R2 must not overwrite the original capture proof'
    assert sha(OLD_VIDEO)==OLD_SHA,'Original protected Defense video changed'
    p=read(source/'provenance.json');g=read(source/'geometry.json')
    events=rows(source/'events.csv');telemetry=rows(source/'telemetry.csv')
    assert p.get('allFramesSaved') is True,'Dry/partial trial is not a full video source'
    assert p['won'] and p['lives']==3 and p['remaining']==0,'Natural three-life Win required'
    assert p['mode']=='Defense' and p['level']==10 and p['arrows']>=2
    arrow_count=p['arrows'];reserved=p['reservedLastArrow']
    assert g['totalArrows']==arrow_count and g['level']==p['level']
    assert (p['width'],p['height'],p['fps'])==(1080,1920,30)
    total=p['frames'];assert len(telemetry)==total and total>0
    assert [int(t['frame']) for t in telemetry]==list(range(total)),'Telemetry frame sequence'
    assert all(abs(float(t['time_seconds'])-i/30)<.00002 for i,t in enumerate(telemetry))
    freeze,first,win=p['freezeFrame'],p['firstFireFrame'],p['winFrame']
    assert 0<freeze<first<win<total
    observed_freeze=float(telemetry[freeze]['leading_progress'])
    assert .75<=p['freezeThreshold']<=.825 and p['freezeThreshold']<=observed_freeze<=p['freezeThreshold']+.003
    assert all(float(telemetry[i]['leading_progress'])>float(telemetry[i-1]['leading_progress']) for i in range(1,freeze))
    freeze_events=[e for e in events if e['event']=='freeze'];assert len(freeze_events)==1
    assert int(freeze_events[0]['frame'])==freeze and freeze_events[0]['detail']=='real_button'
    assert telemetry[freeze-1]['freeze_charge']=='1' and telemetry[freeze]['freeze_charge']=='0'
    assert all(t['freeze_charge']=='0' for t in telemetry[freeze:])
    paused=telemetry[freeze:first];assert len(paused)==21,'21 native frozen idle frames required'
    assert all(t['frozen']=='True' and t['leader_x']==paused[0]['leader_x'] and t['leader_y']==paused[0]['leader_y'] for t in paused)
    shots=[e for e in events if e['event'] in ('fire','rainbow_fire')]
    completed=[e for e in events if e['event']=='flight_complete']
    assert len(shots)==len(completed)==arrow_count and len({e['arrow'] for e in shots})==arrow_count
    assert len({e['arrow'] for e in completed})==arrow_count
    assert sorted(e['arrow'] for e in shots)==sorted(e['arrow'] for e in completed)
    assert min(int(e['frame']) for e in completed)>first and max(int(e['frame']) for e in completed)<=win
    assert int(shots[0]['frame'])==first
    shot_frames=[int(e['frame']) for e in shots]
    assert all(b-a>=10 for a,b in zip(shot_frames,shot_frames[1:])),'Shots must be visibly spaced'
    assert p['cadenceFrames']>=10
    assert all(e['event']=='fire' for e in shots[:-1]) and shots[-1]['event']=='rainbow_fire'
    assert int(shots[-1]['arrow'])==reserved
    assert shots[-2]['remaining']=='1' and shots[-1]['remaining']=='0'
    hold=shot_frames[-1]-shot_frames[-2]
    assert hold>=32 and p['lastHoldFrames']>=32,'Reserved natural Rainbow needs a separate hold'
    assert [int(e['remaining']) for e in shots]==list(range(arrow_count-1,-1,-1))
    normal_flights=[e for e in completed if int(e['arrow'])!=reserved]
    rainbow_flights=[e for e in completed if int(e['arrow'])==reserved]
    assert len(rainbow_flights)==1 and int(rainbow_flights[0]['frame'])==win,'Rainbow must be the final completed flight'
    assert max(int(e['frame']) for e in normal_flights)<=shot_frames[-1],'Finish ordinary flights before the separate Rainbow'
    map_path=ROOT/g['mapAsset'];map_text=map_path.read_text(encoding='utf-8-sig')
    width=int(re.search(r'_gridWidth: (\d+)',map_text)[1]);height=int(re.search(r'_gridHeight: (\d+)',map_text)[1])
    remaining={a['index']:a for a in g['arrows']};assert len(remaining)==arrow_count
    assert {int(e['arrow']) for e in shots}==set(remaining),'Capture event IDs must match this generated layout'
    step={'Up':(0,1),'Right':(1,0),'Down':(0,-1),'Left':(-1,0)}
    for event in shots:
        index=int(event['arrow']);assert index in remaining,'Repeated or unknown arrow ID'
        arrow=remaining[index];dx,dy=step[arrow['direction']];head=arrow['points'][0]
        assert event['detail']=='weight='+str(arrow['weight']),'Event/geometry arrow identity mismatch'
        x,y=head['x']+dx,head['y']+dy
        occupied={(p['x'],p['y']) for a in remaining.values() for p in a['points']}
        while 0<=x<width and 0<=y<height:
            assert (x,y) not in occupied,f'Illegal source shot{index}';x+=dx;y+=dy
        del remaining[index]
    damage=[e for e in events if e['event']=='projectile_damage']
    kills=[e for e in events if e['event']=='projectile_kill']
    cleanup=[e for e in events if e['event'] in ('cleanup_kill','cleanup_damage')]
    assert not cleanup and p['cleanupKills']==0,'Every enemy must die from actual shots'
    assert len(damage)==p['projectileHits'] and len(damage)>0
    assert len(kills)==p['projectileKills']==p['spawnedMobs']>0
    assert {int(e['arrow']) for e in kills}==set(range(p['spawnedMobs'])),'Spawn/death ID coverage'
    assert len({e['arrow'] for e in kills})==len(kills),'Duplicate projectile death ID'
    assert not any(e['event']=='mob_finish' for e in events),'No enemy may finish the path'
    assert not any('damage=9999;' in e['detail'] for e in damage)
    paired={(e['frame'],e['arrow']) for e in damage}
    assert all((e['frame'],e['arrow']) in paired for e in kills),'Deaths must have paired projectile damage'
    rainbow_hits=[e for e in damage if e['detail'].startswith('damage=999;') and int(e['frame'])>=shot_frames[-1]]
    assert rainbow_hits,'Final production Rainbow must visibly hit enemies'
    assert all(t['lives']=='3' for t in telemetry)
    wins=[e for e in events if e['event']=='won'];assert len(wins)==1 and int(wins[0]['frame'])==win
    assert wins[0]['detail']=='production' and telemetry[win]['alive_mobs']=='0'
    assert max(int(e['frame']) for e in kills)<=win
    halfway=next(i for i,t in enumerate(telemetry[:freeze]) if float(t['leading_progress'])>=.5)
    assert .5<=float(telemetry[halfway]['leading_progress'])<.503
    old_t=rows(OLD/'telemetry.csv');old_p=read(OLD/'provenance.json')
    old_observed_freeze=float(old_t[old_p['freezeFrame']]['leading_progress'])
    earlier_seconds=(old_observed_freeze-observed_freeze)*g['pathLength']/g['baseMobSpeed']
    assert earlier_seconds>0,'Freeze must occur earlier in the native path traversal'
    opening_count=int(telemetry[halfway]['alive_mobs']);old_opening=int(old_t[506]['alive_mobs'])
    freeze_count=int(telemetry[freeze]['alive_mobs']);old_freeze=int(old_t[old_p['freezeFrame']]['alive_mobs'])
    peak=max(int(t['alive_mobs']) for t in telemetry)
    assert opening_count<old_opening and freeze_count<old_freeze==23 and peak<old_freeze,'Reduced enemy density required'
    paths=sorted(source.glob('frame_*.jpg'));assert len(paths)==total,'Missing or extra full-source frames'
    files=[]
    for i in range(total):
        path=source/f'frame_{i:05d}.jpg'
        with Image.open(path) as im:assert im.size==(1080,1920),path.name
        files.append({'file':path.name,'bytes':path.stat().st_size,'sha256':sha(path)})
    assert sha(OLD_VIDEO)==OLD_SHA,'Protected original changed during verification'
    proof={'passed':True,'revision':'Defense R2','frames':total,'allFramesSaved':True,'width':1080,'height':1920,'fps':30,
        'arrows':arrow_count,'legalShotReplay':arrow_count,'shotOrder':[int(e['arrow']) for e in shots],'grid':[width,height],
        'freezeFrame':freeze,'firstShotFrame':first,'rainbowFrame':shot_frames[-1],'winFrame':win,
        'naturalApproachStrictlyIncreasing':True,'freezeCharge':[1,0],'configuredFreezeThreshold':p['freezeThreshold'],
        'leadingProgressAtFreeze':observed_freeze,'previousLeadingProgressAtFreeze':old_observed_freeze,
        'freezeEarlierInNaturalTraversalSeconds':earlier_seconds,
        'leadingMobFrozenPositionVerifiedFrames':len(paused),'minimumShotSpacingFrames':min(b-a for a,b in zip(shot_frames,shot_frames[1:])),
        'rainbowNaturalPromotion':True,'reservedRainbowArrow':reserved,'rainbowHoldFrames':hold,'rainbowDamageEvents':len(rainbow_hits),
        'ordinaryFlightsCompletedBeforeRainbow':True,'rainbowIsFinalCompletedFlight':True,
        'completedFlights':arrow_count,'damageEventsExcludingCleanup':len(damage),'spawnedMobs':p['spawnedMobs'],
        'projectileKills':len(kills),'victoryCleanupKills':0,'mobFinishes':0,'lives':3,'remaining':0,
        'halfwayOpeningFrame':halfway,'halfwayLeadingProgress':float(telemetry[halfway]['leading_progress']),
        'halfwayOpeningAliveMobs':opening_count,'previousOpeningAliveMobs':old_opening,
        'freezeAliveMobs':freeze_count,'previousFreezeAliveMobs':old_freeze,'peakAliveMobs':peak,
        'sourceAudioRecorded':False,'generationSeed':p.get('generationSeed'),
        'sourceMapSha256':sha(map_path),'sourceGeometrySha256':sha(source/'geometry.json'),
        'sourceProvenanceSha256':sha(source/'provenance.json'),'originalDefenseVideoSha256':OLD_SHA,
        'claimLimit':'All spawned enemies have paired projectile deaths; no cleanup kill, forced outcome or enemy-finish witness in this capture.'}
    write(source/'frame_manifest.json',files);write(source/'native_capture_checks.json',proof)
    if output is not None:
        assert output.resolve()!=(MEDIA/'05_Reels/DefensePilot01_20261115').resolve(),'Original preview is protected'
        output.mkdir(parents=True,exist_ok=True);write(output/'native_capture_checks.json',proof)
    return proof

def main():
    p=argparse.ArgumentParser();p.add_argument('--source',required=True,type=Path);p.add_argument('--output',type=Path)
    a=p.parse_args();print(json.dumps(verify(a.source.resolve(),a.output),ensure_ascii=False,indent=2))
if __name__=='__main__':main()
