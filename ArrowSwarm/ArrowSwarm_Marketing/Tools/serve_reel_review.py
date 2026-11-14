"""Loopback-only pilot preview with byte ranges for reliable HTML5 seeking."""
from http.server import SimpleHTTPRequestHandler,ThreadingHTTPServer
from pathlib import Path
import argparse,functools,re

class ReviewHandler(SimpleHTTPRequestHandler):
    def list_directory(self,path):
        self.send_error(403,'Directory listing disabled');return None

    def send_head(self):
        self._remaining=None
        path=Path(self.translate_path(self.path))
        if path.is_dir():path=path/'index.html'
        root=Path(self.directory).resolve()
        if not path.resolve().is_relative_to(root) or not path.is_file():
            self.send_error(404);return None
        f=path.open('rb');size=path.stat().st_size;start,end=0,size-1
        header=self.headers.get('Range')
        if header:
            m=re.fullmatch(r'bytes=(\d*)-(\d*)',header)
            if not m or not any(m.groups()):
                f.close();self.send_error(416);return None
            if not m[1]:start=max(0,size-int(m[2]))
            else:
                start=int(m[1]);end=min(end,int(m[2])) if m[2] else end
            if start>=size or end<start:
                f.close();self.send_response(416);self.send_header('Content-Range',f'bytes */{size}');self.end_headers();return None
        self.send_response(206 if header else 200)
        self.send_header('Content-Type',self.guess_type(str(path)))
        self.send_header('Accept-Ranges','bytes')
        self.send_header('Cache-Control','no-store')
        self.send_header('Content-Length',str(end-start+1))
        if header:self.send_header('Content-Range',f'bytes {start}-{end}/{size}')
        self.end_headers();f.seek(start);self._remaining=end-start+1
        return f

    def copyfile(self,source,outputfile):
        remaining=self._remaining
        try:
            while remaining:
                chunk=source.read(min(128*1024,remaining))
                if not chunk:break
                outputfile.write(chunk);remaining-=len(chunk)
        except (BrokenPipeError,ConnectionResetError):pass

def main():
    p=argparse.ArgumentParser();p.add_argument('--port',type=int,default=8191)
    p.add_argument('--revision',choices=['original','r2','r3','series','short','defense'],default='r3');a=p.parse_args()
    folder={'original':'ReferencePilot02_20261114','r2':'ReferencePilot02_R2_20261114','r3':'ReferencePilot02_R3_20261114','series':'ShapesSeries_20261114','short':'CreativeShort01_20261114','defense':'DefensePilot01_20261115'}[a.revision]
    directory=Path(__file__).resolve().parents[1]/'05_Reels'/folder
    handler=functools.partial(ReviewHandler,directory=str(directory))
    print(f'Preview: http://127.0.0.1:{a.port}/index.html',flush=True)
    with ThreadingHTTPServer(('127.0.0.1',a.port),handler) as server:
        try:server.serve_forever()
        except KeyboardInterrupt:pass
if __name__=='__main__':main()
