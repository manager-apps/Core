# Table of Contents

- [About the project](#about-the-project)
- [Built With](#built-with)

# About the project

This project is a monitoring agent based system that collects various system metrics such as
CPU usage, memory usage, disk usage, and network activity. The agent is designed to run as Windows
service, allowing it to operate in the background and start automatically when the system boots up.
The collected metrics are then sent to a central server for analysis and visualization. The agent
can retrieve instructions from the server, allowing for dynamic configuration and control. The client
integrated with AI assistant describe instruction problem and solution.

# Built With

- C# .Net 10
- Grafana
- ClickHouse
- PostgreSQL
- ReactJS Typescript

# Architectural design

<img width="1780" height="885" alt="image" src="https://github.com/user-attachments/assets/2c5782f0-2a19-4463-bfb9-34dcbb956bfc" />

#### Agent design
Agent is designed to run as a Windows service, allowing it to operate in the background and start automatically when the system boots up.
Communication between the agent and the server is established via mTLS, ensuring secure data transmission. When there is no connection with server,
the agent will store the collected metrics locally and send them to the server once the connection is re-established.
The response for metric request is instructions to execute, which can be used to dynamically configure the agent's behavior and control its operations.

<img width="1398" height="310" alt="image" src="https://github.com/user-attachments/assets/68aaa73c-2ccd-44d6-96ba-0f96d142d168" />

After the tls connection is established, the agent will synchronize the hardware and config information with the server, allowing the server to have
an up-to-date view of the agent's environment and capabilities.

<img width="1524" height="618" alt="image" src="https://github.com/user-attachments/assets/e06d6dc4-04d0-4f51-a468-bfb9754505a0" />


The agent can execute custom instructions:
<img width="1508" height="885" alt="image" src="https://github.com/user-attachments/assets/3a5758f7-9975-4301-83ac-f8ed1f4c22be" />

Instruction types:
- Shell command: execute shell command and return the result to server.
<img width="1484" height="763" alt="image" src="https://github.com/user-attachments/assets/69ac93f6-609b-42e3-b408-1fa4533832b9" />

- Config update: update the agent's configuration, can configure metric collectors, instruction executors, and other settings.
<img width="1513" height="840" alt="image" src="https://github.com/user-attachments/assets/cf0f92be-12b1-4ee3-b8eb-da9bef313c0d" />

- GPO update: update the agent's Windows Group Policy Object (GPO)
<img width="1515" height="736" alt="image" src="https://github.com/user-attachments/assets/57379cd1-f84f-4e41-a8b4-b99f9eb90404" />

<img width="1850" height="1054" alt="image" src="https://github.com/user-attachments/assets/a647b98a-cc76-4907-9c74-5104dbe8e927" />
<img width="1866" height="1049" alt="image" src="https://github.com/user-attachments/assets/9aa6839a-ad61-42d7-ad06-bccb68396108" />
<img width="1867" height="1051" alt="image" src="https://github.com/user-attachments/assets/fe8dbe9f-3300-40ca-b8a1-51ce74a9ac31" />
<img width="1866" height="1049" alt="image" src="https://github.com/user-attachments/assets/605a988b-235d-40a7-a9c3-437fc4c3a1c1" />
<img width="1852" height="1050" alt="image" src="https://github.com/user-attachments/assets/6918d5ad-83c6-44da-b96a-3297b906f754" />
<img width="2559" height="1271" alt="image" src="https://github.com/user-attachments/assets/a366bad9-630e-43ce-b39c-e72661287096" />
<img width="1402" height="924" alt="image" src="https://github.com/user-attachments/assets/378ed85a-0e8b-40ee-a5ee-b698fac34b54" />
<img width="597" height="463" alt="image" src="https://github.com/user-attachments/assets/73cc1277-a602-46de-926d-d60bd3e6bb7e" />
<img width="600" height="467" alt="image" src="https://github.com/user-attachments/assets/d21e3275-5079-41b0-839f-204da21936dd" />
<img width="601" height="465" alt="image" src="https://github.com/user-attachments/assets/1ac9023b-262e-44fd-95c9-393e079071ce" />
<img width="2554" height="1270" alt="image" src="https://github.com/user-attachments/assets/a473c538-3035-4f2d-bb04-07c4d47e9e35" />
<img width="2556" height="1271" alt="image" src="https://github.com/user-attachments/assets/a2761377-0133-4d4e-831e-ad957fd1d8fc" />
<img width="600" height="468" alt="image" src="https://github.com/user-attachments/assets/486864ba-b6b8-417b-b018-9ea15a87642a" />
<img width="596" height="466" alt="image" src="https://github.com/user-attachments/assets/6550e86a-9c4d-49fd-b259-870da93fdeb8" />
<img width="2554" height="1268" alt="image" src="https://github.com/user-attachments/assets/674438c0-3845-4282-b94e-8e5a471eb23b" />


