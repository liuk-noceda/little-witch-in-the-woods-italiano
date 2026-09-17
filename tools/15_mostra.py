"""Stampa le voci ancora da tradurre di una tabella, una per riga.

Serve perche' stampare i testi con gli a capo veri li fa troncare quando si
incolonna l'uscita: una descrizione lunga sembra finita a meta' e si traduce
monca. Qui ogni voce sta su **una sola riga**, con gli a capo scritti `\n`.

    python tools/15_mostra.py Tutorial [da] [quante]
"""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402


def main():
    tabella = sys.argv[1]
    da = int(sys.argv[2]) if len(sys.argv) > 2 else 0
    quante = int(sys.argv[3]) if len(sys.argv) > 3 else 60

    with open(os.path.join(lwiw.EXTRACTED, "Locale", "en", tabella + ".json"), encoding="utf-8") as f:
        inglese = json.load(f)
    percorso = os.path.join(lwiw.TRAD, "parts", tabella + ".json")
    italiano = {}
    if os.path.isfile(percorso):
        with open(percorso, encoding="utf-8") as f:
            italiano = json.load(f)

    resta = [(k, v) for k, v in sorted(inglese.items()) if v and k not in italiano]
    print(f"{tabella}: {len(resta)} voci da tradurre, mostro da {da}\n")
    for chiave, valore in resta[da:da + quante]:
        print(f"{chiave!r}\t{valore!r}")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
