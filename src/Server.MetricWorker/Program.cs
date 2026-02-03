using Microsoft.Extensions.Caching.Hybrid;
using Server.MetricWorker;
using Server.MetricWorker.Infrastructure;
using Server.MetricWorker.Interfaces;
using Microsoft.EntityFrameworkCore;
using Server.MetricWorker.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<WorkerOption>(
  builder.Configuration.GetSection("Worker"));

var connectionStringDb = builder.Configuration["Database:Postgres:ConnectionString"]!;
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(connectionStringDb));

var connectionString = builder.Configuration["Database:ClickHouse:ConnectionString"]!;
builder.Services.AddSingleton<ClickHouse.Driver.ADO.ClickHouseConnection>(_ =>
  new ClickHouse.Driver.ADO.ClickHouseConnection(connectionString));
builder.Services.AddScoped<IMetricStorage, ClickHouseMetricStorage>();

// Cache
var redisConnectionString = builder.Configuration["Cache:Redis:ConnectionString"]!;
builder.Services.AddStackExchangeRedisCache(options =>
{
  options.Configuration = redisConnectionString;
  options.InstanceName = builder.Configuration["Cache:Redis:InstanceName"];
});
builder.Services.AddHybridCache(options =>
{
  options.DefaultEntryOptions = new HybridCacheEntryOptions
  {
    Expiration = TimeSpan.FromMinutes(5),
    LocalCacheExpiration = TimeSpan.FromMinutes(5)
  };
});

var host = builder.Build();
host.Run();
