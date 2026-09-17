"""Riempie le battute che non c'e' bisogno di tradurre a mano.

Due casi, nessuno dei due e' pigrizia:

1. **Battute di solo codice.** `[lua(GetLocalizedString("Choice", "Yes"))]` non e'
   inglese: e' una chiamata che il gioco risolve da se' sulla tabella `Choice`, gia'
   tradotta. Tradurla sarebbe un errore, non un miglioramento. Sono 1.197 battute.

2. **Battute identiche gia' tradotte altrove.** I dialoghi si ripetono: «What do you
   mean?» compare 50 volte, «...» 327. Riusare la resa gia' scritta non fa risparmiare
   e basta: impedisce che lo stesso personaggio dica la stessa cosa in due modi diversi
   a due minuti di distanza.

Non tocca mai una battuta gia' tradotta.

    python tools/22_dialoghi_gratis.py            mostra e basta
    python tools/22_dialoghi_gratis.py --scrivi   applica
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import dialoghi  # noqa: E402


def main():
    scrivi = "--scrivi" in sys.argv
    en, it = dialoghi.inglese(), dialoghi.italiano()

    # Cosa e' gia' stato deciso per un certo inglese.
    gia = {}
    for cid, voci in it.items():
        for vid, testo in voci.items():
            originale = en.get(cid, {}).get("voci", {}).get(vid)
            if originale:
                gia.setdefault(originale.strip(), testo)

    codice, gemelle = {}, {}
    for cid, conv in en.items():
        nostre = it.get(cid, {})
        for vid, testo in conv["voci"].items():
            if vid in nostre:
                continue
            if dialoghi.SOLO_CODICE.match(testo):
                codice.setdefault(cid, {})[vid] = testo
            elif testo.strip() in gia:
                gemelle.setdefault(cid, {})[vid] = gia[testo.strip()]

    n_codice = sum(len(v) for v in codice.values())
    n_gemelle = sum(len(v) for v in gemelle.values())
    print(f"battute di solo codice, si copiano identiche : {n_codice}")
    print(f"battute gia' tradotte altrove parola per parola: {n_gemelle}")

    if not scrivi:
        print("\n(prova a vuoto: rilancia con --scrivi per applicare)")
        return

    if codice:
        dialoghi.scrivi("00_solo_codice", codice)
    if gemelle:
        # Si accumulano: rilanciando dopo aver tradotto altro, ne trova altre.
        percorso = os.path.join(dialoghi.DIALOGHI, "00_gemelle.json")
        vecchie = {}
        if os.path.isfile(percorso):
            import json
            with open(percorso, encoding="utf-8") as f:
                vecchie = json.load(f).get("conversazioni", {})
        for cid, voci in gemelle.items():
            vecchie.setdefault(cid, {}).update(voci)
        dialoghi.scrivi("00_gemelle", vecchie)

    fatte, totali, restano = dialoghi.conteggio()
    print(f"\nadesso: {fatte}/{totali} battute, {restano:,} caratteri da tradurre"
          .replace(",", "."))


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
