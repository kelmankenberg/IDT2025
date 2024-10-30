; To run this script when performing a Build:
; Within C:\dev\IDT2025\IDT2025.csproj, include and uncomment the following lines:
; <Target Name="PostBuild" AfterTargets="PostBuildEvent">
;    <Exec Command="&quot;C:\Program Files (x86)\Inno Setup 6\ISCC.exe&quot; &quot;C:\dev\IDT2025\InnoSetup\IDT2025Setup.iss&quot; " />
;  </Target>

[Setup]
AppId={{FCA5CC62-1A3A-4504-995C-629BFF1F6E41}}
AppName=IDT2025
AppVersion=0.0.5
AppPublisher=Your Company
AppPublisherURL=https://yourcompany.com
AppSupportURL=https://yourcompany.com/support
AppUpdatesURL=https://yourcompany.com/updates
DefaultDirName={pf64}\IDT2025
DefaultGroupName=IDT2025
OutputBaseFilename=IDT2025Setup
Compression=lzma
SolidCompression=yes
OutputDir=Output\InnoSetup

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Include all files in the Output directory except the InnoSetup folder
Source: "Output\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Exclude the InnoSetup folder
;Source: "Output\InnoSetup\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "Output\InnoSetup\*"

[Icons]
Name: "{group}\IDT2025"; Filename: "{app}\IDT2025.exe"
Name: "{group}\{cm:UninstallProgram,IDT2025}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\IDT2025"; Filename: "{app}\IDT2025.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\IDT2025.exe"; Description: "{cm:LaunchProgram,IDT2025}"; Flags: nowait postinstall skipifsilent

