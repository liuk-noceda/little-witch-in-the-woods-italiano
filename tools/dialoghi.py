"""Roba comune agli strumenti dei dialoghi (20, 21, 22).

I dialoghi non sono una tabella come le altre: il testo sta annidato in
`conversations[idConversazione].voci[idVoce].en`, e i marcatori da conservare
non sono i segnaposto `{...}` dell'interfaccia ma altri tre.
"""
import glob
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

DIALOGHI = os.path.join(lwiw.TRAD, "dialoghi")

# --- i marcatori da ricopiare identici -------------------------------------
#
# Enfasi di PixelCrushers e chiamate di codice. `[lua(...)]` esegue davvero
# codice: cambiarne anche una virgola rompe la battuta.
QUADRE = re.compile(r"\[/?(?:em\d|lua\([^\]]*\))\]")

# Solo i veri tag di TextMeshPro. Il gioco usa le parentesi angolari anche per
# le didascalie — `<Waiting for a gift>` — e quelle vanno tradotte, non conservate.
# Stessa distinzione che fa `12_valida.py` per l'interfaccia.
TAG = re.compile(
    r"</?(?:color|size|b|i|u|s|sprite|link|align|font|mark|nobr|indent|line-height"
    r"|cspace|mspace|voffset|width|style|gradient|rotate|space|pos|alpha|sup|sub"
    r"|lowercase|uppercase|smallcaps|noparse|wave|shake|rainbow|bounce|dangle|fade"
    r"|incr|pend|swing|slide|jump)[^<>]*>", re.IGNORECASE)

# Una battuta «senza inglese»: solo codice, marcatori e punteggiatura. Non c'e'
# niente da tradurre, si copia identica — come le voci di sola composizione di `Item`.
SOLO_CODICE = re.compile(
    r"^(?:\s|\[/?em\d\]|\[lua\([^\]]*\)\]|<[^>]*>|[.,!?~…\-–—'\"()]|\r|\n)*$")


def inglese():
    """{idConversazione: {"titolo": str, "voci": {idVoce: testo}}}, solo voci con testo."""
    percorso = os.path.join(lwiw.EXTRACTED, "Dialoghi", "en.json")
    with open(percorso, encoding="utf-8") as f:
        dati = json.load(f)
    fuori = {}
    for cid, conv in dati["conversations"].items():
        voci = {vid: v["en"] for vid, v in conv["voci"].items() if v.get("en")}
        if voci:
            fuori[cid] = {"titolo": conv.get("titolo", ""), "voci": voci}
    return fuori


def italiano():
    """Tutte le traduzioni gia' scritte, fuse: {idConversazione: {idVoce: testo}}."""
    fuori = {}
    for percorso in sorted(glob.glob(os.path.join(DIALOGHI, "*.json"))):
        with open(percorso, encoding="utf-8") as f:
            blocco = json.load(f)
        for cid, voci in (blocco.get("conversazioni") or {}).items():
            fuori.setdefault(cid, {}).update({k: v for k, v in voci.items() if v})
    return fuori


def scrivi(nome, conversazioni):
    """Salva un file di traduzione, creando la cartella se manca."""
    os.makedirs(DIALOGHI, exist_ok=True)
    percorso = os.path.join(DIALOGHI, nome + ".json")
    with open(percorso, "w", encoding="utf-8") as f:
        json.dump({"conversazioni": conversazioni}, f,
                  ensure_ascii=False, indent=1, sort_keys=True)
    return percorso


def problemi(originale, tradotto):
    """Elenco (eventualmente vuoto) di cosa non combacia fra originale e traduzione."""
    fuori = []
    a, b = sorted(QUADRE.findall(originale)), sorted(QUADRE.findall(tradotto))
    if a != b:
        fuori.append(f"marcatori diversi\n      en: {a}\n      it: {b}")
    a, b = sorted(TAG.findall(originale)), sorted(TAG.findall(tradotto))
    if a != b:
        fuori.append(f"tag diversi\n      en: {a}\n      it: {b}")
    if originale.count("\n") != tradotto.count("\n"):
        fuori.append(f"a capo {originale.count(chr(10))} -> {tradotto.count(chr(10))}")
    return fuori


def conteggio():
    """(battute tradotte, battute totali, caratteri inglesi ancora da tradurre)."""
    en, it = inglese(), italiano()
    fatte = totali = restano = 0
    for cid, conv in en.items():
        nostre = it.get(cid, {})
        for vid, testo in conv["voci"].items():
            totali += 1
            if vid in nostre:
                fatte += 1
            else:
                restano += len(testo)
    return fatte, totali, restano
