"""Controlla le traduzioni dei dialoghi contro l'originale inglese.

Verifica, per ogni battuta tradotta:
  - che la conversazione e la voce esistano davvero (un refuso nell'id produrrebbe
    una battuta che non si vede mai);
  - che i **marcatori** siano gli stessi: `[em1]`, `[lua(...)]`. Il `lua` esegue
    codice: cambiarne una virgola rompe la battuta;
  - che i **tag** TextMeshPro siano gli stessi (`<shake>`, `<color=...>`).
    Attenzione: `<Waiting for a gift>` non e' un tag, e' testo da tradurre;
  - che il numero di **a capo** combaci, perche' regola l'impaginazione del fumetto.

    python tools/21_dialoghi_valida.py                       controlla tutto
    python tools/21_dialoghi_valida.py --unisci blocco.json nome

Con `--unisci` fonde un blocco `{idConversazione: {idVoce: testo}}` dentro
`Mod/translations/dialoghi/<nome>.json` e poi valida.
"""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import dialoghi  # noqa: E402


def unisci(percorso_blocco, nome):
    with open(percorso_blocco, encoding="utf-8") as f:
        blocco = json.load(f)
    # Il blocco puo' arrivare gia' avvolto in "conversazioni", o senza.
    nuove = blocco.get("conversazioni", blocco)

    dest = os.path.join(dialoghi.DIALOGHI, nome + ".json")
    esistenti = {}
    if os.path.isfile(dest):
        with open(dest, encoding="utf-8") as f:
            esistenti = json.load(f).get("conversazioni", {})

    prima = sum(len(v) for v in esistenti.values())
    for cid, voci in nuove.items():
        esistenti.setdefault(cid, {}).update(voci)
    dopo = sum(len(v) for v in esistenti.values())

    dialoghi.scrivi(nome, esistenti)
    print(f"{nome}: {prima} + {dopo - prima} nuove = {dopo} battute\n")


def valida():
    en, it = dialoghi.inglese(), dialoghi.italiano()
    guai = 0
    for cid in sorted(it):
        if cid not in en:
            print(f"  conversazione inesistente: {cid}")
            guai += 1
            continue
        voci = en[cid]["voci"]
        for vid, testo in sorted(it[cid].items()):
            if vid not in voci:
                print(f"  [{en[cid]['titolo']}] voce inesistente: {vid}")
                guai += 1
                continue
            for guaio in dialoghi.problemi(voci[vid], testo):
                print(f"  [{en[cid]['titolo']}] {vid}: {guaio}")
                guai += 1

    fatte, totali, restano = dialoghi.conteggio()
    nota = "tutto a posto" if guai == 0 else f"{guai} problemi"
    print(f"dialoghi   {fatte}/{totali} battute   {nota}")
    print(f"           {restano:,} caratteri inglesi ancora da tradurre".replace(",", "."))
    return guai


def main():
    argomenti = sys.argv[1:]
    if argomenti and argomenti[0] == "--unisci":
        unisci(argomenti[1], argomenti[2])
    return 1 if valida() else 0


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.exit(main())
