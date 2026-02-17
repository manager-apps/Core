# Table of Contents

- [About the project](#about-the-project)
- [Built With](#built-with)
- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Agent Installation](#agent-installation)
    - [Option 1: Manual Installation (Windows)](#option-1-manual-installation-windows)
    - [Option 2: Automated Installation (Ansible)](#option-2-automated-installation-ansible)
- [Architectural design](#architectural-design)
  - [Server design](#server-design)
  - [Agent design](#agent-design)
- [Connection and communication](#connection-and-communication)
  - [Agent State Machine](#agent-state-machine)
  - [Agent Instructions](#agent-instructions)
    - [Instruction Types](#instruction-types)
    - [AI Assistant Integration](#ai-assistant-integration)
    - [Metrics visualization](#metrics-visualization)

# About the project

This project is a monitoring agent-based system that collects various system metrics such as
CPU usage, memory usage, disk usage, and network activity. The agent is designed to run as a Windows
service, allowing it to operate in the background and start automatically when the system boots up.
The collected metrics are then sent to a central server for analysis and visualization. The agent
can retrieve instructions from the server, allowing for dynamic configuration and control. The client
is integrated with an AI assistant to describe instruction problems and solutions.

# Built With

- C# .Net 10
- Grafana
- ClickHouse
- PostgreSQL
- ReactJS Typescript

# Features

- **Secure mTLS Authentication** - Mutual TLS with internal CA and certificate management
- **Real-time Monitoring** - CPU, memory, disk, and network metrics collection
- **Remote Instruction Execution** - Execute shell commands, apply GPO policies, update configurations
- **Multi-agent Management** - Centralized management of multiple Windows agents
- **Offline Resilience** - Local SQLite storage when server is unreachable
- **Automatic Certificate Lifecycle** - Enrollment, renewal, and revocation handling
- **Outbox Pattern** - Reliable message processing and delivery
- **Time-series Analytics** - ClickHouse integration for high-performance metrics storage
- **Custom Dashboards** - Grafana integration for visualization
- **AI-powered Assistance** - Integrated AI assistant for troubleshooting
- **RESTful API** - Full API for automation and integration
- **Docker Deployment** - Easy deployment with Docker Compose
- **Ansible Automation** - Automated agent installation and configuration

# Getting Started

## Prerequisites

- **For Server**:
  - Docker and Docker Compose
  - Certificates for mTLS (can be generated using provided PowerShell script)

- **For Agent**:
  - Windows OS (tested on Windows Server 2016+, Windows 10+)
  - .NET 10 Runtime (included in installer)

## Agent Installation

### Option 1: Manual Installation (Windows)

1. **Build the agent installer**:
   ```powershell
   cd Core
   dotnet publish src/Agent.WindowsService/Agent.WindowsService.csproj `
     -c Release -r win-x64 --self-contained true `
     /p:PublishSingleFile=true -o publish

   # Build installer with Inno Setup
   "C:\Program Files (x86)\Inno Setup 6\iscc.exe" src/Agent.WindowsService/Agent.WindowsService.iss
   ```

2. **Create enrollment token** via UI:
   - Navigate to `http://localhost:3001`
   - Go to Agents → Create Enrollment Token
   - Specify agent name and validity period
   - Copy the generated token

3. **Install the agent**:
   ```powershell
   .\DciAgentService-1.0.0-Setup.exe `
     /VERYSILENT /NORESTART /SUPPRESSMSGBOXES `
     /SERVERURL="http://your-server-ip:5140" `
     /TAG="production" `
     /AGENTNAME="windows-server-01" `
     /ENROLLMENTTOKEN="<your-token-here>"
   ```

4. **Verify installation**:
   - Check Windows Services for "DciAgentService"
   - Check logs in `C:\Program Files\Manager\logs\`
   - Verify agent appears in UI

### Option 2: Automated Installation (Ansible)

1. **Configure Ansible inventory**:
   ```ini
   # Deploy/ansible/inventory.ini
   [windows_agents]
   server1 ansible_host=192.168.1.10
   server2 ansible_host=192.168.1.11

   [windows_agents:vars]
   ansible_user=Administrator
   ansible_password=YourPassword
   ansible_connection=winrm
   ansible_winrm_server_cert_validation=ignore
   ```

2. **Run installation playbook**:
   ```bash
   cd Deploy/ansible
   ansible-playbook -i inventory.ini install_agent.yml \
     -e agent_tag=production \
     -e enrollment_token=<your-token>
   ```

**Configuration Override** (upgrade without losing config):
```powershell
# Upgrade existing installation, keep configuration
.\DciAgentService-1.0.1-Setup.exe /VERYSILENT /NORESTART

# Upgrade and reconfigure
.\DciAgentService-1.0.1-Setup.exe /VERYSILENT /NORESTART `
  /OVERWRITECONFIG=1 `
  /SERVERURL="http://new-server:5140" `
  /TAG="staging"
```

# Architectural design

This section describes the architectural design of the monitoring agent system, including the server and agent components, their interactions, and the overall system architecture.

#### Server design

<img width="1780" height="885" alt="image" src="https://github.com/user-attachments/assets/2c5782f0-2a19-4463-bfb9-34dcbb956bfc" />

The server is designed to handle incoming data from multiple agents. It uses the outbox message pattern to ensure reliable data processing and delivery.
The server stores the collected metrics and instruction results in a database, then instruction and metric workers process the
data and store the processed data in ClickHouse or PostgreSQL for analysis and visualization. The server also provides a web interface for users
to view the collected metrics via Grafana iframe, and a custom UI for agent management and instruction execution.

#### Agent design

foto here

The agent is designed to run as a Windows service, allowing it to operate in the background and start automatically when the system boots up.
Communication between the agent and the server is established via mTLS, ensuring secure data transmission. When there is no connection with the server,
the agent will store the collected metrics locally and send them to the server once the connection is re-established.
The response to metric requests includes instructions to execute, which can be used to dynamically configure the agent's behavior and control its operations.


# Connection and communication

The system uses mutual TLS (mTLS) authentication to establish secure communication between agents and the server. This ensures that both the agent and server verify each other's identity before exchanging data.

<img width="910" height="1289" alt="image" src="https://github.com/user-attachments/assets/5773647f-18a7-47a2-bc44-4c96cb5e8731" />

**Communication Flow:**

1. **Enrollment** - Admin creates a one-time enrollment token via UI, which is used by the agent to obtain a client certificate:
   - Admin generates enrollment token (32-byte random, SHA-256 hashed with salt)
   - Agent sends CSR (Certificate Signing Request) with token to Ingest Server (HTTP)
   - Server validates token, signs certificate with internal CA, stores it in database
   - Agent receives certificate and CA cert, stores them in Windows Certificate Store

2. **mTLS Handshake** - Agent establishes secure connection with Ingest Server:
   - Agent initiates TLS connection with client certificate (HTTPS port 5141)
   - Server validates certificate signature against CA
   - Server checks certificate in database (active, not revoked, not expired)
   - Server creates authenticated session with claims (AgentName, Thumbprint, Serial)

3. **Synchronization** - After TLS connection is established, the agent synchronizes hardware and config information with the server:
   - Agent sends: OS version, machine name, processor count, memory, current configuration
   - Server updates agent record in database with hardware info
   - Server responds with configuration (intervals, limits, allowed collectors/instructions)

4. **Heartbeat Loop** - Agent continuously reports metrics and receives instructions:
   - Agent collects system metrics (CPU, memory, disk, network)
   - Agent sends metrics + instruction results to server via POST /report
   - Server stores data as outbox messages for async processing
   - Server responds with pending instructions for the agent to execute
   - Agent executes instructions and stores results locally until next heartbeat

5. **Certificate Lifecycle** - Automatic renewal and revocation handling:
   - Agent automatically renews certificate before expiration (configurable threshold)
   - Admin can revoke certificates via UI, blocking agent access immediately
   - Revoked agents can re-enroll using a new enrollment token

All communication after initial enrollment is secured with mTLS, ensuring end-to-end encryption and mutual authentication. When offline, the agent stores metrics locally (SQLite) and syncs when connection is restored.

## Agent State Machine

The agent operates using a state machine pattern to manage its lifecycle and ensure reliable operation. The state machine handles authentication, synchronization, metric collection, instruction execution, and error recovery.

<img width="798" height="814" alt="image" src="https://github.com/user-attachments/assets/dda09966-4452-4e61-af27-c743539aed3c" />

**State Descriptions:**

- **Idle**: Initial state when the agent service starts. Waits for the start trigger.

- **Authentication**: Validates or obtains mTLS certificate:
  - Checks if valid certificate exists in Windows Certificate Store
  - If no certificate exists, performs enrollment using enrollment token
  - If certificate is expiring soon, performs renewal via mTLS
  - If certificate is revoked, attempts re-enrollment
  - Transitions to Synchronization on success, or Error on failure

- **Synchronization**: Initial handshake with server after authentication:
  - Sends hardware information (OS version, machine name, processors, memory)
  - Sends current agent configuration
  - Receives server configuration settings
  - Updates agent state with server response
  - Transitions to Running on success, or Error on failure

- **Running**: Main operational state with heartbeat loop:
  - Collects system metrics (CPU, memory, disk, network) via configured collectors
  - Retrieves stored instruction results from local database
  - Sends metrics and instruction results to server via POST /report
  - Receives pending instructions from server
  - Stores instructions in local database for execution
  - Waits for configured interval (RunningExitIntervalSeconds)
  - Transitions to Execution if instructions exist, or loops back to Running

- **Execution**: Executes pending instructions from the server:
  - Retrieves pending instructions from local database (up to configured limit)
  - Executes instructions based on type:
    - **Shell Command**: Executes PowerShell/CMD commands and captures output
    - **GPO Update**: Applies Windows Group Policy changes
    - **Config Update**: Modifies agent configuration (collectors, executors, intervals)
  - Stores execution results in local database
  - Waits for configured interval (ExecutionExitIntervalSeconds)
  - Transitions back to Running to report results

- **Error**: Error recovery state with exponential backoff:
  - Logs error details for debugging
  - Waits for configured interval (AuthenticationExitIntervalSeconds)
  - Automatically retries by transitioning to Authentication
  - Can be stopped manually via service control

**Triggers:**

- `Start`: Service starts → moves from Idle to Authentication
- `Stop`: Service stops → returns to Idle from any state
- `AuthSuccess` / `AuthFailure`: Authentication result
- `SyncSuccess` / `SyncFailure`: Synchronization result
- `RunSuccess` / `RunFailure`: Running state result (has instructions or error)
- `ExecutionSuccess` / `ExecutionFailure`: Instruction execution result
- `Retry`: Automatic retry from Error state

The state machine ensures that the agent always recovers from errors and maintains a consistent operational flow. All state transitions are logged for monitoring and debugging purposes.

## Agent Instructions

The agent can execute custom instructions sent from the server, allowing for remote management and control.

<img width="1508" height="885" alt="Agent Instructions UI" src="https://github.com/user-attachments/assets/3a5758f7-9975-4301-83ac-f8ed1f4c22be" />

### Instruction Types

#### 1. Shell Command
Execute PowerShell or CMD commands and return the results to the server.

<img width="1484" height="763" alt="Shell Command Instruction" src="https://github.com/user-attachments/assets/69ac93f6-609b-42e3-b408-1fa4533832b9" />

**Use cases:**
- Run diagnostic scripts
- Execute system maintenance tasks
- Collect custom information
- Perform automated remediation

#### 2. Config Update
Update the agent's configuration dynamically, including metric collectors, instruction executors, intervals, and limits.

<img width="1513" height="840" alt="Config Update Instruction" src="https://github.com/user-attachments/assets/cf0f92be-12b1-4ee3-b8eb-da9bef313c0d" />

**Configurable parameters:**
- Heartbeat intervals
- Execution limits
- Enabled collectors
- Enabled instruction types
- Retry intervals

The instruction is created automatically when the configuration for the current agent has been changed.

#### 3. GPO Update
Apply Windows Group Policy Object (GPO) settings to the agent machine.

<img width="1515" height="736" alt="GPO Update Instruction" src="https://github.com/user-attachments/assets/57379cd1-f84f-4e41-a8b4-b99f9eb90404" />

**Capabilities:**
- Apply security policies
- Configure system settings
- Enforce organizational standards
- Manage Windows features

### AI Assistant Integration

The UI includes an AI-powered assistant to help troubleshoot instruction failures and suggest solutions. Example:

<img width="2559" height="1271" alt="AI Assistant" src="https://github.com/user-attachments/assets/a366bad9-630e-43ce-b39c-e72661287096" />

### Metrics visualization

The server integrates with Grafana to provide real-time visualization of collected metrics. Users can create
custom dashboards to monitor CPU usage, memory usage, disk activity, and network performance across all agents. Example:

<img width="2553" height="1265" alt="image" src="https://github.com/user-attachments/assets/5481cb21-26d6-429b-97d9-538ef7a96d59" />





