"""Match perceived loudness across the five finals without changing video packets."""
import json,subprocess
import build_remaining_videos as b
def loudness(p):
    r=subprocess.run([b.FF,'-hide_banner','-i',str(p),'-af','loudnorm=I=-18:TP=-2:LRA=11:print_format=json','-f','null','-'],capture_output=True,text=True,encoding='utf8',errors='replace',check=True)
    return json.JSONDecoder().raw_decode(r.stderr[r.stderr.rfind('{'):])[0]
def video_hash(p):return subprocess.check_output([b.FF,'-v','error','-i',str(p),'-map','0:v:0','-c','copy','-f','hash','-hash','sha256','-']).decode().strip()
def main():
    manifest=b.m.read(b.OUT/'manifest.json')
    for row in manifest['items']:
        p=b.OUT/row['file'];report=b.OUT/(p.stem+'_audio.json')
        if report.exists() and b.m.read(report).get('finalSha256')==b.m.sha(p):continue
        before=loudness(p);gain=min(-18-float(before['input_i']),-2.5-float(before['input_tp']));vh=video_hash(p)
        tmp=b.OUT/(p.stem+'_audio_master.mp4')
        b.m.run([b.FF,'-y','-i',str(p),'-map','0:v:0','-map','0:a:0','-c:v','copy','-af',f'volume={gain:.5f}dB','-c:a','aac','-b:a','160k','-movflags','+faststart',str(tmp)],p.stem+'_master')
        assert video_hash(tmp)==vh
        p.replace(b.OUT/(p.stem+'_pre_audio.mp4'));tmp.replace(p);after=loudness(p)
        assert abs(float(after['input_i'])+18)<.4 and float(after['input_tp'])<-1.5
        row['qa'].update(bytes=p.stat().st_size,sha256=b.m.sha(p))
        b.dump(report,{'passed':True,'gainDb':gain,'before':before,'after':after,'encodedVideoPacketsUnchanged':True,'videoPacketHash':vh,'finalSha256':b.m.sha(p)})
        b.dump(b.OUT/(p.stem+'_manifest.json'),row);print(p.name,after['input_i'],after['input_tp'],flush=True)
    b.dump(b.OUT/'manifest.json',manifest)
if __name__=='__main__':main()
