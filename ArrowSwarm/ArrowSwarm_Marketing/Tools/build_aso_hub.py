"""Build self-contained 07_ASO_SEO_GEO/index.html copy-paste hub for Google Play Console."""
from __future__ import annotations

import csv
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "07_ASO_SEO_GEO"


def main() -> None:
    meta = json.loads((ROOT / "store_metadata.json").read_text(encoding="utf-8"))
    qa = json.loads((ROOT / "ASO_QA.json").read_text(encoding="utf-8"))
    kw_rows = list(csv.DictReader((ROOT / "KEYWORD_MAP.csv").open(encoding="utf-8-sig")))

    payload = {
        "languages": meta["languages"],
        "qa": qa["metadata"],
        "keywords": kw_rows,
    }
    payload_json = json.dumps(payload, ensure_ascii=False)

    html = f"""<!doctype html>
<html lang="tr">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>ArrowSwarm — ASO, SEO &amp; GEO Play Console Merkezi (10 Dil)</title>
  <style>
    :root {{
      --bg: #080d1a;
      --card: #0e1629;
      --card2: #121d36;
      --border: rgba(0, 212, 255, 0.24);
      --text: #eaf6ff;
      --muted: #8ea9c4;
      --cyan: #00d4ff;
      --gold: #ffe066;
      --green: #3ddc97;
      --purple: #b56bff;
    }}
    * {{ box-sizing: border-box; }}
    body {{
      margin: 0;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
      background: radial-gradient(circle at top right, rgba(181, 107, 255, 0.12), transparent 40%), var(--bg);
      color: var(--text);
      line-height: 1.55;
      padding: 24px 16px 64px;
    }}
    .wrap {{ max-width: 1120px; margin: 0 auto; }}
    .header {{
      background: var(--card);
      border: 1px solid var(--border);
      border-radius: 16px;
      padding: 24px;
      margin-bottom: 24px;
      display: flex;
      flex-wrap: wrap;
      justify-content: space-between;
      align-items: center;
      gap: 16px;
    }}
    .header h1 {{ margin: 0 0 6px; font-size: 1.6rem; color: var(--cyan); }}
    .header p {{ margin: 0; color: var(--muted); font-size: 0.94rem; }}
    .pills {{ display: flex; flex-wrap: wrap; gap: 8px; margin-top: 10px; }}
    .pill {{
      padding: 4px 10px;
      border-radius: 999px;
      font-size: 0.78rem;
      font-weight: 600;
      background: rgba(61, 220, 151, 0.12);
      border: 1px solid rgba(61, 220, 151, 0.35);
      color: var(--green);
    }}
    .grid-2 {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 18px; margin-bottom: 24px; }}
    .card {{
      background: var(--card);
      border: 1px solid var(--border);
      border-radius: 16px;
      padding: 20px;
      margin-bottom: 20px;
    }}
    .card h2 {{ margin: 0 0 14px; font-size: 1.2rem; color: var(--cyan); }}
    .card h3 {{ margin: 16px 0 8px; font-size: 1rem; color: var(--gold); }}
    .url-row {{
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 10px;
      background: var(--card2);
      border: 1px solid rgba(255,255,255,0.08);
      padding: 10px 12px;
      border-radius: 10px;
      margin-bottom: 10px;
      font-size: 0.88rem;
      word-break: break-all;
    }}
    .url-label {{ color: var(--muted); font-size: 0.78rem; display: block; }}
    .tabs {{ display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 16px; }}
    .tab {{
      padding: 8px 14px;
      border-radius: 10px;
      border: 1px solid var(--border);
      background: var(--card2);
      color: var(--text);
      cursor: pointer;
      font-weight: 600;
      font-size: 0.88rem;
    }}
    .tab.active {{
      background: linear-gradient(135deg, #00d4ff, #8a5cf6);
      color: #060a14;
      border-color: transparent;
    }}
    .field-box {{
      background: var(--card2);
      border: 1px solid rgba(255,255,255,0.09);
      border-radius: 12px;
      padding: 14px;
      margin-bottom: 14px;
    }}
    .field-head {{
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
      gap: 10px;
      flex-wrap: wrap;
    }}
    .field-title {{ font-weight: 700; color: var(--gold); font-size: 0.92rem; }}
    .count {{ font-size: 0.8rem; color: var(--green); font-family: monospace; }}
    pre {{
      margin: 0;
      white-space: pre-wrap;
      word-break: break-word;
      font-family: inherit;
      font-size: 0.94rem;
      color: var(--text);
    }}
    button.copy-btn {{
      padding: 6px 12px;
      border-radius: 8px;
      border: 1px solid var(--cyan);
      background: rgba(0, 212, 255, 0.12);
      color: var(--cyan);
      font-weight: 600;
      font-size: 0.82rem;
      cursor: pointer;
      shrink: 0;
    }}
    button.copy-btn.copied {{
      background: var(--green);
      border-color: var(--green);
      color: #06130d;
    }}
    table {{ width: 100%; border-collapse: collapse; font-size: 0.86rem; }}
    th, td {{ text-align: left; padding: 8px 10px; border-bottom: 1px solid rgba(255,255,255,0.08); }}
    th {{ color: var(--cyan); }}
    td {{ color: var(--muted); }}
    td strong {{ color: var(--text); }}
    ul {{ margin: 8px 0 0; padding-left: 20px; color: var(--muted); font-size: 0.9rem; }}
    li {{ margin-bottom: 6px; }}
    li strong {{ color: var(--text); }}
  </style>
</head>
<body>
<div class="wrap">
  <div class="header">
    <div>
      <h1>ArrowSwarm — ASO, SEO &amp; GEO Play Console Merkezi</h1>
      <p>10 oyun içi dil için doğrulanmış Play Store metinleri, Veri Güvenliği (Data Safety) form cevapları ve GEO sinyalleri.</p>
      <div class="pills">
        <span class="pill">10 Dil Doğrulandı (PASS)</span>
        <span class="pill">farzetkigames.com.tr Canlı</span>
        <span class="pill">31 Anahtar Kelime Niyeti</span>
      </div>
    </div>
  </div>

  <div class="grid-2">
    <div class="card">
      <h2>Play Console Bağlantıları (Tek Tık Kopyala)</h2>
      <div class="url-row">
        <div><span class="url-label">Gizlilik Politikası (App Content)</span>https://farzetkigames.com.tr/arrowswarm/privacy-policy</div>
        <button class="copy-btn" onclick="copyText(this, 'https://farzetkigames.com.tr/arrowswarm/privacy-policy')">Kopyala</button>
      </div>
      <div class="url-row">
        <div><span class="url-label">Veri Silme / İletişim Formu</span>https://farzetkigames.com.tr/contact</div>
        <button class="copy-btn" onclick="copyText(this, 'https://farzetkigames.com.tr/contact')">Kopyala</button>
      </div>
      <div class="url-row">
        <div><span class="url-label">Geliştirici Web Sitesi (Store Settings — app-ads.txt için kök adres)</span>https://farzetkigames.com.tr</div>
        <button class="copy-btn" onclick="copyText(this, 'https://farzetkigames.com.tr')">Kopyala</button>
      </div>
      <div class="url-row">
        <div><span class="url-label">Paket Adı (Package Name)</span>com.farzetkigames.arrowswarm</div>
        <button class="copy-btn" onclick="copyText(this, 'com.farzetkigames.arrowswarm')">Kopyala</button>
      </div>
    </div>

    <div class="card">
      <h2>Veri Güvenliği (Data Safety) &amp; İçerik Beyanı</h2>
      <ul>
        <li><strong>Kategori &amp; Etiketler:</strong> Oyunlar → Bulmaca (Puzzle) · Etiketler: Puzzle, Logic, Single player, Casual.</li>
        <li><strong>Reklam Beyanı:</strong> Evet, reklam içeriyor (Google AdMob — Ödüllü + Geçiş).</li>
        <li><strong>Hedef Yaş:</strong> 13-15, 16-17, 18+ (13 yaş altı seçilmez; Aileler kapsamı dışı).</li>
        <li><strong>Veri Toplama:</strong> <strong>Evet</strong> (Unity Gaming Services + AdMob SDK). Aktarım şifreli (HTTPS): <strong>Evet</strong>. Silme talebi yöntemi: <strong>Evet</strong>.</li>
        <li><strong>İşaretlenecek Veri Türleri:</strong> Yaklaşık Konum (IP), Kullanıcı Kimlikleri / Takma Ad (UGS), Uygulama Etkileşimleri, Kilitlenme/Tanılama Günlükleri, Cihaz/Reklam Kimlikleri (Advertising ID).</li>
      </ul>
    </div>
  </div>

  <div class="card">
    <h2>Google Play Mağaza Metinleri — 10 Dil (TR, EN, DE, FR, ES, PT, IT, RU, JA, KO)</h2>
    <div id="tabs" class="tabs"></div>
    <div id="localeContent"></div>
  </div>

  <div class="card">
    <h2>SEO, GEO &amp; İlk 3 Mağaza Deneyi (Store Listing Experiments)</h2>
    <ul>
      <li><strong>Web SEO &amp; GEO:</strong> <code>farzetkigames.com.tr</code> üzerinde <code>sitemap.xml</code>, <code>llms.txt</code>, <code>robots.txt</code> AI crawler izinleri ve <code>VideoGame</code> + <code>FAQPage</code> JSON-LD yapılandırıldı.</li>
      <li><strong>ASO-01 (İlk Görsel Testi):</strong> Mevcut <code>01_Home</code> yerine doğrudan <code>03_Shapes</code> tahtasını 1. sıraya koyarak kurulum dönüşümü testi.</li>
      <li><strong>ASO-02 (İkon Testi):</strong> Seçili ikon vs. yüksek kontrastlı varyasyon testi.</li>
      <li><strong>ASO-03 (Kısa Açıklama Testi):</strong> 3 mod özeti vs. ok yönü/bulmaca kancası testi.</li>
    </ul>
    <h3>Anahtar Kelime Haritası Özeti (<span id="kwCount"></span> terim)</h3>
    <div style="overflow-x:auto">
      <table>
        <thead><tr><th>Dil</th><th>Terim</th><th>Niyet</th><th>Önerilen Alan</th><th>Kanıt</th></tr></thead>
        <tbody id="kwTable"></tbody>
      </table>
    </div>
  </div>
</div>

<script>
const DATA = {payload_json};
const ORDER = ['TR', 'EN', 'DE', 'FR', 'ES', 'PT', 'IT', 'RU', 'JA', 'KO'];
let active = 'TR';

function copyText(btn, text) {{
  navigator.clipboard.writeText(text).then(() => {{
    const prev = btn.textContent;
    btn.textContent = 'Kopyalandı!';
    btn.classList.add('copied');
    setTimeout(() => {{ btn.textContent = prev; btn.classList.remove('copied'); }}, 1400);
  }});
}}

function renderTabs() {{
  const el = document.getElementById('tabs');
  el.innerHTML = '';
  ORDER.forEach(code => {{
    const b = document.createElement('button');
    b.className = 'tab' + (code === active ? ' active' : '');
    b.textContent = code + ' (' + DATA.languages[code].locale + ')';
    b.onclick = () => {{ active = code; renderTabs(); renderLocale(); }};
    el.appendChild(b);
  }});
}}

function renderLocale() {{
  const item = DATA.languages[active];
  const counts = DATA.qa[active].counts;
  const wrap = document.getElementById('localeContent');
  wrap.innerHTML = '';

  const fields = [
    ['title', 'Uygulama Adı (App Name · maks 30)'],
    ['short', 'Kısa Açıklama (Short Description · maks 80)'],
    ['long', 'Uzun Açıklama (Full Description · maks 4000)']
  ];

  fields.forEach(([key, label]) => {{
    const box = document.createElement('div');
    box.className = 'field-box';
    const head = document.createElement('div');
    head.className = 'field-head';
    const t = document.createElement('span');
    t.className = 'field-title';
    t.textContent = label;
    const c = document.createElement('span');
    c.className = 'count';
    c.textContent = counts[key].unicodeCodePoints + ' / ' + counts[key].limit + ' kr · PASS';
    const btn = document.createElement('button');
    btn.className = 'copy-btn';
    btn.textContent = 'Kopyala';
    btn.onclick = () => copyText(btn, item[key]);
    const right = document.createElement('div');
    right.style.display = 'flex';
    right.style.alignItems = 'center';
    right.style.gap = '10px';
    right.appendChild(c);
    right.appendChild(btn);
    head.appendChild(t);
    head.appendChild(right);
    const pre = document.createElement('pre');
    pre.textContent = item[key];
    box.appendChild(head);
    box.appendChild(pre);
    wrap.appendChild(box);
  }});
}}

function renderKeywords() {{
  document.getElementById('kwCount').textContent = DATA.keywords.length;
  const tbody = document.getElementById('kwTable');
  DATA.keywords.forEach(row => {{
    const tr = document.createElement('tr');
    tr.innerHTML = `<td><strong>${{row.language}}</strong></td><td><strong>${{row.term}}</strong></td><td>${{row.intent}}</td><td>${{row.proposed_placement}}</td><td>${{row.product_evidence}}</td>`;
    tbody.appendChild(tr);
  }});
}}

renderTabs();
renderLocale();
renderKeywords();
</script>
</body>
</html>
"""
    (ROOT / "index.html").write_text(html, encoding="utf-8")
    print(f"Generated {ROOT / 'index.html'} ({len(html)} chars)")


if __name__ == "__main__":
    main()
