; Inno Setup Script for Agent Windows Service
#define MyAppName "Agent Windows Service"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "TUKE"
#define MyAppURL "https://yourcompany.com"
#define MyAppExeName "Agent.WindowsService.exe"
#define SourceDir "..\..\publish"
#define OutputDir "..\..\installer-output"

[Setup]
AppId={{12345678-1234-1234-1234-123456789012}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir={#OutputDir}
OutputBaseFilename=AgentWindowsService-{#MyAppVersion}-Setup
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\*.json"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "sc.exe"; Parameters: "create AgentWindowsService binPath= ""{app}\{#MyAppExeName}"" start= auto DisplayName= ""Agent Windows Service"" obj= LocalSystem"; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "description AgentWindowsService ""Agent for remote management and monitoring"""; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "failure AgentWindowsService reset= 60 actions= restart/5000"; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "start AgentWindowsService"; Flags: runhidden waituntilterminated

[UninstallRun]
Filename: "sc.exe"; Parameters: "stop AgentWindowsService"; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "delete AgentWindowsService"; Flags: runhidden waituntilterminated

[Code]
function IsAdmin: Boolean;
begin
  Result := IsAdminLoggedOn;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    if not IsAdmin then
    begin
      MsgBox('This service installation requires administrator privileges.' + #13 +
             'Please run the installer as administrator.', mbCriticalError, MB_OK);
      Abort;
    end;
  end;

  if CurStep = ssPostInstall then
  begin
    MsgBox('Agent Windows Service has been installed successfully!' + #13 + #13 +
           'The service is running under LocalSystem account with full permissions for monitoring.' + #13 + #13 +
           'You can check the service status in Services (services.msc).', mbInformation, MB_OK);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    MsgBox('Agent Windows Service has been uninstalled.', mbInformation, MB_OK);
  end;
end;
