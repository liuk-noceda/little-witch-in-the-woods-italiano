# Little Witch in the Woods in italiano - installatore
# Traduzione amatoriale di Liuk Noceda (Luca Nasi).
#
# Non modifica nessun file del gioco: aggiunge BepInEx e il plugin della
# traduzione, che inserisce l'italiano in memoria a ogni avvio.
# Disinstallando si torna esattamente alla situazione di partenza.

$ErrorActionPreference = 'Stop'
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch {}

$RadiceInstaller = Split-Path -Parent $MyInvocation.MyCommand.Path
$Contenuto       = Join-Path $RadiceInstaller 'contenuto'
$NomePlugin      = 'LittleWitchItalian.dll'
$CartellaPlugin  = 'BepInEx\plugins\LiukNoceda'
$ChiavePrefs     = 'HKCU:\Software\SunnySideUp\Little Witch In The Woods'

# --- aspetto ---------------------------------------------------------------

function Titolo {
    Clear-Host
    Write-Host ''
    Write-Host '  Little Witch in the Woods - traduzione italiana' -ForegroundColor Magenta
    Write-Host '  di Liuk Noceda' -ForegroundColor DarkGray
    Write-Host '  ---------------------------------------------------------' -ForegroundColor DarkGray
    Write-Host ''
}

function Ok    ($t) { Write-Host "  [ok]  $t" -ForegroundColor Green }
function Info  ($t) { Write-Host "        $t" -ForegroundColor Gray }
function Avviso($t) { Write-Host "  [!]   $t" -ForegroundColor Yellow }
function Errore($t) { Write-Host "  [X]   $t" -ForegroundColor Red }

function Pausa {
    Write-Host ''
    Read-Host '  Premi INVIO per continuare'
}

# --- trovare il gioco ------------------------------------------------------

function E-IlGioco ($cartella) {
    if (-not $cartella) { return $false }
    if (-not (Test-Path -LiteralPath $cartella)) { return $false }
    return (Test-Path -LiteralPath (Join-Path $cartella 'LWIW.exe')) -and
           (Test-Path -LiteralPath (Join-Path $cartella 'LWIW_Data'))
}

