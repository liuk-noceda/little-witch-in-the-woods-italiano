; ============================================================================
;  Little Witch in the Woods in italiano — Liuk Noceda (Luca Nasi)
;  Installer del gestore (Inno Setup 6)
;
;  Nota: questo setup NON tocca la cartella del gioco. Installa il gestore, che
;  e' il programma con cui si applica e si toglie la traduzione. La patch viene
;  scritta solo quando l'utente preme "Installa la traduzione".
; ============================================================================

#define AppName        "Little Witch in the Woods in italiano"

; La versione la passa build.ps1 con /DAppVersion, presa dal .csproj del gestore,
; che e' l'unico posto in cui e' scritta. Il ripiego serve solo a chi compila
; questo file a mano dall'IDE di Inno, e dice a voce alta che il numero non e'
; quello vero invece di inventarne uno credibile.
#ifndef AppVersion
  #define AppVersion "0.0.0-compilato-a-mano"
#endif

#define AppPublisher   "Liuk Noceda (Luca Nasi)"
#define AppURL         "https://github.com/liuk-noceda"
#define AppExe         "LittleWitchInItaliano.exe"
#define SourceDir      "dist\LittleWitchInItaliano"

[Setup]
AppId={{2E9B7C14-6A3D-4F58-9B21-7D0E5A4C8F63}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
DefaultDirName={autopf}\Liuk Noceda\Little Witch in the Woods in italiano
DefaultGroupName=Liuk Noceda
DisableProgramGroupPage=yes
OutputDir=dist
OutputBaseFilename=LittleWitchInItaliano-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\Mod\manager\Resources\icon.ico
WizardImageFile=..\Mod\manager\Resources\banner.png
WizardSmallImageFile=..\Mod\manager\Resources\banner_small.png
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
; nessun componente di sistema: basta l'utente corrente
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
LicenseFile=LICENZA-IT.txt
InfoBeforeFile=AVVISO-IT.txt

[Languages]
Name: "italiano"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "english";  MessagesFile: "compiler:Default.isl"

[Messages]
; il file e' salvato in UTF-8 con BOM, quindi le accentate si scrivono direttamente
italiano.WelcomeLabel2=Questo programma installa il gestore di Little Witch in the Woods in italiano (traduzione e icone controller PlayStation).%n%nNessun file del gioco viene modificato adesso: la traduzione e le mod si applicano dal gestore, e si possono togliere in qualsiasi momento.
english.WelcomeLabel2=This will install the manager for Little Witch in the Woods in Italian (translation and PlayStation controller prompts).%n%nNo game file is modified now: the mods are applied from the manager, and can be removed at any time.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}";              Filename: "{app}\{#AppExe}"
Name: "{group}\Disinstalla {#AppName}";  Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";        Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
{ Il gestore richiede .NET Desktop Runtime 8. Se manca, si offre di scaricarlo. }
function DotNet8DesktopPresente: Boolean;
var
  trovato: Boolean;
  nomi: TArrayOfString;
  i: Integer;
  radice: String;
begin
  trovato := False;
  radice := 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  if RegGetValueNames(HKEY_LOCAL_MACHINE, radice, nomi) then
    for i := 0 to GetArrayLength(nomi) - 1 do
      if Pos('8.', nomi[i]) = 1 then
        trovato := True;

  if not trovato then
  begin
    radice := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
    if RegGetValueNames(HKEY_LOCAL_MACHINE, radice, nomi) then
      for i := 0 to GetArrayLength(nomi) - 1 do
        if Pos('8.', nomi[i]) = 1 then
          trovato := True;
  end;

  Result := trovato;
end;

function InitializeSetup(): Boolean;
var
  err: Integer;
begin
  Result := True;
  if not DotNet8DesktopPresente then
  begin
    if MsgBox('Per funzionare, il gestore richiede .NET Desktop Runtime 8,' + #13#10 +
              'che non risulta installato.' + #13#10#13#10 +
              'Vuoi aprire la pagina di download adesso?' + #13#10 +
              '(Puoi proseguire comunque e installarlo dopo.)',
              mbConfirmation, MB_YESNO) = IDYES then
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0/runtime',
                '', '', SW_SHOW, ewNoWait, err);
  end;
end;

{ Avvisa se la traduzione e' ancora applicata: va tolta dal gestore, non da qui,
  altrimenti nella cartella del gioco resterebbero BepInEx e il plugin orfani. }
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    MsgBox('Ricorda: se la traduzione è ancora applicata al gioco, toglila dal gestore' + #13#10 +
           'PRIMA di disinstallarlo, con il pulsante "Rimuovi".' + #13#10#13#10 +
           'Questa disinstallazione rimuove solo il gestore, non la traduzione dal gioco.',
           mbInformation, MB_OK);
end;
