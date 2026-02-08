using Server.Api.Common.Extensions;
using Server.Api.Features.Agent;
using Server.Api.Features.Instruction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwagger(builder.Configuration);
builder.Services.AddApiVersioning(builder.Configuration);
builder.Services.AddPsqlDatabase(builder.Configuration);
builder.Services.AddClickHouseDatabase(builder.Configuration);
builder.Services.AddCors(builder.Configuration);
builder.Services.AddHybridCache(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddAgentServices();
builder.Services.AddInstructionServices();

var app = builder.Build();
await app.ApplyMigrationsAsync();

app.UseSwaggerDocs();
app.UseCors();

var group = app
  .MapGroup("/api/v{version:apiVersion}")
  .WithApiVersionSet(app.CreateVersionSet());

group.MapAgentEndpoints();
group.MapInstructionEndpoints();

app.MapGet("/health",
  () => Results.Ok(new { status = "Healthy" }));

app.Run();
