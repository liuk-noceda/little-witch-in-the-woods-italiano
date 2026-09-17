# Prompt per continuare in Antigravity

Copia tutto quello che sta **sotto la riga** e incollalo come primo messaggio in Antigravity,
dopo aver aperto la cartella del progetto.

---

Riprendi la traduzione amatoriale italiana del gioco **Little Witch in the Woods**.

Cartella di lavoro:
`C:\Users\luca_nasi\Luca\Little Witch in the Woods\Little Witch in the Woods in Italiano`

## Prima di toccare qualsiasi cosa

Leggi in quest'ordine, per intero, due file che stanno nella cartella di lavoro:

1. **`RIPRENDI.md`** — stato attuale, da dove riprendere, giro di lavoro, marcatori,
   registri dei personaggi, termini fissati.
2. **`CONTINUA_QUI.md`** — come è fatto il gioco, come funziona il mod, l'elenco completo
   dei nomi propri e dei termini, e soprattutto i **vicoli ciechi già pagati**.

Quei due file sono la verità del progetto. Non riscoprire quello che c'è scritto: è già
costato tempo. In particolare **non** rimettere in discussione la struttura del mod, il
campo dei dialoghi, il tempismo di Addressables o i font: sono risolti.

## Dov'è il lavoro

- Interfaccia: **100%** (8.656 voci, 24 tabelle).
- Dialoghi: **89,7%** — 35.014 battute su 39.035, restano **193.085 caratteri** inglesi.
- Il plugin BepInEx e l'installatore sono **finiti e collaudati**: non serve ricompilare
  niente, il plugin rilegge i JSON a ogni avvio del gioco.

Manca solo da tradurre. I gruppi rimasti, in ordine di dimensione:

| gruppo | battute | caratteri |
|---|---:|---:|
| `Event2/WhiteCat` (capitoli **019** e **021**) | 1.891 | 81.095 |
| `Event2/Museum` | 287 | 17.115 |
| `Objects/RainbowForest` | 303 | 16.324 |
| `Event/WelcomeParty` | 216 | 11.092 |
| `NewPrologue/ReadWitchBookShelf` | 101 | 6.166 |
| `Event2/Cat` | 136 | 5.826 |
| `Objects/WitchBookShelf` | 88 | 5.769 |
| `Event2/Tutorial` | 105 | 5.588 |
| `Objects/WitchHouse` | 91 | 5.150 |
| `Objects/GreenForest` | 105 | 4.828 |
| `Objects/Village` | 90 | 4.432 |
| la coda di `Objects/`, `NewPrologue/`, `Event/ZoneBlock`, `Event2/Ellie` | ~700 | ~34.000 |

**Riparti da `Event2/WhiteCat/019`**, poi `021`, poi i gruppi medi, poi la coda.

## Il ciclo di lavoro, da ripetere blocco dopo blocco

Tutti i comandi si lanciano dalla cartella di lavoro. Python 3.12 è già installato.

**1. Vedi cosa manca.**

```bash
python tools/20_dialoghi_mostra.py Event2/WhiteCat/019 --quante 250
```

Stampa una battuta per riga con gli a capo resi `\n`. Usa **sempre** questo strumento: non
leggere mai i JSON inglesi grezzi, perché gli a capo veri troncano l'output e ti ritrovi a
tradurre frasi mozzate senza accorgertene. Occhio al tetto `--quante`: quando dice «fermato
a N battute», le conversazioni successive **non compaiono affatto**, nemmeno come titolo.

**2. Traduci il blocco in uno script Python** che costruisce il JSON
`{idConversazione: {idVoce: testo}}` e lo salva. Struttura che funziona bene:

```python
# -*- coding: utf-8 -*-
import json, io, os
GROTTA = '[lua(GetLocalizedString("Common", "Common_Theme_StarSeaCave"))]'
conv = {}
conv["95041"] = {
    "1": "Cos'è questa? Una campana?",
    "2": "Perché mai ci sarebbe una campana?",
}
uscita = os.path.join(os.path.dirname(os.path.abspath(__file__)), "blocco.json")
with io.open(uscita, "w", encoding="utf-8") as f:
    json.dump(conv, f, ensure_ascii=False, indent=1)
print(sum(len(v) for v in conv.values()), "battute ->", uscita)
```

Mettere i `[lua(...)]` in variabili in cima evita il campo minato delle virgolette doppie
dentro il JSON scritto a mano.

**Scrivi questo script come file vero, con lo strumento di scrittura file.** Non generarlo
con un heredoc di shell: le sequenze `\n` e `\\` si perdono per strada, ed è già costato un
giro di correzioni. Se ti serve un a capo dentro una battuta usa `chr(10)`, non `"\n"`
dentro una stringa passata alla shell.

