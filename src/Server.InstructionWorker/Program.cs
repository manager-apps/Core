using Server.InstructionWorker;
using Server.InstructionWorker.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var connectionString = builder.Configuration["Database:Postgres:ConnectionString"]!;
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(connectionString));


var host = builder.Build();
host.Run();