function Librerie-Steam {
    $librerie = @()
    $radici = @()
    foreach ($chiave in @('HKCU:\Software\Valve\Steam',
                          'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam',
                          'HKLM:\SOFTWARE\Valve\Steam')) {
        try {
            $v = Get-ItemProperty -Path $chiave -ErrorAction Stop
            foreach ($n in @('SteamPath', 'InstallPath')) {
                if ($v.$n) { $radici += ($v.$n -replace '/', '\') }
            }
        } catch {}
    }
    $radici += "${env:ProgramFiles(x86)}\Steam"
    $radici += "${env:ProgramFiles}\Steam"

    foreach ($r in ($radici | Select-Object -Unique)) {
        if (-not (Test-Path -LiteralPath $r)) { continue }
        $librerie += $r
        $vdf = Join-Path $r 'steamapps\libraryfolders.vdf'
        if (Test-Path -LiteralPath $vdf) {
            foreach ($riga in (Get-Content -LiteralPath $vdf -ErrorAction SilentlyContinue)) {
                if ($riga -match '"path"\s+"(.+?)"') {
                    $librerie += ($Matches[1] -replace '\\\\', '\')
                }
            }
        }
    }
    return ($librerie | Select-Object -Unique)
}

function Cerca-Gioco {
    $candidati = @(
        'C:\Games\Little Witch in the Woods',
        "$env:ProgramFiles\Little Witch in the Woods",
        "${env:ProgramFiles(x86)}\Little Witch in the Woods"
    )

    foreach ($lib in (Librerie-Steam)) {
        $comune = Join-Path $lib 'steamapps\common'
        if (-not (Test-Path -LiteralPath $comune)) { continue }
        foreach ($d in (Get-ChildItem -LiteralPath $comune -Directory -ErrorAction SilentlyContinue)) {
            $candidati += $d.FullName
        }
    }

    # GOG
    foreach ($chiave in @('HKLM:\SOFTWARE\WOW6432Node\GOG.com\Games',
                          'HKLM:\SOFTWARE\GOG.com\Games')) {
        try {
            foreach ($g in (Get-ChildItem -Path $chiave -ErrorAction Stop)) {
                $p = (Get-ItemProperty -Path $g.PSPath -ErrorAction SilentlyContinue).path
                if ($p) { $candidati += $p }
            }
        } catch {}
    }

    foreach ($c in ($candidati | Select-Object -Unique)) {
        if (E-IlGioco $c) { return $c }
    }
    return $null
}

function Chiedi-Cartella {
    Write-Host ''
    Info 'Trascina qui la cartella del gioco (quella con LWIW.exe) e premi INVIO,'
    Info 'oppure incolla il percorso. Lascia vuoto per annullare.'
    Write-Host ''
    while ($true) {
        $r = Read-Host '  Cartella'
        if ([string]::IsNullOrWhiteSpace($r)) { return $null }
        $r = $r.Trim().Trim('"').Trim("'")
        if (E-IlGioco $r) { return $r }
        # magari hanno trascinato l'eseguibile invece della cartella
        $padre = Split-Path -Parent $r
        if (E-IlGioco $padre) { return $padre }
        Errore 'Lì dentro non c''è LWIW.exe. Riprova.'
    }
}

function Ottieni-Gioco {
    $g = Cerca-Gioco
    if ($g) {
        Ok "Gioco trovato: $g"
        Write-Host ''
        $r = Read-Host '  È quello giusto? [S/n]'
        if ($r -eq '' -or $r -match '^[sSyY]') { return $g }
    } else {
        Avviso 'Non sono riuscito a trovare il gioco da solo.'
    }
    return (Chiedi-Cartella)
}

# --- permessi di scrittura -------------------------------------------------

function Posso-Scrivere ($cartella) {
    $prova = Join-Path $cartella ('.prova_' + [guid]::NewGuid().ToString('N') + '.tmp')
    try {
        [System.IO.File]::WriteAllText($prova, 'x')
        Remove-Item -LiteralPath $prova -Force
        return $true
    } catch { return $false }
}

function Rilancia-Da-Amministratore {
    Avviso 'La cartella del gioco richiede i permessi di amministratore.'
    Write-Host ''
    $r = Read-Host '  Rilancio l''installatore come amministratore? [S/n]'
    if ($r -ne '' -and $r -notmatch '^[sSyY]') { return $false }
    $argomenti = @('-NoProfile', '-ExecutionPolicy', 'Bypass',
                   '-File', "`"$($MyInvocation.MyCommand.Path)`"")
    try {
        Start-Process -FilePath 'powershell.exe' -ArgumentList $argomenti -Verb RunAs | Out-Null
        return $true
    } catch {
        Errore 'Avvio come amministratore rifiutato o non riuscito.'
        return $false
    }
}

# --- installazione ---------------------------------------------------------

function Installa-BepInEx ($gioco) {
    $zip = Get-ChildItem -LiteralPath $Contenuto -Filter 'BepInEx_*.zip' -ErrorAction SilentlyContinue |
           Select-Object -First 1
    if (-not $zip) {
        Errore 'Manca il pacchetto di BepInEx dentro la cartella "contenuto".'
        return $false
    }
    if (Test-Path -LiteralPath (Join-Path $gioco 'winhttp.dll')) {
        Ok 'BepInEx era già installato: lo lascio com''è.'
        return $true
    }
    Info "Installo $($zip.Name)..."
    $temp = Join-Path ([System.IO.Path]::GetTempPath()) ('lwiw_' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $temp -Force | Out-Null
    try {
        Expand-Archive -LiteralPath $zip.FullName -DestinationPath $temp -Force
        Copy-Item -Path (Join-Path $temp '*') -Destination $gioco -Recurse -Force
        Ok 'BepInEx installato.'
        return $true
    } finally {
        Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction SilentlyContinue
    }
}

function Installa ($gioco) {
    Write-Host ''
    if (-not (Posso-Scrivere $gioco)) {
        if (Rilancia-Da-Amministratore) { exit 0 }
        return
    }

    $bloccato = Get-Process -Name 'LWIW' -ErrorAction SilentlyContinue
    if ($bloccato) {
        Errore 'Il gioco è aperto. Chiudilo e riprova.'
        return
    }

    if (-not (Installa-BepInEx $gioco)) { return }

    $dest = Join-Path $gioco $CartellaPlugin
    New-Item -ItemType Directory -Path $dest -Force | Out-Null

    $dll = Join-Path $Contenuto $NomePlugin
    if (-not (Test-Path -LiteralPath $dll)) {
        Errore "Manca $NomePlugin dentro la cartella ""contenuto""."
        return
    }
    Copy-Item -LiteralPath $dll -Destination $dest -Force
    Ok 'Plugin della traduzione copiato.'

    # Le traduzioni vengono rifatte da zero, cosi' i file vecchi non restano in giro.
    $traduzioni = Join-Path $dest 'traduzioni'
    if (Test-Path -LiteralPath $traduzioni) {
        Remove-Item -LiteralPath $traduzioni -Recurse -Force
    }
    Copy-Item -LiteralPath (Join-Path $Contenuto 'traduzioni') -Destination $dest -Recurse -Force

    $ui   = @(Get-ChildItem -LiteralPath $traduzioni -Filter '*.json' -ErrorAction SilentlyContinue).Count
    $dial = @(Get-ChildItem -LiteralPath (Join-Path $traduzioni 'dialoghi') -Filter '*.json' -ErrorAction SilentlyContinue).Count
    Ok "Testi copiati: $ui file di interfaccia, $dial di dialoghi."

    Write-Host ''
    Write-Host '  Installazione completata.' -ForegroundColor Green
    Write-Host ''
    Info 'Adesso avvia il gioco e vai in Impostazioni -> Lingua -> Italiano.'
    Info 'La lingua si applica al RIAVVIO successivo del gioco: chiudilo e riaprilo.'
}

# --- disinstallazione ------------------------------------------------------

function Rimetti-Inglese {
    if (-not (Test-Path -LiteralPath $ChiavePrefs)) { return }
    $byte = [System.Text.Encoding]::UTF8.GetBytes('en') + [byte]0
    foreach ($nome in @('Language_h3872303031', 'SelectedLanguageCode_h729211379')) {
        try {
            Set-ItemProperty -LiteralPath $ChiavePrefs -Name $nome -Value $byte -Type Binary
        } catch {}
    }
    Ok 'Lingua del gioco rimessa su inglese.'
}

function Disinstalla ($gioco) {
    Write-Host ''
    if (-not (Posso-Scrivere $gioco)) {
        if (Rilancia-Da-Amministratore) { exit 0 }
        return
    }
    $bloccato = Get-Process -Name 'LWIW' -ErrorAction SilentlyContinue
    if ($bloccato) {
        Errore 'Il gioco è aperto. Chiudilo e riprova.'
        return
    }

    $dest = Join-Path $gioco $CartellaPlugin
    if (Test-Path -LiteralPath $dest) {
        Remove-Item -LiteralPath $dest -Recurse -Force
        Ok 'Traduzione rimossa.'
    } else {
        Info 'La traduzione non risultava installata.'
    }

    Rimetti-Inglese

    # BepInEx si tocca solo se non lo usa nessun'altra mod.
    $plugins = Join-Path $gioco 'BepInEx\plugins'
    $altre = @()
    if (Test-Path -LiteralPath $plugins) {
        $altre = @(Get-ChildItem -LiteralPath $plugins -Force -ErrorAction SilentlyContinue)
    }
    if ($altre.Count -gt 0) {
        Write-Host ''
        Info 'BepInEx resta installato: ci sono altre mod dentro BepInEx\plugins.'
        return
    }

    Write-Host ''
    $r = Read-Host '  Tolgo anche BepInEx? Serviva solo a questa traduzione. [S/n]'
    if ($r -ne '' -and $r -notmatch '^[sSyY]') {
        Info 'BepInEx lasciato dov''è.'
        return
    }
    foreach ($nome in @('BepInEx', 'winhttp.dll', 'doorstop_config.ini',
                        '.doorstop_version', 'changelog.txt')) {
        $p = Join-Path $gioco $nome
        if (Test-Path -LiteralPath $p) {
            Remove-Item -LiteralPath $p -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    Ok 'BepInEx rimosso. Il gioco è tornato come prima.'
}

# --- stato -----------------------------------------------------------------

function Stato ($gioco) {
    $dest = Join-Path $gioco $CartellaPlugin
    $installata = Test-Path -LiteralPath (Join-Path $dest $NomePlugin)
    if ($installata) {
        $t = Join-Path $dest 'traduzioni'
        $ui   = @(Get-ChildItem -LiteralPath $t -Filter '*.json' -ErrorAction SilentlyContinue).Count
        $dial = @(Get-ChildItem -LiteralPath (Join-Path $t 'dialoghi') -Filter '*.json' -ErrorAction SilentlyContinue).Count
        Write-Host "  Stato: traduzione INSTALLATA ($ui + $dial file di testo)" -ForegroundColor Green
    } else {
        Write-Host '  Stato: traduzione non installata' -ForegroundColor DarkGray
    }
}

function Mostra-Log ($gioco) {
    $log = Join-Path $gioco 'BepInEx\LogOutput.log'
    Write-Host ''
    if (-not (Test-Path -LiteralPath $log)) {
        Avviso 'Nessun log: avvia il gioco almeno una volta dopo aver installato.'
        return
    }
    $righe = @(Select-String -LiteralPath $log -Pattern '\[tabella\]|\[dialoghi\]|\[accenti\]|Lingua aggiunta|lingue disponibili' -ErrorAction SilentlyContinue)
    if ($righe.Count -eq 0) {
        Avviso 'Il log non contiene righe della traduzione. Il plugin non è partito?'
        return
    }
    Info "Ultime righe utili di ${log}:"
    Write-Host ''
    foreach ($r in ($righe | Select-Object -Last 25)) {
        Write-Host ('        ' + $r.Line.Trim()) -ForegroundColor DarkGray
    }
    Write-Host ''
    if ($righe -match 'risultano vuote') {
        Avviso 'Attenzione: il log segnala battute vuote. Segnalalo all''autore.'
    } else {
        Ok 'Il log non segnala problemi.'
    }
}

# --- menu ------------------------------------------------------------------

Titolo
$gioco = Ottieni-Gioco
if (-not $gioco) {
    Write-Host ''
    Errore 'Nessuna cartella scelta. Esco.'
    Pausa
    exit 1
}

while ($true) {
    Titolo
    Write-Host "  Gioco: $gioco" -ForegroundColor White
    Stato $gioco
    Write-Host ''
    Write-Host '    1) Installa (o aggiorna) la traduzione italiana'
    Write-Host '    2) Disinstalla la traduzione'
    Write-Host '    3) Controlla il log del gioco'
    Write-Host '    4) Cambia la cartella del gioco'
    Write-Host '    0) Esci'
    Write-Host ''
    $scelta = Read-Host '  Scelta'
    switch ($scelta) {
        '1' { Installa $gioco;    Pausa }
        '2' { Disinstalla $gioco; Pausa }
        '3' { Mostra-Log $gioco;  Pausa }
        '4' {
            $nuovo = Chiedi-Cartella
            if ($nuovo) { $gioco = $nuovo }
        }
        '0' { exit 0 }
        default { }
    }
}
