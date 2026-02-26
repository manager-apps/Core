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
#define DefaultServerCertUrl "https://localhost:5141"
#define DefaultServerNotCertUrl "http://localhost:5140"

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
Filename: "{app}\{#MyAppExeName}"; Parameters: "{code:GetCertParams}"; Flags: runhidden waituntilterminated; Check: ShouldInitCertificate; StatusMsg: "Configuring certificate..."
Filename: "{app}\{#MyAppExeName}"; Parameters: "--set-version --agent-version ""{#MyAppVersion}"""; Flags: runhidden waituntilterminated; Check: ConfigFileExists(); StatusMsg: "Updating version..."

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
  CertPage: TInputQueryWizardPage;
  OverwritePage: TWizardPage;
  KeepExistingRadio: TRadioButton;
  OverwriteRadio: TRadioButton;
  ConfigExists: Boolean;

const
  ConfigFilePath = '{commonappdata}\DCIAgent\config.json';

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

function ShouldInitCertificate(): Boolean;
var
  CertPath: string;
begin
  Result := False;
  CertPath := GetCommandLineParam('/CERTPATH');
  if WizardSilent then
  begin
    Result := CertPath <> '';
    Exit;
  end;

  if Assigned(CertPage) and (CertPage.Values[0] <> '') then
    Result := True;
end;

function GetConfigParams(Param: string): string;
var
  ServerCertUrl, ServerNotCertUrl, Tag, AgentName, EnrollmentToken: string;
begin
  if not ShouldOverwriteConfig() then
  begin
    Result := '';
    Exit;
  end;

  ServerCertUrl := GetCommandLineParam('/SERVERCERTURL');
  ServerNotCertUrl := GetCommandLineParam('/SERVERNOTCERTURL');
  Tag := GetCommandLineParam('/TAG');
  AgentName := GetCommandLineParam('/AGENTNAME');
  EnrollmentToken := GetCommandLineParam('/ENROLLMENTTOKEN');

  if not WizardSilent then
  begin
    if (ConfigPage.Values[0] <> '') then
      ServerCertUrl := ConfigPage.Values[0];
    if (ConfigPage.Values[1] <> '') then
      ServerNotCertUrl := ConfigPage.Values[1];
    if (ConfigPage.Values[2] <> '') then
      Tag := ConfigPage.Values[2];
    if (ConfigPage.Values[3] <> '') then
      AgentName := ConfigPage.Values[3];
    if Assigned(CertPage) and (CertPage.Values[1] <> '') then
      EnrollmentToken := CertPage.Values[1];
  end;

  Result := '--init-config';
  if ServerCertUrl <> '' then
    Result := Result + ' --server-cert-url "' + ServerCertUrl + '"'
  else
    Result := Result + ' --server-cert-url "{#DefaultServerCertUrl}"';

  if ServerNotCertUrl <> '' then
    Result := Result + ' --server-not-cert-url "' + ServerNotCertUrl + '"'
  else
    Result := Result + ' --server-not-cert-url "{#DefaultServerNotCertUrl}"';

  if AgentName <> '' then
    Result := Result + ' --agent-name "' + AgentName + '"';

  if Tag <> '' then
    Result := Result + ' --tag "' + Tag + '"';

  if EnrollmentToken <> '' then
    Result := Result + ' --enrollment-token "' + EnrollmentToken + '"';

  Result := Result + ' --agent-version "' + '{#MyAppVersion}' + '"';
end;

function GetCertParams(Param: string): string;
var
  CertPath, CertPassword: string;
begin
  Result := '';
  if not ShouldInitCertificate() then Exit;

  CertPath := GetCommandLineParam('/CERTPATH');
  CertPassword := GetCommandLineParam('/CERTPASSWORD');

  if not WizardSilent then
  begin
    if Assigned(CertPage) then
    begin
      if CertPage.Values[0] <> '' then
        CertPath := CertPage.Values[0];
    end;
  end;

  if CertPath <> '' then
  begin
    Result := '--init-cert --cert-path "' + CertPath + '"';
    if CertPassword <> '' then
      Result := Result + ' --cert-password "' + CertPassword + '"';
  end;
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
      'Enter the basic configuration values below:')
  else
    ConfigPage := CreateInputQueryPage(wpSelectDir,
      'Agent Configuration', 'Configure the DCI Agent settings',
      'Enter the basic configuration values below:');

  ConfigPage.Add('Server Cert URL:', False);
  ConfigPage.Add('Server Not Cert URL:', False);
  ConfigPage.Add('Source Tag:', False);
  ConfigPage.Add('Agent Name (must be unique):', False);

  ConfigPage.Values[0] := '{#DefaultServerCertUrl}';
  ConfigPage.Values[1] := '{#DefaultServerNotCertUrl}';
  ConfigPage.Values[2] := '';
  ConfigPage.Values[3] := '';

  CertPage := CreateInputQueryPage(ConfigPage.ID,
    'Certificate Configuration', 'Configure certificate and enrollment',
    'Enter certificate and enrollment token information:');

  CertPage.Add('Certificate Path (optional, for pre-provisioned cert):', False);
  CertPage.Add('Enrollment Token (required for initial setup):', True);

  CertPage.Values[0] := '';
  CertPage.Values[1] := '';
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if WizardSilent then Exit;
  if ConfigExists and ((PageID = ConfigPage.ID) or (PageID = CertPage.ID)) then
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
