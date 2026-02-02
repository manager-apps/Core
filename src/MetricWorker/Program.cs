using MetricWorker;
using MetricWorker.Infrastructure;
using MetricWorker.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var connectionStringDb = builder.Configuration["Database:Postgres:ConnectionString"]!;
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(connectionStringDb));

var connectionString = builder.Configuration["Database:ClickHouse:ConnectionString"]!;
builder.Services.AddSingleton<ClickHouse.Driver.ADO.ClickHouseConnection>(_ =>
  new ClickHouse.Driver.ADO.ClickHouseConnection(connectionString));
builder.Services.AddScoped<IMetricStorage, ClickHouseMetricStorage>();

var host = builder.Build();
host.Run();
