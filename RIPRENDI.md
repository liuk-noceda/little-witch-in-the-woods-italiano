# Prompt per riprendere in una chat nuova

Copia tutto quello che sta **sotto la riga**, incollalo come primo messaggio.

---

Riprendi la traduzione italiana di **Little Witch in the Woods**.

Cartella di lavoro: `C:\Users\luca_nasi\Luca\Little Witch in the Woods\Little Witch in the Woods in Italiano`

**Prima cosa da fare: leggi `CONTINUA_QUI.md`.** Contiene tutto — come è fatto il gioco,
come funziona il plugin, i vicoli ciechi già pagati (le trappole di tempismo di Addressables,
il fatto che `Update()` del plugin BepInEx non gira, `HasCharacters` che mente sui font
dinamici, il campo dei dialoghi che non è `Dialogue Text`), le regole di stile e i comandi.
Non riscoprire quelle cose: sono già costate.

## Dove siamo

## ✅ TRADUZIONE COMPLETATA AL 100%

Il plugin è **finito e collaudato**. Non serve toccarlo.

**Interfaccia: 100%.** 8.656 voci su 8.656, tutte e ventiquattro le tabelle chiuse.
`python tools/12_valida.py` senza argomenti dice «nessun problema».

**Dialoghi: 100%.** 39.035 battute su 39.035. **0 caratteri inglesi ancora da tradurre.**

**L'installatore è fatto e provato:**
- Setup eseguibile (Inno Setup come Rubinite): `Installer/dist/LittleWitchInItaliano-1.0.0.exe` (3.4 MB)
- Versione portatile (zip): `_dist/LittleWitchInItaliano-portatile.zip` (1.6 MB)

Tutti i gruppi sono chiusi: `People/`, `Book/`, `Potion/`, `etc/`, `Event/`, `Event2/`, `NewPrologue/`, `Objects/`. Non resta nulla da tradurre.

## Il giro di lavoro

```bash
python tools/20_dialoghi_mostra.py                 # quali conversazioni mancano
python tools/20_dialoghi_mostra.py Objects/Village # le battute di quel gruppo
```

Stampa una battuta per riga, con gli a capo scritti `\n`. **Usa sempre questo**, mai
stampare i testi grezzi: gli a capo veri li fanno troncare e ti ritrovi a tradurre frasi
mozzate senza accorgertene (è già successo).

**Attenzione al tetto di 400 battute.** Lo strumento si ferma lì e scrive «fermato a 400
battute»: le conversazioni che vengono dopo in ordine alfabetico **non compaiono affatto**,
nemmeno come titolo. Su un gruppo grosso conviene chiedere per sottogruppi. Prima di
dichiarare chiuso un gruppo rilancia finché non risponde «niente da tradurre».

Scrivi il blocco tradotto in un JSON `{idConversazione: {idVoce: testo}}` nello
scratchpad. Il modo comodo è uno **script Python** che definisce i marcatori `[lua(...)]`
come variabili in cima e poi fa `json.dump`: le virgolette doppie dentro i `lua` rendono il
JSON scritto a mano un campo minato. Comodo anche definire una funzione
`L(tab, key)` che compone `[lua(GetLocalizedString("tab", "key"))]`, e mettere in una
costante le battute che si ripetono identiche dentro la stessa conversazione. Poi:

```bash
python tools/21_dialoghi_valida.py --unisci blocco.json 30_oggetti
```

fonde in `Mod/translations/dialoghi/30_oggetti.json` e **valida** contro l'originale.
Se segnala qualcosa, quasi sempre l'errore è nella traduzione — correggila, non allentare
il controllo.

**Scrivi lo script con lo strumento Write, non con un heredoc di bash**: nell'heredoc le
sequenze `\n` e `\\` si perdono per strada, ed è già costato un giro di correzioni sui
ritratti della biblioteca.

Poi, **dopo ogni blocco**:

```bash
python tools/22_dialoghi_gratis.py --scrivi
```

