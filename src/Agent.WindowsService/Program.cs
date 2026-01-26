using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Application;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using Agent.WindowsService.Infrastructure.Communication;
using Agent.WindowsService.Infrastructure.Executors;
using Agent.WindowsService.Infrastructure.Metric;
using Agent.WindowsService.Infrastructure.Store;
using Agent.WindowsService.Validators;
using Agent.WindowsService;
using FluentValidation;
using Serilog;

Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Information()
  .WriteTo.Console()
  .WriteTo.File(PathConfig.LogsFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
  .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IStateMachine, StateMachine>();
builder.Services.AddSingleton<IMetricCollector, MetricCollector>();

builder.Services.AddTransient<IValidator<Instruction>, InstructionValidator>();
builder.Services.AddTransient<IInstructionExecutor, ShellExecutor>();
builder.Services.AddTransient<IInstructionExecutor, GpoExecutor>();

builder.Services.AddSingleton<ISecretStore, DpapiSecretStore>();
builder.Services.AddSingleton<IConfigurationStore, JsonConfigurationStore >();
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
