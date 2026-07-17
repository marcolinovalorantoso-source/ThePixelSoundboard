[Setup]
AppName=ThePixelSoundboard
AppVersion=3.0.0
AppPublisher=Marco Venditti (ThePixelBoys)
DefaultDirName={autopf}\ThePixelSoundboard
DefaultGroupName=ThePixelSoundboard
OutputDir=C:\Users\marco\Desktop
OutputBaseFilename=ThePixelSoundboard_v3.0.0_Setup
Compression=lzma
SolidCompression=yes
SetupIconFile=SoundBoard\app_icon.ico
UninstallDisplayIcon={app}\SoundBoard.exe
PrivilegesRequired=admin

[Files]
Source: "SoundBoard\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\ThePixelSoundboard"; Filename: "{app}\SoundBoard.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\ThePixelSoundboard"; Filename: "{app}\SoundBoard.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
; Avvia l'app al termine
Filename: "{app}\SoundBoard.exe"; Description: "{cm:LaunchProgram,ThePixelSoundboard}"; Flags: nowait postinstall skipifsilent
