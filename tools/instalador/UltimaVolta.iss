; Instalador do Última Volta para Windows (Inno Setup 6).
; Gera Entrega\UltimaVolta_Instalador_v2.1.exe a partir do build em Builds\UltimaVolta.
; Para compilar: abra este arquivo no Inno Setup e clique em Build > Compile,
; ou rode: ISCC.exe tools\instalador\UltimaVolta.iss

#define AppName "Última Volta"
#define AppVersion "2.1"
#define AppPublisher "Grupo Última Volta"
#define AppExe "UltimaVolta.exe"

[Setup]
AppId={{02CC5385-07C9-42C2-B4FB-9FF4BFB968C8}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/Cesar-Otavio/jogo-ultima-volta
AppSupportURL=https://github.com/Cesar-Otavio/jogo-ultima-volta
; Instala só para o usuário atual: não pede senha de administrador.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
DefaultDirName={autopf}\Ultima Volta
DisableProgramGroupPage=yes
OutputDir=..\..\Entrega
OutputBaseFilename=UltimaVolta_Instalador_v{#AppVersion}
SetupIconFile=icone.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; A pasta "BackUpThisFolder_ButDontShipItWithYourGame" é só para depuração e fica de fora.
Source: "..\..\Builds\UltimaVolta\*"; DestDir: "{app}"; Excludes: "*DontShip*"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
