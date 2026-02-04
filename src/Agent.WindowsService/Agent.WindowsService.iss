; Inno Setup Script for DCI Agent Service

#define MyAppName "DCI Agent Service"
#define MyAppVersion "1.0.1"
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
Filename: "{app}\{#MyAppExeName}"; Parameters: "{code:GetConfigParams}"; Flags: runhidden waituntilterminated; StatusMsg: "Configuring agent..."
Filename: "taskkill.exe"; Parameters: "/F /IM {#MyAppExeName}"; Flags: runhidden waituntilterminated; Check: ServiceExists('{#ServiceName}')

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
  OverwriteCheckbox: TNewCheckBox;
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
  // In silent mode, check for /OVERWRITECONFIG=1 parameter
  if WizardSilent then
  begin
    Result := (GetCommandLineParam('/OVERWRITECONFIG') = '1');
  end
  else
  begin
    if not ConfigExists then
      Result := True
    else
      Result := OverwriteCheckbox.Checked;
  end;
end;

function GetConfigParams(Param: string): string;
var
  ServerUrl, AreaName, AgentName, ClientSecret: string;
begin
  // If config exists and user chose not to overwrite, skip configuration
  if not ShouldOverwriteConfig() then
  begin
    Result := '--show-config';  // Just show config, don't modify
    Exit;
  end;

  // First check command line parameters (for silent install)
  ServerUrl := GetCommandLineParam('/SERVERURL');
  AreaName := GetCommandLineParam('/AREANAME');
  AgentName := GetCommandLineParam('/AGENTNAME');
  ClientSecret := GetCommandLineParam('/CLIENTSECRET');

  if not WizardSilent then
  begin
    if ConfigPage.Values[0] <> '' then
      ServerUrl := ConfigPage.Values[0];
    if ConfigPage.Values[1] <> '' then
      AreaName := ConfigPage.Values[1];
    if ConfigPage.Values[2] <> '' then
      AgentName := ConfigPage.Values[2];
    if ConfigPage.Values[3] <> '' then
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
  if AreaName <> '' then
    Result := Result + ' --area-name "' + AreaName + '"';
end;

procedure InitializeWizard();
var
  InfoLabel: TNewStaticText;
begin
  ConfigExists := ConfigFileExists();
  ConfigPage := CreateInputQueryPage(wpSelectDir,
    'Agent Configuration', 'Configure the DCI Agent settings',
    'Enter the configuration values below:');

  ConfigPage.Add('Server URL:', False);
  ConfigPage.Add('Area Name (required):', False);
  ConfigPage.Add('Agent Name (required, must be unique):', False);
  ConfigPage.Add('Secret (required):', True);  // True = password mask

  ConfigPage.Values[0] := '{#DefaultServerUrl}';
  ConfigPage.Values[1] := '';
  ConfigPage.Values[2] := '';
  ConfigPage.Values[3] := '';

  OverwriteCheckbox := TNewCheckBox.Create(ConfigPage);
  OverwriteCheckbox.Parent := ConfigPage.Surface;
  OverwriteCheckbox.Top := ConfigPage.Edits[3].Top + ConfigPage.Edits[3].Height + ScaleY(16);
  OverwriteCheckbox.Left := 0;
  OverwriteCheckbox.Width := ConfigPage.SurfaceWidth;
  OverwriteCheckbox.Height := 20;

  if ConfigExists then
  begin
    OverwriteCheckbox.Caption := 'Overwrite existing configuration (WARNING: this will replace current settings)';
    OverwriteCheckbox.Checked := False;
    OverwriteCheckbox.Visible := True;

    InfoLabel := TNewStaticText.Create(ConfigPage);
    InfoLabel.Parent := ConfigPage.Surface;
    InfoLabel.Top := OverwriteCheckbox.Top + OverwriteCheckbox.Height + 8;
    InfoLabel.Left := 0;
    InfoLabel.Width := ConfigPage.SurfaceWidth;
    InfoLabel.Caption := 'Existing configuration found at: ' + ExpandConstant(ConfigFilePath) + #13#10 + 'Check the box above to replace it with new settings.';
    InfoLabel.Font.Style := [fsItalic];
    InfoLabel.Font.Color := clGray;
  end
  else
  begin
    OverwriteCheckbox.Caption := 'New installation - configuration will be created';
    OverwriteCheckbox.Checked := True;
    OverwriteCheckbox.Enabled := False;
    OverwriteCheckbox.Visible := True;
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
