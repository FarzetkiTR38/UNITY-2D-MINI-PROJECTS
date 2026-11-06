"""Build a renamed OFL title font; keep native letter bodies and add Turkish accents.

Requires fontTools 4.x. Run from any directory. No Unity asset is modified in place.
"""
from copy import deepcopy
from hashlib import sha256
from pathlib import Path
import json

from fontTools.pens.ttGlyphPen import TTGlyphPen
from fontTools.ttLib import TTFont
from fontTools.ttLib.tables._g_l_y_f import GlyphCoordinates


ROOT = Path(__file__).resolve().parents[2]
FONTS = ROOT / "Assets/_Project/Fonts"
SOURCE = FONTS / "LilitaOne-Regular.ttf"
TARGET = FONTS / "Farzetki_Title_Font_H1.ttf"
LICENSE = FONTS / "Farzetki_Title_Font_H1-OFL.txt"
PROOF = Path(__file__).with_name("title-font-proof.json")
FAMILY = "Farzetki Title Font H1"
MISSING = {0x130: ("Idotaccent", "I"), 0x11E: ("Gbreve", "G"),
           0x11F: ("gbreve", "g"), 0x15E: ("Scedilla", "S"),
           0x15F: ("scedilla", "s")}


def native_dot(glyphs):
    """Copy the existing i's dot contour without redrawing or thinning it."""
    glyph = deepcopy(glyphs["i"])
    start = glyph.endPtsOfContours[-2] + 1
    coords, _, flags = glyph.getCoordinates(glyphs)
    glyph.coordinates = GlyphCoordinates((x - 137, y - 549) for x, y in coords[start:])
    glyph.flags = flags[start:]
    glyph.endPtsOfContours = [len(glyph.coordinates) - 1]
    glyph.numberOfContours = 1
    glyph.program.fromBytecode([])
    glyph.recalcBounds(glyphs)
    return glyph


def breve():
    """A softly rounded bowl, with a native-weight 90-unit inner/outer gap."""
    pen = TTGlyphPen(None)
    pen.moveTo((-150, 160))
    pen.qCurveTo((-150, 0), (0, 0))
    pen.qCurveTo((150, 0), (150, 160))
    pen.lineTo((60, 160))
    pen.qCurveTo((60, 90), (0, 90))
    pen.qCurveTo((-60, 90), (-60, 160))
    pen.closePath()
    return pen.glyph()


def composite(glyphs, body, accent, offset):
    """Reference the complete native body unchanged, plus a positioned accent."""
    pen = TTGlyphPen(glyphs)
    pen.addComponent(body, (1, 0, 0, 1, 0, 0))
    pen.addComponent(accent, (1, 0, 0, 1, *offset))
    return pen.glyph()


def native_digest(font, names):
    """Hash compiled outlines and metrics in the same stable source-glyph order."""
    digest = sha256()
    for name in names:
        digest.update(font["glyf"][name].compile(font["glyf"]))
        digest.update(str(font["hmtx"][name]).encode("ascii"))
    return digest.hexdigest()


def rename(font):
    replacements = {1: FAMILY, 2: "Regular", 3: "Farzetki:TitleFontH1:1.003",
                    4: FAMILY, 5: "Version 1.003", 6: "FarzetkiTitleFontH1-Regular",
                    7: "Original Lilita One is a trademark of Juan Montoreano.",
                    10: "Renamed derivative with native-weight Turkish accents; native bodies preserved.",
                    16: FAMILY, 17: "Regular"}
    for record in list(font["name"].names):
        if record.nameID in replacements:
            font["name"].setName(replacements[record.nameID], record.nameID,
                                 record.platformID, record.platEncID, record.langID)
    for name_id, value in replacements.items():
        font["name"].setName(value, name_id, 3, 1, 0x409)
    font["head"].fontRevision = 1.003
    if "DSIG" in font:
        del font["DSIG"]  # An upstream signature cannot authenticate a derivative.


def build():
    source = TTFont(SOURCE, recalcTimestamp=False)
    font = TTFont(SOURCE, recalcTimestamp=False)
    glyphs = font["glyf"]
    original_order = source.getGlyphOrder()
    original_cmap = source.getBestCmap()
    assert all(code not in original_cmap for code in MISSING)
    glyphs["farzetkiDot"] = native_dot(glyphs)
    glyphs["farzetkiBreve"] = breve()
    font["hmtx"]["farzetkiDot"] = (0, 0)
    font["hmtx"]["farzetkiBreve"] = (0, 0)
    components = {0x130: ("farzetkiDot", (214, 749)),
                  0x11E: ("farzetkiBreve", (303, 745)),
                  0x11F: ("farzetkiBreve", (248, 545)),
                  0x15E: ("cedilla", (67, -10)),
                  0x15F: ("cedilla", (28, -10))}
    for code, (name, body) in MISSING.items():
        accent, offset = components[code]
        glyphs[name] = composite(glyphs, body, accent, offset)
        font["hmtx"][name] = source["hmtx"][body]
        for table in font["cmap"].tables:
            if table.isUnicode():
                table.cmap[code] = name
    font.setGlyphOrder(original_order + ["farzetkiDot", "farzetkiBreve"] +
                       [pair[0] for pair in MISSING.values()])
    font["OS/2"].recalcUnicodeRanges(font)
    rename(font)
    font.save(TARGET, reorderTables=True)

    # Include upstream attribution and the full existing, unmodified OFL 1.1.
    existing = (ROOT / "Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt").read_text(encoding="utf-8-sig")
    license_body = existing[existing.index("SIL OPEN FONT LICENSE Version 1.1"):]
    copyright_notice = next(n.toUnicode() for n in source["name"].names if n.nameID == 0)
    LICENSE.write_text(copyright_notice + "\n\n" + FAMILY +
                       " is a renamed modified version. Original glyph bodies and attribution are preserved.\n"
                       "Turkish accents and derivative build: Farzetki Games, 2026.\n\n" + license_body,
                       encoding="utf-8", newline="\n")

    result = TTFont(TARGET, recalcTimestamp=False)
    same_outline_metrics = native_digest(source, original_order) == native_digest(result, original_order)
    same_cmap = all(result.getBestCmap().get(c) == name for c, name in original_cmap.items())
    assert same_outline_metrics and same_cmap
    pairs = []
    for code, (name, body) in MISSING.items():
        assert result["hmtx"][name] == source["hmtx"][body]
        assert result["glyf"][name].components[0].glyphName == body
        pairs.append({"unicode": f"U+{code:04X}", "glyph": name, "native_body": body,
                      "same_advance_and_bearing": True})
    proof = {"status": "PASS", "source_sha256": sha256(SOURCE.read_bytes()).hexdigest(),
             "output_sha256": sha256(TARGET.read_bytes()).hexdigest(), "output_bytes": TARGET.stat().st_size,
             "unchanged_native_glyphs": len(original_order), "unchanged_native_outlines_metrics": same_outline_metrics,
             "unchanged_original_cmap": same_cmap, "added_characters": pairs,
             "all_turkish_characters_present": all(ord(c) in result.getBestCmap() for c in "İıĞğŞşÇçÖöÜü"),
             "family": FAMILY, "license": "SIL Open Font License 1.1; original reserved name not used for derivative"}
    assert proof["all_turkish_characters_present"]
    Path(__file__).with_name("title-characters.txt").write_text(
        "".join(chr(code) for code in sorted(result.getBestCmap())), encoding="utf-8")
    PROOF.write_text(json.dumps(proof, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(proof, indent=2))


if __name__ == "__main__":
    build()
