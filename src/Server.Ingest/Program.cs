using Server.Ingest.Common.Extensions;
using Server.Ingest.Features.Cert;
using Server.Ingest.Features.Report;
using Server.Ingest.Features.Sync;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiVersioning(builder.Configuration);
builder.Services.AddPsqlDatabase(builder.Configuration);
builder.Services.AddMtls(builder.Configuration);
builder.Services.AddSwagger(builder.Configuration);
builder.Services.AddCertServices();
builder.Services.AddCertificateAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddReportServices();
builder.Services.AddSyncServices();

builder.ConfigureKestrelWithMtls();

var app = builder.Build();
app.UseSwaggerDocs();
app.UseAuthentication();
app.UseAuthorization();

var group = app
  .MapGroup("/api/v{version:apiVersion}")
  .WithApiVersionSet(app.CreateVersionSet());

group.MapCertEndpoints();
group.MapReportEndpoints();
group.MapSyncEndpoints();

app.MapGet("/health",
  () => Results.Ok(new { status = "Healthy" }));

app.Run();


