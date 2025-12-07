#define MyAppName "Yemen WhatsApp Desktop"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Yemen Software"
#define MyAppURL "https://www.yemenwhatsapp.com"
#define MyAppExeName "YemenWhatsApp.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE
OutputDir=Output
OutputBaseFilename=YemenWhatsApp_Setup
SetupIconFile=app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

[Languages]
Name: "arabic"; MessagesFile: "compiler:Languages\Arabic.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "Dist\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "app.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // التحقق من تثبيت .NET 6.0
  if not IsDotNetInstalled('Microsoft.NETCore.App', '6.0.0') then
  begin
    MsgBox('Yemen WhatsApp يتطلب .NET 6.0 Desktop Runtime.' + #13#10 +
           'يرجى تثبيته من موقع Microsoft ثم إعادة تشغيل التثبيت.',
           mbInformation, MB_OK);
    Result := False;
  end;
end;

function IsDotNetInstalled(Profile: String; Version: String): Boolean;
var
  Success: Boolean;
  Install: Cardinal;
begin
  Success := RegQueryDWordValue(HKLM, 
    'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + Profile, 
    'Release', Install);
  Result := Success and (Install >= StrToInt(Version));
end;