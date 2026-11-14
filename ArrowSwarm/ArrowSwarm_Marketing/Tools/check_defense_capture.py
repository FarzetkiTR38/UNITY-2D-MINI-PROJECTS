"""Verify genuine Defense actions, natural approach, charged freeze and source files."""
import argparse,csv,hashlib,json,re
from pathlib import Path
from render_reference_reel import Image

ROOT=Path(__file__).resolve().parents[2];MEDIA=ROOT/'ArrowSwarm_Marketing'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def rows(p):
    with p.open(encoding='utf-8-sig',newline='') as f:return list(csv.DictReader(f))
def main():
    p=argparse.ArgumentParser();p.add_argument('--source',required=True,type=Path);a=p.parse_args();source=a.source.resolve()
    provenance=read(source/'provenance.json');geometry=read(source/'geometry.json');events=rows(source/'events.csv');telemetry=rows(source/'telemetry.csv')
    assert provenance['won'] and provenance['lives']==3 and provenance['remaining']==0
    assert (provenance['width'],provenance['height'],provenance['fps'])==(1080,1920,30)
    assert len(telemetry)==provenance['frames']==1140
    freeze=provenance['freezeFrame'];first=provenance['firstFireFrame'];win=provenance['winFrame']
    assert freeze<first<win and telemetry[freeze-1]['freeze_charge']=='1' and telemetry[freeze]['freeze_charge']=='0'
    assert float(telemetry[freeze]['leading_progress'])>=provenance['freezeThreshold']
    assert all(float(telemetry[i]['leading_progress'])>float(telemetry[i-1]['leading_progress']) for i in range(1,freeze))
    paused=telemetry[freeze:first];assert len(paused)==21
    assert all(t['frozen']=='True' and t['leader_x']==paused[0]['leader_x'] and t['leader_y']==paused[0]['leader_y'] for t in paused)
    shots=[e for e in events if e['event'] in ('fire','rainbow_fire')];completed=[e for e in events if e['event']=='flight_complete']
    assert len(shots)==len(completed)==provenance['arrows']==15 and len({e['arrow'] for e in shots})==15
    assert sorted(e['arrow'] for e in shots)==sorted(e['arrow'] for e in completed)
    assert max(int(e['frame']) for e in completed)<=win and shots[-1]['event']=='rainbow_fire'
    maptext=(ROOT/geometry['mapAsset']).read_text();width=int(re.search(r'_gridWidth: (\d+)',maptext)[1]);height=int(re.search(r'_gridHeight: (\d+)',maptext)[1])
    remaining={r['index']:r for r in geometry['arrows']};step={'Up':(0,1),'Right':(1,0),'Down':(0,-1),'Left':(-1,0)}
    for event in shots:
        index=int(event['arrow']);arrow=remaining[index];dx,dy=step[arrow['direction']];head=arrow['points'][0];x,y=head['x']+dx,head['y']+dy
        occupied={(p['x'],p['y']) for r in remaining.values() for p in r['points']}
        while 0<=x<width and 0<=y<height:
            assert (x,y) not in occupied,f'Illegal source shot{index}';x+=dx;y+=dy
        del remaining[index]
    damage=[e for e in events if e['event']=='projectile_damage'];kills=[e for e in events if e['event']=='projectile_kill'];cleanup=[e for e in events if e['event']=='cleanup_kill']
    assert len(damage)==provenance['projectileHits']==78 and len(kills)==provenance['projectileKills']==22
    assert len(cleanup)==1 and not any(e['event']=='mob_finish' for e in events)
    assert all('damage=9999' not in e['detail'] for e in damage)
    files=[]
    for i in range(provenance['frames']):
        path=source/f'frame_{i:05d}.jpg'
        with Image.open(path) as im:assert im.size==(1080,1920)
        files.append({'file':path.name,'bytes':path.stat().st_size,'sha256':sha(path)})
    (source/'frame_manifest.json').write_text(json.dumps(files,indent=2),encoding='utf-8')
    proof={'passed':True,'frames':len(files),'legalShotReplay':15,'grid':[width,height],'freezeFrame':freeze,'firstShotFrame':first,'winFrame':win,
        'naturalApproachStrictlyIncreasing':True,'freezeCharge':[1,0],'leadingProgressAtFreeze':float(telemetry[freeze]['leading_progress']),
        'leadingMobFrozenPositionVerifiedFrames':len(paused),'rainbowFromProductionLastArrow':True,'completedFlights':15,
        'damageEventsExcludingCleanup':len(damage),'projectileKills':len(kills),'victoryCleanupKills':len(cleanup),'mobFinishes':0,'lives':3,
        'sourceAudioRecorded':False,'sourceMapSha256':sha(ROOT/geometry['mapAsset']),
        'claimLimit':'One surviving front mob is cleared by the normal victory cleanup. Do not attribute it to a projectile or say rainbow kills every enemy.'}
    (source/'native_capture_checks.json').write_text(json.dumps(proof,indent=2),encoding='utf-8')
    out=MEDIA/'05_Reels/DefensePilot01_20261115';out.mkdir(parents=True,exist_ok=True)
    (out/'native_capture_checks.json').write_text(json.dumps(proof,indent=2),encoding='utf-8')
    print(json.dumps(proof,indent=2))
if __name__=='__main__':main()