riusa le battute con l'inglese identico già reso altrove. Ogni pezzo tradotto ne sblocca
altri: finora ha regalato oltre **3.400 battute** senza scrivere una parola.

Infine installa:

```bash
python tools/10_aggiorna_gioco.py --solo-testi
```

Funziona anche col gioco aperto. Non serve mai ricompilare: il plugin rilegge i JSON a
ogni avvio.

## L'installatore

`python tools/30_crea_installer.py` costruisce, in `_dist/`, la cartella
**«Little Witch in the Woods in italiano»** e il suo zip. Dentro c'è un `.bat` da
cliccare, un `LEGGIMI.txt` e un `_installer/` con lo script PowerShell e il carico
(BepInEx, il plugin, tutti i testi).

Lo script trova il gioco da solo (registro di Steam, `libraryfolders.vdf`, GOG,
`C:\Games`), controlla che ci siano `LWIW.exe` e `LWIW_Data`, installa BepInEx solo se
manca, e ha voci per disinstallare e per leggere il log. Se la cartella del gioco è
protetta si rilancia da amministratore. Disinstallando rimette la lingua su inglese
scrivendo `en` nelle due chiavi PlayerPrefs
(`HKCU\Software\SunnySideUp\Little Witch In The Woods`, valori `Language_h3872303031` e
`SelectedLanguageCode_h729211379`, stringhe UTF-8 binarie con lo zero finale).

**Il `.ps1` va distribuito con il BOM UTF-8**: senza, Windows PowerShell 5.1 lo legge come
ANSI e ogni accentata diventa spazzatura. Ci pensa `30_crea_installer.py`, che ricopia i
testi con `utf-8-sig`. E dentro le stringhe a singolo apice di PowerShell **l'apostrofo va
raddoppiato** (`com''è`), altrimenti chiude la stringa.

Provato davvero il 2026-08-24: installazione da zero su una cartella finta (BepInEx
estratto, 18 file di core), aggiornamento sul gioco vero, disinstallazione con lingua
rimessa su inglese, e la voce che rilegge il log.

## Prima di tradurre un gruppo nuovo

Due comandi che fanno risparmiare errori:

```bash
# la scheda del personaggio, già tradotta: dà tono, ruolo e sesso
python -c "import json;en=json.load(open('Extracted/Locale/en/PeopleNote.json',encoding='utf-8'));it=json.load(open('Mod/translations/parts/PeopleNote.json',encoding='utf-8'));[print('EN:',en[k][:200],'\nIT:',it.get(k),'\n') for k in en if 'Clala' in k]"
```

E soprattutto: **prima di scrivere, estrai tutti i `[lua(...)]` del gruppo e risolvili**
sulle tabelle italiane già tradotte, così sai genere e numero dei nomi che ci finiscono
dentro (`la [lua(...)]` vs `il [lua(...)]`). Nello scratchpad delle sessioni scorse c'era
`lua.py`, dieci righe che scorrono `Mod/translations/parts/*.json`: rifarlo è un minuto.

## Marcatori da conservare intatti

Nei dialoghi non ci sono i segnaposto `{...}` dell'interfaccia. Ci sono questi:

- `[em1]…[/em1]` e `[em2]…[/em2]` — enfasi di PixelCrushers;
- `[lua(...)]` — **esegue codice**. Forme viste:
  `[lua(GetLocalizedString("Item", "TearOfDragon_Name"))]`, `[lua(GetItemName("MoonTear"))]`,
  `[lua(GetItemName(GetNPCPresentStorageItemID()))]`,
  `[lua(GetLocalizedString("Common", GetGlobalVariable("$711")))]`;
- i tag TextMeshPro `<shake>`, `<wave>`, `<swing>`, `<wiggle>`, `<color=…>`, `<font="…">`;
- `[var=System.RitoringPresentNumber]` e simili.

Attenzione: `<Waiting for a gift>` **non** è un tag, è una didascalia da tradurre.
`<Slumbering Tree>` invece **sì**: il validatore lo legge come un tag `<s…>` e va ricopiato
in inglese (è capitato nelle lettere di Brad).

