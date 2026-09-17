<#
  Assembla il distribuibile di «Little Witch in the Woods in italiano».

    1. compila i plugin BepInEx (traduzione + icone controller) e il gestore WPF
    2. costruisce Payload\ (BepInEx + Traduzione + ControllerPrompts)
    3. produce dist\LittleWitchInItaliano\, utilizzabile come versione portatile
    4. se Inno Setup e' installato, genera l'installer LittleWitchInItaliano-<versione>.exe

  Uso:  .\build.ps1  [-SkipInno] [-SkipPlugin]
#>
param([switch]$SkipInno, [switch]$SkipPlugin)

$ErrorActionPreference = 'Stop'
$installer = $PSScriptRoot
$root      = Split-Path -Parent $installer
$dist      = Join-Path $installer 'dist'
$stage     = Join-Path $dist 'LittleWitchInItaliano'
$payload   = Join-Path $stage 'Payload'

$manager      = Join-Path $root 'Mod\manager\LittleWitchInItaliano.csproj'
$plugin       = Join-Path $root 'Mod\LittleWitchIT\LittleWitchIT.csproj'
$pluginDll    = Join-Path $root 'Mod\LittleWitchIT\bin\Release\netstandard2.1\LittleWitchItalian.dll'
$padPlugin    = Join-Path $root 'Mod\ControllerPrompts\ControllerPrompts.csproj'
$padPluginDll = Join-Path $root 'Mod\ControllerPrompts\bin\Release\netstandard2.1\ControllerPrompts.dll'

function Passo($m) { Write-Host "`n=== $m ===" -ForegroundColor Cyan }

# ----------------------------------------------------------------- compilazione
if (-not $SkipPlugin) {
    Passo 'Compilazione dei plugin BepInEx'
    dotnet build $plugin -c Release --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw 'compilazione di LittleWitchIT fallita' }

    dotnet build $padPlugin -c Release --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw 'compilazione di ControllerPrompts fallita' }
}
if (-not (Test-Path $pluginDll)) { throw "plugin traduzione non trovato: $pluginDll" }
if (-not (Test-Path $padPluginDll)) { throw "plugin controller non trovato: $padPluginDll" }

if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage | Out-Null

Passo 'Pubblicazione del gestore (un solo .exe)'
dotnet publish $manager -c Release -r win-x64 --self-contained false `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $stage --nologo -v q
if ($LASTEXITCODE -ne 0) { throw 'pubblicazione del gestore fallita' }

# Risorse grafiche accanto all'eseguibile
$res = Join-Path $stage 'Resources'
New-Item -ItemType Directory -Force -Path $res | Out-Null
Copy-Item (Join-Path $root 'Mod\manager\Resources\*') $res -Force

# i .pdb non servono a chi installa
Get-ChildItem $stage -Filter *.pdb -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue

# --------------------------------------------------------------------- payload
Passo 'Preparazione del carico (Payload)'

# BepInEx estratto dallo zip ufficiale
$bepzip = Get-ChildItem (Join-Path $root '_downloads') -Filter 'BepInEx_win_x64_*.zip' |
          Sort-Object Name | Select-Object -Last 1
if (-not $bepzip) { throw 'BepInEx non trovato in _downloads' }
$bepDst = Join-Path $payload 'BepInEx'
New-Item -ItemType Directory -Force -Path $bepDst | Out-Null
Expand-Archive -LiteralPath $bepzip.FullName -DestinationPath $bepDst -Force
Remove-Item (Join-Path $bepDst 'changelog.txt') -Force -ErrorAction SilentlyContinue
Write-Host "  BepInEx: $($bepzip.Name)"

# Modulo 1: Traduzione
$tr = Join-Path $payload 'Traduzione'
New-Item -ItemType Directory -Force -Path $tr | Out-Null
Copy-Item $pluginDll $tr -Force

$trad = Join-Path $tr 'traduzioni'
$dial = Join-Path $trad 'dialoghi'
New-Item -ItemType Directory -Force -Path $dial | Out-Null
Copy-Item (Join-Path $root 'Mod\translations\parts\*.json') $trad -Force
Copy-Item (Join-Path $root 'Mod\translations\dialoghi\*.json') $dial -Force
$ui = @(Get-ChildItem $trad -Filter *.json).Count
$dl = @(Get-ChildItem $dial -Filter *.json).Count
Write-Host "  Modulo Traduzione: $ui file UI, $dl dialoghi"

# Modulo 2: Prompt controller PlayStation
$cp = Join-Path $payload 'ControllerPrompts'
New-Item -ItemType Directory -Force -Path $cp | Out-Null
Copy-Item $padPluginDll $cp -Force
Write-Host "  Modulo ControllerPrompts: 1 file"

# Documentazione a corredo
$leggimi = Get-Content (Join-Path $installer 'LEGGIMI.txt') -Raw -Encoding UTF8
[System.IO.File]::WriteAllText((Join-Path $stage 'LEGGIMI.txt'), $leggimi,
                               (New-Object System.Text.UTF8Encoding $true))
Copy-Item (Join-Path $installer 'AVVISO-IT.txt')  $stage -Force
Copy-Item (Join-Path $installer 'LICENZA-IT.txt') $stage -Force

$mb = (Get-ChildItem $stage -Recurse -File | Measure-Object Length -Sum).Sum / 1MB
Write-Host ("`nCartella pronta: {0}  ({1:N1} MB)" -f $stage, $mb) -ForegroundColor Green

# ------------------------------------------------------------------ Inno Setup
if ($SkipInno) { return }

$iscc = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    Write-Host "`nInno Setup non installato: salto il setup .exe." -ForegroundColor Yellow
    Write-Host "La cartella $stage e' comunque utilizzabile come versione portatile."
    return
}

Passo 'Generazione del setup .exe'
$version = ([xml](Get-Content $manager)).Project.PropertyGroup.Version |
           Where-Object { $_ } | Select-Object -First 1
if (-not $version) { throw "versione non trovata in $manager" }
Write-Host "  versione $version"

& $iscc "/DAppVersion=$version" (Join-Path $installer 'setup.iss')
if ($LASTEXITCODE -ne 0) { throw 'Inno Setup ha fallito' }
Write-Host "`nSetup generato in $dist" -ForegroundColor Green
