using Server.Api.Common.Extensions;
using Server.Api.Features.Agent;
using Server.Api.Features.Instruction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwagger(builder.Configuration);
builder.Services.AddPsqlDatabase(builder.Configuration);
builder.Services.AddClickHouseDatabase(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddCors(builder.Configuration);
builder.Services.AddHybridCache(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddAgentServices();
builder.Services.AddInstructionServices();

var app = builder.Build();
await app.ApplyMigrationsAsync();

// Only for load testing purposes
// await app.SeedLoadTestDataAsync();

app.UseSwaggerDocs();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

var group = app.MapGroup("/api/v1");

group.MapAgentEndpoints();
group.MapInstructionEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.UseHttpsRedirection();

app.Run();

