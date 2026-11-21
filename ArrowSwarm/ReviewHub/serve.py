"""Local review hub: discover current/future HTML previews and serve their relative media."""
from pathlib import Path
from http.server import ThreadingHTTPServer
from urllib.parse import unquote, urlsplit, quote
import argparse, functools, json, re, sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'ArrowSwarm_Marketing/Tools'))
from serve_reel_review import ReviewHandler

MOUNTS = {'captures': ROOT / 'Captures', 'marketing': ROOT / 'ArrowSwarm_Marketing'}
NAMES = {'AudioReview': 'Ses kütüphanesi ve değerlendirme', 'AudioRevisions': 'Ses revizeleri',
 'Revision6': 'Tasarım referansları · Önce / Sonra', 'DefensePilot02_20261115': 'Defense R2 · 21s · Onaylı kurgu',
 'DefenseSeries01_20261115': 'Beş yeni Defense videosu · Normal / Tip', 'StoreSet_EN_20261115': '8 İngilizce mağaza görseli',
 'ShapesSeries_20261114': 'Shapes video serisi', 'CreativeShort01_20261114': 'Shapes · Kısa yaratıcı kurgu'}

def catalog():
    items = []
    for mount, root in MOUNTS.items():
        for path in root.rglob('*.html'):
            rel = path.relative_to(root)
            if any(part.startswith('.') or part.lower() in ('tools', 'node_modules', '__pycache__') for part in rel.parts): continue
            if 'template' in path.name.lower(): continue
            text = path.read_text(encoding='utf-8-sig', errors='replace')
            title = re.search(r'<title[^>]*>(.*?)</title>', text, re.I | re.S)
            name = NAMES.get(path.parent.name, re.sub(r'\s+', ' ', title[1]).strip() if title else path.parent.name)
            if path.name != 'index.html': name += ' · ' + path.stem
            group = 'Sesler' if 'audio' in str(rel).lower() else ('Videolar' if '05_Reels' in rel.parts else ('Mağaza ve pazarlama' if mount == 'marketing' else 'Oyun ve tasarım'))
            items.append({'name': name, 'group': group, 'path': str(rel).replace('\\', '/'),
                'url': '/' + mount + '/' + quote(rel.as_posix()), 'modified': path.stat().st_mtime})
    return sorted(items, key=lambda x: (x['group'], -x['modified'], x['name']))

class Handler(ReviewHandler):
    def do_GET(self):
        if urlsplit(self.path).path == '/catalog.json':
            data = json.dumps(catalog(), ensure_ascii=False).encode()
            self.send_response(200); self.send_header('Content-Type', 'application/json; charset=utf-8')
            self.send_header('Cache-Control', 'no-store'); self.send_header('Content-Length', str(len(data)))
            self.end_headers(); self.wfile.write(data); return
        super().do_GET()

    def translate_path(self, value):
        route = unquote(urlsplit(value).path).lstrip('/')
        if route in ('', 'index.html'): return str(ROOT / 'ReviewHub/index.html')
        # Existing audio reports resolve ../../Assets paths; expose only public game audio.
        audio_root = (ROOT / 'Assets/_Project/Audio').resolve()
        if route.startswith('Assets/_Project/Audio/'):
            audio = (ROOT / route).resolve()
            if audio.is_relative_to(audio_root) and audio.suffix.lower() in ('.wav', '.ogg', '.mp3'):
                return str(audio)
        mount, _, rest = route.partition('/')
        base = MOUNTS.get(mount)
        if base:
            target = (base / rest).resolve()
            if target.is_relative_to(base.resolve()) and not any(x.startswith('.') for x in Path(rest).parts):
                return str(target)
        return str(ROOT / 'ReviewHub/__not_available__')

if __name__ == '__main__':
    parser = argparse.ArgumentParser(); parser.add_argument('--port', type=int, default=8200); args = parser.parse_args()
    print(f'Arrow Swarm Hub: http://127.0.0.1:{args.port}/', flush=True)
    with ThreadingHTTPServer(('127.0.0.1', args.port), functools.partial(Handler, directory=str(ROOT))) as server:
        server.serve_forever()
