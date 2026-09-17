"""Estrae le tabelle di testo di Unity Localization dai bundle addressable.

Scrive Extracted/Locale/<lingua>/<Tabella>.json  ->  {chiave: testo}
e stampa il riepilogo di quante voci e quanti caratteri ci sono per tabella.

    python tools/01_estrai_testi.py [lingua ...]     (default: tutte)
"""
import glob
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402  (imposta il path del tappo brotli)

import UnityPy  # noqa: E402

SHARED_BUNDLE = "localization-assets-shared_assets_all.bundle"
RE_TABELLE = re.compile(r"^localization-string-tables-(.+)_assets_all\.bundle$")


def chiavi_condivise():
    """{nome collezione: {id voce: chiave}} letto dagli asset 'Shared Data'."""
    mappa = {}
    env = UnityPy.load(lwiw.bundle(SHARED_BUNDLE))
    for o in env.objects:
        if o.type.name != "MonoBehaviour":
            continue
        t = o.read_typetree()
        collezione = t.get("m_TableCollectionName")
        if not collezione or "m_Entries" not in t:
            continue
        mappa[collezione] = {e["m_Id"]: e["m_Key"] for e in t["m_Entries"]}
    return mappa


def tabelle_di(percorso_bundle, condivise):
    """[(nome collezione, codice lingua, {chiave: testo})] da un bundle di tabelle."""
    fuori = []
    env = UnityPy.load(percorso_bundle)
    for o in env.objects:
        if o.type.name != "MonoBehaviour":
            continue
        t = o.read_typetree()
        if "m_TableData" not in t:
            continue
        nome = t.get("m_Name", "")
        codice = (t.get("m_LocaleId") or {}).get("m_Code", "")
        # 'UI_en' -> collezione 'UI'; il suffisso e' il codice lingua
        collezione = nome[: -(len(codice) + 1)] if codice and nome.endswith("_" + codice) else nome
        per_id = condivise.get(collezione, {})
        voci = {}
        for e in t["m_TableData"]:
            testo = e.get("m_Localized")
            if testo is None:
                continue
            chiave = per_id.get(e["m_Id"], f"#id:{e['m_Id']}")
            voci[chiave] = testo
        fuori.append((collezione, codice, voci))
    return fuori


def main():
    volute = set(a.lower() for a in sys.argv[1:])
    condivise = chiavi_condivise()
    print(f"Shared Data: {len(condivise)} collezioni, "
          f"{sum(len(v) for v in condivise.values())} chiavi\n")

    for percorso in sorted(glob.glob(lwiw.bundle("localization-string-tables-*.bundle"))):
        m = RE_TABELLE.match(os.path.basename(percorso))
        if not m:
            continue
        etichetta = m.group(1)                       # es. 'english(en)'
        codice_atteso = etichetta.split("(")[-1].rstrip(")")
        if volute and codice_atteso.lower() not in volute:
            continue

        tabelle = tabelle_di(percorso, condivise)
        fuori = lwiw.assicura(os.path.join(lwiw.EXTRACTED, "Locale", codice_atteso))
        tot_voci = tot_car = 0
        print(f"=== {etichetta}  ({len(tabelle)} tabelle)")
        for collezione, codice, voci in sorted(tabelle):
            car = sum(len(v) for v in voci.values())
            tot_voci += len(voci)
            tot_car += car
            print(f"    {collezione:<28} {len(voci):>5} voci  {car:>8} caratteri")
            with open(os.path.join(fuori, collezione + ".json"), "w", encoding="utf-8") as f:
                json.dump(voci, f, ensure_ascii=False, indent=1, sort_keys=True)
        print(f"    {'TOTALE':<28} {tot_voci:>5} voci  {tot_car:>8} caratteri\n")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
