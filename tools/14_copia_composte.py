"""Copia le voci di sola composizione, in qualunque tabella.

Una voce fatta soltanto di riferimenti ad altre tabelle — per esempio
`{Common.Theme_GreenForest}` oppure `{BroomstickTheme.X_Description}` — **non va
tradotta**: i riferimenti si risolvono sulle tabelle italiane, quindi il testo
composto e' gia' italiano com'e'. Va pero' messa nei file di traduzione lo stesso,
per due motivi: il conteggio della copertura dice il vero, e nessuno la ritraduce
per sbaglio credendola dimenticata.

E' la stessa idea di `13_componi_nomi.py`, che pero' e' legato a `Item` perche' li'
deve anche riordinare le parole. Questo vale per tutte le altre tabelle, dove non
c'e' niente da riordinare.

    python tools/14_copia_composte.py            mostra cosa farebbe
    python tools/14_copia_composte.py --scrivi   aggiorna i file in parts/
"""
import glob
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

RIFERIMENTO = re.compile(r"\{[^{}]*\}")


def sola_composizione(testo):
    return bool(RIFERIMENTO.search(testo)) and not RIFERIMENTO.sub("", testo).strip()


def main():
    scrivi = "--scrivi" in sys.argv
    totale = 0

    for percorso in sorted(glob.glob(os.path.join(lwiw.EXTRACTED, "Locale", "en", "*.json"))):
        tabella = os.path.basename(percorso)[:-5]
        if tabella == "Item":
            continue                      # se ne occupa 13_componi_nomi.py

        with open(percorso, encoding="utf-8") as f:
            inglese = json.load(f)
        dest = os.path.join(lwiw.TRAD, "parts", tabella + ".json")
        italiano = {}
        if os.path.isfile(dest):
            with open(dest, encoding="utf-8") as f:
                italiano = json.load(f)

        nuove = {k: v for k, v in inglese.items()
                 if v and k not in italiano and sola_composizione(v)}
        if not nuove:
            continue

        print(f"{tabella:<24} {len(nuove)} voci di sola composizione")
        for chiave in list(nuove)[:2]:
            print(f"    {chiave}: {nuove[chiave]!r}")
        totale += len(nuove)

        if scrivi:
            italiano.update(nuove)
            with open(dest, "w", encoding="utf-8") as f:
                json.dump(italiano, f, ensure_ascii=False, indent=2, sort_keys=True)

    print(f"\n{totale} voci {'copiate' if scrivi else 'copiabili'}")
    if not scrivi:
        print("(prova a vuoto: rilancia con --scrivi)")
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.exit(main())