Il font astrale `<font="Astronomicon SDF">X</font>` fascia **un glifo solo** e va ricopiato
carattere per carattere, spazi compresi: nella storia del Guardiano Silenzioso c'è perfino
un `<font="Astronomicon SDF" >W </font >` con gli spazi sbagliati, e va riprodotto così.

**I marcatori rotti dell'originale vanno riprodotti rotti.** Ce ne sono parecchi e sono già
stati incontrati: `[em1]cat furniture[em1]` senza barra (Bjorn), `[/em1]]` con una quadra di
troppo (Enite, Arin), `[em1][em1]…[/em1][/em1]` doppi (Teo, Kyla, Enite), `[/em1]…[em1]`
invertiti (Diane), `<color=#4b49a5>…<color=#4b49a5>` senza chiusura (Enite), `<rainb>…</rainb>`
troncato (Kyla), `<shake>[em1]parola</shake>[/em1]` incrociati (Enite). Il validatore
confronta la struttura, quindi «correggerli» fa fallire la validazione. Riscrivili identici.

**Occhio anche ai residui coreani** lasciati dai traduttori (`1개` dentro una frase, un `에`
iniziale): quelli sono spazzatura, si tolgono.

**Le quantità** si rendono con la cifra dopo il nome: l'inglese `10x [em1][lua(...)][/em1]`
diventa `[em1][lua(...)][/em1] x10`. Il `lua` restituisce il nome al singolare, quindi
«10 Ramo gigante» sarebbe sgrammaticato mentre «Ramo gigante x10» è la forma normale nei
giochi in italiano. L'`[em1]` resta a fasciare esattamente le stesse parole. Le 58 battute
che avevano ancora la forma inglese sono state uniformate.

**Le due varianti con doppio spazio esistono davvero**: `GetLocalizedString("Common",  Get…`
con due spazi e con uno solo compaiono entrambe nella stessa conversazione. Vanno copiate
carattere per carattere. Idem `GetLocalizedString("Item","X")` senza spazio dopo la virgola.

## Chi parla

- **Ellie**: la protagonista, strega apprendista, quindici anni scarsi. Esuberante,
  impulsiva, sarcastica, sempre pronta a cacciarsi nei guai. Dà del tu a tutti.
- **Virgil**: il suo cappello parlante. Voce adulta, asciutta, pedante; la rimprovera di
  continuo e ha conosciuto sua madre Aria. Il contrasto fra i due è metà del gioco. Quando
  la insulta usa metafore da cappello: «cappello sciocco e incompiuto», «cappello di pelo
  scucito», «cappello di lana logoro e sfilacciato».

Registri già fissati, da non cambiare:

| chi | come parla a Ellie |
|---|---|
| Arden, Bjorn, Vinch, Freddie, Rex, Kyla, Enite, Rubrum, Diane, Theo, Clala, Roy, Arin… | **tu** |
| il **barista** del treno, il **personale del treno** | **lei** |
| **Kent**, funzionario del Dipartimento | **lei**, «signorina Ellie», e *tosse* ovunque |
| **Baobab** (corvo di destra) | **lei**, la chiama «signorina Ellie» |
| **Laurel** (corvo di sinistra) | **tu**, la chiama «Blueriver» |
| **Dio Gatto Nero** | **tu** ieratico: «ella», «donde», futuri solenni |
| **Dio Gatto Bianco** (che poi è Lisa) | **tu** caldo e ironico |
| **Rahel Redember** | **tu** sornione, strascicato con la tilde: «davvero~» |
| **Alvin** | **lei**, «signorina strega» / «signorina Ellie»; idioma teatrale |
| **Aurea** | **tu** asciutto, da mercante |
| il **Guardiano Silenzioso** (il libro) | **voi** solenne e sprezzante, da oracolo |

Voci fissate nelle sessioni scorse:

- **Theo** (mai «Teo»: nel testo inglese «Theo» vince 62 a 1): monello di città, più giovane
  di Ellie, spavaldo. La sua risatina «Teehee» è **«Ihih»**, «Hahaha» è «Ahahah».
