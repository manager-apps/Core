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
        case "--init-config":
          options.InitConfig = true;
          options.RunService = false;
          break;

        case "--init-cert":
        case "--init-certificate":
          options.InitCertificate = true;
          options.RunService = false;
          break;

        case "--server-url":
          if (i + 1 < args.Length)
            options.ServerUrl = args[++i];
          break;

        case "--set-version":
          options.SetVersion = true;
          options.RunService = false;
          break;

        case "--agent-version":
          if (i + 1 < args.Length)
            options.Version = args[++i];
          break;

        case "--agent-name":
          if (i + 1 < args.Length)
            options.AgentName = args[++i];
          break;

        case "--tag":
          if (i + 1 < args.Length)
            options.Tag = args[++i];
          break;

        case "--enrollment-token":
          if (i + 1 < args.Length)
            options.EnrollmentToken = args[++i];
          break;

        case "--cert-path":
        case "--certificate-path":
          if (i + 1 < args.Length)
            options.CertificatePath = args[++i];
          break;

        case "--cert-password":
        case "--certificate-password":
          if (i + 1 < args.Length)
            options.CertificatePassword = args[++i];
          break;

        case "--run":
          options.RunService = true;
          break;
      }
    }

    return options;
  }
}
