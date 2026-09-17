"""Aggiunge le lettere accentate italiane a un font del gioco, componendole.

I font della lingua inglese sono TrueType e contengono gia' sia le lettere base
sia i segni diacritici (`grave`, `acute`, `dieresis`): mancano solo le lettere
accentate come glifo unico. Questo script le costruisce come **glifi compositi** —
un riferimento alla lettera piu' uno al segno, spostato al posto giusto — e le
mappa nella cmap. Gli accenti risultano nello stile esatto del font originale,
perche' sono i suoi.

    python tools/06_accenta_font.py [nome_font]     (default: JejuHallasan)

Scrive Extracted/Font/<nome>-IT.ttf e stampa la verifica.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

from fontTools.ttLib import TTFont  # noqa: E402
from fontTools.ttLib.tables._g_l_y_f import Glyph, GlyphComponent  # noqa: E402

# lettera accentata -> (glifo base, glifo del segno)
DA_COMPORRE = {
    0x00E0: ("a", "grave"),   0x00E8: ("e", "grave"),   0x00EC: ("i", "grave"),
    0x00F2: ("o", "grave"),   0x00F9: ("u", "grave"),
    0x00E9: ("e", "acute"),   0x00ED: ("i", "acute"),   0x00F3: ("o", "acute"),
    0x00FA: ("u", "acute"),   0x00E1: ("a", "acute"),
    0x00C0: ("A", "grave"),   0x00C8: ("E", "grave"),   0x00CC: ("I", "grave"),
    0x00D2: ("O", "grave"),   0x00D9: ("U", "grave"),
    0x00C9: ("E", "acute"),
}

# Nelle lettere accentate la «i» perde il puntino, se il font ha la versione senza.
SENZA_PUNTINO = {"i": "dotlessi"}

STACCO = 40   # spazio in unita' em fra la cima della lettera e il segno


def bbox(glyf, nome, glifi):
    g = glyf[nome]
    if g.numberOfContours == 0:
        return None
    g.recalcBounds(glyf)
    return g.xMin, g.yMin, g.xMax, g.yMax


def main():
    nome_font = sys.argv[1] if len(sys.argv) > 1 else "JejuHallasan"
    sorgente = os.path.join(lwiw.EXTRACTED, "Font", nome_font + ".ttf")
    destinazione = os.path.join(lwiw.EXTRACTED, "Font", nome_font + "-IT.ttf")

    f = TTFont(sorgente)
    glyf = f["glyf"]
    hmtx = f["hmtx"]
    glifi = set(f.getGlyphOrder())

    fatti, saltati = [], []
    for codepoint, (base, segno) in DA_COMPORRE.items():
        nuovo = f"uni{codepoint:04X}"
        base_reale = SENZA_PUNTINO.get(base, base) if SENZA_PUNTINO.get(base) in glifi else base
        if base_reale not in glifi or segno not in glifi:
            saltati.append((chr(codepoint), f"manca {base_reale if base_reale not in glifi else segno}"))
            continue

        bb_base = bbox(glyf, base_reale, glifi)
        bb_segno = bbox(glyf, segno, glifi)
        if not bb_base or not bb_segno:
            saltati.append((chr(codepoint), "glifo vuoto"))
            continue

        # segno centrato sulla lettera e appoggiato sopra di essa
        dx = round((bb_base[0] + bb_base[2]) / 2 - (bb_segno[0] + bb_segno[2]) / 2)
        dy = round(bb_base[3] + STACCO - bb_segno[1])

        g = Glyph()
        g.numberOfContours = -1          # composito
        g.components = []
        for nome_comp, ox, oy in ((base_reale, 0, 0), (segno, dx, dy)):
            c = GlyphComponent()
            c.glyphName = nome_comp
            c.x, c.y = ox, oy
            c.flags = 0x04                 # ROUND_XY_TO_GRID
            g.components.append(c)

        glyf[nuovo] = g
        hmtx[nuovo] = hmtx[base_reale]
        fatti.append((chr(codepoint), nuovo, base_reale, segno))

    # mappa i nuovi glifi nella cmap (tutte le sottotabelle Unicode)
    for tab in f["cmap"].tables:
        if tab.isUnicode():
            for c, nuovo, _, _ in fatti:
                tab.cmap[ord(c)] = nuovo

    f.save(destinazione)

    print(f"font di partenza : {os.path.relpath(sorgente, lwiw.ROOT)}")
    print(f"font prodotto    : {os.path.relpath(destinazione, lwiw.ROOT)}")
    print(f"lettere composte : {len(fatti)}  {''.join(c for c, *_ in fatti)}")
    for c, _, base, segno in fatti:
        print(f"    {c}  =  {base} + {segno}")
    if saltati:
        print(f"non riuscite     : {len(saltati)}")
        for c, perche in saltati:
            print(f"    {c}  {perche}")

    # verifica: rileggere il font salvato e ritrovare i codepoint nella cmap
    ric = TTFont(destinazione, lazy=True)
    presenti = set()
    for tab in ric["cmap"].tables:
        presenti |= set(tab.cmap.keys())
    manca = [c for c, *_ in fatti if ord(c) not in presenti]
    print(f"verifica cmap    : {'tutte presenti' if not manca else 'MANCANO ' + ''.join(manca)}")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
