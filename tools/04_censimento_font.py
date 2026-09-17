"""Cerca TUTTI i TMP_FontAsset del gioco, non solo quelli del bundle dei font.

Passa in rassegna bundle addressable, resources.assets e sharedassets*, e per
ognuno dice quanti glifi ha, se l'atlas e' statico o dinamico e se contiene le
vocali accentate italiane.

    python tools/04_censimento_font.py
"""
import glob
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

import UnityPy  # noqa: E402

ACCENTATE = "àèéìòùÀÈÉÌÒÙ"
MODO_ATLAS = {0: "Static", 1: "Dynamic", 2: "DynamicOS"}


def sorgenti():
    for p in sorted(glob.glob(os.path.join(lwiw.AA, "*.bundle"))):
        yield p
    for nome in ("resources.assets", "globalgamemanagers.assets"):
        yield os.path.join(lwiw.DATA, nome)
    for p in sorted(glob.glob(os.path.join(lwiw.DATA, "sharedassets*.assets"))):
        yield p


def main():
    totale = 0
    for percorso in sorgenti():
        try:
            env = UnityPy.load(percorso)
            oggetti = list(env.objects)
        except Exception as e:
            print(f"!! {os.path.basename(percorso)}: {type(e).__name__}: {e}")
            continue

        righe = []
        for o in oggetti:
            if o.type.name != "MonoBehaviour":
                continue
            try:
                t = o.read_typetree()
            except Exception:
                continue
            if "m_CharacterTable" not in t:
                continue
            unicodi = {c["m_Unicode"] for c in t["m_CharacterTable"]}
            manca = [c for c in ACCENTATE if ord(c) not in unicodi]
            righe.append((t.get("m_Name", "?"), len(unicodi),
                          MODO_ATLAS.get(t.get("m_AtlasPopulationMode")),
                          "OK" if not manca else "manca " + "".join(manca)))
        if righe:
            print(f"--- {os.path.basename(percorso)}")
            for nome, n, modo, stato in sorted(righe):
                print(f"      {nome:<42} glifi={n:>6}  {str(modo):<10} accenti: {stato}")
            totale += len(righe)
    print(f"\n{totale} TMP_FontAsset in tutto il gioco")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
