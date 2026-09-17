"""Estrae il database dialoghi (PixelCrushers Dialogue System) dal bundle addressable.

Scrive Extracted/Dialoghi/<lingua>.json con la struttura grezza dei campi
tradotti (Dialogue Text, Menu Text, ...) e stampa quanto testo c'e' davvero.

    python tools/02_estrai_dialoghi.py [codice ...]      (default: en)
"""
import collections
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

import UnityPy  # noqa: E402

# I campi che contengono testo mostrato a schermo. Gli altri (Actor, Conditions,
# Script, Sequence...) sono logica e non vanno toccati.
#
# **Il campo che conta di piu' non e' qui dentro: si chiama come il codice lingua.**
# Le battute non stanno in `Dialogue Text` — che esiste in tutte le 48.776 voci ma e'
# quasi sempre vuoto — bensi' in un campo chiamato `en`, `ko`, `ja`... Per questo il
# codice lingua si aggiunge a questo insieme al momento dell'estrazione: cercare solo
# qui dentro fa concludere che il gioco non abbia dialoghi.
CAMPI_TESTO = {"Dialogue Text", "Menu Text", "Title", "Description", "Name",
               "Display Name", "Response Menu Text"}


def campi(elenco):
    return {f["title"]: f.get("value", "") for f in (elenco or [])}


def estrai(tree, codice):
    """Riduce il database a {sezione: {id: {campo: testo}}}, solo campi di testo.

    `codice` e' la lingua del database (`en`, `ko`...): e' anche il nome del campo
    che contiene le battute, quindi va tenuto insieme agli altri campi di testo.
    """
    tenuti = CAMPI_TESTO | {codice}
    fuori = {}
    for sezione in ("actors", "items", "locations", "variables"):
        blocco = {}
        for v in tree.get(sezione) or []:
            c = campi(v.get("fields"))
            testi = {k: t for k, t in c.items() if k in tenuti and t}
            if testi:
                blocco[str(v.get("id"))] = testi
        if blocco:
            fuori[sezione] = blocco

    conversazioni = {}
    for conv in tree.get("conversations") or []:
        cid = str(conv.get("id"))
        c = campi(conv.get("fields"))
        voci = {}
        for e in conv.get("dialogueEntries") or []:
            ec = campi(e.get("fields"))
            testi = {k: t for k, t in ec.items() if k in tenuti and t}
            if testi:
                voci[str(e.get("id"))] = testi
        if voci or c.get("Title"):
            conversazioni[cid] = {"titolo": c.get("Title", ""), "voci": voci}
    if conversazioni:
        fuori["conversations"] = conversazioni
    return fuori


def conta(dati):
    n = car = 0
    per_campo = collections.Counter()
    def visita(d):
        nonlocal n, car
        for k, t in d.items():
            n += 1
            car += len(t)
            per_campo[k] += 1
    for sezione, blocco in dati.items():
        if sezione == "conversations":
            for conv in blocco.values():
                for voce in conv["voci"].values():
                    visita(voce)
        else:
            for voce in blocco.values():
                visita(voce)
    return n, car, per_campo


def main():
    volute = [a for a in sys.argv[1:]] or ["en"]
    env = UnityPy.load(lwiw.bundle("dialoguedb_assets_all.bundle"))
    fuori = lwiw.assicura(os.path.join(lwiw.EXTRACTED, "Dialoghi"))

    for o in env.objects:
        if o.type.name != "MonoBehaviour":
            continue
        t = o.read_typetree()
        nome = t.get("m_Name", "")
        if not nome.startswith("DialogueDB_"):
            continue
        codice = nome[len("DialogueDB_"):]
        if codice not in volute:
            continue

        dati = estrai(t, codice)
        with open(os.path.join(fuori, codice + ".json"), "w", encoding="utf-8") as f:
            json.dump(dati, f, ensure_ascii=False, indent=1, sort_keys=True)

        n, car, per_campo = conta(dati)
        conv = len(dati.get("conversations", {}))
        print(f"=== {nome}")
        print(f"    conversazioni : {conv}")
        for sezione in ("actors", "items", "locations", "variables"):
            if sezione in dati:
                print(f"    {sezione:<14}: {len(dati[sezione])}")
        print(f"    campi di testo: {n}  ({car} caratteri)")
        for k, v in per_campo.most_common():
            print(f"        {k:<22} {v}")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
