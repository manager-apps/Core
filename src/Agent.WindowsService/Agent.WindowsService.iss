; Inno Setup Script for DCI Agent Service

#define MyAppName "DCI Agent Service"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "TUKE"
#define MyAppURL "https://yourcompany.com"
#define MyAppExeName "Agent.WindowsService.exe"

#define SourceDir "..\..\publish"
#define OutputDir "..\..\installer-output"

#define ServiceName "DciAgentService"
#define ServiceDisplayName "DCI Agent Service"
#define DefaultServerUrl "http://147.232.52.190:5000"

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
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
Filename: "sc.exe"; Parameters: "stop {#ServiceName}"; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')
Filename: "taskkill.exe"; Parameters: "/F /IM {#MyAppExeName}"; Flags: runhidden waituntilterminated
Filename: "{app}\{#MyAppExeName}"; Parameters: "{code:GetConfigParams}"; Flags: runhidden waituntilterminated; Check: ShouldOverwriteConfig; StatusMsg: "Configuring agent..."

Filename: "sc.exe"; Parameters: "create {#ServiceName} binPath= ""{app}\{#MyAppExeName}"" start= auto DisplayName= ""{#ServiceDisplayName}"" obj= LocalSystem"; Flags: runhidden waituntilterminated; Check: not ServiceExists('{#ServiceName}')
Filename: "sc.exe"; Parameters: "config {#ServiceName} binPath= ""{app}\{#MyAppExeName}"" start= auto obj= LocalSystem DisplayName= ""{#ServiceDisplayName}"""; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')
Filename: "sc.exe"; Parameters: "description {#ServiceName} ""Custom agent for remote management and monitoring"""; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "failure {#ServiceName} reset= 60 actions= restart/5000"; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "start {#ServiceName}"; Flags: runhidden waituntilterminated

[UninstallRun]
Filename: "sc.exe"; Parameters: "failure {#ServiceName} reset= 0 actions= restart/86400000"; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')
Filename: "sc.exe"; Parameters: "config {#ServiceName} start= disabled"; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')
Filename: "sc.exe"; Parameters: "stop {#ServiceName}"; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')
Filename: "taskkill.exe"; Parameters: "/F /IM {#MyAppExeName}"; Flags: runhidden waituntilterminated; Check: FileExists(ExpandConstant('{app}\{#MyAppExeName}'))
Filename: "sc.exe"; Parameters: "delete {#ServiceName}"; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')

[Code]
var
  ConfigPage: TInputQueryWizardPage;
  OverwritePage: TWizardPage;
  KeepExistingRadio: TRadioButton;
  OverwriteRadio: TRadioButton;
  ConfigExists: Boolean;

const
  ConfigFilePath = '{commonappdata}\Manager\config.json';

function ServiceExists(const ServiceName: string): Boolean;
var
  ResultCode: Integer;
