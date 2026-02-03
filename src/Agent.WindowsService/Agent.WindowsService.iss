; Inno Setup Script for DCI Agent Service

#define MyAppName "DCI Agent Service"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "TUKE"
#define MyAppURL "https://yourcompany.com"
#define MyAppExeName "Agent.WindowsService.exe"

#define SourceDir "..\..\publish"
#define OutputDir "..\..\installer-output"

; Service technical name (no spaces)
#define ServiceName "DciAgentService"
; Service display name (visible in services.msc)
#define ServiceDisplayName "DCI Agent Service"

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
OutputBaseFilename=DciAgentService-{#MyAppVersion}-Setup
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Copy the entire publish folder
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
; Create or update service
Filename: "sc.exe"; Parameters: "create {#ServiceName} binPath= ""{app}\{#MyAppExeName}"" start= auto DisplayName= ""{#ServiceDisplayName}"" obj= LocalSystem"; Flags: runhidden waituntilterminated; Check: not ServiceExists('{#ServiceName}')
Filename: "sc.exe"; Parameters: "config {#ServiceName} binPath= ""{app}\{#MyAppExeName}"" start= auto obj= LocalSystem DisplayName= ""{#ServiceDisplayName}"""; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')

Filename: "sc.exe"; Parameters: "description {#ServiceName} ""Custom agent for remote management and monitoring"""; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "failure {#ServiceName} reset= 60 actions= restart/5000"; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "start {#ServiceName}"; Flags: runhidden waituntilterminated

[UninstallRun]
Filename: "sc.exe"; Parameters: "stop {#ServiceName}"; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')
Filename: "sc.exe"; Parameters: "delete {#ServiceName}"; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')

[Code]
function ServiceExists(const ServiceName: string): Boolean;
var
  ResultCode: Integer;
begin
  Result :=
    Exec('sc.exe', 'query "' + ServiceName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
    and (ResultCode = 0);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    if not WizardSilent then
      MsgBox('{#MyAppName} has been installed successfully.', mbInformation, MB_OK);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if not WizardSilent then
      MsgBox('{#MyAppName} has been uninstalled.', mbInformation, MB_OK);
  end;
end;
