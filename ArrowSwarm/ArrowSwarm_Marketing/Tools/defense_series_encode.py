"""Bounded-CPU video encoding while Unity records other series episodes."""
import subprocess
import render_reference_reel as m


def encode_episode(ep, ff):
    comment = (f'Genuine Unity Defense{ep.level}; no Freeze. ' +
               ('One actual charged Tip and its highlighted shot. ' if ep.tip_used else 'Normal legal play, no skills. ') +
               'Sequential arrows, natural final Rainbow, every enemy killed by projectiles. ' +
               'Both native HUDs preserved; rates labelled. Editorial audio; store mock.')
    args = [ff, '-hide_banner', '-y', '-f', 'rawvideo', '-pixel_format', 'rgb24',
            '-video_size', '1080x1920', '-framerate', '30', '-i', 'pipe:0',
            '-i', str(ep.out/'editorial_mix.wav'), '-filter_threads', '2',
            '-vf', 'scale=iw:ih:in_range=pc:out_range=tv:out_color_matrix=bt709,format=yuv420p',
            '-c:v', 'libx264', '-threads', '2', '-preset', 'fast', '-crf', '18', '-r', '30',
            '-color_range', 'tv', '-colorspace', 'bt709', '-color_primaries', 'bt709', '-color_trc', 'bt709',
            '-c:a', 'aac', '-b:a', '192k', '-ar', '48000', '-ac', '2',
            '-af', 'loudnorm=I=-16:TP=-1.5:LRA=8', '-t', str(ep.seconds),
            '-movflags', '+faststart', '-metadata', 'comment='+comment, str(ep.final)]
    ep.write('ffmpeg_command.json', args)
    with (ep.out/'encode.log').open('w', encoding='utf-8') as log:
        process = subprocess.Popen(args, stdin=subprocess.PIPE, stderr=log)
        try:
            for index in range(ep.seconds*30):
                process.stdin.write(ep.frame(index/30).tobytes())
                if index % 90 == 0:
                    print(f'L{ep.level:02d}: encoded input {index}/{ep.seconds*30}', flush=True)
            process.stdin.close()
            code = process.wait()
        except BaseException:
            process.kill()
            raise
    assert code == 0, 'Encoding failed; see episode encode.log'
    return ep.final