begin
  Result :=
    Exec('sc.exe', 'query "' + ServiceName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
    and (ResultCode = 0);
end;

function ConfigFileExists(): Boolean;
begin
  Result := FileExists(ExpandConstant(ConfigFilePath));
end;

function GetCommandLineParam(const ParamName: string): string;
var
  I: Integer;
  Param: string;
begin
  Result := '';
  for I := 1 to ParamCount do
  begin
    Param := ParamStr(I);
    if (Pos(ParamName + '=', Param) = 1) then
    begin
      Result := Copy(Param, Length(ParamName) + 2, MaxInt);
      Exit;
    end;
  end;
end;

function ShouldOverwriteConfig(): Boolean;
begin
  if WizardSilent then
  begin
    Result := (GetCommandLineParam('/OVERWRITECONFIG') = '1');
    Exit;
  end;

  if not ConfigExists then
  begin
    Result := True;
    Exit;
  end;
  Result := Assigned(OverwriteRadio) and OverwriteRadio.Checked;
end;

function GetConfigParams(Param: string): string;
var
  ServerUrl, Tag, AgentName, ClientSecret: string;
begin
  if not ShouldOverwriteConfig() then
  begin
    Result := '';
    Exit;
  end;

  ServerUrl := GetCommandLineParam('/SERVERURL');
  Tag := GetCommandLineParam('/TAG');
  AgentName := GetCommandLineParam('/AGENTNAME');
  ClientSecret := GetCommandLineParam('/CLIENTSECRET');

  if not WizardSilent then
  begin
    if (ConfigPage.Values[0] <> '') then
      ServerUrl := ConfigPage.Values[0];
    if (ConfigPage.Values[1] <> '') then
      Tag := ConfigPage.Values[1];
    if (ConfigPage.Values[2] <> '') then
      AgentName := ConfigPage.Values[2];
    if (ConfigPage.Values[3] <> '') then
      ClientSecret := ConfigPage.Values[3];
  end;

  Result := '--init-config';
  if ServerUrl <> '' then
    Result := Result + ' --server-url "' + ServerUrl + '"'
  else
    Result := Result + ' --server-url "{#DefaultServerUrl}"';

  if AgentName <> '' then
    Result := Result + ' --agent-name "' + AgentName + '"';

  if ClientSecret <> '' then
    Result := Result + ' --init-secrets --client-secret "' + ClientSecret + '"';

  if Tag <> '' then
    Result := Result + ' --tag "' + Tag + '"';
end;

procedure InitializeWizard();
var
  InfoLabel: TNewStaticText;
begin
  ConfigExists := ConfigFileExists();

  if ConfigExists then
  begin
    OverwritePage := CreateCustomPage(
      wpSelectDir,
      'Existing configuration detected',
      'Choose how to proceed'
    );

    InfoLabel := TNewStaticText.Create(OverwritePage);
    InfoLabel.Parent := OverwritePage.Surface;
    InfoLabel.Left := 0;
    InfoLabel.Top := 0;
    InfoLabel.Width := OverwritePage.SurfaceWidth;
    InfoLabel.Height := ScaleY(40);
    InfoLabel.Caption :=
      'An existing configuration was found at:' + #13#10 +
      ExpandConstant(ConfigFilePath) + #13#10 +
      'Do you want to keep it or overwrite it?';

    KeepExistingRadio := TRadioButton.Create(OverwritePage);
    KeepExistingRadio.Parent := OverwritePage.Surface;
    KeepExistingRadio.Left := 0;
    KeepExistingRadio.Top := InfoLabel.Top + InfoLabel.Height + ScaleY(12);
    KeepExistingRadio.Width := OverwritePage.SurfaceWidth;
    KeepExistingRadio.Caption := 'Keep existing configuration (recommended)';
    KeepExistingRadio.Checked := True;

    OverwriteRadio := TRadioButton.Create(OverwritePage);
    OverwriteRadio.Parent := OverwritePage.Surface;
    OverwriteRadio.Left := 0;
    OverwriteRadio.Top := KeepExistingRadio.Top + ScaleY(24);
    OverwriteRadio.Width := OverwritePage.SurfaceWidth;
    OverwriteRadio.Caption := 'Overwrite configuration and enter new values';
  end;

  if ConfigExists then
    ConfigPage := CreateInputQueryPage(OverwritePage.ID,
      'Agent Configuration', 'Configure the DCI Agent settings',
      'Enter the configuration values below:')
  else
    ConfigPage := CreateInputQueryPage(wpSelectDir,
      'Agent Configuration', 'Configure the DCI Agent settings',
      'Enter the configuration values below:');

  ConfigPage.Add('Server URL:', False);
  ConfigPage.Add('Source Tag (required):', False);
  ConfigPage.Add('Agent Name (required, must be unique):', False);
  ConfigPage.Add('Secret (required):', True);

  ConfigPage.Values[0] := '{#DefaultServerUrl}';
  ConfigPage.Values[1] := '';
  ConfigPage.Values[2] := '';
  ConfigPage.Values[3] := '';
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if WizardSilent then Exit;
  if ConfigExists and (PageID = ConfigPage.ID) then
  begin
    if Assigned(KeepExistingRadio) and KeepExistingRadio.Checked then
      Result := True;
  end;
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
    if not UninstallSilent then
      MsgBox('{#MyAppName} has been uninstalled.', mbInformation, MB_OK);
  end;
end;
