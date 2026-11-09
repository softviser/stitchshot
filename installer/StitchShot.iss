; Inno Setup script for the Windows installer. Built by CI from the published single-file exe:
;   iscc /DMyAppVersion=1.0.0 /DSourceExe=path\to\StitchShot.exe /O<output dir> installer\StitchShot.iss
; Installs per user (no admin prompt), so the Run key it writes is the same HKCU value the app manages.

#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif
#ifndef SourceExe
  #define SourceExe "..\publish\full\StitchShot.exe"
#endif

[Setup]
AppId={{A7C3E1F2-5B84-4D6E-9F01-3C2B7A8D4E51}
AppName=StitchShot
AppVersion={#MyAppVersion}
AppVerName=StitchShot {#MyAppVersion}
AppPublisher=Softviser Yazılım
AppPublisherURL=https://softviser.com.tr
AppSupportURL=https://github.com/softviser/stitchshot/issues
AppUpdatesURL=https://github.com/softviser/stitchshot/releases
DefaultDirName={autopf}\StitchShot
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
LicenseFile=..\LICENSE
SetupIconFile=..\src\StitchShot.App\Assets\StitchShot.ico
UninstallDisplayIcon={app}\StitchShot.exe
OutputBaseFilename=StitchShot-{#MyAppVersion}-win-x64-setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "startup"; Description: "Start StitchShot when I log in"; GroupDescription: "Startup:"
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceExe}"; DestDir: "{app}"; DestName: "StitchShot.exe"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\StitchShot"; Filename: "{app}\StitchShot.exe"
Name: "{autodesktop}\StitchShot"; Filename: "{app}\StitchShot.exe"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "StitchShot"; ValueData: """{app}\StitchShot.exe"""; Flags: uninsdeletevalue; Tasks: startup
; The app can turn startup on later from Settings; make sure uninstall removes that entry too.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "StitchShot"; Flags: uninsdeletevalue dontcreatekey

[Run]
Filename: "{app}\StitchShot.exe"; Description: "{cm:LaunchProgram,StitchShot}"; Flags: nowait postinstall skipifsilent
