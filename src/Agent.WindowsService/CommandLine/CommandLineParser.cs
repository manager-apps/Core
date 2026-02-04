namespace Agent.WindowsService.CommandLine;

/// <summary>
/// Parses command line arguments into CommandLineOptions.
/// </summary>
public static class CommandLineParser
{
    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLowerInvariant();

            switch (arg)
            {
                case "--help":
                case "-h":
                case "/?":
                    options.ShowHelp = true;
                    options.RunService = false;
                    break;

                case "--show-config":
                    options.ShowConfig = true;
                    options.RunService = false;
                    break;

                case "--init-config":
                    options.InitConfig = true;
                    options.RunService = false;
                    break;

                case "--init-secrets":
                    options.InitSecrets = true;
                    options.RunService = false;
                    break;

                case "--server-url":
                    if (i + 1 < args.Length)
                    {
                        options.ServerUrl = args[++i];
                    }
                    break;

                case "--agent-name":
                    if (i + 1 < args.Length)
                    {
                        options.AgentName = args[++i];
                    }
                    break;

                case "--client-secret":
                    if (i + 1 < args.Length)
                    {
                        options.ClientSecret = args[++i];
                    }
                    break;

                case "--area-name":
                    if (i + 1 < args.Length)
                    {
                        options.AreaName = args[++i];
                    }
                    break;

                case "--run":
                    options.RunService = true;
                    break;
            }
        }

        return options;
    }

    public static void PrintHelp()
    {
        Console.WriteLine(@"
Agent.WindowsService - Configuration and Management Tool

USAGE:
    Agent.WindowsService.exe [OPTIONS]

OPTIONS:
    --help, -h, /?          Show this help message
    --show-config           Display current configuration
    --init-config           Initialize configuration file with default or specified values
    --init-secrets          Initialize secrets store with specified values
    --server-url <URL>      Server URL (used with --init-config)
    --agent-name <NAME>     Agent name (used with --init-config)
    --area-name <AREA>      Area name (used with --init-config)
    --client-secret <SECRET> Client secret (used with --init-secrets)
    --run                   Run the service after configuration (can be combined with init options)

EXAMPLES:
    # Show current configuration
    Agent.WindowsService.exe --show-config

    # Initialize with default configuration
    Agent.WindowsService.exe --init-config

    # Initialize with custom server URL
    Agent.WindowsService.exe --init-config --server-url ""https://myserver.com:5000""

    # Initialize with custom configuration
    Agent.WindowsService.exe --init-config --server-url ""https://myserver.com:5000"" --agent-name ""MyAgent""

    # Set client secret
    Agent.WindowsService.exe --init-secrets --client-secret ""my-secret-value""

    # Full setup with all options
    Agent.WindowsService.exe --init-config --server-url ""https://myserver.com:5000"" --init-secrets --client-secret ""my-secret"" --run

    # Silent installation (for Inno Setup)
    Agent.WindowsService.exe --init-config --server-url ""{serverUrl}"" --agent-name ""{agentName}"" --init-secrets --client-secret ""{clientSecret}""

NOTES:
    - Configuration is stored in: %ProgramData%\Manager\config.json
    - Secrets are encrypted and stored in: %ProgramData%\Manager\secrets.dat
    - When run without arguments, the service starts normally
");
    }
}
