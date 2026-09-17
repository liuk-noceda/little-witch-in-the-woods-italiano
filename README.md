# Little Witch in the Woods in italiano

Traduzione italiana amatoriale completa per **[Little Witch in the Woods](https://store.steampowered.com/app/1594940/Little_Witch_in_the_Woods/)** (SUNNYSIDEUP), realizzata da **Liuk Noceda (Luca Nasi)**.

È una **mod runtime non invasiva**: la traduzione agisce in memoria all'avvio del gioco tramite BepInEx e HarmonyX. Nessun file originale dell'installazione viene sovrascritto o alterato in modo permanente, e la disinstallazione dal gestore ripristina byte per byte lo stato originario.

---

## Caratteristiche

* **Traduzione al 100%:**
  * Tutti i testi di interfaccia, menu, opzioni, descrizioni di creature, piante, pozioni ed enciclopedia (8.656 voci).
  * Tutti i dialoghi e gli eventi di trama (39.035 battute).
* **Integrazione nativa nel menu lingua:**
  * Aggiunge la voce **«Italiano»** nel selettore della lingua nelle impostazioni del gioco, mantenendo intatte tutte le lingue originali (incluso l'inglese).
* **Modulo Icone PlayStation (DualSense):**
  * Attiva a runtime i simboli PlayStation (Croce, Cerchio, Quadrato, Triangolo, L1/R1, L2/R2) per i controller DualSense, DualShock 4 e Steam Input, sfruttando la grafica ufficiale già presente negli archivi di gioco.
* **Gestore Mod e Installer Windows:**
  * Mod Manager grafico in WPF che permette di attivare o disattivare modularmente la Traduzione e le Icone PlayStation con semplici interruttori.
  * Setup autoinstallante generato con Inno Setup 6.

---

## Struttura del Progetto

```text
├── Mod/
│   ├── LittleWitchIT/        # Plugin BepInEx principale per la traduzione e il font TMPro
│   ├── ControllerPrompts/    # Plugin BepInEx per le icone gamepad PlayStation
│   ├── manager/              # Gestore desktop WPF (LittleWitchInItaliano.exe)
│   ├── translations/         # File JSON della traduzione (interfaccia e dialoghi)
│   └── installer/            # Script per eventuale versione portatile
├── Installer/
│   ├── build.ps1             # Script PowerShell per compilare e assemblare il distribuibile
│   ├── setup.iss             # Configurazione dell'installer Inno Setup 6
│   ├── LEGGIMI.txt           # Guida rapida inclusa nell'installer
│   ├── AVVISO-IT.txt         # Disclaimer non ufficiale
│   └── LICENZA-IT.txt        # Licenza
├── tools/                    # Script Python per estrazione, validazione e generazione asset
├── CONTINUA_QUI.md           # Note tecniche e documentazione interna del progetto
└── RIPRENDI.md               # Registro di lavoro e glossario dei termini
```

---

## Compilazione e Creazione dell'Installer

### Requisiti
* Windows 10 o 11 (64-bit)
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Inno Setup 6](https://jrsoftware.org/isdl.php) (opzionale, per generare il file setup `.exe`)
* Python 3.10+ (con `Pillow` per la rigenerazione di icone o asset)

### Assemblaggio
Aprire una console PowerShell ed eseguire:

```powershell
.\Installer\build.ps1
```

Lo script:
1. Compila i plugin BepInEx in `Release` (`netstandard2.1`).
2. Pubblica l'eseguibile del Mod Manager in modalità single-file.
3. Prepara il payload con BepInEx, i moduli e i file JSON di traduzione.
4. Genera l'eseguibile di installazione in `Installer/dist/LittleWitchInItaliano-<versione>.exe`.

---

## Note Legali e Crediti

* Progetto amatoriale indipendente, non affiliato, autorizzato né sponsorizzato da **SUNNYSIDEUP**.
* Tutti i marchi, loghi, illustrazioni e nomi di personaggi appartengono a SUNNYSIDEUP o ai rispettivi aventi diritto.
* **Componenti di terze parti:**
  * [BepInEx 5.4.23.3](https://github.com/BepInEx/BepInEx) — LGPL-2.1
  * [HarmonyX](https://github.com/BepInEx/HarmonyX) — Licenza MIT
  * [Newtonsoft.Json](https://www.newtonsoft.com/json) — Licenza MIT