- **Diane Greenwind**: impiegata del Catalogo delle Streghe, allegra e professionale, si
  presenta come «Diane, quella che porta la felicità».
- **Kyla**: fabbra e falegname, grossa risata «Ahahah», diretta, beve volentieri. Chiama
  Ellie «signorina strega» ma le dà del tu.
- **Enite**: capovillaggio anziana, calorosa, materna. Risata «Ohoho». Chiama Ellie «cara».
- **Clala**: panettiera della Munchy Bakery, sorella maggiore di Dana. Perfezionista,
  ansiosa, si nasconde dietro il lavoro. Risata **«Ehehe»**.
- **Roy**: fioraio, ex soldato, pacato e cerimonioso ma sul tu. Parla lento e con cura;
  «Heh» diventa **«Eh»**.
- **Arin**: bibliotecaria, mezza strega e mezza umana. Parla **lentamente, con tanti puntini
  di sospensione a metà frase**: quella spezzatura va tenuta, è il suo tratto.
- **Rex**: esploratore chiassoso. La sua risata «Wahaha» è **«Uahahah»** (uniformata).

## Regole di stile

- **Ellie è femmina**, tutti gli accordi la seguono. Al giocatore si dà del **tu**.
- Le accentate si usano liberamente (à è é ì ò ù): il font di ripiego funziona. **Non usare
  `«»` né `…`**: sono i glifi a larghezza piena dei font CJK e a schermo vengono larghi e
  centrati. Virgolette dritte `"` e tre punti normali `...`. I 51 residui sono già stati
  ripuliti.
- L'elenco completo dei nomi propri, dei luoghi e dei termini fissati è in `CONTINUA_QUI.md`,
  sezione «Regole di stile». **Leggilo prima di cominciare**: è lungo e serve tutto.

Termini fissati o chiariti nelle ultime sessioni:

Honey Bear → **Orsetto del miele** · adventure party → **compagnia d'avventura** ·
Bitter Grape Tea Tree → **albero del tè d'uva amara** · Cat Gods → **Dei Gatti** (uniformato:
non «Dei Gatto» né «Dèi Gatto») · tote bag → **borsa** · Cleansing Herb → **erba
purificatrice** · Glowpetal Bloom → **fiore di petalucente** · village hall → **municipio** ·
wisteria tree → **glicine** · Star Scouts → **Esploratori delle Stelle** ·
sprinkles → **codette** · powdered sugar → **zucchero a velo** · drizzle → **glassa a filo** ·
Squishychub → **Cicciotondo** · Miscella's Toy House → **Casa dei Giocattoli di Miscella** ·
Moonlight Jonnie → **Jonnie Chiardiluna** · Dessert Contest → **Concorso dei Dolci** ·
Garden of Empty Words → **Giardino delle Parole Vuote** · pansy → **viola del pensiero**
(uniformato anche nei mobili) · Hanging Vine → **vite rampicante** · Little Honey Pumpkin →
**zucchetta al miele** · Book of Memories → **Libro dei Ricordi** · Reminiscence Potion →
**Pozione della rimembranza** · Silent Guardian → **Guardiano Silenzioso** · Book of Stars →
**Libro delle Stelle** · ancient witch language → **antica lingua delle streghe** ·
freshman → **matricola** · child awakened by starlight → **bambina risvegliata dalla luce
delle stelle** · witch brooch → **spilla da strega** · Dreamers of Eternity → **Sognatori
dell'Eternità** · half-blood → **mezzosangue** · Fallen Star → **Stella Caduta** ·
Sweetleaf → **Dolcefoglia** · Maze of Letters → **Labirinto delle Lettere** ·
Witch Association → **Associazione delle Streghe** · magic lens → **lente magica**.

Termini fissati nella sessione del 2026-09-15 (Grotta Stellata e ponti di Wisteria):

