[Setup]
AppName=rpf2fivem
AppVersion=1.0
DefaultDirName={commonpf}\rpf2fivem
DefaultGroupName=rpf2fivem
OutputDir=.
OutputBaseFilename=rpf2fivem-installer
PrivilegesRequired=admin
Compression=lzma
SolidCompression=yes
SetupIconFile=installer.ico
WizardImageFile=installer-side.bmp
WizardSmallImageFile=installer.bmp


[Files]
; Include ALL files and folders recursively from your build output
Source: "build_output\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\rpf2fivem"; Filename: "{app}\rpf2fivem.exe"
Name: "{commondesktop}\rpf2fivem"; Filename: "{app}\rpf2fivem.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Run]
; Then run the updater normally with admin right actually.
Filename: "{app}\rpf2fivem.exe"; Description: "Start rpf2fivem..."; Flags: nowait postinstall skipifsilent runascurrentuser
