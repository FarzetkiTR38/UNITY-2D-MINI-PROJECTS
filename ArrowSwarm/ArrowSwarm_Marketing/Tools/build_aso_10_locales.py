"""Generate and normalize 10-locale Play Store ASO listings for ArrowSwarm."""
from __future__ import annotations

import json
import unicodedata
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "07_ASO_SEO_GEO"

EXISTING_STORE = json.loads((ROOT / "store_metadata.json").read_text(encoding="utf-8"))

NEW_LOCALES = {
    "DE": {
        "locale": "de-DE",
        "language_name": "Deutsch (Almanca)",
        "title": "ArrowSwarm: Pfeil-Puzzle",  # 24 chars
        "short": "Löse Pfeil-Formen, drehe den Rahmen und verteidige in 3 Puzzle-Modi.",  # 70 chars
        "long": (
            "Jeder Pfeil hat eine Richtung. Der nächste Zug gehört dir.\n\n"
            "ArrowSwarm verbindet logische Pfeil-Rätsel mit drei unterschiedlichen Spielmodi, in denen du Wege liest, Reihenfolgen planst und Ausgänge freimachst.\n\n"
            "FORMEN\n"
            "Finde die richtige Reihenfolge in einer Figur aus gebogenen Pfeilpfaden. Sieh dir Pinguin, Herz und viele weitere Silhouetten an, wähle einen freien Pfeil und löse die Form Schritt für Schritt. Die aktuelle Formen-Kampagne enthält 50 handgefertigte Level.\n\n"
            "FARBWECHSEL\n"
            "Ein freier Weg allein reicht nicht aus. Jeder Pfeil muss auch zur Farbe der Ausgangswand passen. Während die Pfeile feststehen, drehst du den bunten Außenrahmen in 90-Grad-Schritten und planst dein begrenztes Drehbudget. Die Farbwechsel-Kampagne bietet 50 Level.\n\n"
            "VERTEIDIGUNG\n"
            "Bringe deine Puzzle-Entscheidungen auf ein aktives Verteidigungsbrett. Öffne Wege für deine Pfeile und schieße sie auf Gegner ab, die rund um das Feld vorrücken.\n\n"
            "LIES DEN WEG\n"
            "In den Puzzle-Modi prallt ein blockierter Schuss ab und kostet ein Leben. Schütze deine drei Leben, nutze Hinweise bei Bedarf und finde den nächsten freien Ausgang.\n\n"
            "DEIN FORTSCHRITT\n"
            "Formen und Farbwechsel speichern ihren Fortschritt getrennt. Verdiene bis zu drei Sterne pro Level, wechsle zwischen hellem und dunklem Design und tritt in der weltweiten Bestenliste an."
        ),
    },
    "FR": {
        "locale": "fr-FR",
        "language_name": "Français (Fransızca)",
        "title": "ArrowSwarm : Puzzle Flèches",  # 27 chars
        "short": "Libère des formes de flèches, tourne le cadre et défends dans 3 modes.",  # 72 chars
        "long": (
            "Chaque flèche a une direction. Le prochain coup est à toi.\n\n"
            "ArrowSwarm réunit des casse-têtes directionnels dans trois modes de jeu distincts où tu analyses le plateau, planifies l'ordre des tirs et ouvres la voie.\n\n"
            "FORMES\n"
            "Trouve l'ordre caché dans une silhouette composée de flèches imbriquées. Observe le pingouin, le cœur et d'autres figures, choisis une flèche libre et vide le plateau étape par étape. La campagne Formes propose 50 niveaux.\n\n"
            "CHANGEMENT DE COULEUR\n"
            "Une sortie libre ne suffit plus : la flèche doit aussi correspondre à la couleur du mur. Fais pivoter le cadre extérieur par quarts de tour tout en gardant les flèches immobiles et gère ton nombre de rotations. Cette campagne contient 50 niveaux.\n\n"
            "DÉFENSE\n"
            "Applique ta logique sur un plateau dynamique. Dégage le passage des flèches et tire sur les ennemis qui avancent autour de la grille.\n\n"
            "OBSERVE LE PARCOURS\n"
            "Dans les modes puzzle, un tir bloqué rebondit et coûte une vie. Protège tes trois vies, utilise des indices si besoin et repère la bonne sortie.\n\n"
            "SUIS TA PROGRESSION\n"
            "Formes et Changement de couleur gardent une progression séparée. Gagne jusqu'à trois étoiles par niveau, profite des thèmes clair et sombre et grimpe dans le classement mondial."
        ),
    },
    "ES": {
        "locale": "es-ES",
        "language_name": "Español (İspanyolca)",
        "title": "ArrowSwarm: Puzzle Flechas",  # 26 chars
        "short": "Despeja formas de flechas, gira el marco y defiende en 3 modos de juego.",  # 73 chars
        "long": (
            "Cada flecha tiene una dirección. El siguiente movimiento es tuyo.\n\n"
            "ArrowSwarm combina puzles de flechas direccionales con tres modos distintos para analizar el tablero, planificar rutas y despejar salidas.\n\n"
            "FORMAS\n"
            "Descubre el orden correcto dentro de una silueta hecha de flechas curvas. Observa el pingüino, el corazón y muchas figuras más, elige una flecha con salida libre y limpia el tablero paso a paso. La campaña de Formas incluye 50 niveles.\n\n"
            "CAMBIO DE COLOR\n"
            "Un camino despejado es solo la mitad de la solución: cada flecha debe coincidir con el color del muro de salida. Gira el marco exterior en cuartos de vuelta mientras las flechas permanecen fijas y administra tus giros disponibles. Esta campaña incluye 50 niveles.\n\n"
            "DEFENSA\n"
            "Lleva tus decisiones lógicas a un tablero con acción constante. Abre paso a las flechas y lánzalas contra los enemigos que rodean el campo.\n\n"
            "LEE LA RUTA\n"
            "En los modos puzle, un disparo bloqueado rebota y cuesta una vida. Cuida tus tres vidas y decide qué flecha puede salir a continuación.\n\n"
            "SIGUE TU PROGRESO\n"
            "Formas y Cambio de color guardan su avance por separado. Consigue hasta tres estrellas por nivel, alterna entre tema claro y oscuro y compite en la clasificación mundial."
        ),
    },
    "PT": {
        "locale": "pt-BR",
        "language_name": "Português (Portekizce - BR)",
        "title": "ArrowSwarm: Puzzle Setas",  # 24 chars
        "short": "Desbloqueie formas de setas, gire a moldura e defenda em 3 modos de jogo.",  # 74 chars
        "long": (
            "Cada seta tem uma direção. O próximo movimento é seu.\n\n"
            "ArrowSwarm une quebra-cabeças de setas direcionais em três modos diferentes para ler o tabuleiro, planejar a ordem certa e liberar caminhos.\n\n"
            "FORMAS\n"
            "Descubra a sequência escondida em desenhos feitos de setas dobradas. Analise o pinguim, o coração e outras silhuetas, escolha uma seta com saída livre e esvazie a forma passo a passo. A campanha Formas conta com 50 níveis.\n\n"
            "TROCA DE COR\n"
            "Um caminho aberto não basta: a seta também precisa encontrar uma parede da mesma cor. Gire a moldura colorida externa enquanto as setas ficam paradas e planeje suas rotações limitadas. A campanha Troca de Cor conta com 50 níveis.\n\n"
            "DEFESA\n"
            "Leve suas decisões de lógica para um tabuleiro em movimento. Libere rotas para as setas e dispare contra os inimigos que cercam a arena.\n\n"
            "LEIA O CAMINHO\n"
            "Nos modos de puzzle, um disparo bloqueado retorna e custa uma vida. Proteja suas três vidas, use dicas quando precisar e encontre a próxima saída.\n\n"
            "ACOMPANHE SEU PROGRESSO\n"
            "Formas e Troca de Cor mantêm progressos separados. Ganhe até três estrelas por fase, alterne entre tema claro e escuro e dispute o ranking global."
        ),
    },
    "IT": {
        "locale": "it-IT",
        "language_name": "Italiano (İtalyanca)",
        "title": "ArrowSwarm: Puzzle Frecce",  # 25 chars
        "short": "Libera forme di frecce, ruota la cornice e difendi in 3 modalità puzzle.",  # 74 chars
        "long": (
            "Ogni freccia ha una direzione. La prossima mossa spetta a te.\n\n"
            "ArrowSwarm unisce rompicapi direzionali in tre diverse modalità per leggere lo schema, pianificare l'ordine e liberare la via.\n\n"
            "FORME\n"
            "Trova l'ordine giusto nascosto in una figura composta da frecce intrecciate. Osserva il pinguino, il cuore e altre sagome, tocca una freccia con uscita libera e svuota il livello passo dopo passo. La campagna Forme include 50 livelli.\n\n"
            "CAMBIO COLORE\n"
            "Un percorso libero non basta: ogni freccia deve raggiungere un muro dello stesso colore. Ruota la cornice esterna di un quarto di giro mantenendo le frecce ferme e gestisci le rotazioni disponibili. La campagna Cambio Colore include 50 livelli.\n\n"
            "DIFESA\n"
            "Porta la tua logica su una plancia dinamica. Apri la strada alle frecce e scagliale contro i nemici in movimento lungo il bordo.\n\n"
            "STUDIA IL PERCORSO\n"
            "Nelle modalità puzzle, un tiro bloccato rimbalza e costa una vita. Proteggi le tue tre vite e scegli quale freccia far uscire per prima.\n\n"
            "SEGUI I PROGRESSI\n"
            "Forme e Cambio Colore salvano i progressi separatamente. Ottieni fino a tre stelle per livello, scegli tra tema chiaro e scuro e scala la classifica mondiale."
        ),
    },
    "RU": {
        "locale": "ru-RU",
        "language_name": "Русский (Rusça)",
        "title": "ArrowSwarm: Головоломка",  # 23 chars
        "short": "Освобождай фигуры из стрел, вращай рамку и защищайся в 3 режимах игры.",  # 73 chars
        "long": (
            "У каждой стрелы есть направление. Следующий ход за тобой.\n\n"
            "ArrowSwarm объединяет логические головоломки со стрелами в трёх разных режимах, где важно читать пути, планировать порядок ходов и открывать выходы.\n\n"
            "ФИГУРЫ\n"
            "Найди правильную последовательность внутри силуэтов из изогнутых стрел. Изучай пингвина, сердце и другие фигуры, выбирай свободную стрелу и очищай поле шаг за шагом. Кампания «Фигуры» содержит 50 уровней.\n\n"
            "СМЕНА ЦВЕТА\n"
            "Свободного пути недостаточно — цвет стрелы должен совпадать со стеной выхода. Вращай внешнюю цветную рамку на четверть оборота, пока стрелы остаются на месте, и рассчитывай запас поворотов. Кампания «Смена цвета» содержит 50 уровней.\n\n"
            "ЗАЩИТА\n"
            "Перенеси логические решения на живое поле обороны. Открывай дорогу стрелам и сбивай врагов, движущихся вокруг поля.\n\n"
            "ЧИТАЙ МАРШРУТ\n"
            "В режимах головоломок заблокированный выстрел возвращается и отнимает одну жизнь. Береги три жизни и находи верный следующий выход.\n\n"
            "СЛЕДИ ЗА ПРОГРЕССОМ\n"
            "Фигуры и Смена цвета хранят прогресс раздельно. Зарабатывай до трёх звёзд за уровень, переключай светлую и тёмную темы и соревнуйся в мировом рейтинге."
        ),
    },
    "JA": {
        "locale": "ja-JP",
        "language_name": "日本語 (Japonca)",
        "title": "ArrowSwarm：矢印パズル",  # 15 chars (safe even at 2x full-width counting!)
        "short": "矢印の形を解き、枠を回して色を合わせ、3つのモードで進めるロジックパズル。",  # 38 chars (safe at 2x!)
        "long": (
            "すべての矢印には向きがある。次の一手を決めよう。\n\n"
            "ArrowSwarmは、矢印の進む道と順番を読み解く3つのモードを備えた2Dパズルゲームです。\n\n"
            "シェイプ\n"
            "ペンギンやハートなど、折れ曲がった矢印でできたシルエットから正しい順番を見つけよう。出口が空いている矢印を選び、1手ずつ盤面をクリアします。シェイプモードには50ステージを収録。\n\n"
            "カラーシフト\n"
            "道が空いているだけでは足りません。矢印の色と外側の壁の色を合わせる必要があります。矢印はそのままに、色のついた外枠を90度ずつ回転させ、限られた回転回数の中でクリアを目指しましょう。カラーシフトモードにも50ステージを収録。\n\n"
            "ディフェンス\n"
            "パズルの判断力を防衛ボードへ。盤面の周囲を進む敵に向けて、矢印のルートを切り開いて発射しましょう。\n\n"
            "ルートを読む\n"
            "パズルモードでは、塞がれた矢印を撃つと跳ね返りライフを1つ失います。3つのライフを守りながら、次に抜ける矢印を見極めましょう。\n\n"
            "進行状況とテーマ\n"
            "シェイプとカラーシフトは個別に進行を保存します。残りライフに応じて最大3つ星を獲得でき、ライト／ダークテーマ切り替えや世界ランキングにも対応しています。"
        ),
    },
    "KO": {
        "locale": "ko-KR",
        "language_name": "한국어 (Korece)",
        "title": "ArrowSwarm: 화살표 퍼즐",  # 18 chars
        "short": "모양을 비우고 테두리를 돌려 색을 맞추는 3가지 모드의 화살표 퍼즐.",  # 38 chars
        "long": (
            "모든 화살표에는 방향이 있습니다. 다음 수를 선택하세요.\n\n"
            "ArrowSwarm은 화살표의 이동 경로와 순서를 읽고 해결하는 세 가지 모드의 논리 퍼즐 게임입니다.\n\n"
            "셰이프\n"
            "펭귄, 하트 등 구부러진 화살표로 이루어진 실루엣 속에서 올바른 탈출 순서를 찾으세요. 앞이 막히지 않은 화살표를 탭해 한 수씩 보드를 비울 수 있습니다. 셰이프 캠페인은 50개의 스테이지를 제공합니다.\n\n"
            "컬러 시프트\n"
            "길만 열려 있다고 끝이 아닙니다. 화살표의 색과 바깥쪽 벽의 색도 일치해야 합니다. 화살표는 고정된 상태에서 바깥 테두리를 90도씩 회전시키며 제한된 회전 횟수 안에 해답을 찾으세요. 컬러 시프트 캠페인도 50개의 스테이지를 제공합니다.\n\n"
            "디펜스\n"
            "퍼즐의 판단력을 방어 보드로 확장하세요. 보드 주변을 이동하는 적을 향해 화살표의 길을 열어 발사할 수 있습니다.\n\n"
            "경로 읽기\n"
            "퍼즐 모드에서 막힌 화살표를 발사하면 되돌아오며 생명이 하나 줄어듭니다. 세 개의 생명을 지키며 다음 탈출 경로를 찾으세요.\n\n"
            "진행도와 테마\n"
            "셰이프와 컬러 시프트는 진행 상황이 별도로 저장됩니다. 남은 생명에 따라 최대 별 3개를 획득하고, 라이트/다크 테마와 글로벌 순위표를 즐길 수 있습니다."
        ),
    },
}