starry flash → **favilla stellare** · Starwhale → **balena stellare** · step board →
**pedana** · weights → **zavorre** · witch barrier → **barriera della strega** ·
household bricks → **mattoni di famiglia** · stepping stone bridge → **ponte a pietre** ·
Earthgem Shroom → **fungo gemmaterra** · lunaphobic → **lunafobico** (e il `<lunaphobic>`
fra parentesi angolari **non** è un tag: si traduce, `<lunafobico>`) · spliced plants →
**piante innestate** · the weird kid → **il tipo strano** (è Orsetto del miele, il
soprannome regge fino alla rivelazione) · Furnishing Bell/Clock → **orologio
dell'arredamento** (l'inglese oscilla, in gioco è l'orologio) · Olivia's Pot → **La pentola
di Olivia** · witch dice → **dado da strega** (l'oggetto in inventario resta «Dadi
dell'antica strega») · Tanis Rustystone e Luminaria Edis invariati · captain →
**capitano**, deputy captain → **vicecapitano** · Forest of Witches → **La Foresta delle
Streghe** · Ellie Red-river (la battuta scherzosa) → **Ellie Redriver**, che tiene il gioco
con Blueriver · summa cum laude resta in latino, come «magna cum laude» nel prologo.

Cognomi: **SilverRain** (come la targhetta in gioco, non «Silverrain») e **Eastchip**
invariabile (mai «gli Eastchips»). L'inglese usa anche «Istakipper» per gli Eastchip: vince
Eastchip, perché è come Rex si presenta.

`Choice/NotHaveItem` è un suffisso incollato a una lista di oggetti: era « serve.», che
sbaglia il numero con due oggetti. Adesso è **« da procurare.»**, che regge singolare e
plurale.

Dove l'inglese è incoerente con se stesso (*Diana* per *Diane*, *Chuity Bakery* per *Munchy
Bakery*, *Vince* per *Vinch*, *Starlight Cave* per *Grotta Stellata*, *Eastchip* /
*Istakipper*), vince il nome che il giocatore vede davvero in gioco.

**Se ti accorgi che due tabelle rendono lo stesso termine in due modi diversi, uniformale
subito** con uno script e rilancia `12_valida.py`: è già successo sei volte ed è il modo
sicuro di finire con due nomi diversi per la stessa cosa sotto gli occhi del giocatore.

## Come procedere

**Non fermarti fra un blocco e l'altro a chiedere il permesso**: traduci, valida, riusa i
gemelli, installa e passa al successivo. Aggiorna `CONTINUA_QUI.md` quando chiudi un gruppo.

Il ritmo misurato: la sessione del 2026-08-24 ha portato i dialoghi dal 62,8% all'82,6%
(7.738 battute) chiudendo tutti i personaggi e costruendo l'installatore; quella del
2026-09-15 dall'82,6% all'**89,7%** (2.753 battute), chiudendo `Event/WhiteCat` per intero
e i capitoli 017-018 di `Event2/WhiteCat`. Restano 193.085 caratteri: una sessione piena.

Quando lo spazio della chat sta per finire, **aggiorna questo file `RIPRENDI.md`** con i
numeri nuovi e dimmi di aprire una chat nuova incollandolo: è così che il lavoro prosegue
senza perdere niente.

## Una cosa mai vista a schermo

L'italiano dei dialoghi non è ancora mai stato guardato in gioco. Al primo avvio il log
(`C:\Games\Little Witch in the Woods\BepInEx\LogOutput.log`) dice da solo se l'innesto dei
dialoghi ha funzionato: cerca le righe `[dialoghi]`. Se compare
`ATTENZIONE: N battute risultano vuote`, le battute sarebbero mute; se compare
`rilettura: N battute controllate, nessuna vuota`, è a posto. La voce 3 dell'installatore
fa proprio questo controllo e lo riassume in italiano.

Il log attuale è vecchio (18 agosto, prima che le tabelle fossero finite) e mostra ancora
`Item: 0 tradotte`: **non è un problema del plugin**, è solo che quel giorno le traduzioni
non c'erano. Riavviare il gioco lo rigenera.
