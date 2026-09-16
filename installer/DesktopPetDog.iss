#define MyAppName "Desktop Pet Dog"
#define MyAppVersion "1.0.0"
#define MyAppExeName "DesktopPetDog.exe"

[Setup]
AppId={{A27820F9-3192-4EBD-84F3-D3A3CC52A04B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={localappdata}\Programs\DesktopPetDog
DefaultGroupName={#MyAppName}
PrivilegesRequired=lowest
OutputDir=..\artifacts
OutputBaseFilename=DesktopPetDog-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName={#MyAppName}
CloseApplications=yes
RestartApplications=no
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{autodesktop}\Pet, Come Here!"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--summon"; Comment: "Launch or summon Desktop Pet Dog"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "DesktopPetDog"; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Desktop Pet Dog"; Flags: nowait postinstall skipifsilent

[Code]
var KeepSettings: Boolean;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var ResultCode: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    { End the visible app before removing its executable. No background helper is installed. }
    Exec(ExpandConstant('{sys}\taskkill.exe'), '/IM DesktopPetDog.exe /F', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    KeepSettings := MsgBox('Would you like to keep your pet settings for a future reinstall?'#13#10#13#10 +
      'Choose Yes to keep settings and message history.'#13#10 +
      'Choose No for complete removal of preferences, mood answers, cached animations, logs, and all locally saved data.',
      mbConfirmation, MB_YESNO) = IDYES;
    if not KeepSettings then
      DelTree(ExpandConstant('{localappdata}\DesktopPetDog'), True, True, True);
    RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'DesktopPetDog');
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
    ForceDirectories(ExpandConstant('{localappdata}\DesktopPetDog\Cache'));
end;
