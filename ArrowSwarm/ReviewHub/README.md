# Arrow Swarm preview hub

Start from the ArrowSwarm project directory:

```powershell
py -3.13 ReviewHub/serve.py --port 8200
```

Open http://127.0.0.1:8200/ . The hub discovers HTML files under `Captures/` and `ArrowSwarm_Marketing/`, including ignored local capture galleries. Templates and tool folders are excluded. Relative images/audio/video continue working through the same server; byte ranges support seeking. The server binds only to loopback and does not expose the repository root or private preference backups.

New previews: save their HTML and relative media under either discovery directory. They appear on refresh or within30seconds, without editing the hub. Use descriptive HTML titles. Do not relocate existing previews or their assets. New outputs should always be linked through this hub in future deliveries. Existing review servers/URLs remain intact.

Current video series files are present and independent encoded checks pass; its final browser/delivery checkpoint is tracked separately in `ArrowSwarm_Marketing/05_Reels/DEFENSE_SERIES_01.md`. Creating the hub does not claim that pending video delivery work has been completed.
