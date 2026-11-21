"""Loopback-only preview of the five new Defense reels and their source comparisons."""
from http.server import ThreadingHTTPServer
from pathlib import Path
import argparse
import functools

from serve_reel_review import ReviewHandler


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--port', type=int, default=8199)
    args = parser.parse_args()
    directory = Path(__file__).resolve().parents[1] / '05_Reels/DefenseSeries01_20261115'
    handler = functools.partial(ReviewHandler, directory=str(directory))
    print(f'Preview: http://127.0.0.1:{args.port}/index.html', flush=True)
    with ThreadingHTTPServer(('127.0.0.1', args.port), handler) as server:
        try:
            server.serve_forever()
        except KeyboardInterrupt:
            pass


if __name__ == '__main__':
    main()
