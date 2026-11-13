"""Encode the complete genuine image sequence, without editorial captions/audio."""
from pathlib import Path
import argparse, json, re, subprocess
import render_reel as media

def main():
    p=argparse.ArgumentParser();p.add_argument("--source",required=True,type=Path);a=p.parse_args()
    source=a.source.resolve();provenance=json.loads((source/"provenance.json").read_text(encoding="utf-8-sig"))
    frames=int(provenance["frames"]);fps=int(provenance["fps"])
    assert (provenance["width"],provenance["height"],fps)==(1080,1920,30)
    assert all((source/f"frame_{i:05d}.jpg").is_file() for i in range(frames))
    ffmpeg=media.imageio_ffmpeg.get_ffmpeg_exe();video=source/"raw_gameplay_1080x1920_30fps_SILENT.mp4"
    command=[ffmpeg,"-hide_banner","-y","-framerate",str(fps),"-i",str(source/"frame_%05d.jpg"),"-frames:v",str(frames),"-vf","scale=iw:ih:out_color_matrix=bt709:out_range=tv,format=yuv420p","-c:v","libx264","-preset","medium","-crf","18","-pix_fmt","yuv420p","-color_range","tv","-colorspace","bt709","-color_primaries","bt709","-color_trc","bt709","-an","-movflags","+faststart","-metadata","comment=Real Unity gameplay capture; all recorded frames at30fps. No audio was captured. No editorial overlays.",str(video)]
    (source/"raw_encode_command.json").write_text(json.dumps(command,indent=2),encoding="utf-8")
    media.run(command,source/"raw_encode.log")
    decoded=media.run([ffmpeg,"-hide_banner","-i",str(video),"-map","0:v:0","-f","null","-","-progress","pipe:1"],source/"raw_decode.log")
    counts=re.findall(r"^frame=(\d+)$",decoded.stdout,re.M);assert int(counts[-1])==frames
    assert all(token in decoded.stderr for token in ["1080x1920","h264","yuv420p"])
    assert "Audio:" not in decoded.stderr
    report={"passed":True,"file":video.name,"frames":frames,"fps":fps,"durationSeconds":frames/fps,"width":1080,"height":1920,"videoCodec":"h264","pixelFormat":"yuv420p","audio":"None: screen-frame capture did not record Unity audio","editorialOverlay":False,"allSourceFramesPreservedInOrder":True,"bytes":video.stat().st_size,"sha256":media.sha(video),"sourceProvenanceSha256":media.sha(source/"provenance.json")}
    (source/"raw_video_QA.json").write_text(json.dumps(report,indent=2),encoding="utf-8")
    print(json.dumps(report,indent=2),flush=True)

if __name__=="__main__":main()
