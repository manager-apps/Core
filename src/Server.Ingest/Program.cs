using Server.Ingest.Common.Extensions;
using Server.Ingest.Features.Cert;
using Server.Ingest.Features.Report;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPsqlDatabase(builder.Configuration);
builder.Services.AddMtls(builder.Configuration);

builder.Services.AddCertificateAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddReportServices();

builder.ConfigureKestrelWithMtls();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapCertEndpoints();
app.MapReportEndpoints();

app.Run();


