"""Tira fuori i file TTF/OTF incorporati nei bundle e ne legge la tabella cmap.

I FontAsset in modalita' Dynamic non hanno glifi precotti: TMP li genera a runtime
dal TTF incorporato. Quindi la domanda «l'italiano si vede?» diventa «questo TTF
contiene le vocali accentate?», ed e' quello che questo script risponde.

    python tools/05_estrai_ttf.py
"""
import glob
import io
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

import UnityPy  # noqa: E402
from fontTools.ttLib import TTFont  # noqa: E402

ACCENTATE = "àèéìòùÀÈÉÌÒÙ"
TIPOGRAFIA = "«»“”‘’…–—"


def main():
    fuori = lwiw.assicura(os.path.join(lwiw.EXTRACTED, "Font"))
    for percorso in sorted(glob.glob(os.path.join(lwiw.AA, "*.bundle"))):
        try:
            env = UnityPy.load(percorso)
            oggetti = list(env.objects)
        except Exception:
            continue
        for o in oggetti:
            if o.type.name != "Font":
                continue
            t = o.read_typetree()
            nome = t.get("m_Name", "font")
            dati = t.get("m_FontData") or b""
            if not dati:
                continue
            dati = bytes(dati)
            est = ".otf" if dati[:4] == b"OTTO" else ".ttf"
            dest = os.path.join(fuori, nome + est)
            with open(dest, "wb") as f:
                f.write(dati)

            try:
                tf = TTFont(io.BytesIO(dati), fontNumber=0, lazy=True)
                cmap = set()
                for tab in tf["cmap"].tables:
                    cmap |= set(tab.cmap.keys())
            except Exception as e:
                print(f"{nome:<26} {len(dati):>9} byte  cmap illeggibile ({type(e).__name__})")
                continue

            manca_acc = [c for c in ACCENTATE if ord(c) not in cmap]
            manca_tip = [c for c in TIPOGRAFIA if ord(c) not in cmap]
            ascii_ok = sum(1 for c in range(0x20, 0x7F) if c in cmap)
            print(f"{nome:<26} {len(dati):>9} byte  {len(cmap):>6} caratteri  "
                  f"ascii={ascii_ok}/95")
            print(f"{'':<26} accenti : {'TUTTI PRESENTI' if not manca_acc else 'mancano ' + ''.join(manca_acc)}")
            print(f"{'':<26} tipogr. : {'tutta presente' if not manca_tip else 'mancano ' + ''.join(manca_tip)}")
            print(f"{'':<26} salvato : {os.path.relpath(dest, lwiw.ROOT)}")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
