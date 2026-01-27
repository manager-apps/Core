using WebApi.Common.Extensions;
using WebApi.Features.Agent;
using WebApi.Features.Instruction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwagger();
builder.Services.AddPsqlDatabase(builder.Configuration);
builder.Services.AddClickHouseDatabase(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddCors(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddAgentServices();
builder.Services.AddInstructionServices();

var app = builder.Build();
await app.ApplyMigrationsAsync();

app.UseSwaggerUI();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

var group = app.MapGroup("/api/v1");

group.MapAgentEndpoints();
group.MapInstructionEndpoints();

app.UseHttpsRedirection();

app.Run();

