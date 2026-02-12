using Server.Ingest.Features.Report.Create;

namespace Server.Ingest.Features.Report;

public static class ReportExtension
{
  public static void AddReportServices(this IServiceCollection services)
  {
      services.AddScoped<IReportCreateHandler, ReportCreateHandler>();
  }

  public static void MapReportEndpoints(this IEndpointRouteBuilder app)
  {
      app.MapAgentReportCreateEndpoint();
  }
}
