"""Controlla se i font del gioco contengono le lettere accentate italiane.

E' la domanda che decide se la traduzione e' possibile senza rifare i font:
il gioco ha solo inglese e lingue CJK, quindi le vocali accentate potrebbero
non essere mai state incluse negli atlas TMP.

    python tools/03_verifica_font.py
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

import UnityPy  # noqa: E402

# Tutto cio' che l'italiano scritto bene richiede oltre all'ASCII.
ACCENTATE = "àèéìíîòóùúÀÈÉÌÒÙ"
VIRGOLETTE = "«»“”‘’…–—"

MODO_ATLAS = {0: "Static", 1: "Dynamic", 2: "DynamicOS"}


def main():
    env = UnityPy.load(lwiw.bundle("fonts_assets_all.bundle"))
    trovati = 0
    for o in env.objects:
        if o.type.name != "MonoBehaviour":
            continue
        t = o.read_typetree()
        if "m_CharacterTable" not in t:
            continue          # non e' un TMP_FontAsset
        trovati += 1
        nome = t.get("m_Name", "?")
        unicodi = {c["m_Unicode"] for c in t["m_CharacterTable"]}
        modo = MODO_ATLAS.get(t.get("m_AtlasPopulationMode"), t.get("m_AtlasPopulationMode"))

        manca_acc = [c for c in ACCENTATE if ord(c) not in unicodi]
        manca_virg = [c for c in VIRGOLETTE if ord(c) not in unicodi]
        latino_base = sum(1 for c in range(0x41, 0x7B) if c in unicodi)

        stato = "OK" if not manca_acc else "MANCANO " + "".join(manca_acc)
        print(f"{nome:<44} glifi={len(unicodi):>6}  atlas={modo:<10} "
              f"latino_base={latino_base}/58  accenti: {stato}")
        if manca_virg:
            print(f"{'':<44} tipografia mancante: {''.join(manca_virg)}")
    print(f"\n{trovati} font asset esaminati")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
