"""Stampa le battute ancora da tradurre, una per riga, conversazione per conversazione.

Come `15_mostra.py` per l'interfaccia: **mai stampare i testi grezzi**, perche' gli a
capo veri li fanno troncare quando si incolonna l'uscita e ci si ritrova a tradurre
frasi mozzate senza accorgersene.

    python tools/20_dialoghi_mostra.py                  quali conversazioni mancano
    python tools/20_dialoghi_mostra.py People/Arden     le battute che iniziano cosi'
    python tools/20_dialoghi_mostra.py 1234             una conversazione per id

Con `--quante N` limita quante battute stampare (default 400).
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import dialoghi  # noqa: E402


def elenco(en, it):
    """Le conversazioni con qualcosa da tradurre, in ordine di titolo."""
    fuori = []
    for cid, conv in en.items():
        nostre = it.get(cid, {})
        resta = [(vid, t) for vid, t in conv["voci"].items() if vid not in nostre]
        if resta:
            car = sum(len(t) for _, t in resta)
            fuori.append((conv["titolo"], cid, len(resta), car))
    return sorted(fuori)


def main():
    argomenti = sys.argv[1:]
    quante = 400
    if "--quante" in argomenti:
        i = argomenti.index("--quante")
        quante = int(argomenti[i + 1])
        del argomenti[i:i + 2]
    filtro = argomenti[0] if argomenti else None

    en, it = dialoghi.inglese(), dialoghi.italiano()
    fatte, totali, restano = dialoghi.conteggio()
    print(f"dialoghi: {fatte}/{totali} battute tradotte, "
          f"{restano:,} caratteri ancora da scrivere\n".replace(",", "."))

    resta = elenco(en, it)
    if not filtro:
        print(f"{len(resta)} conversazioni da finire (titolo, id, battute, caratteri):\n")
        for titolo, cid, n, car in resta[:60]:
            print(f"  {titolo:<58} {cid:>6} {n:>5} {car:>7}")
        if len(resta) > 60:
            print(f"  ... e altre {len(resta) - 60}")
        return

    scelte = [r for r in resta if r[0].startswith(filtro) or r[1] == filtro]
    if not scelte:
        print(f"niente da tradurre per '{filtro}'")
        return

    stampate = 0
    for titolo, cid, n, car in scelte:
        if stampate >= quante:
            print(f"\n... fermato a {quante} battute, rilancia per il resto")
            break
        print(f"\n### {titolo}   (conversazione {cid}, {n} battute, {car} caratteri)")
        nostre = it.get(cid, {})
        for vid, testo in sorted(en[cid]["voci"].items(), key=lambda x: int(x[0])):
            if vid in nostre:
                continue
            print(f"{vid}\t{testo!r}")
            stampate += 1
            if stampate >= quante:
                break


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
