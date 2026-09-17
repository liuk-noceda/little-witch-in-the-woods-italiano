# Little Witch in the Woods in italiano — stato del progetto

Incolla questo file (o il suo contenuto) all'inizio di una nuova chat per riprendere.

---

## Contesto

Traduzione italiana amatoriale di **Little Witch in the Woods** (Sunny Side Up), sul
modello di quanto fatto per Rubinite: **mod runtime non invasiva**, nessun file del gioco
modificato, la lingua aggiunta in memoria all'avvio. Progetto **Liuk Noceda (Luca Nasi)**.

| | |
|---|---|
| Gioco | `C:\Games\Little Witch in the Woods` — Unity **2022.3.62f3**, Mono, x64 |
| Localizzazione UI | **Unity Localization** (string table dentro bundle Addressables) |
| Dialoghi | **Dialogue System for Unity** (PixelCrushers), database separato per lingua |
| Loader | BepInEx **5.4.23.3** (x64), installato e funzionante |
| Cartella di lavoro | `C:\Users\luca_nasi\Luca\Little Witch in the Woods\Little Witch in the Woods in Italiano` |
| Stato | **plugin e installatore completi**. Interfaccia al **100%**, dialoghi all'**82,6%** |

Ambiente: Windows ARM64 (il gioco e' x64 in emulazione), Python 3.12.10, .NET 8 SDK.

---

## Cosa e' fatto

**L'italiano e' una lingua vera del gioco**, verificato a schermo il 2026-08-18: il plugin
la registra all'avvio, le tabelle italiane vengono costruite e i testi tradotti compaiono.

- 17 script Python funzionanti in `tools\` (vedi «Comandi»)
- Testi inglesi estratti in `Extracted\Locale\en\` e `Extracted\Dialoghi\en.json`
- Font estratti in `Extracted\Font\`, piu' `JejuHallasan-IT.ttf` **con gli accenti italiani**
- Assembly di localizzazione decompilati in `Extracted\Decompiled\`
- **Plugin BepInEx** in `Mod\LittleWitchIT\` (assembly: `LittleWitchItalian.dll`)
- **Aggancio dei dialoghi** (`Dialoghi.cs`): il database inglese viene caricato anche in
  italiano e a ogni battuta si innesta un campo `it`
- **L'interfaccia e' tradotta e validata al 100%**: 8.656 voci su 8.656, tutte e
  ventiquattro le tabelle chiuse, circa 496.000 caratteri di italiano.

  | | | | |
  |---|---|---|---|
  | `Item` 3.860 | `QuestNode` 1.517 | `UI` 640 | `Encyclopedia` 423 |
  | `Mail` 279 | `KeywordAndDelivery` 269 | `Tutorial` 252 | `MerchantDialogue` 224 |
  | `InteriorPropBasicName` 215 | `QuestUI` 158 | `EncyclopediaAchievement` 139 | `InteractableObject` 117 |
  | `KeyGuide` 114 | `Common` 69 | `PeopleNote` 69 | `InteriorPropTheme` 55 |
  | `BroomstickTheme` 46 | `Post` 46 | `EffectFoodCommon` 39 | `Cat` 32 |
  | `Map` 30 | `StringFormatTemplate` 29 | `Choice` 27 | `BroomstickCommon` 7 |

  Resta **il testo dei dialoghi**: 39.035 battute, 1.966.485 caratteri. Tradotte
  **32.261 battute (82,6%)**. Restano **323.882 caratteri**. Sono chiusi al 100%: tutto il
  **prologo** (vecchio e nuovo, a meno di qualche frammento di `NewPrologue/`), i gruppi
  **`Book/`, `Potion/`, `etc/`** e — dopo la sessione del 2026-08-24 — **tutti i
  personaggi**, nessuno escluso: sotto `People/` non resta una sola battuta.

  Quello che manca sono i gruppi non legati a un personaggio, cioe' soprattutto le
  cinematiche della trama principale: `Event2/WhiteCat` 131k, `Event/WhiteCat` 81k,
  `Event2/Museum` 17k, `Objects/RainbowForest` 16k, `Event/WelcomeParty` 11k, gli altri
  `Objects/` (~35k in tutto) e i frammenti di `NewPrologue/` (~12k).

### La scorciatoia che ha reso `Item` fattibile

`Item` sembrava 3.860 voci da scrivere a mano. In realta' **la maggior parte non contiene
una parola d'inglese**: sono composizioni di riferimenti ad altre tabelle, tipo
`{InteriorPropTheme.AnglersDream} {InteriorPropBasicName.Bed}`, e quelle tabelle erano
gia' tradotte. `tools/13_componi_nomi.py` le genera:

- per i **nomi** rimette le parole nell'ordine italiano — l'inglese antepone il tema al
  sostantivo (*Fisher's Dream Bed*), l'italiano fa il contrario (*Letto Sogno del
  Pescatore*);
- per le **descrizioni** di sola composizione non c'e' niente da tradurre: i riferimenti
  si risolvono da soli sulle tabelle italiane, quindi si copiano identiche.

Cosi' 3.149 voci su 3.860 si sono fatte da se', e a mano sono rimaste solo quelle con
inglese vero. Se si ritocca una tabella di base, basta rilanciare lo script.

`tools/14_copia_composte.py` fa la stessa cosa **sulle altre tabelle**, dove non c'e'
niente da riordinare: copia intatte le voci di sola composizione. Fuori da `Item` pero'
sono appena 22, quindi il grosso resta da scrivere a mano.

**Le taglie sono al maschile** (`grande`, `medio`, `piccolo`) perche' gli unici sostantivi
che le prendono sono *Tappeto* e *Deposito*, maschili entrambi: verificato, non tirato a
indovinare.

### Le tabelle si ripetono fra loro: 127 voci gratis

Le tabelle non sono mondi separati. Il nome di una creatura sta in `Encyclopedia` **e** in
`Item`; una voce di menu ricompare in `UI`; una descrizione e' identica in due posti. Se
la stessa stringa inglese e' gia' stata resa in italiano, ritradurla non e' solo lavoro
doppio: e' il modo sicuro di finire con **due nomi diversi per la stessa cosa** sotto gli
occhi del giocatore, a un minuto di distanza.

`tools/17_riusa_gemelli.py` copia la traduzione quando l'inglese combacia esattamente (a
parte gli spazi ai bordi), e non tocca mai una voce gia' tradotta. Ha regalato **127
voci**: 111 in `Encyclopedia` (i nomi di caramelle, pozioni e creature, tutti gia' in
`Item`), 15 in `QuestNode`, 1 in `Item`. Va rilanciato ogni volta che si chiude una
tabella, perche' ogni tabella nuova ne riempie altre.

`tools/18_parenti.py` risolve il caso in cui l'inglese **non** combacia. Le chiavi hanno
un prefisso comune — `Baitty_Name` in `Encyclopedia` e `Baitty_Collect_Name` in `Item`
parlano della stessa creatura — e lo script stampa, accanto alle voci da tradurre, il
glossario di quello che e' gia' fissato per quel prefisso. E' cosi' che *Moon Brilliance*
e' diventato **Lucelunare** e non un secondo nome inventato: la sua squama era gia'
«squama caudale di Lucelunare» in `Item`.

### I dialoghi non sono una tabella come le altre

Il testo sta annidato in `conversations[idConversazione].voci[idVoce].en`, e i marcatori
da conservare **non sono** i segnaposto `{...}` dell'interfaccia. Sono tre:

- `[em1]…[/em1]` e `[em2]…[/em2]`, l'enfasi di PixelCrushers (4.300 e 56 occorrenze);
- `[lua(...)]`, che **esegue codice** — `[lua(GetItemName("Fish_Axe"))]` — e cambiarne una
  virgola rompe la battuta;
- i tag TextMeshPro `<shake>`, `<wave>`, `<swing>`, `<wiggle>`, `<color=…>`,
  `<font="Astronomicon SDF">`.

Attenzione: `<Waiting for a gift>` **non** e' un tag, e' una didascalia da tradurre.
`21_dialoghi_valida.py` fa la stessa distinzione che fa `12_valida.py` sull'interfaccia,
e ha gia' preso un errore vero: una battuta in cui la parola enfatizzata era sparita
insieme ai suoi marcatori.

**I marcatori rotti dell'originale vanno riprodotti rotti.** In `People/Bjorn` c'e' un
`[em1]cat furniture[em1]` senza barra e un `[/em1]]` con una quadra di troppo. Il
validatore confronta la struttura dei marcatori, non la loro correttezza: «correggerli»
fa fallire la validazione. Si riscrivono identici.

**Le quantita' si girano.** L'inglese scrive `10x [em1][lua(...)][/em1]`, ma il `lua`
restituisce il nome al singolare: «10 Ramo gigante» sarebbe sgrammaticato. In italiano si
scrive `[em1][lua(...)][/em1] x10`, che e' la forma normale nei giochi e lascia l'`[em1]`
a fasciare esattamente le stesse parole.

**`20_dialoghi_mostra.py` si ferma a 400 battute e nasconde il resto.** Non tronca solo
l'elenco delle battute: le conversazioni che vengono dopo in ordine alfabetico non
compaiono **affatto**, nemmeno come titolo. Su Arden aveva nascosto tutto il gruppo
`Waterfall/`, su Vinch tre conversazioni intere che sembravano gia' fatte. Prima di
dichiarare chiuso un personaggio va rilanciato finche' non risponde «niente da tradurre».

**Il blocco tradotto conviene scriverlo con uno script Python, non a mano in JSON.** I
`[lua(GetLocalizedString("Item", "..."))]` sono pieni di virgolette doppie: definirli come
variabili in cima allo script e comporre le battute con le f-string toglie di mezzo una
classe intera di errori di escaping, e permette di generare da un ciclo le conversazioni
che ripetono la stessa frase su nove materiali diversi.

**`actors`, `items` e `variables` non si traducono.** Sembrerebbero tre sezioni di testo,
e invece contengono nomi interni (`Player`, `Eventer`) e note in coreano degli
sviluppatori: sono logica. Da tradurre c'e' **solo** `conversations`.

**1.197 battute non vanno tradotte affatto.** Sono chiamate come
`[lua(GetLocalizedString("Choice", "Yes"))]`: non sono inglese, sono riferimenti che il
gioco risolve da se' sulla tabella `Choice`, gia' tradotta. `22_dialoghi_gratis.py` le
copia identiche, e nello stesso giro riusa le battute con l'inglese identico gia' reso
altrove (`...` compare 327 volte, «What do you mean?» 50). Va rilanciato **dopo ogni
blocco**: ogni pezzo tradotto ne sblocca altri — finora ha regalato oltre 3.400 battute.

### Il difetto delle etichette col due punti

`13_componi_nomi.py` mandava in fondo **tutte** le parole sciolte, perche' in italiano il
tema segue il sostantivo. Ma «Recipe:» non e' un tema: e' un'etichetta che annuncia quello
che viene dopo, e sette nomi di ricette erano diventati *«Pozione dimagrante Ricetta:»*.
Corretto nello script (una parola che finisce col due punti resta davanti), non a mano:
altrimenti al primo rilancio sarebbero tornati sbagliati.

### Deroghe registrate

`tools/12_valida.py` confronta ogni traduzione con l'originale (segnaposto, tag TMP, a
capo). Dove lo scostamento e' voluto non si allenta il controllo: si registra la deroga
con la ragione in `Mod/translations/eccezioni.json`. Al momento sono 21, tutte su `Item`:
20 sono a capo spurii dentro i nomi degli oggetti (refusi dell'originale) e uno e' una
graffa mancante nell'originale, corretta.

Dal log della prova del 2026-08-18:

```
Lingua aggiunta: 'it' (Italian)
Fornitore di tabelle installato
Lingua d'avvio forzata a 'it'
[asset]   Fonts: 4 voci ereditate dall'inglese
[asset]   FontMaterials: 11 voci ereditate dall'inglese
[asset]   TextSettings: 27 voci ereditate dall'inglese
[accenti] font di ripiego: '思源黑体CN-Medium' (lingua zh)
[accenti] verifica 'JejuHallasan': ora le ha
[tabella] Common: 69 tradotte, 0 in inglese
[tabella] UI: 640 tradotte, 7 in inglese
[tabella] KeyGuide: 114 tradotte, 0 in inglese
[menu] lingue disponibili: 6   (en, ko, zh, zh-TW, it, ja)
```

Verificato a schermo il 2026-08-18: la schermata del titolo dice **«Premi un tasto
qualsiasi»**.

**Non ancora verificato a schermo**, perche' il gioco non risponde a `SendKeys` (usa il
nuovo Input System, che legge i dispositivi e ignora i messaggi di finestra) e perche'
serve entrare in partita:

- la voce «Italiano» in Impostazioni → Lingua;
- l'**innesto dei dialoghi**, che scatta solo quando parte una partita. Al primo avvio il
  log dira' da solo se ha funzionato: cerca le righe `[dialoghi]`. Se compare
  `ATTENZIONE: N battute risultano vuote`, qualcosa non va e le battute sarebbero mute;
  se compare `rilettura: N battute controllate, nessuna vuota`, e' a posto.

---

## Le tre differenze che contano rispetto a Rubinite

Non e' «lo stesso lavoro su un altro gioco». Tre cose cambiano la natura del progetto.

### 1. La mole e' circa 30 volte quella di Rubinite

Misurato, non stimato:

| | voci | caratteri |
|---|---:|---:|
| String table UI (25 tabelle) | 8.741 | 471.898 |
| Dialoghi (2.344 conversazioni) | 48.629 | 1.966.485 |
| **Totale** | **57.370** | **2.438.383** |

Rubinite erano **1.948 termini**. Qui siamo a circa **2,4 milioni di caratteri**, nell'ordine
delle 400.000 parole: il volume di alcuni romanzi. E' il fatto che decide tutto il resto —
tempi, metodo, e se ha senso puntare al 100% da subito.

Le tabelle piu' grosse: `Item` 3.880 voci (233k caratteri), `QuestNode` 1.574 (57k),
`Encyclopedia` 423 (27k), `Mail` 279 (27k), `Tutorial` 252 (24k), `QuestUI` 158 (21k),
`UI` 647 (18k).

### 2. Nessun font del gioco sa scrivere in italiano

Il gioco ha solo inglese, coreano, giapponese e i due cinesi: le vocali accentate non sono
mai servite. Censiti **15 TMP_FontAsset**, controllati i TTF incorporati:

| Font | usato da | accenti `àèéìòù` |
|---|---|---|
| `JejuHallasan` | **inglese** | **nessuno** |
| `Handwriting-Regular` | **inglese** | **nessuno** (74 caratteri in tutto: e' un sottoinsieme ritagliato) |
| `Cafe24Dongdong`, `Cafe24Ohsquare` | condivisi | mancano `èéìòÌÙ` |
| `Kyobo Handwriting 2019`, `SDSamliphopangche`, `Ondol` | coreano | nessuno |
| `思源黑体CN-Medium`, `濑户字体` | cinese/giapponese | tutti presenti |

Nessuno dei due font della lingua inglese ha un solo accento. Senza intervenire, ogni
*perché* a schermo diventa un rettangolo vuoto.

**Risolto in gioco**, verificato dal log il 2026-08-18. Ci sono due soluzioni, una che gira
gia' e una migliore da finire.

*Quella che gira* — `Mod\LittleWitchIT\Accenti.cs`. I font sono in modalita' **Dynamic**:
TMP genera i glifi a runtime dal TTF incorporato, e ha un meccanismo di **ripiego**
(`fallbackFontAssetTable`) per i caratteri che il font principale non sa fare. Fra i font
del gioco stesso ce n'e' uno che le accentate le ha tutte: `思源黑体CN-Medium`, quello del
cinese. Il plugin lo aggancia come ripiego ai quattro font dell'italiano. Non ridistribuisce
niente e non tocca nessun file. Dal log:

```
[accenti] font 'JejuHallasan': mancano àèéìòùÀÈÉÌÒÙ
[accenti] font di ripiego: '思源黑体CN-Medium' (lingua zh)
[accenti] verifica 'JejuHallasan': ora le ha
```

Difetto: le lettere accentate hanno il disegno di un altro font, e in mezzo a una parola si
notano.

*Quella migliore, da finire* — `tools\06_accenta_font.py`. `JejuHallasan` e' TrueType e ha
gia' dentro **sia le lettere base sia i segni** `grave`, `acute`, `dieresis`: manca solo la
lettera accentata come glifo unico. Lo script la costruisce come **glifo composito**
(riferimento alla lettera + riferimento al segno, centrato sopra) e la mappa nella cmap.
Sedici lettere — `àèéìíòóùúá ÀÈÉÌÒÙ` — con la `i` presa da `dotlessi`, come vuole la
tipografia. Gli accenti sono quelli del font originale, quindi lo stile combacia: si vede in
`Extracted\Font\prova_JejuHallasan-IT.png`. **Manca solo il modo di far digerire a TMP un
TTF che non sta in un bundle del gioco** (vedi «Il nodo del font nostro»).

Resta un dettaglio tipografico: `«» …` esistono in `JejuHallasan` ma sono i glifi **a
larghezza piena** delle lingue CJK e a schermo risultano larghi e centrati. Meglio usare le
virgolette dritte e i tre punti normali.

**Per sapere se un font Dynamic sa scrivere una lettera, `HasCharacters` non serve.**
Risponde di no anche quando il TTF sotto ce l'ha, perche' guarda solo i glifi gia' cotti — e
questi font partono con **zero** glifi. Va usato `TryAddCharacters`, che prova a generarli
davvero. Con `HasCharacters` il primo giro aveva concluso «nessun font del gioco contiene le
vocali accentate», che e' falso.

### Il nodo del font nostro

Per usare `JejuHallasan-IT.ttf` servirebbe darlo a TMP, e in Unity 2022.3 non e' immediato:
`TMP_FontAsset.CreateFontAsset()` accetta solo un `Font` di Unity, che a runtime si ottiene
o da un AssetBundle o da un font installato nel sistema operativo. `FontEngine` ha bene
`LoadFontFace(byte[])`, ma TMP ricarica comunque la faccia da `sourceFontFile` ogni volta
che deve aggiungere un glifo. Le tre strade, da valutare:

1. impacchettare il TTF in un AssetBundle nostro (fatto con UnityPy partendo da uno del
   gioco: attenzione ai nomi CAB che collidono);
2. installarlo come font utente (niente permessi di amministratore, ma tocca il registro);
3. iniettare a mano i sedici glifi nell'atlas del font del gioco con `FontEngine`.

### 3. Il testo sta in due sistemi diversi, non in uno

Rubinite aveva un unico `I2Languages`. Qui:

- **UI** → Unity Localization. Le `StringTable` stanno nei bundle
  `localization-string-tables-<lingua>_assets_all.bundle`; i nomi delle chiavi stanno negli
  asset `<Nome> Shared Data` dentro `localization-assets-shared_assets_all.bundle`.
- **Dialoghi** → PixelCrushers. Un `DialogueDB_<codice>.asset` per lingua, tutti dentro
  `dialoguedb_assets_all.bundle` (127 MB). `DialogueSystemLocaleInitializer.Start()` carica
  `"DialogueDB_" + codice + ".asset"` via Addressables.

Vanno tradotti e agganciati **tutti e due**, con due meccanismi diversi.

---

## Fatti tecnici da non riscoprire

**Il testo dei dialoghi non sta nel campo `Dialogue Text`, e per un giro intero
`02_estrai_dialoghi.py` non l'ha estratto.** Lo script teneva solo i campi «di testo»
noti (`Dialogue Text`, `Menu Text`, `Title`...) e `en.json` conteneva **la sola
struttura**: 6.873 titoli e nemmeno una battuta. Corretto il 2026-08-18 aggiungendo il
codice lingua ai campi da tenere; adesso `en.json` ha le **39.035 battute** e i
**1.966.485 caratteri** attesi. Se il conteggio a fine estrazione non mostra una riga
`en` con decine di migliaia di voci, il file e' di nuovo vuoto di testo.

**Il testo dei dialoghi non sta nel campo `Dialogue Text`.** Sta in un campo che si chiama
come il **codice lingua**: `en`, `ko`, `ja`... Il campo `Dialogue Text` esiste in tutte le
48.776 voci ma e' quasi sempre vuoto (270 caratteri in totale, roba tipo `<납품 UI>`).
Cercare li' fa concludere che il gioco non abbia dialoghi.

**Il menu delle lingue non si costruisce dalle lingue disponibili.** `LocalizationSetter.Awake()`
(`SunnySideUp.UI.Settings.dll`) parte da `_localeNames`, un array **serializzato nella scena**
che accoppia ogni `Locale` a un `LocalizedString` col nome da mostrare, e ne tiene solo le
voci il cui Locale e' anche in `GetAvailableLocales()`. Aggiungere il Locale italiano a
`AvailableLocales` **non basta**: senza una voce corrispondente in `_localeNames` (campo
privato, raggiungibile per reflection) l'italiano resta invisibile nel menu.

**La lingua si applica al riavvio, non subito.** La scelta finisce in `PlayerPrefs`
(`SelectedLanguageCode`), e viene letta all'avvio da `PlayerPrefsLocaleSelector.GetStartupLocale()`
con `CultureInfo.GetCultureInfo(codice)`. `"it"` e' una cultura valida, non da' problemi.
C'e' gia' un messaggio a schermo che avvisa di riavviare.

**Font, materiale e impostazioni di testo sono per-lingua.** `LocalizeFontAssetEvent`,
`LocalizeFontMaterialEvent`, `LocalizeTextSettingEvent` risolvono un
`LocalizedAsset<...>` sulla lingua corrente. Se si registra il Locale italiano senza
riempire anche le **asset table**, il testo resta senza font. La contromossa e' clonare a
runtime le tabelle inglesi sull'italiano — cosi' font e stile restano quelli latini
dell'inglese — e sovrascrivere solo le stringhe.

**Le tabelle italiane si forniscono con `ITableProvider`, non con la reflection.**
`LocalizedDatabase.TableProvider` e' una proprieta' pubblica, e `LoadTableOperation`
la interroga **prima** di andare su Addressables (`TryLoadWithTableProvider()`); se si
restituisce un handle non valido, il caricamento normale prosegue. E' il punto di
aggancio giusto: le altre lingue non vengono neanche sfiorate. Vale sia per
`StringDatabase` sia per `AssetDatabase`.

**Le tabelle vanno clonate da quelle inglesi, non costruite da zero.** Ogni voce e'
indirizzata da un **id numerico** che sta nel `SharedTableData`, non dal nome della
chiave: riusando lo stesso `SharedData` dell'inglese gli id restano quelli che il gioco
si aspetta. Clonare anche le **asset table** (`Fonts`, `FontMaterials`, `TextSettings`)
fa ereditare all'italiano la grafica latina invece di lasciarlo senza font.

**Tre trappole di tempismo, tutte pagate.** Sono la ragione per cui il plugin ha questa
forma e non una piu' ovvia.

1. **`PlayerPrefsLocaleSelector.GetStartupLocale()` gira dentro una callback di
   Addressables.** Sembra il posto ideale per preparare tutto — le lingue sono caricate,
   nessuna tabella e' stata ancora chiesta — ma qualunque `WaitForCompletion()` li' dentro
   lancia *«Reentering the Update method is not allowed»*. Li' si puo' solo registrare la
   lingua, che e' sincrono e non carica niente.

2. **Percio' le tabelle si costruiscono a catena, non aspettando.** `ProvideTableAsync`
   viene chiamata anch'essa dentro l'Update del ResourceManager. Invece di aspettare la
   tabella inglese si restituisce un `ResourceManager.CreateChainOperation` che la clona
   quando sara' pronta. In piu' e' piu' economico: costruisce solo le tabelle che il gioco
   chiede davvero.

3. **L'oggetto del plugin BepInEx smette di ricevere `Update()`.** `Awake()` parte e le
   patch Harmony restano attive per sempre (sono statiche), ma `Update()` non viene mai
   chiamato: verificato con un contatore che non ha mai scritto una riga. **Quindi le
   coroutine del plugin non girano.** Tutto cio' che va fatto «piu' tardi» va agganciato a
   un metodo del gioco, non a una coroutine nostra.

**Dove si puo' aspettare in sicurezza: `DialogueSystemLocaleInitializer.Start()`.** E' un
`MonoBehaviour` con `[DefaultExecutionOrder(-1)]` che fa gia' `WaitForCompletion()` per
conto suo sui propri handle Addressables — prova che li' l'attesa sincrona e' lecita. E'
il punto in cui il plugin carica i font e aggancia il ripiego per gli accenti.

**I typetree non sono strippati.** Al contrario di Rubinite, UnityPy legge i MonoBehaviour
direttamente con `read_typetree()`. Niente parser binari a mano.

**UnityPy su questa macchina va installato a pezzi.** ARM64 non ha wheel per `brotli` e
`etcpak`, che non compilano. Serve:

```
pip install "UnityPy==1.25.3" --no-deps
pip install lz4 attrs fsspec pillow tpk_ar texture2ddecoder fonttools
```

piu' il tappo `tools\_shims\brotli.py`, che `tools\lwiw.py` mette nel path da solo. I bundle
del gioco sono LZ4, brotli non viene mai chiamato davvero.

**ilspycmd va forzato a girare su .NET 8.** La versione piu' recente non si installa
(`DotnetToolSettings.xml` mancante); la 8.2.0.7535 si installa ma vuole .NET 6. Si risolve
con `DOTNET_ROLL_FORWARD=LatestMajor`:

```
dotnet tool install --global ilspycmd --version 8.2.0.7535
export DOTNET_ROLL_FORWARD=LatestMajor
```

**La copia in `C:\Games` gira con l'emulatore Goldberg**, non con Steam
(`steam_api64.dll.bak` + cartella `steam_settings` accanto alla DLL). Va bene per provare la
mod, ma **non riproduce il DRM di Steam**: il guaio che su Rubinite e' costato una sessione
intera — il gioco che si rilancia e uccide il processo in cui e' entrato BepInEx — qui non si
manifesterebbe. Prima di distribuire, provare su una copia Steam vera.

---

## Struttura

```
Little Witch in the Woods in Italiano\
├── tools\                  script Python di analisi
│   ├── lwiw.py             percorsi comuni + tappo brotli
│   ├── _shims\brotli.py
│   ├── 01_estrai_testi.py
│   ├── 02_estrai_dialoghi.py
│   ├── 03_verifica_font.py
│   ├── 04_censimento_font.py
│   ├── 05_estrai_ttf.py
│   ├── 06_accenta_font.py
│   ├── 07_prova_font.py
│   ├── 10_aggiorna_gioco.py    copia plugin e traduzioni nel gioco
│   ├── 12_valida.py            controlla e fonde i blocchi tradotti
│   ├── 13_componi_nomi.py      genera i nomi composti di Item
│   ├── 14_copia_composte.py    copia le voci di sola composizione
│   ├── 15_mostra.py            elenca cosa manca, una voce per riga
│   ├── 16_gia_tradotto.py      segnala i gemelli gia' tradotti altrove
│   ├── 17_riusa_gemelli.py     riusa le traduzioni con l'inglese identico
│   ├── 18_parenti.py           glossario dei nomi gia' fissati per prefisso
│   ├── dialoghi.py             roba comune ai tre strumenti dei dialoghi
│   ├── 20_dialoghi_mostra.py   elenca le battute da tradurre
│   ├── 21_dialoghi_valida.py   controlla e fonde i blocchi di dialogo
│   ├── 22_dialoghi_gratis.py   copia il codice e riusa i doppioni
│   └── 30_crea_installer.py    assembla il pacchetto da distribuire
├── Extracted\
│   ├── Locale\en\*.json    25 tabelle UI inglesi
│   ├── Dialoghi\en.json    dialoghi inglesi (39.035 battute)
│   ├── Font\*.ttf          i TTF incorporati, estratti, piu' JejuHallasan-IT.ttf
│   └── Decompiled\         Localization, UISettings, TMP, FontEngine (ILSpy)
├── Mod\
│   ├── LittleWitchIT\      il plugin BepInEx
│   │   ├── Plugin.cs           avvio, patch Harmony, menu della lingua
│   │   ├── TabelleItaliane.cs  ITableProvider: clona l'inglese e ci mette l'italiano
│   │   ├── Accenti.cs          font di ripiego per le vocali accentate
│   │   ├── Battito.cs          oggetto nostro che riceve Update()
│   │   └── Testi.cs            lettura dei JSON di traduzione
│   ├── installer\             sorgenti dell'installatore (bat, ps1, LEGGIMI)
│   ├── translations\parts\*.json      interfaccia  {chiave: testo}
│   ├── translations\dialoghi\*.json   dialoghi  {conversazioni: {id: {id: testo}}}
│   └── _refs\BepInEx-core\         DLL di riferimento per compilare
├── _downloads\             BepInEx_win_x64_5.4.23.3.zip
└── _dist\                  il pacchetto pronto da distribuire (generato)
```

Installazione nel gioco: `C:\Games\Little Witch in the Woods\BepInEx\plugins\LiukNoceda\`,
con le traduzioni nella sottocartella `traduzioni\`.

---

## Comandi

```bash
python tools/01_estrai_testi.py en        # tabelle UI  -> Extracted/Locale/<lingua>/
python tools/02_estrai_dialoghi.py en     # dialoghi    -> Extracted/Dialoghi/<lingua>.json
python tools/03_verifica_font.py          # accenti nei font del bundle principale
python tools/04_censimento_font.py        # tutti i TMP_FontAsset del gioco
python tools/05_estrai_ttf.py             # TTF incorporati + cmap
python tools/12_valida.py                 # controlla le traduzioni contro l'inglese
python tools/13_componi_nomi.py --scrivi  # rigenera i nomi composti di Item
python tools/14_copia_composte.py --scrivi   # copia le voci di sola composizione
python tools/12_valida.py --unisci blocco.json UI    # fonde un blocco e valida
python tools/15_mostra.py Tutorial 0 60   # elenca cosa manca, una voce per riga
python tools/16_gia_tradotto.py Encyclopedia # come 15, ma segnala i gemelli gia' tradotti
python tools/17_riusa_gemelli.py --scrivi    # riusa le traduzioni con l'inglese identico
python tools/18_parenti.py Encyclopedia 0 40 # da tradurre + glossario dei nomi gia' fissati
python tools/20_dialoghi_mostra.py           # quali conversazioni mancano
python tools/20_dialoghi_mostra.py People/Arden   # le battute di quel gruppo
python tools/21_dialoghi_valida.py           # controlla i dialoghi contro l'inglese
python tools/21_dialoghi_valida.py --unisci blocco.json 04_gruppo   # fonde e valida
python tools/22_dialoghi_gratis.py --scrivi  # copia il codice e riusa i doppioni
python tools/06_accenta_font.py           # compone le accentate -> Font/<nome>-IT.ttf
python tools/07_prova_font.py             # foglio di prova PNG da guardare
```

Giro «modifico → provo»:

```bash
cd Mod/LittleWitchIT && dotnet build -c Release   # compila il plugin
python tools/10_aggiorna_gioco.py                 # copia nel gioco e azzera il log
python tools/10_aggiorna_gioco.py --solo-testi   # solo i JSON, col gioco aperto
```

Per cambiare solo le traduzioni **non serve ricompilare**: basta `10_aggiorna_gioco.py` e
rilanciare, perche' il plugin legge i JSON all'avvio. Il log sta in
`C:\Games\Little Witch in the Woods\BepInEx\LogOutput.log`.

Senza argomenti, `01` estrae tutte le lingue: utile per confrontare come i traduttori
ufficiali hanno reso un termine.

---

## L'installatore

Fatto e collaudato il 2026-08-24.

```bash
python tools/30_crea_installer.py            # cartella + zip in _dist/
python tools/30_crea_installer.py --no-zip   # solo la cartella
```

Assembla in `_dist/Little Witch in the Woods in italiano/`:

```
Installa la traduzione italiana.bat      il lanciatore da cliccare
LEGGIMI.txt
_installer/installer.ps1                 la logica
_installer/contenuto/                    BepInEx, il plugin, tutti i testi
```

I sorgenti stanno in `Mod/installer/` (`avvia.bat`, `installer.ps1`, `LEGGIMI.txt`); il
carico viene preso ogni volta dalla build corrente e da `Mod/translations/`, quindi basta
rilanciare lo script per rifare il pacchetto dopo aver tradotto altro.

**Cosa fa lo script PowerShell.** Trova il gioco da solo — registro di Steam
(`SteamPath`/`InstallPath`), tutte le librerie di `steamapps/libraryfolders.vdf`, il
registro di GOG, `C:\Games` — e controlla che nella cartella ci siano `LWIW.exe` e
`LWIW_Data`. Se non lo trova, si fa trascinare la cartella (accetta anche l'eseguibile:
risale al padre). Poi un menu: installa/aggiorna, disinstalla, leggi il log, cambia
cartella.

- **Installa**: estrae BepInEx solo se `winhttp.dll` manca, copia il plugin, e **rifa da
  zero** la cartella `traduzioni` cosi' i file di una versione vecchia non restano in giro.
- **Disinstalla**: toglie `BepInEx/plugins/LiukNoceda`, rimette la lingua su inglese, e
  propone di togliere anche BepInEx **solo se non ci sono altre mod** in `BepInEx/plugins`.
- **Log**: filtra `LogOutput.log` sulle righe `[tabella] [dialoghi] [accenti]` e dice se
  ci sono battute vuote.

Se la cartella del gioco non e' scrivibile si offre di rilanciarsi come amministratore.

**Tre trappole gia' pagate qui.**

1. **Il `.ps1` va scritto con il BOM UTF-8.** Windows PowerShell 5.1 legge come ANSI i file
   senza BOM: senza il BOM ogni `è` diventa spazzatura. Ci pensa `30_crea_installer.py`,
   che ricopia i testi con `utf-8-sig` (i sorgenti in `Mod/installer/` restano normali).
2. **Dentro le stringhe a singolo apice di PowerShell l'apostrofo va raddoppiato**:
   `'lo lascio com''è.'`. Con uno solo la stringa si chiude a meta' e il file non compila.
3. **`"$env:ProgramFiles(x86)"` non funziona**: PowerShell legge `$env:ProgramFiles` e poi
   il letterale `(x86)`. Serve `"${env:ProgramFiles(x86)}"`.

**La lingua nel registro.** I PlayerPrefs del gioco stanno in
`HKCU\Software\SunnySideUp\Little Witch In The Woods`. Le due chiavi della lingua sono
`Language_h3872303031` e `SelectedLanguageCode_h729211379`, entrambe **binarie**: la
stringa UTF-8 piu' uno zero finale. Disinstallando ci si scrive `en`, cosi' il giocatore
non resta con una lingua che non esiste piu'.

**Cosa e' stato provato davvero**: installazione da zero su una cartella finta (BepInEx
estratto, 18 file in `BepInEx/core`), aggiornamento sopra un'installazione esistente,
disinstallazione con la lingua rimessa su inglese, e la voce che rilegge il log. Resta da
provare il rilancio come amministratore su una cartella protetta.

---

## Cosa resta

Deciso con l'utente il 2026-08-18: **prima il plugin, poi i testi**, e le traduzioni le
scrive Claude a blocchi con revisione dell'utente.

1. **Finire i dialoghi.** E' l'unico testo che manca. Al 2026-08-24 restano
   **323.882 caratteri**: i personaggi sono tutti chiusi, restano le cinematiche
   (`Event2/WhiteCat` 131k, `Event/WhiteCat` 81k), il museo (`Event2/Museum` 17k),
   gli `Objects/` (~57k), `Event/WelcomeParty` 11k e i frammenti di `NewPrologue/`.
   Gli strumenti ci sono tutti (vedi «I dialoghi non sono una tabella come le altre»).
2. **Guardare a schermo la voce «Italiano»** in Impostazioni → Lingua, la resa delle
   accentate e l'innesto dei dialoghi. E' l'unico pezzo scritto ma non ancora visto: al
   primo avvio il log dice da solo se l'innesto ha funzionato (cerca le righe
   `[dialoghi]`).
3. **Glossario** — le regole di stile sono in uso da 8.656 voci, ma non sono ancora
   scritte in un `GLOSSARIO.md` come quello di Rubinite.
4. **Il font nostro**, per togliere il ripiego cinese (vedi «Il nodo del font nostro»).
5. ~~**Installer e manager**~~ — **fatto** il 2026-08-24, vedi «L'installatore».
6. **Provare su una copia Steam vera**, che questa non e' (vedi Goldberg piu' sopra).

### Regole di stile, in uso su tutta l'interfaccia e su 11.000 battute

- **Ellie e' femmina**, tutti gli accordi la seguono. Al giocatore si da' del **tu**.
- Nomi propri invariati: Ellie, Arden, Arin, Kyla, Rex, Theo, Vinch, Roy, Clala, Enite,
  Rubrum, Diane, Aurea, Bjorn, Alvin, Dana, Freddie, Laurel, Baobab, Rahel, Tanis,
  Wisteria, Highlion, Bellody, Deviyark, Lumineon, Nebulune, Noctilux, Ponglin, Violuna,
  Angelica, Lisa Whitegarden, Ellie Blueriver, Angelo Skystone, Greenvine, Redember,
  Blueriver, Dosca, Stellarium, Janifer, Merlin, Tatton.
  I **titoli e i nomi dei luoghi** si traducono: Casa della Strega, Foresta Verde, Valle
  delle Nuvole, Grotta Stellata, Museo degli Archivi, Museo Centrale degli Archivi, Scuola
  per Streghe, Catalogo delle Streghe, Amministrazione delle Streghe, Ferrovie Brightman,
  Ministero delle Ferrovie.
- Termini fissati: Candy → Caramella, Potion → Pozione, Craft → Crea/Creazione, Quill →
  piuma, Encyclopedia → enciclopedia, Memo Board → bacheca, Friendship → amicizia,
  Energy → Energia, Stamina → Resistenza, Workbench → banco da lavoro, Extractor →
  estrattore, Roaster → tostatrice, Incubator → incubatrice, prickly vine → rovo spinoso,
  Ritoring's Gift → Dono di Ritoring, Luna Coin → moneta lunare, **bag → borsa** (mai
  «zaino»: due voci di `Choice` sono state allineate apposta).
- Fissati sui dialoghi: the Warrior → **la Guerriera**; journal → **diario**;
  thousand-year dragon → **drago millenario**; hair tie → **fermacapelli**; night light →
  **luce notturna**; Tear of Dragon → *Lacrima di drago*, **femminile** («la», «le»);
  prickly vine core → **cuore del rovo spinoso**; Caww!/Caw! → **Craa!** / **Cra!**;
  Save → **Salva**; Load Game → **Carica partita**; Purchase → **Acquista**; Sell →
  **Vendi**; Donate → **Dona**; Expand Workshop → **Amplia il laboratorio**; Delivery →
  **consegna**; First Witch → **Prima Strega**; knowledge of the star → **sapere della
  stella**; wisdom of the stars → **saggezza delle stelle**; Star Crystal → **cristallo
  stellare**; Drop of Emotion → **goccia di emozione**; White/Black Cat God → **Dio Gatto
  Bianco/Nero**; eagerness (Gatto Nero) → **ardore**; Token of Friendship → **Pegno di
  amicizia**; star honey wine → **vino di miele stellare**; cat furniture → **mobili per
  gatti**; blue rose → **rosa blu**; Flower of Love → **Fiore dell'Amore**; stone skipping
  → **far rimbalzare i sassi**; Twinkle-Twinkle Juice → **Succo scintillante**; Meteor Fall
  → **Pioggia di meteore**; Seafloor Cave in the Sky → **Grotta del Fondale in Cielo**;
  grafting → **innesto**; bond → **legame**; **Alvin e' uno stregone**, non «una strega»:
  e' un uomo, e a un certo punto il gioco stesso passa a 'wizard' — le istituzioni pero'
  restano al femminile (Societa' delle Streghe, Scuola per Streghe); «Red Light, Green Light» → **Un, due, tre,
  stella!** (e' lo stesso gioco, non una traduzione letterale); cassette (l'aggeggio di Vinch) → **camera
  oscura**; Records Department → **Dipartimento degli Archivi**; Stellar Knowledge →
  **Sapere della Stella**; delivery girl → **fattorina**; bakery → **forno** (ma *Munchy
  Bakery* resta invariato); Society of Witches → **Societa' delle
  Streghe**; Witch Administration → **Amministrazione delle Streghe**; Witch-Human
  Information Exchange Department → **Dipartimento di Scambio Informazioni fra Streghe e
  Umani**; witch brooch → **spilla da strega**; census → **censimento**.
- **Il registro e' fissato per personaggio.** Danno del **lei** a Ellie solo il **barista**
  del treno, il **personale del treno**, **Kent** e **Baobab** (che la chiamano «signorina
  Ellie»).
  Tutti gli altri danno del **tu**: Laurel la chiama «Blueriver», il **Dio Gatto Nero** usa
  un tu ieratico («ella», «donde»), il **Dio Gatto Bianco** — che poi e' Lisa — un tu caldo
  e ironico. Vinch e' cerimonioso ma sul tu: al suo «you don't have to be so formal»
  corrisponde «non serve che tu sia cosi' cerimonioso», perche' fra loro il lei non c'e'
  mai stato.
- L'ordine dei segnaposto nei `StringFormatTemplate` e' invertito rispetto all'inglese,
  perche' in italiano il sostantivo viene prima del tema (*Sedia Quercia*, non *Quercia
  Sedia*). Le **etichette col due punti** restano invece davanti (*Ricetta: ...*).
- Le misure dell'enciclopedia usano la **virgola** decimale: `0,6` e non `0.6`.
- Le accentate si possono usare liberamente: il font di ripiego funziona. Evita pero'
  `«»` e `…` a larghezza piena dei font CJK; usa virgolette e puntini normali.
- Dove l'inglese e' incoerente con se stesso (*Chuity Bakery* per *Munchy Bakery*, *Vince*
  per *Vinch*, *Bomb Potion* per *Boom Potion*, *Starlight Cave* per la *Grotta Stellata*),
  vince il nome che il giocatore vede davvero in gioco.

### Termini fissati nella sessione del 2026-08-24

Enite, Clala, Roy e Arin — cioe' tutti i personaggi rimasti — sono stati chiusi in questa
sessione. Termini nuovi o chiariti:

Bitter Grape Tea Tree → **albero del te' d'uva amara** · Cat Gods → **Dei Gatti** ·
tote bag → **borsa** · Cleansing Herb → **erba purificatrice** · Glowpetal Bloom →
**fiore di petalucente** · village hall → **municipio** · wisteria tree → **glicine** ·
Star Scouts → **Esploratori delle Stelle** · sprinkles → **codette** · powdered sugar →
**zucchero a velo** · drizzle → **glassa a filo** · Squishychub → **Cicciotondo** ·
Miscella's Toy House → **Casa dei Giocattoli di Miscella** · Moonlight Jonnie → **Jonnie
Chiardiluna** · Dessert Contest → **Concorso dei Dolci** · Garden of Empty Words →
**Giardino delle Parole Vuote** · pansy → **viola del pensiero** · Hanging Vine → **vite
rampicante** · Little Honey Pumpkin → **zucchetta al miele** · Silent Guardian →
**Guardiano Silenzioso** · Book of Stars → **Libro delle Stelle** · ancient witch language
→ **antica lingua delle streghe** · freshman → **matricola** · child awakened by starlight
→ **bambina risvegliata dalla luce delle stelle** · Dreamers of Eternity → **Sognatori
dell'Eternita'** · half-blood → **mezzosangue** · Fallen Star → **Stella Caduta** ·
Sweetleaf → **Dolcefoglia** · Maze of Letters → **Labirinto delle Lettere** ·
Witch Association → **Associazione delle Streghe** · magic lens → **lente magica**.

Cognomi: **SilverRain** (come la targhetta in gioco `InteractableObject/People_Pax`, non
«Silverrain») e **Eastchip** invariabile, mai «gli Eastchips». L'inglese usa anche
«Istakipper» per la famiglia di Rex: vince Eastchip, perche' e' come Rex si presenta.

Voci nuove: **Clala** ride «Ehehe», **Roy** «Eh», **Rex** «Uahahah» (uniformato dalle
otto battute che avevano ancora «Wahahah»), **Arin** parla lentamente e spezza le frasi
con i puntini di sospensione — quella spezzatura e' il suo tratto e va tenuta. Il
**Guardiano Silenzioso** (il libro parlante) da' del voi, solenne e sprezzante.

### Cinque uniformazioni fatte a posteriori

Sono tutte partite dallo stesso sintomo: lo stesso termine reso in due modi in due file
diversi. Vanno cercate appena si sospettano, non a fine lavoro.

1. **Il plurale di «Dio Gatto»** era «Dei Gatto», «Dei Gatti» e «Dei Gatto»: adesso e'
   sempre **Dei Gatti**, in dieci punti fra dialoghi e tabelle.
2. **Le quantita'** avevano ancora 58 battute con il prefisso inglese `Nx [lua(...)]`:
   girate tutte in `[lua(...)] xN`.
3. **`«»` e `…`** erano rimasti in 51 voci: sostituiti con virgolette dritte e tre punti,
   perche' sono glifi a larghezza piena dei font CJK e a schermo vengono larghi e centrati.
4. **`pansy`** era «viola» nei mobili e «viola del pensiero» nei dialoghi: adesso e'
   sempre **viola del pensiero**, anche in `InteriorPropBasicName`.
5. **`Choice/NotHaveItem`** e' un suffisso incollato a una lista di oggetti. Era « serve.»,
   che sbaglia il numero quando gli oggetti sono due; adesso e' **« da procurare.»**, che
   regge singolare e plurale.

### Due trappole nuove sui marcatori

- **`<Slumbering Tree>`** (il negozio nelle lettere di Brad) **non** e' testo: il
  validatore lo legge come un tag `<s…>` di TextMeshPro, quindi va ricopiato in inglese.
  Vale per qualunque `<parola>` che cominci con una lettera usata dai tag TMP.
- Il font astrale `<font="Astronomicon SDF">X</font>` fascia **un glifo solo** e va
  ricopiato carattere per carattere. Nella storia del Guardiano Silenzioso c'e' perfino un
  `<font="Astronomicon SDF" >W </font >` con gli spazi sbagliati: va riprodotto cosi'.

### Come scrivere i blocchi

Gli script che generano i blocchi vanno scritti con lo strumento **Write**, non con un
heredoc di bash: nell'heredoc le sequenze `\n` e `\` si perdono per strada. E' costato un
giro di correzioni sui ritratti della biblioteca, dove `\n` doveva essere un a capo vero e
invece era finito nel JSON come due caratteri.
