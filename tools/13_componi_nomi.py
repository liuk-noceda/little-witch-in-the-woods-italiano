"""Genera i nomi degli oggetti composti, invece di scriverli a mano.

Dei 2.801 nomi della tabella `Item`, **2.074 sono componibili senza tradurre nulla**:
sono composizioni di riferimenti ad altre tabelle, tipo

    {InteriorPropTheme.AnglersDream} {InteriorPropBasicName.Bed}

Tradurli a mano sarebbe assurdo e pieno di sviste: le tabelle a cui puntano sono
gia' tradotte, e quello che serve e' solo **rimettere le parole nell'ordine
italiano**. L'inglese antepone il tema al sostantivo — *Fisher's Dream Bed* —
mentre l'italiano fa il contrario: *Letto Sogno del Pescatore*.

La regola applicata:

    [sostantivi]  [parole come «grande», «Liv.2»]  [tema o qualita']

Di questi, la maggior parte e' solo riferimenti; il resto ha accanto qualche parola
sciolta, e quelle parole stanno tutte in VOCABOLARIO qui sotto: sono una trentina.

I restanti 727 sono inglese vero e vanno tradotti a mano: lo script li elenca in
un file a parte invece di inventarseli.

    python tools/13_componi_nomi.py            mostra cosa farebbe
    python tools/13_componi_nomi.py --scrivi   scrive parts/Item.json
"""
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

RIFERIMENTO = re.compile(r"\{[^{}]*\}")

# Riferimenti che in italiano vanno spostati in fondo: il tema dell'arredo e la
# qualita' del cibo si comportano come apposizioni, non come aggettivi anteposti.
IN_FONDO = re.compile(r"\{(InteriorPropTheme\.|EffectFoodCommon\.Food_(Normal|Delicious|Perfect))")

# Una parola che finisce col due punti non e' un aggettivo: e' un'etichetta che
# annuncia quello che viene dopo, e in italiano resta davanti come in inglese.
# «Recipe: {PotionName.DietPotion}» deve dare «Ricetta: Pozione dimagrante», non
# «Pozione dimagrante Ricetta:».
ETICHETTA = re.compile(r":$")

# Le uniche parole sciolte che compaiono accanto ai riferimenti.
# «grande/medio/piccolo» sono al maschile perche' gli unici sostantivi che
# prendono una taglia sono Tappeto e Deposito, maschili entrambi.
VOCABOLARIO = {
    "Extra Large": "grandissimo",
    "Extra Small": "piccolissimo",
    "Large": "grande",
    "Medium": "medio",
    "Small": "piccolo",
    "Lv1": "Liv.1",
    "Lv2": "Liv.2",
    "Lv3": "Liv.3",
    "Standing": "da terra",
    "Hanging": "da parete",
    "Mixer": "Impastatrice",
    "Broom Storage": "Rastrelliera per scope",
    "Broom Repair Bench": "Banco per riparare le scope",
    "Girl in a Blue Hat": "Ragazza col cappello blu",
    "Lamp": "Lampada",
    "Wall Lamp": "Lampada da parete",
    "Wall Planter": "Fioriera da parete",
    "Recipe:": "Ricetta:",
    "Syrup Donut": "Ciambella allo sciroppo",
    "Chocolate Donut": "Ciambella al cioccolato",
    "Walnut Cream Donut": "Ciambella alla crema di noci",
    "Meat Kebab": "Spiedino di carne",
    "Steak": "Bistecca",
    "Beef Bread": "Pane al manzo",
    "Black Tea": "Te' nero",
    "Green Tea": "Te' verde",
    "Fruit Tea": "Te' alla frutta",
    "Herb Tea": "Tisana alle erbe",
    "Spicy Rib Stew": "Stufato di costine piccante",
}

# Nel gioco c'e' un refuso: in una voce manca la graffa di apertura, e a schermo
# comparirebbe il nome del riferimento invece dell'oggetto.
# Il lookbehind non e' un vezzo: senza, la ricerca colpirebbe anche i nomi giusti,
# perche' «InteriorPropBasicName.Pot}» e' contenuto in «{InteriorPropBasicName.Pot}».
REFUSO = re.compile(r"(?<!\{)InteriorPropBasicName\.Pot\}")


