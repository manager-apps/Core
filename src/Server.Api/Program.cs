using Server.Api.Features.Agent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.MapAgentFeatures();

app.UseHttpsRedirection();

app.Run();

