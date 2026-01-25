using Server.Api.Common.Extensions;
using Server.Api.Features.Agent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddCors(builder.Configuration);

builder.Services.AddAgentServices();

var app = builder.Build();
await app.ApplyMigrationsAsync();

// todo: move to extension folder
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
  });
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapAgentEndpoints();
app.UseHttpsRedirection();

app.Run();

