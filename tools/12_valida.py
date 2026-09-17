"""Controlla le traduzioni contro l'originale inglese.

Verifica, per ogni voce tradotta:
  - che la chiave esista davvero nella tabella inglese (un refuso nel nome
    produrrebbe una voce che non si vede mai);
  - che i **segnaposto** siano gli stessi: `{0}`, `{theme}`, `{Common.Energy}`.
    Sbagliarne uno fa comparire la graffa a schermo o lancia un'eccezione;
  - che i **tag** siano gli stessi: `<color=…>`, `<sprite …>`. Sono comandi di
    resa, non testo;
  - che il numero di **a capo** combaci, perche' spesso regola l'impaginazione.

    python tools/12_valida.py [Tabella ...]      (default: tutte)

Con `--unisci <file.json> <Tabella>` fonde un blocco dentro parts/<Tabella>.json
prima di validare.
"""
import glob
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

# Un segnaposto e' `{nome}` o `{nome:formato}`. Del formato non ci importa:
# `{0:MMMM d, yyyy}` diventa legittimamente `{0:d MMMM yyyy}` in italiano, perche'
# la data si scrive in un altro ordine. Confrontiamo quindi solo il **nome**.
SEGNAPOSTO = re.compile(r"\{([^{}:]*)(?::[^{}]*)?\}")

# Solo i veri tag di TextMeshPro. Il gioco usa le parentesi angolari anche per
# citare i titoli delle missioni — `<Esplorare la Grotta Stellata>` — e quelli
# vanno tradotti, non conservati.
TAG = re.compile(
    r"</?(?:color|size|b|i|u|s|sprite|link|align|font|mark|nobr|indent|line-height"
    r"|cspace|mspace|voffset|width|style|gradient|rotate|space|pos|alpha|sup|sub"
    r"|lowercase|uppercase|smallcaps|noparse|wave|shake|rainbow|bounce|dangle|fade"
    r"|incr|pend|swing|slide|jump)[^<>]*>", re.IGNORECASE)


def eccezioni():
    """Scostamenti voluti dall'originale, con la ragione scritta accanto.

    Serve a non indebolire i controlli per far passare pochi casi: la regola
    resta severa dappertutto, e le deroghe sono elencate una per una in
    `Mod/translations/eccezioni.json`, con la chiave nella forma `Tabella/chiave`.
    """
    percorso = os.path.join(lwiw.TRAD, "eccezioni.json")
    if not os.path.isfile(percorso):
        return {}
    with open(percorso, encoding="utf-8") as f:
        return json.load(f)


def carica(percorso):
    with open(percorso, encoding="utf-8") as f:
        return json.load(f)


def unisci(blocco, tabella):
    dest = os.path.join(lwiw.TRAD, "parts", tabella + ".json")
    esistenti = carica(dest) if os.path.isfile(dest) else {}
    nuovi = carica(blocco)
    prima = len(esistenti)
    esistenti.update(nuovi)
    with open(dest, "w", encoding="utf-8") as f:
        json.dump(esistenti, f, ensure_ascii=False, indent=2, sort_keys=True)
    print(f"{tabella}: {prima} + {len(nuovi)} nuove = {len(esistenti)} voci\n")


def valida(tabella):
    percorso_it = os.path.join(lwiw.TRAD, "parts", tabella + ".json")
    percorso_en = os.path.join(lwiw.EXTRACTED, "Locale", "en", tabella + ".json")
    if not os.path.isfile(percorso_en):
        print(f"!! {tabella}: manca l'originale inglese, salto")
        return 0
    it, en = carica(percorso_it), carica(percorso_en)
    deroghe = eccezioni()

    problemi = perdonati = 0
    for chiave, testo in sorted(it.items()):
        if chiave not in en:
            print(f"  [{tabella}] chiave inesistente: {chiave!r}")
            problemi += 1
            continue
        originale = en[chiave]
        derogata = f"{tabella}/{chiave}" in deroghe

        def segnala(descrizione):
            """Conta il problema, a meno che non sia una deroga registrata."""
            nonlocal problemi, perdonati
            if derogata:
                perdonati += 1
            else:
                print(f"  [{tabella}] {chiave}: {descrizione}")
                problemi += 1

        a, b = sorted(SEGNAPOSTO.findall(originale)), sorted(SEGNAPOSTO.findall(testo))
        if a != b:
            segnala(f"segnaposto diversi\n      en: {a}\n      it: {b}")

        a, b = sorted(TAG.findall(originale)), sorted(TAG.findall(testo))
        if a != b:
            segnala(f"tag diversi\n      en: {a}\n      it: {b}")

        if originale.count("\n") != testo.count("\n"):
            segnala(f"a capo {originale.count(chr(10))} -> {testo.count(chr(10))}")

    coperto = len([k for k, v in en.items() if v])
    nota = "tutto a posto" if problemi == 0 else f"{problemi} problemi"
    if perdonati:
        nota += f"  ({perdonati} deroghe volute)"
    print(f"{tabella:<26} {len(it):>5}/{coperto:<5} voci   {nota}")
    return problemi


def main():
    argomenti = sys.argv[1:]
    if argomenti and argomenti[0] == "--unisci":
        unisci(argomenti[1], argomenti[2])
        argomenti = [argomenti[2]]

    tabelle = argomenti or [
        os.path.basename(p)[:-5]
        for p in sorted(glob.glob(os.path.join(lwiw.TRAD, "parts", "*.json")))
    ]
    totale = sum(valida(t) for t in tabelle)
    print(f"\n{'nessun problema' if totale == 0 else str(totale) + ' problemi in tutto'}")
    return 1 if totale else 0


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.exit(main())
