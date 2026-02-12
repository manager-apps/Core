using Server.InstructionWorker;
using Server.InstructionWorker.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Server.InstructionWorker.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<WorkerOption>(
  builder.Configuration.GetSection("Worker"));

var connectionString = builder.Configuration["Database:Postgres:ConnectionString"]!;
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(connectionString));

var host = builder.Build();
host.Run();
