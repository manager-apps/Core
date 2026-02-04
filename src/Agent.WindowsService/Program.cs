using Agent.WindowsService;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Application;
using Agent.WindowsService.CommandLine;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using Agent.WindowsService.Infrastructure.Communication;
using Agent.WindowsService.Infrastructure.Executors;
using Agent.WindowsService.Infrastructure.Metric;
using Agent.WindowsService.Infrastructure.Store;
using Agent.WindowsService.Validators;
using FluentValidation;
using Serilog;

// Parse command line arguments
var options = CommandLineParser.Parse(args);

// Process command line options (init config, show config, etc.)
var shouldRunService = await CommandLineHandler.ProcessAsync(options);

if (!shouldRunService)
{
    return;
}

Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .WriteTo.Console()
  .WriteTo.File(PathConfig.LogsFilePath,
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 7)
  .CreateLogger();
try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddWindowsService(serviceOptions =>
    {
      serviceOptions.ServiceName = "DCIAgentService";
    });

    builder.Services.AddSingleton<IStateMachine, StateMachine>();
    builder.Services.AddSingleton<IMetricCollector, MetricCollector>();

    builder.Services.AddTransient<IValidator<Instruction>, InstructionValidator>();
    builder.Services.AddTransient<IInstructionExecutor, ShellExecutor>();
    builder.Services.AddTransient<IInstructionExecutor, GpoExecutor>();

    builder.Services.AddSingleton<ISecretStore, DpapiSecretStore>();
    builder.Services.AddSingleton<IConfigurationStore, JsonConfigurationStore>();
    builder.Services.AddSingleton<IMetricStore, SqliteMetricStore>();
    builder.Services.AddSingleton<IInstructionStore, SqliteInstructionStore>();

    builder.Services.AddSerilog();

    builder.Services.AddHttpClient<IServerClient, HttpServerClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "Agent.WindowsService/1.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = 10
        });

    builder.Services.AddHostedService<Worker>();
    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
