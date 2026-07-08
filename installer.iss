[Setup]
AppName=ThePixelSoundboard
AppVersion=1.0
AppPublisher=Marco Venditti (ThePixelBoys)
DefaultDirName={autopf}\ThePixelSoundboard
DefaultGroupName=ThePixelSoundboard
OutputDir=C:\Users\marco\Desktop
OutputBaseFilename=ThePixelSoundboard_Setup
Compression=lzma
SolidCompression=yes
SetupIconFile=SoundBoard\app_icon.ico
UninstallDisplayIcon={app}\SoundBoard.exe
PrivilegesRequired=admin

[Types]
Name: "full"; Description: "Installazione completa (con Driver Audio Virtuale)"
Name: "custom"; Description: "Installazione libera (senza Driver)"

[Components]
Name: "app"; Description: "ThePixelSoundboard (Applicazione)"; Types: full custom; Flags: fixed
Name: "driver"; Description: "Driver Audio Virtuale (consigliato per Discord)"; Types: full

[Files]
Source: "SoundBoard\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "installer\vbcable\*"; DestDir: "{tmp}\vbcable"; Flags: deleteafterinstall; Components: driver
Source: "installer\rename_device.ps1"; DestDir: "{app}"; Flags: ignoreversion; Components: driver

[Icons]
Name: "{group}\ThePixelSoundboard"; Filename: "{app}\SoundBoard.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\ThePixelSoundboard"; Filename: "{app}\SoundBoard.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
; Installa il driver VB-Cable solo se il componente è selezionato e il driver non è già presente
Filename: "{tmp}\vbcable\VBCABLE_Setup_x64.exe"; Parameters: "-i"; StatusMsg: "Installazione del driver audio virtuale in corso (potrebbe richiedere qualche secondo)..."; Flags: runascurrentuser; Components: driver; Check: not IsVBCableInstalled
; Esegue lo script PowerShell per rinominare i dispositivi nel Registro di sistema in "ThePixelSoundboard Audio / Mic"
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\rename_device.ps1"""; Flags: runhidden; Components: driver
; Avvia l'app al termine (solo se non è necessario il riavvio)
Filename: "{app}\SoundBoard.exe"; Description: "{cm:LaunchProgram,ThePixelSoundboard}"; Flags: nowait postinstall skipifsilent; Check: not NeedRestart

[Code]
function IsVBCableInstalled(): Boolean;
begin
  Result := RegKeyExists(HKLM64, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\VB:Cable_Cable_Driver') or
            RegKeyExists(HKLM32, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\VB:Cable_Cable_Driver');
end;

function NeedRestart(): Boolean;
begin
  // Consiglia il riavvio del PC solo se l'utente ha installato il driver da zero in questa sessione
  Result := WizardIsComponentSelected('driver') and not IsVBCableInstalled();
end;