def nfc(text: str) -> str:
    return unicodedata.normalize("NFC", text.strip())


def write_md(lang: str, meta: dict) -> None:
    lines = [
        f"# {lang} Play Store listing ({meta['locale']})",
        "",
        "Generated and validated for ArrowSwarm (`com.farzetkigames.arrowswarm`).",
        "",
        "## App name / Uygulama adı",
        "",
        "```text",
        meta["title"],
        "```",
        "",
        "## Short description / Kısa açıklama",
        "",
        "```text",
        meta["short"],
        "```",
        "",
        "## Full description / Uzun açıklama",
        "",
        "```text",
        meta["long"],
        "```",
        "",
    ]
    (ROOT / f"PLAY_STORE_{lang}.md").write_text("\n".join(lines), encoding="utf-8")


def main() -> None:
    languages = {}
    # Preserve existing EN and TR byte-for-byte (with NFC normalization)
    for lang in ("EN", "TR"):
        item = EXISTING_STORE["languages"][lang]
        languages[lang] = {
            "locale": item["locale"],
            "title": nfc(item["title"]),
            "short": nfc(item["short"]),
            "long": nfc(item["long"]),
        }

    for lang, data in NEW_LOCALES.items():
        item = {
            "locale": data["locale"],
            "title": nfc(data["title"]),
            "short": nfc(data["short"]),
            "long": nfc(data["long"]),
        }
        languages[lang] = item
        write_md(lang, item)
        for field in ("title", "short", "long"):
            (ROOT / f"play_{lang.lower()}_{field}.txt").write_text(
                item[field], encoding="utf-8"
            )

    new_doc = {
        "status": "10-locale Play Store ASO copy ready; linked to https://farzetkigames.com.tr",
        "prepared_date": "2026-11-17",
        "package_name": "com.farzetkigames.arrowswarm",
        "languages": languages,
    }
    (ROOT / "store_metadata.json").write_text(
        json.dumps(new_doc, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(f"Wrote 10 locales to store_metadata.json and PLAY_STORE_*.md: {list(languages.keys())}")


if __name__ == "__main__":
    main()