**3. Valida e fondi.**

```bash
python tools/21_dialoghi_valida.py --unisci blocco.json 32_gatto_bianco_2
```

Fonde in `Mod/translations/dialoghi/32_gatto_bianco_2.json` e confronta con l'originale il
numero di marcatori, di tag TextMeshPro e di a capo. **Se segnala qualcosa, l'errore è quasi
sempre nella traduzione: correggi quella, non allentare il controllo.**

**4. Recupera i gemelli gratis.**

```bash
python tools/22_dialoghi_gratis.py --scrivi
```

Riusa le battute con l'inglese identico già reso altrove. Ogni blocco ne sblocca altri:
finora ha regalato oltre 3.400 battute gratis.

**5. Installa nel gioco.**

```bash
python tools/10_aggiorna_gioco.py --solo-testi
```

Funziona anche col gioco aperto. Poi torna al punto 1 con il blocco successivo.

**Non fermarti fra un blocco e l'altro a chiedere il permesso.** Traduci, valida, fondi,
installa, passa al successivo. Blocchi da 200-350 battute sono la misura giusta.

## Le cose che fanno fallire la validazione

- `[em1]…[/em1]`, `[em2]…[/em2]`: enfasi. Vanno conservati e devono fasciare le stesse
  parole.
- `[lua(...)]`: **esegue codice**. Si ricopiano carattere per carattere, spazi doppi
  compresi (esistono davvero sia `GetLocalizedString("Common",  Get…` con due spazi sia con
  uno solo, nella stessa conversazione).
- Tag TextMeshPro `<shake>`, `<wave>`, `<color=…>`, `<font="Astronomicon SDF">`: conservati.
  Ma `<Waiting for a gift>` e `<lunaphobic>` **non** sono tag: sono testo da tradurre.
  `<Slumbering Tree>` invece il validatore lo legge come tag `<s…>`, quindi va ricopiato in
  inglese.
- **I marcatori rotti dell'originale vanno riprodotti rotti**: `[em1]parola[em1]` senza
  barra, `[/em1]]` con una quadra di troppo, `[em1][em1]…[/em1][/em1]` doppi. «Correggerli»
  fa fallire la validazione.
- Gli **a capo** vanno riprodotti nello stesso numero, e gli **spazi finali** vanno tenuti.
- Le **quantità** si girano: l'inglese `10x [em1][lua(...)][/em1]` diventa
  `[em1][lua(...)][/em1] x10`.

## Prima di tradurre un gruppo nuovo

Estrai tutti i `[lua(...)]` del gruppo e risolvili sulle tabelle italiane già tradotte in
`Mod/translations/parts/*.json`, così sai genere e numero dei nomi che ci finiscono dentro
(`la [lua(...)]` contro `il [lua(...)]`). Un `grep` sul nome della chiave basta:

```bash
grep -rh -o '"MoonTear_Name": "[^"]*"' Mod/translations/parts/*.json
```

Utile anche cercare come un termine inglese è già stato reso nei dialoghi tradotti, per non
inventarne due versioni.

## Stile, in breve (il dettaglio è in `RIPRENDI.md`)

- **Ellie è femmina**, quindici anni, esuberante e sarcastica; dà del tu a tutti.
  **Virgil** è il suo cappello parlante: voce adulta, asciutta, pedante.
- Le accentate si usano liberamente. **Mai `«»` né `…`**: sono glifi a larghezza piena dei
  font CJK e a schermo vengono larghi e sbagliati. Usa `"` dritte e `...`.
- Danno del **lei** a Ellie solo: il barista e il personale del treno, Kent, Baobab e Alvin.
  Tutti gli altri le danno del tu.
- L'elenco dei nomi propri e dei termini fissati è lungo e serve tutto: sta in `RIPRENDI.md`
  e in `CONTINUA_QUI.md`, sezione «Regole di stile». Leggilo prima di cominciare.
- Se ti accorgi che due file rendono lo stesso termine in due modi diversi, **uniformali
  subito** con uno script e rivalida.

## Quando hai finito, o quando lo spazio sta per esaurirsi

Aggiorna `RIPRENDI.md` e `CONTINUA_QUI.md` con i numeri nuovi e con i termini che hai
fissato, poi dimmi di aprire una sessione nuova. È così che il lavoro prosegue senza
perdere niente.

Se chiudi tutti i dialoghi, rilancia anche:

```bash
python tools/30_crea_installer.py
```

per rigenerare il pacchetto portatile in `_dist/` con i testi aggiornati.
