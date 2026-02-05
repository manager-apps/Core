namespace Agent.WindowsService.CommandLine;

/// <summary>
/// Represents parsed command line options for the agent.
/// </summary>
public class CommandLineOptions
{
    /// <summary>
    /// Initialize default configuration file.
    /// </summary>
    public bool InitConfig { get; set; }

    /// <summary>
    /// Initialize secrets store.
    /// </summary>
    public bool InitSecrets { get; set; }

    /// <summary>
    /// Indicates whether to set the agent version during configuration.
    /// </summary>
    public bool SetVersion { get; set; }

    /// <summary>
    /// Version of the agent software to configure.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Server URL to configure.
    /// </summary>
    public string? ServerUrl { get; set; }

    /// <summary>
    /// Agent name to configure.
    /// </summary>
    public string? AgentName { get; set; }

    /// <summary>
    /// Client secret to store securely.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Tag to configure.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Run the service normally after configuration.
    /// </summary>
    public bool RunService { get; set; } = true;
}
