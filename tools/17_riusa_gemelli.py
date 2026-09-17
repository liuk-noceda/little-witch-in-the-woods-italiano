"""Riusa la traduzione gia' fatta quando l'inglese e' identico parola per parola.

Le tabelle si sovrappongono: il nome di una creatura sta in `Encyclopedia` e in
`Item`, una voce di menu ricompare in `UI`. Tradurre due volte la stessa stringa
non e' solo lavoro doppio: e' il modo sicuro di finire con due nomi diversi per
la stessa cosa sotto gli occhi del giocatore.

Copia **solo** quando l'inglese combacia esattamente (a parte gli spazi ai bordi),
e non tocca mai una voce gia' tradotta.

    python tools/17_riusa_gemelli.py Encyclopedia          # mostra e basta
    python tools/17_riusa_gemelli.py Encyclopedia --scrivi # applica
    python tools/17_riusa_gemelli.py --scrivi              # tutte le tabelle
"""
import glob
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402


def carica(percorso):
    with open(percorso, encoding="utf-8") as f:
        return json.load(f)


def indice(escludi=None):
    """Mappa ogni inglese gia' tradotto alla sua resa italiana.

    `escludi` tiene fuori la tabella di destinazione: una voce non deve pescare
    da se stessa.
    """
    mappa = {}
    for percorso in sorted(glob.glob(os.path.join(lwiw.EXTRACTED, "Locale", "en", "*.json"))):
        tabella = os.path.basename(percorso)[:-5]
        if tabella == escludi:
            continue
        dest = os.path.join(lwiw.TRAD, "parts", tabella + ".json")
        if not os.path.isfile(dest):
            continue
        en, it = carica(percorso), carica(dest)
        for chiave, testo in en.items():
            if testo and chiave in it:
                mappa.setdefault(testo.strip(), (tabella, it[chiave]))
    return mappa


def riusa(tabella, scrivi):
    percorso_en = os.path.join(lwiw.EXTRACTED, "Locale", "en", tabella + ".json")
    percorso_it = os.path.join(lwiw.TRAD, "parts", tabella + ".json")
    en = carica(percorso_en)
    it = carica(percorso_it) if os.path.isfile(percorso_it) else {}
    mappa = indice(escludi=tabella)

    nuove = {}
    for chiave, testo in sorted(en.items()):
        if not testo or chiave in it:
            continue
        gemello = mappa.get(testo.strip())
        if gemello:
            nuove[chiave] = gemello[1]

    if not nuove:
        return 0
    print(f"{tabella:<26} {len(nuove):>4} voci riusate")
    if scrivi:
        it.update(nuove)
        with open(percorso_it, "w", encoding="utf-8") as f:
            json.dump(it, f, ensure_ascii=False, indent=2, sort_keys=True)
    return len(nuove)


def main():
    argomenti = [a for a in sys.argv[1:] if a != "--scrivi"]
    scrivi = "--scrivi" in sys.argv
    tabelle = argomenti or [
        os.path.basename(p)[:-5]
        for p in sorted(glob.glob(os.path.join(lwiw.EXTRACTED, "Locale", "en", "*.json")))
    ]
    totale = sum(riusa(t, scrivi) for t in tabelle)
    coda = "" if scrivi else "   (prova: aggiungi --scrivi per applicare)"
    print(f"\n{totale} voci in tutto{coda}")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
