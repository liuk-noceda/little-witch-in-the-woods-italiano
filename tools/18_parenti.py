"""Mostra le voci da tradurre, e sotto i nomi gia' fissati per la stessa cosa.

Le chiavi hanno un prefisso comune: `Baitty_Name` in `Encyclopedia` parla della
stessa creatura di `Baitty_Collect_Name` in `Item`. Se quel nome e' gia' stato
reso in italiano, la voce nuova deve usarlo, non inventarne un altro: il
giocatore legge le due schede a un minuto di distanza.

    python tools/18_parenti.py Encyclopedia [da] [quante]

Prima l'elenco da tradurre (una voce per riga, a capo scritti `\\n`), poi il
glossario dei parenti gia' tradotti, un prefisso per blocco.
"""
import glob
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

# Le code che distinguono le varianti di una stessa cosa: il prefisso e' cio'
# che resta togliendole.  `Baitty_Collect_Name` -> `Baitty`.
CODE = re.compile(
    r"_(Collect|Jelly|Powder|Water|Name|Description|Summary|Height|Weight"
    r"|Description_Gather|Description_General|Description_Potion)(_.*)?$")


def prefisso(chiave):
    precedente = None
    while precedente != chiave:
        precedente = chiave
        chiave = CODE.sub("", chiave)
    return chiave


def carica(percorso):
    with open(percorso, encoding="utf-8") as f:
        return json.load(f)


def main():
    tabella = sys.argv[1]
    da = int(sys.argv[2]) if len(sys.argv) > 2 else 0
    quante = int(sys.argv[3]) if len(sys.argv) > 3 else 40

    inglese = carica(os.path.join(lwiw.EXTRACTED, "Locale", "en", tabella + ".json"))
    percorso = os.path.join(lwiw.TRAD, "parts", tabella + ".json")
    italiano = carica(percorso) if os.path.isfile(percorso) else {}

    resta = [(k, v) for k, v in sorted(inglese.items()) if v and k not in italiano]
    blocco = resta[da:da + quante]
    print(f"{tabella}: {len(resta)} da tradurre, mostro da {da}\n")
    for chiave, valore in blocco:
        print(f"{chiave!r}\t{valore!r}")

    # Il glossario: per ogni prefisso del blocco, le voci gia' tradotte altrove.
    prefissi = {prefisso(k) for k, _ in blocco}
    righe = {}
    for percorso_en in sorted(glob.glob(os.path.join(lwiw.EXTRACTED, "Locale", "en", "*.json"))):
        altra = os.path.basename(percorso_en)[:-5]
        dest = os.path.join(lwiw.TRAD, "parts", altra + ".json")
        if not os.path.isfile(dest):
            continue
        en, it = carica(percorso_en), carica(dest)
        for chiave, testo in sorted(en.items()):
            if not testo or chiave not in it or len(testo) > 90:
                continue
            p = prefisso(chiave)
            if p in prefissi:
                righe.setdefault(p, []).append(f"    [{altra}] {testo!r} -> {it[chiave]!r}")

    if righe:
        print("\n--- gia' fissato altrove ---")
        for p in sorted(righe):
            print(f"  {p}")
            for riga in righe[p][:6]:
                print(riga)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
