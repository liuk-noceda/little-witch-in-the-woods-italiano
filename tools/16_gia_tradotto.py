"""Per ogni voce ancora da tradurre, dice se lo stesso inglese esiste gia' altrove.

Le tabelle si ripetono fra loro: il nome di una creatura sta sia in `Encyclopedia`
sia in `Item`, una descrizione compare identica in due posti. Quando la stessa
stringa inglese e' gia' stata resa in italiano, riusarla non e' pigrizia: e' l'unico
modo perche' il giocatore non legga due nomi diversi per la stessa cosa.

    python tools/16_gia_tradotto.py Encyclopedia [da] [quante]

Stampa, una voce per riga:
    chiave  ->  inglese                     [= traduzione gia' esistente, se c'e']
"""
import glob
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402


def indice():
    """Mappa ogni stringa inglese gia' tradotta alla sua resa italiana."""
    mappa = {}
    for percorso in sorted(glob.glob(os.path.join(lwiw.EXTRACTED, "Locale", "en", "*.json"))):
        tabella = os.path.basename(percorso)[:-5]
        with open(percorso, encoding="utf-8") as f:
            en = json.load(f)
        dest = os.path.join(lwiw.TRAD, "parts", tabella + ".json")
        if not os.path.isfile(dest):
            continue
        with open(dest, encoding="utf-8") as f:
            it = json.load(f)
        for chiave, testo in en.items():
            if testo and chiave in it:
                mappa.setdefault(testo.strip(), (tabella, chiave, it[chiave]))
    return mappa


def main():
    tabella = sys.argv[1]
    da = int(sys.argv[2]) if len(sys.argv) > 2 else 0
    quante = int(sys.argv[3]) if len(sys.argv) > 3 else 50

    with open(os.path.join(lwiw.EXTRACTED, "Locale", "en", tabella + ".json"), encoding="utf-8") as f:
        inglese = json.load(f)
    percorso = os.path.join(lwiw.TRAD, "parts", tabella + ".json")
    italiano = {}
    if os.path.isfile(percorso):
        with open(percorso, encoding="utf-8") as f:
            italiano = json.load(f)

    mappa = indice()
    resta = [(k, v) for k, v in sorted(inglese.items()) if v and k not in italiano]
    trovate = sum(1 for _, v in resta if v.strip() in mappa)
    print(f"{tabella}: {len(resta)} da tradurre, {trovate} hanno gia' un gemello tradotto\n")
    for chiave, valore in resta[da:da + quante]:
        riga = f"{chiave!r}\t{valore!r}"
        gemello = mappa.get(valore.strip())
        if gemello:
            riga += f"\t= [{gemello[0]}] {gemello[2]!r}"
        print(riga)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