def spezza(testo):
    """Divide il nome in pezzi: ('rif', '{...}') oppure ('parola', 'testo')."""
    pezzi, ultimo = [], 0
    for m in RIFERIMENTO.finditer(testo):
        prima = testo[ultimo:m.start()].strip()
        if prima:
            pezzi.append(("parola", prima))
        pezzi.append(("rif", m.group(0)))
        ultimo = m.end()
    coda = testo[ultimo:].strip()
    if coda:
        pezzi.append(("parola", coda))
    return pezzi


def componi(inglese):
    """Nome italiano, oppure None se il nome non e' componibile."""
    testo = REFUSO.sub("{InteriorPropBasicName.Pot}", inglese)
    pezzi = spezza(testo)
    if not any(t == "rif" for t, _ in pezzi):
        return None                      # inglese vero: va tradotto a mano

    etichette, sostantivi, parole, in_fondo = [], [], [], []
    for tipo, valore in pezzi:
        if tipo == "rif":
            (in_fondo if IN_FONDO.match(valore) else sostantivi).append(valore)
        else:
            tradotta = VOCABOLARIO.get(valore)
            if tradotta is None:
                return None              # parola sconosciuta: meglio non tirare a indovinare
            (etichette if ETICHETTA.search(valore) else parole).append(tradotta)

    return " ".join(etichette + sostantivi + parole + in_fondo)


def gia_italiana(testo):
    """Vero se la voce e' fatta di soli riferimenti, senza una parola propria.

    Queste voci — quasi tutte descrizioni, tipo
    `{BroomstickTheme.X_Description}\\n{BroomstickCommon.Type_Description}` —
    **non vanno tradotte**: i riferimenti si risolvono sulle tabelle italiane, e
    quindi il testo composto e' gia' in italiano cosi' com'e'. Si copiano
    identiche, a capo compresi, cosi' il conteggio della copertura dice il vero
    e nessuno le ritraduce per sbaglio.
    """
    return bool(RIFERIMENTO.search(testo)) and not RIFERIMENTO.sub("", testo).strip()


def main():
    scrivi = "--scrivi" in sys.argv
    inglese = json.load(open(os.path.join(lwiw.EXTRACTED, "Locale", "en", "Item.json"),
                             encoding="utf-8"))
    nomi = {k: v for k, v in inglese.items() if v and k.endswith("_Name")}

    fatti, amano = {}, {}
    for chiave, valore in nomi.items():
        italiano = componi(valore)
        if italiano:
            fatti[chiave] = italiano
        else:
            amano[chiave] = valore

    # Tutto il resto della tabella (descrizioni e voci sciolte): quelle di sola
    # composizione si copiano intatte, le altre vanno tradotte a mano.
    for chiave, valore in inglese.items():
        if not valore or chiave in nomi:
            continue
        if gia_italiana(valore):
            fatti[chiave] = valore
        else:
            amano[chiave] = valore

    print(f"generabili senza tradurre : {len(fatti)}")
    print(f"da tradurre a mano        : {len(amano)}")
    print("\nesempi generati:")
    for chiave in list(fatti)[:6]:
        print(f"  {nomi[chiave]}\n    -> {fatti[chiave]}")

    if not scrivi:
        print("\n(prova a vuoto: rilancia con --scrivi per salvare)")
        return 0

    dest = os.path.join(lwiw.TRAD, "parts", "Item.json")
    esistenti = json.load(open(dest, encoding="utf-8")) if os.path.isfile(dest) else {}
    # Cio' che e' gia' tradotto a mano vince sempre sul generato.
    unione = dict(fatti)
    unione.update(esistenti)
    with open(dest, "w", encoding="utf-8") as f:
        json.dump(unione, f, ensure_ascii=False, indent=2, sort_keys=True)
    print(f"\nscritto {os.path.relpath(dest, lwiw.ROOT)}: {len(unione)} voci")

    # L'elenco «da tradurre» deve dire cosa manca **davvero**: le voci gia'
    # tradotte a mano in un giro precedente non ci vanno.
    manca = {k: v for k, v in amano.items() if k not in unione}
    resto = os.path.join(lwiw.TRAD, "Item_da_tradurre.json")
    with open(resto, "w", encoding="utf-8") as f:
        json.dump(manca, f, ensure_ascii=False, indent=2, sort_keys=True)
    print(f"scritto {os.path.relpath(resto, lwiw.ROOT)}: {len(manca)} voci ancora in inglese")
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.exit(main())
