; ============================================================
;  SoundBoard — Inno Setup Script
;  Genera: SoundBoardSetup.exe
; ============================================================

#define AppName      "SoundBoard"
#define AppVersion   "1.0.0"
#define AppPublisher "Marco"
#define AppURL       ""
#define AppExeName   "SoundBoard.exe"
#define PublishDir   "..\SoundBoard\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
; ── Identificazione univoca (non cambiare dopo la prima release!)
AppId={{A3F2C1D4-8B5E-4F7A-9C2D-1E6B3A8F5D29}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
; Cartella output dell'installer (relativa a dove si trova questo .iss)
OutputDir=Output
OutputBaseFilename=SoundBoardSetup
SetupIconFile=..\SoundBoard\app_icon.ico
Compression=lzma2/ultra64
SolidCompression=yes
; Richiede privilegi di amministratore per installare in Program Files
PrivilegesRequired=admin
; Supporta sia x64 che arm64
ArchitecturesInstallIn64BitMode=x64compatible
; Stile moderno Windows 11
WizardStyle=modern
; Immagine laterale wizard (opzionale — commentare se non presente)
; WizardImageFile=wizard_image.bmp
; Banner top wizard (opzionale)
; WizardSmallImageFile=wizard_banner.bmp

[Languages]
Name: "italian";  MessagesFile: "compiler:Languages\Italian.isl"
Name: "english";  MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";  Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon";  Description: "Avvia SoundBoard all'accensione del PC"; GroupDescription: "Altro:"; Flags: unchecked

[Files]
; Copia tutti i file dalla cartella publish (self-contained → un solo .exe)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Icona nel menu Start
Name: "{group}\{#AppName}";      Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\{#AppExeName}"
Name: "{group}\Disinstalla {#AppName}"; Filename: "{uninstallexe}"
; Icona sul Desktop (opzionale, controllata dal task sopra)
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon; IconFilename: "{app}\{#AppExeName}"

[Registry]
; Avvio automatico con Windows (opzionale, controllato dal task sopra)
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#AppName}"; ValueData: """{app}\{#AppExeName}"""; Tasks: startupicon; Flags: uninsdeletevalue

[Run]
; Avvia l'app al termine dell'installazione
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Rimuove anche le impostazioni utente all'uninstall (opzionale — commentare per preservarle)
; Type: filesandordirs; Name: "{userappdata}\SoundBoard"
